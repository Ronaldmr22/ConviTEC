using Backend.Models;
using Backend.Services;
using Backend.Estructuras.Arboles;
using Backend.Persistencia;
using Backend.Mensajes;

Console.WriteLine("\n=== PRUEBA SOLICITUD DE RESERVA ===");

SolicitudReserva solicitud = new SolicitudReserva(
    "MSG-001",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "PED-001",
    "MON-001",
    5
);

Console.WriteLine("Mensaje: " + solicitud.GetId());
Console.WriteLine("Tipo: " + solicitud.GetTipo());
Console.WriteLine("Origen: " + solicitud.GetSedeOrigen());
Console.WriteLine("Destino: " + solicitud.GetSedeDestino());
Console.WriteLine("Pedido: " + solicitud.GetIdPedido());
Console.WriteLine("Producto: " + solicitud.GetCodigoProducto());
Console.WriteLine("Cantidad: " + solicitud.GetCantidad());
Console.WriteLine("Correlacion: " + solicitud.GetCorrelationId());


Console.WriteLine("\n=== PRUEBA RESPUESTA RESERVA ===");

RespuestaReserva respuesta = new RespuestaReserva(
    "MSG-002",
    "ALA",
    "LIB",
    "SOL-001",
    DateTime.UtcNow,
    true,
    "RES-001",
    ""
);

Console.WriteLine("Mensaje: " + respuesta.GetId());
Console.WriteLine("Tipo: " + respuesta.GetTipo());
Console.WriteLine("Origen: " + respuesta.GetSedeOrigen());
Console.WriteLine("Destino: " + respuesta.GetSedeDestino());
Console.WriteLine("Correlacion: " + respuesta.GetCorrelationId());
Console.WriteLine("Aceptada: " + respuesta.GetAceptada());
Console.WriteLine("Reserva: " + respuesta.GetIdReserva());
Console.WriteLine("Motivo: " + respuesta.GetMotivo());


Console.WriteLine("\n=== PRUEBA CONFIRMAR RESERVA ===");

ConfirmarReserva confirmacion = new ConfirmarReserva(
    "MSG-003",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "RES-001"
);

Console.WriteLine("Tipo: " + confirmacion.GetTipo());
Console.WriteLine("Origen: " + confirmacion.GetSedeOrigen());
Console.WriteLine("Destino: " + confirmacion.GetSedeDestino());
Console.WriteLine("Reserva: " + confirmacion.GetIdReserva());

Console.WriteLine("\n=== PRUEBA LIBERAR RESERVA ===");

LiberarReserva liberacion = new LiberarReserva(
    "MSG-004",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "RES-001"
);

Console.WriteLine("Tipo: " + liberacion.GetTipo());
Console.WriteLine("Origen: " + liberacion.GetSedeOrigen());
Console.WriteLine("Destino: " + liberacion.GetSedeDestino());
Console.WriteLine("Reserva: " + liberacion.GetIdReserva());


Console.WriteLine(
    "\n=== PRUEBA ACTUALIZACION INVENTARIO ==="
);

ActualizacionInventario actualizacion =
    new ActualizacionInventario(
        "MSG-005",
        "LIB",
        "ALA",
        "SYNC-001",
        DateTime.UtcNow,
        "MON-001",
        15
    );

Console.WriteLine("Mensaje: " + actualizacion.GetId());
Console.WriteLine("Tipo: " + actualizacion.GetTipo());
Console.WriteLine("Origen: " + actualizacion.GetSedeOrigen());
Console.WriteLine("Destino: " + actualizacion.GetSedeDestino());
Console.WriteLine("Producto: " + actualizacion.GetCodigoProducto());
Console.WriteLine("Disponible: " + actualizacion.GetDisponible());


Console.WriteLine("\n=== PRUEBA SOLICITUD SNAPSHOT ===");

SolicitudSnapshot solicitudSnapshot =
    new SolicitudSnapshot(
        "MSG-006",
        "LIM",
        "LIB",
        "SYNC-002",
        DateTime.UtcNow
    );

Console.WriteLine("Mensaje: " + solicitudSnapshot.GetId());
Console.WriteLine("Tipo: " + solicitudSnapshot.GetTipo());
Console.WriteLine("Origen: " + solicitudSnapshot.GetSedeOrigen());
Console.WriteLine("Destino: " + solicitudSnapshot.GetSedeDestino());
Console.WriteLine("Correlacion: " + solicitudSnapshot.GetCorrelationId());
