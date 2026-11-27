using System.Text.Json;
using Backend.Mensajes;
using Backend.Models;
using Backend.Pedidos;
using Backend.Persistencia;
using Backend.Servicios;
using Microsoft.Extensions.Options;

// Lado "proveedor" de las reservas: administra las reservas que se hacen sobre
// el inventario de ESTA sede, tanto las que piden otras sedes por RabbitMQ como
// las de los pedidos registrados aquí mismo.
//
// La sede dueña del producto es la única que decide sobre su stock: si dos sedes
// piden lo mismo a la vez, las solicitudes se atienden una por una (candado) y
// la que ya no alcanza se rechaza.
//
// Las reservas activas se guardan en reservas.json para que, si la sede se
// reinicia, se puedan seguir confirmando, liberando o venciendo.
public class GestorReservasLocales
{
    private readonly InventarioService inventario;
    private readonly ListaEnlazada ReservasActivas;
    private readonly object Candado = new object();
    private readonly string codigoSede;
    private readonly string rutaArchivo;
    private readonly int minutosExpiracion;
    private readonly ILogger<GestorReservasLocales> logger;

    public GestorReservasLocales(
        InventarioService inventario,
        ConfiguracionSede sede,
        IOptions<ConfiguracionRabbit> configuracionRabbit,
        IWebHostEnvironment entorno,
        ILogger<GestorReservasLocales> logger)
    {
        this.inventario = inventario;
        this.codigoSede = sede.GetCodigo();
        this.minutosExpiracion = configuracionRabbit.Value.MinutosExpiracionReserva;
        this.logger = logger;
        this.ReservasActivas = new ListaEnlazada();
        this.rutaArchivo = Path.Combine(entorno.ContentRootPath, sede.GetRutaDatos(), "reservas.json");

        CargarDesdeArchivo();
    }

    // Atiende una SolicitudReserva que llegó de otra sede
    public RespuestaReserva Reservar(SolicitudReserva solicitud)
    {
        string motivo;
        Reserva? reserva = ReservarLocal(
            solicitud.GetIdPedido(),
            solicitud.GetCodigoProducto(),
            solicitud.GetCantidad(),
            solicitud.GetSedeOrigen(),
            out motivo);

        return new RespuestaReserva(
            Guid.NewGuid().ToString(),
            codigoSede,
            solicitud.GetSedeOrigen(),
            solicitud.GetCorrelationId(),
            DateTime.Now,
            reserva != null,
            reserva != null ? reserva.GetId() : "",
            motivo);
    }

    // Reserva unidades del inventario local. Devuelve null si no se pudo,
    // y en "motivo" la explicación.
    public Reserva? ReservarLocal(string idPedido, string codigoProducto, int cantidad, string sedeSolicitante, out string motivo)
    {
        lock (Candado)
        {
            // Antes de reservar, soltar lo que ya venció para no bloquear stock
            LiberarVencidasSinCandado();

            bool existe;
            try
            {
                existe = inventario.ReservarProducto(codigoProducto, cantidad);
            }
            catch (InvalidOperationException)
            {
                motivo = $"La sede {codigoSede} no tiene suficientes unidades de {codigoProducto}.";
                return null;
            }
            catch (ArgumentException error)
            {
                motivo = error.Message;
                return null;
            }

            if (!existe)
            {
                motivo = $"La sede {codigoSede} no maneja el producto {codigoProducto}.";
                return null;
            }

            Reserva reserva = new Reserva(idPedido, codigoProducto, cantidad, sedeSolicitante, minutosExpiracion);
            ReservasActivas.AgregarDato(reserva);
            Guardar();

            logger.LogInformation("Reserva {Id}: {Cantidad} x {Producto} para el pedido {Pedido} de {Sede}",
                reserva.GetId(), cantidad, codigoProducto, idPedido, sedeSolicitante);

            motivo = "Reserva aceptada.";
            return reserva;
        }
    }

    // Convierte la reserva en un descuento real del inventario
    public bool Confirmar(string idReserva)
    {
        lock (Candado)
        {
            Reserva? reserva = BuscarActiva(idReserva);
            if (reserva == null)
            {
                logger.LogWarning("No se puede confirmar la reserva {Id}: no existe o ya no está activa", idReserva);
                return false;
            }

            try
            {
                inventario.ConfirmarReserva(reserva.GetCodigoProducto(), reserva.GetCantidad());
            }
            catch (Exception error)
            {
                logger.LogError(error, "Error confirmando la reserva {Id} en el inventario", idReserva);
                return false;
            }

            reserva.Confirmar();
            QuitarDeActivas(idReserva);
            Guardar();

            logger.LogInformation("Reserva {Id} confirmada", idReserva);
            return true;
        }
    }

