using Backend.Mensajes;
using Backend.Models;
using Backend.Servicios;

// Mantiene sincronizada la copia del inventario global entre las sedes.
//
//  - Publicar: InventarioService (Persona 1) ya actualiza el registro de esta
//    sede en el inventario global, subiendo su "version" cada vez que cambia el
//    stock. Este servicio revisa periódicamente esos registros y difunde a todas
//    las sedes los que tengan una versión nueva, dentro de un SnapshotInventario.
//  - Recibir: cuando llega un SnapshotInventario de otra sede, se aplica cada
//    registro con InventarioGlobalService. Este ignora versiones viejas o
//    repetidas, así que no importa si un mensaje llega dos veces o desordenado.
//  - Arranque: se difunde el inventario completo de esta sede y se pide a las
//    demás el suyo (SolicitudSnapshot), por si cambiaron mientras estaba apagada.
public class ServicioSincronizacion
{
    public const string TodasLasSedes = "TODAS";

    private readonly IBrokerMensajeria broker;
    private readonly InventarioGlobalService inventarioGlobal;
    private readonly string codigoSede;
    private readonly ILogger<ServicioSincronizacion> logger;

    // Última versión publicada de cada producto de esta sede.
    // Arreglos paralelos: codigosPublicados[i] tiene la versión versionesPublicadas[i].
    private string[] codigosPublicados = new string[8];
    private int[] versionesPublicadas = new int[8];
    private int cantidadPublicados = 0;
    private readonly object candado = new object();

    public ServicioSincronizacion(
        IBrokerMensajeria broker,
        InventarioGlobalService inventarioGlobal,
        ConfiguracionSede sede,
        ILogger<ServicioSincronizacion> logger)
    {
        this.broker = broker;
        this.inventarioGlobal = inventarioGlobal;
        this.codigoSede = sede.GetCodigo();
        this.logger = logger;
    }

    // Se llama al conectar (y al reconectar) con RabbitMQ
    public async Task IniciarAsync()
    {
        lock (candado)
        {
            cantidadPublicados = 0;   // volver a publicar todo
        }
        await PublicarCambiosAsync();

        SolicitudSnapshot solicitud = new SolicitudSnapshot(
            Guid.NewGuid().ToString(), codigoSede, TodasLasSedes, Guid.NewGuid().ToString(), DateTime.Now);
        await broker.DifundirAsync(solicitud);

        logger.LogInformation("Inventario propio difundido y snapshot solicitado a las demás sedes");
    }

    // Difunde los registros de esta sede cuya versión cambió desde la última vez
    public async Task PublicarCambiosAsync()
    {
        if (!broker.EstaConectado)
        {
            return;
        }

        RegistroExistenciaGlobal[] cambios;
        lock (candado)
        {
            cambios = ObtenerCambiosSinPublicar();
        }

        if (cambios.Length == 0)
        {
            return;
        }

        SnapshotInventario mensaje = new SnapshotInventario(
            Guid.NewGuid().ToString(), codigoSede, TodasLasSedes, Guid.NewGuid().ToString(), DateTime.Now, cambios);
        await broker.DifundirAsync(mensaje);

        // Solo se marcan como publicados si el envío no falló
        lock (candado)
        {
            for (int i = 0; i < cambios.Length; i++)
            {
                MarcarPublicado(cambios[i].GetCodigoProducto(), cambios[i].GetVersion());
            }
        }

        logger.LogInformation("Difundidos {Cantidad} cambios de inventario", cambios.Length);
    }

