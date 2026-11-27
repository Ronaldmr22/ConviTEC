using Backend.Mensajes;
using Backend.Models;
using Backend.Pedidos;
using Microsoft.Extensions.Options;

// Lado "solicitante" de las reservas: se encarga de reservar, confirmar o liberar
// los productos de un pedido registrado en ESTA sede.
//
//  - Las líneas que surte la sede local se reservan directo con el gestor local.
//  - Las líneas que surten otras sedes se piden con SolicitudReserva por RabbitMQ
//    y se espera la RespuestaReserva de cada una, con un tiempo límite.
//
// Si todas aceptan, ServicioPedidos llama a ConfirmarPedido; si alguna rechaza o
// no responde a tiempo, llama a LiberarPedido para soltar lo que sí se reservó.
public class CoordinadorReservas
{
    private readonly IBrokerMensajeria Broker;
    private readonly GestorReservasLocales GestorLocal;
    private readonly ListaEnlazada EnCurso;
    private readonly object candadoEnCurso = new object();
    private readonly int TimeoutSegundos;
    private readonly string codigoSede;
    private readonly ILogger<CoordinadorReservas> logger;

    public CoordinadorReservas(
        IBrokerMensajeria broker,
        GestorReservasLocales gestorLocal,
        ConfiguracionSede sede,
        IOptions<ConfiguracionRabbit> configuracionRabbit,
        ILogger<CoordinadorReservas> logger)
    {
        this.Broker = broker;
        this.GestorLocal = gestorLocal;
        this.EnCurso = new ListaEnlazada();
        this.TimeoutSegundos = configuracionRabbit.Value.TimeoutReservaSegundos;
        this.codigoSede = sede.GetCodigo();
        this.logger = logger;
    }