    // Devuelve las unidades reservadas al inventario disponible
    public bool Liberar(string idReserva)
    {
        lock (Candado)
        {
            Reserva? reserva = BuscarActiva(idReserva);
            if (reserva == null)
            {
                return false;
            }

            DevolverAlInventario(reserva);
            reserva.Liberar();
            QuitarDeActivas(idReserva);
            Guardar();

            logger.LogInformation("Reserva {Id} liberada", idReserva);
            return true;
        }
    }

    // Libera las reservas cuyo tiempo venció sin ser confirmadas
    public int LiberarVencidas()
    {
        lock (Candado)
        {
            return LiberarVencidasSinCandado();
        }
    }

    public int CantidadActivas()
    {
        lock (Candado)
        {
            return ReservasActivas.GetSize();
        }
    }

    private int LiberarVencidasSinCandado()
    {
        int liberadas = 0;

        // Se buscan de una en una porque EliminarDato quita la primera que cumple
        object? dato = ReservasActivas.EncontrarDato(r => ((Reserva)r).EstaVencida());
        while (dato != null)
        {
            Reserva reserva = (Reserva)dato;
            DevolverAlInventario(reserva);
            reserva.MarcarVencida();
            QuitarDeActivas(reserva.GetId());
            liberadas++;

            logger.LogInformation("Reserva {Id} vencida y liberada", reserva.GetId());
            dato = ReservasActivas.EncontrarDato(r => ((Reserva)r).EstaVencida());
        }

        if (liberadas > 0)
        {
            Guardar();
        }
        return liberadas;
    }

    private void DevolverAlInventario(Reserva reserva)
    {
        try
        {
            inventario.LiberarReserva(reserva.GetCodigoProducto(), reserva.GetCantidad());
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error liberando la reserva {Id} en el inventario", reserva.GetId());
        }
    }

    private Reserva? BuscarActiva(string idReserva)
    {
        object? dato = ReservasActivas.EncontrarDato(r =>
            ((Reserva)r).GetId() == idReserva && ((Reserva)r).EstaActiva());
        return (Reserva?)dato;
    }

    private void QuitarDeActivas(string idReserva)
    {
        ReservasActivas.EliminarDato(r => ((Reserva)r).GetId() == idReserva);
    }

    // ── Persistencia ──

    private void Guardar()
    {
        RegistroReservaJson[] registros = new RegistroReservaJson[ReservasActivas.GetSize()];
        int i = 0;
        ReservasActivas.Recorrer(dato =>
        {
            Reserva r = (Reserva)dato;
            registros[i] = new RegistroReservaJson
            {
                Id = r.GetId(),
                IdPedido = r.GetIdPedido(),
                CodigoProducto = r.GetCodigoProducto(),
                Cantidad = r.GetCantidad(),
                SedeSolicitante = r.GetSedeSolicitante(),
                FechaExpiracion = r.GetFechaExpiracion(),
                Estado = r.GetEstado().ToString()
            };
            i++;
        });

        string json = JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true });

        string? carpeta = Path.GetDirectoryName(rutaArchivo);
        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        string temporal = rutaArchivo + ".tmp";
        File.WriteAllText(temporal, json);
        File.Move(temporal, rutaArchivo, true);
    }

    private void CargarDesdeArchivo()
    {
        if (!File.Exists(rutaArchivo))
        {
            return;
        }

        RegistroReservaJson[]? registros;
        try
        {
            registros = JsonSerializer.Deserialize<RegistroReservaJson[]>(File.ReadAllText(rutaArchivo));
        }
        catch (JsonException error)
        {
            logger.LogError(error, "El archivo de reservas no es válido: {Ruta}", rutaArchivo);
            return;
        }

        if (registros == null)
        {
            return;
        }

        for (int i = 0; i < registros.Length; i++)
        {
            RegistroReservaJson r = registros[i];
            EstadoReserva estado;
            if (!Enum.TryParse(r.Estado, out estado) || estado != EstadoReserva.Activa)
            {
                continue;
            }

            ReservasActivas.AgregarDato(new Reserva(
                r.Id, r.IdPedido, r.CodigoProducto, r.Cantidad,
                r.SedeSolicitante, r.FechaExpiracion, estado));
        }

        logger.LogInformation("Reservas activas recuperadas: {Cantidad}", ReservasActivas.GetSize());
    }
}
