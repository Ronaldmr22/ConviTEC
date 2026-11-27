using Backend.Mensajes;
using Backend.Models;

// Servicio en segundo plano que arranca junto con la aplicación.
//
//  1. Se conecta a RabbitMQ (reintenta cada 5 s si el servidor no está).
//  2. Escucha las colas de esta sede y, según el tipo de cada mensaje, lo pasa
//     a la clase que corresponde (gestor, coordinador o sincronización).
//  3. Cada 2 s difunde los cambios de inventario y libera reservas vencidas.
public class DespachadorMensajes : BackgroundService
{
    private static readonly TimeSpan EsperaReconexion = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan IntervaloTareas = TimeSpan.FromSeconds(2);

    private readonly IBrokerMensajeria broker;
    private readonly RegistroMensajesProcesados procesados;
    private readonly GestorReservasLocales gestorLocal;
    private readonly CoordinadorReservas coordinador;
    private readonly ServicioSincronizacion sincronizacion;
    private readonly string codigoSede;
    private readonly ILogger<DespachadorMensajes> logger;

    public DespachadorMensajes(
        IBrokerMensajeria broker,
        RegistroMensajesProcesados procesados,
        GestorReservasLocales gestorLocal,
        CoordinadorReservas coordinador,
        ServicioSincronizacion sincronizacion,
        ConfiguracionSede sede,
        ILogger<DespachadorMensajes> logger)
    {
        this.broker = broker;
        this.procesados = procesados;
        this.gestorLocal = gestorLocal;
        this.coordinador = coordinador;
        this.sincronizacion = sincronizacion;
        this.codigoSede = sede.GetCodigo();
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancelacion)
    {
        await ConectarConReintentos(cancelacion);
        if (cancelacion.IsCancellationRequested)
        {
            return;
        }

        await broker.SuscribirAsync(Despachar, cancelacion);
        await EjecutarSinFallar(() => sincronizacion.IniciarAsync(), "iniciar la sincronización");

        while (!cancelacion.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(IntervaloTareas, cancelacion);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await EjecutarSinFallar(() => sincronizacion.PublicarCambiosAsync(), "difundir cambios de inventario");
            await EjecutarSinFallar(() => { gestorLocal.LiberarVencidas(); return Task.CompletedTask; }, "liberar reservas vencidas");
        }
    }

    private async Task ConectarConReintentos(CancellationToken cancelacion)
    {
        while (!cancelacion.IsCancellationRequested)
        {
            try
            {
                await broker.ConectarAsync(cancelacion);
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception error)
            {
                logger.LogWarning("No se pudo conectar a RabbitMQ ({Error}). Reintentando en {Segundos} s...",
                    error.Message, EsperaReconexion.TotalSeconds);
            }

            try
            {
                await Task.Delay(EsperaReconexion, cancelacion);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    // Decide qué hacer con cada mensaje según su tipo
    private async Task Despachar(Mensaje mensaje)
    {
        // Los mensajes de difusión también le llegan a quien los envió
        if (mensaje.GetSedeOrigen() == codigoSede)
        {
            return;
        }

        if (procesados.YaProcesado(mensaje.GetId()))
        {
            logger.LogDebug("Mensaje repetido ignorado: {Id}", mensaje.GetId());
            return;
        }

        logger.LogInformation("Recibido {Tipo} de {Sede}", mensaje.GetTipo(), mensaje.GetSedeOrigen());

        switch (mensaje)
        {
            case SolicitudReserva solicitud:
                RespuestaReserva respuesta = gestorLocal.Reservar(solicitud);
                await broker.PublicarAsync(respuesta, solicitud.GetSedeOrigen());
                break;

            case RespuestaReserva respuestaReserva:
                await coordinador.RecibirRespuesta(respuestaReserva);
                break;

            case ConfirmarReserva confirmar:
                gestorLocal.Confirmar(confirmar.GetIdReserva());
                break;

            case LiberarReserva liberar:
                gestorLocal.Liberar(liberar.GetIdReserva());
                break;

            case SnapshotInventario snapshot:
                sincronizacion.AplicarSnapshot(snapshot);
                break;

            case SolicitudSnapshot solicitudSnapshot:
                await sincronizacion.ResponderSnapshotAsync(solicitudSnapshot);
                break;

            default:
                logger.LogWarning("Tipo de mensaje sin manejador: {Tipo}", mensaje.GetTipo());
                break;
        }

        procesados.Marcar(mensaje.GetId());
    }

    private async Task EjecutarSinFallar(Func<Task> accion, string descripcion)
    {
        try
        {
            await accion();
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Error al {Descripcion}", descripcion);
        }
    }
}