    // Reserva todas las líneas del pedido. Devuelve true solo si TODAS quedaron
    // reservadas. Si devuelve false, el motivo queda en pedido.GetMotivoRechazo()
    // y hay que llamar a LiberarPedido para soltar las que sí se reservaron.
    public async Task<bool> ReservarPedido(Pedido pedido)
    {
        LineaPedido[] lineas = ObtenerLineas(pedido);

        // 1. Contar cuántas líneas son remotas
        int remotas = 0;
        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].GetSedeProveedora() != codigoSede)
            {
                remotas++;
            }
        }

        // 2. Reservar primero lo local (es inmediato)
        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].GetSedeProveedora() != codigoSede)
            {
                continue;
            }

            string motivo;
            Reserva? reserva = GestorLocal.ReservarLocal(
                pedido.GetId(),
                lineas[i].GetProducto().GetCodigo(),
                lineas[i].GetCantidad(),
                codigoSede,
                out motivo);

            if (reserva == null)
            {
                pedido.SetMotivoRechazo(motivo);
                return false;
            }
            lineas[i].SetIdReserva(reserva.GetId());
        }

        if (remotas == 0)
        {
            return true;
        }

        // 3. Pedir lo remoto por RabbitMQ
        if (!Broker.EstaConectado)
        {
            pedido.SetMotivoRechazo("No hay conexión con las otras sedes (RabbitMQ).");
            return false;
        }

        LineaPedido[] lineasRemotas = new LineaPedido[remotas];
        int j = 0;
        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].GetSedeProveedora() != codigoSede)
            {
                lineasRemotas[j] = lineas[i];
                j++;
            }
        }

        SolicitudEnCurso solicitud = new SolicitudEnCurso(pedido.GetId(), lineasRemotas);
        lock (candadoEnCurso)
        {
            EnCurso.AgregarDato(solicitud);
        }

        try
        {
            for (int i = 0; i < solicitud.GetCantidadLineas(); i++)
            {
                LineaPedido linea = solicitud.GetLinea(i);
                string sedeDestino = linea.GetSedeProveedora()!;

                SolicitudReserva mensaje = new SolicitudReserva(
                    Guid.NewGuid().ToString(),
                    codigoSede,
                    sedeDestino,
                    solicitud.GetCorrelationId(i),
                    DateTime.Now,
                    pedido.GetId(),
                    linea.GetProducto().GetCodigo(),
                    linea.GetCantidad());

                try
                {
                    await Broker.PublicarAsync(mensaje, sedeDestino);
                }
                catch (Exception error)
                {
                    logger.LogError(error, "No se pudo enviar la solicitud de reserva a {Sede}", sedeDestino);
                    solicitud.MarcarFallo($"No se pudo contactar a la sede {sedeDestino}.");
                    break;
                }
            }

            // 4. Esperar todas las respuestas o el tiempo límite
            Task limite = Task.Delay(TimeSpan.FromSeconds(TimeoutSegundos));
            Task primera = await Task.WhenAny(solicitud.Tarea, limite);

            bool exito = primera == solicitud.Tarea && solicitud.Tarea.Result;
            if (!exito)
            {
                string motivo = solicitud.GetMotivoRechazo();
                if (motivo == "")
                {
                    motivo = "Una sede no respondió a tiempo.";
                }
                pedido.SetMotivoRechazo(motivo);
            }
            return exito;
        }
        finally
        {
            // A partir de aquí, cualquier respuesta que llegue es tardía
            solicitud.Cerrar();
            lock (candadoEnCurso)
            {
                EnCurso.EliminarDato(s => s == solicitud);
            }
        }
    }

    // Convierte todas las reservas del pedido en descuentos reales
    public async Task ConfirmarPedido(Pedido pedido)
    {
        LineaPedido[] lineas = ObtenerLineas(pedido);

        for (int i = 0; i < lineas.Length; i++)
        {
            string? idReserva = lineas[i].GetIdReserva();
            string? sede = lineas[i].GetSedeProveedora();
            if (idReserva == null || sede == null)
            {
                continue;
            }

            if (sede == codigoSede)
            {
                GestorLocal.Confirmar(idReserva);
                continue;
            }

            ConfirmarReserva mensaje = new ConfirmarReserva(
                Guid.NewGuid().ToString(), codigoSede, sede, pedido.GetId(), DateTime.Now, idReserva);
            try
            {
                await Broker.PublicarAsync(mensaje, sede);
            }
            catch (Exception error)
            {
                // Si no se pudo avisar, la reserva vencerá sola en la otra sede
                logger.LogError(error, "No se pudo confirmar la reserva {Id} en {Sede}", idReserva, sede);
            }
        }
    }

    // Suelta todas las reservas que se hayan hecho para el pedido
    public async Task LiberarPedido(Pedido pedido)
    {
        LineaPedido[] lineas = ObtenerLineas(pedido);

        for (int i = 0; i < lineas.Length; i++)
        {
            string? idReserva = lineas[i].GetIdReserva();
            string? sede = lineas[i].GetSedeProveedora();
            if (idReserva == null || sede == null)
            {
                continue;
            }

            if (sede == codigoSede)
            {
                GestorLocal.Liberar(idReserva);
            }
            else
            {
                await EnviarLiberacion(idReserva, sede, pedido.GetId());
            }
            lineas[i].SetIdReserva(null);
        }
    }

    // Lo llama el DespachadorMensajes cuando llega una RespuestaReserva
    public async Task RecibirRespuesta(RespuestaReserva respuesta)
    {
        SolicitudEnCurso? solicitud;
        lock (candadoEnCurso)
        {
            solicitud = (SolicitudEnCurso?)EnCurso.EncontrarDato(s =>
                ((SolicitudEnCurso)s).Contiene(respuesta.GetCorrelationId()));
        }

        bool registrada = solicitud != null && solicitud.RegistrarRespuesta(respuesta);

        // Respuesta tardía (ya se canceló el pedido por tiempo o por otro
        // rechazo): si la otra sede sí reservó, hay que avisarle que libere.
        if (!registrada && respuesta.GetAceptada())
        {
            logger.LogInformation("Respuesta tardía de {Sede}; se libera la reserva {Id}",
                respuesta.GetSedeOrigen(), respuesta.GetIdReserva());
            await EnviarLiberacion(respuesta.GetIdReserva(), respuesta.GetSedeOrigen(), respuesta.GetCorrelationId());
        }
    }

    private async Task EnviarLiberacion(string idReserva, string sede, string correlationId)
    {
        LiberarReserva mensaje = new LiberarReserva(
            Guid.NewGuid().ToString(), codigoSede, sede, correlationId, DateTime.Now, idReserva);
        try
        {
            await Broker.PublicarAsync(mensaje, sede);
        }
        catch (Exception error)
        {
            // Si no se pudo avisar, la reserva vencerá sola en la otra sede
            logger.LogError(error, "No se pudo liberar la reserva {Id} en {Sede}", idReserva, sede);
        }
    }

    private static LineaPedido[] ObtenerLineas(Pedido pedido)
    {
        ListaEnlazada lista = pedido.GetLineas();
        LineaPedido[] lineas = new LineaPedido[lista.GetSize()];
        int i = 0;
        lista.Recorrer(dato =>
        {
            lineas[i] = (LineaPedido)dato;
            i++;
        });
        return lineas;
    }
}