    // Aplica en la copia global los registros que mandó otra sede
    public void AplicarSnapshot(SnapshotInventario snapshot)
    {
        RegistroExistenciaGlobal[] registros = snapshot.GetExistencias();
        int aplicados = 0;

        for (int i = 0; i < registros.Length; i++)
        {
            RegistroExistenciaGlobal r = registros[i];

            // Esta sede es la dueña de sus propios registros: nunca los toma de afuera
            if (r.GetCodigoSede() == codigoSede)
            {
                continue;
            }

            bool actualizado = inventarioGlobal.ActualizarExistencias(
                r.GetCodigoProducto(), r.GetCodigoSede(), r.GetCantidad(), r.GetCantidadReservada(), r.GetVersion());
            if (actualizado)
            {
                aplicados++;
            }
        }

        if (aplicados > 0)
        {
            logger.LogInformation("Inventario global: {Cantidad} registros actualizados desde {Sede}", aplicados, snapshot.GetSedeOrigen());
        }
    }

    // Otra sede pidió nuestro inventario: se le manda directo a ella
    public async Task ResponderSnapshotAsync(SolicitudSnapshot solicitud)
    {
        RegistroExistenciaGlobal[] propios = ObtenerRegistrosPropios();

        SnapshotInventario respuesta = new SnapshotInventario(
            Guid.NewGuid().ToString(), codigoSede, solicitud.GetSedeOrigen(), solicitud.GetCorrelationId(), DateTime.Now, propios);
        await broker.PublicarAsync(respuesta, solicitud.GetSedeOrigen());
    }

    private RegistroExistenciaGlobal[] ObtenerRegistrosPropios()
    {
        RegistroExistenciaGlobal[] todos = inventarioGlobal.ObtenerRegistros();

        int cantidad = 0;
        for (int i = 0; i < todos.Length; i++)
        {
            if (todos[i].GetCodigoSede() == codigoSede) { cantidad++; }
        }

        RegistroExistenciaGlobal[] propios = new RegistroExistenciaGlobal[cantidad];
        int j = 0;
        for (int i = 0; i < todos.Length; i++)
        {
            if (todos[i].GetCodigoSede() == codigoSede)
            {
                propios[j] = todos[i];
                j++;
            }
        }
        return propios;
    }

    private RegistroExistenciaGlobal[] ObtenerCambiosSinPublicar()
    {
        RegistroExistenciaGlobal[] propios = ObtenerRegistrosPropios();

        int cantidad = 0;
        for (int i = 0; i < propios.Length; i++)
        {
            if (propios[i].GetVersion() > VersionPublicada(propios[i].GetCodigoProducto())) { cantidad++; }
        }

        RegistroExistenciaGlobal[] cambios = new RegistroExistenciaGlobal[cantidad];
        int j = 0;
        for (int i = 0; i < propios.Length; i++)
        {
            if (propios[i].GetVersion() > VersionPublicada(propios[i].GetCodigoProducto()))
            {
                cambios[j] = propios[i];
                j++;
            }
        }
        return cambios;
    }

    private int VersionPublicada(string codigoProducto)
    {
        for (int i = 0; i < cantidadPublicados; i++)
        {
            if (codigosPublicados[i] == codigoProducto)
            {
                return versionesPublicadas[i];
            }
        }
        return 0;
    }

    private void MarcarPublicado(string codigoProducto, int version)
    {
        for (int i = 0; i < cantidadPublicados; i++)
        {
            if (codigosPublicados[i] == codigoProducto)
            {
                if (version > versionesPublicadas[i])
                {
                    versionesPublicadas[i] = version;
                }
                return;
            }
        }

        if (cantidadPublicados == codigosPublicados.Length)
        {
            string[] nuevosCodigos = new string[codigosPublicados.Length * 2];
            int[] nuevasVersiones = new int[versionesPublicadas.Length * 2];
            for (int i = 0; i < cantidadPublicados; i++)
            {
                nuevosCodigos[i] = codigosPublicados[i];
                nuevasVersiones[i] = versionesPublicadas[i];
            }
            codigosPublicados = nuevosCodigos;
            versionesPublicadas = nuevasVersiones;
        }

        codigosPublicados[cantidadPublicados] = codigoProducto;
        versionesPublicadas[cantidadPublicados] = version;
        cantidadPublicados++;
    }
}
