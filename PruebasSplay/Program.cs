using Backend.Mensajes;
using System.Text.Json;
using Backend.Models;

Console.WriteLine("=== PRUEBA JSON MENSAJES ===");

SolicitudReserva original = new SolicitudReserva(
    "MSG-001",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "PED-001",
    "MON-001",
    5
);

// Convertir a JSON
string json = JsonSerializer.Serialize(
    original,
    new JsonSerializerOptions
    {
        WriteIndented = true
    }
);

Console.WriteLine("\nJSON generado:");
Console.WriteLine(json);

// Recuperar desde JSON
SolicitudReserva? recuperada =
    JsonSerializer.Deserialize<SolicitudReserva>(json);

Console.WriteLine("\n=== RESULTADO ===");

Console.WriteLine("Tipo: " + recuperada?.GetTipo());
Console.WriteLine("Origen: " + recuperada?.GetSedeOrigen());
Console.WriteLine("Destino: " + recuperada?.GetSedeDestino());
Console.WriteLine("Producto: " + recuperada?.GetCodigoProducto());
Console.WriteLine("Cantidad: " + recuperada?.GetCantidad());

bool correcta =
    recuperada != null &&
    recuperada.GetTipo() == "SolicitudReserva" &&
    recuperada.GetSedeOrigen() == "LIB" &&
    recuperada.GetSedeDestino() == "ALA" &&
    recuperada.GetCodigoProducto() == "MON-001" &&
    recuperada.GetCantidad() == 5;

Console.WriteLine("\nPrueba correcta: " + correcta);


Console.WriteLine("\n=== PRUEBA JSON RESPUESTA RESERVA ===");

RespuestaReserva respuestaOriginal = new RespuestaReserva(
    "MSG-002",
    "ALA",
    "LIB",
    "SOL-001",
    DateTime.UtcNow,
    true,
    "RES-001",
    ""
);

string jsonRespuesta = JsonSerializer.Serialize(
    respuestaOriginal,
    new JsonSerializerOptions
    {
        WriteIndented = true
    }
);

Console.WriteLine(jsonRespuesta);

RespuestaReserva? respuestaRecuperada =
    JsonSerializer.Deserialize<RespuestaReserva>(jsonRespuesta);

bool respuestaCorrecta =
    respuestaRecuperada != null &&
    respuestaRecuperada.GetTipo() == "RespuestaReserva" &&
    respuestaRecuperada.GetSedeOrigen() == "ALA" &&
    respuestaRecuperada.GetSedeDestino() == "LIB" &&
    respuestaRecuperada.GetCorrelationId() == "SOL-001" &&
    respuestaRecuperada.GetAceptada() &&
    respuestaRecuperada.GetIdReserva() == "RES-001";

Console.WriteLine(
    "\nPrueba respuesta correcta: " + respuestaCorrecta
);


Console.WriteLine("\n=== PRUEBA JSON CONFIRMAR RESERVA ===");

ConfirmarReserva confirmacionOriginal = new ConfirmarReserva(
    "MSG-003",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "RES-001"
);

string jsonConfirmacion =
    JsonSerializer.Serialize(confirmacionOriginal);

ConfirmarReserva? confirmacionRecuperada =
    JsonSerializer.Deserialize<ConfirmarReserva>(jsonConfirmacion);

bool confirmacionCorrecta =
    confirmacionRecuperada != null &&
    confirmacionRecuperada.GetTipo() == "ConfirmarReserva" &&
    confirmacionRecuperada.GetSedeOrigen() == "LIB" &&
    confirmacionRecuperada.GetSedeDestino() == "ALA" &&
    confirmacionRecuperada.GetCorrelationId() == "SOL-001" &&
    confirmacionRecuperada.GetIdReserva() == "RES-001";

Console.WriteLine("JSON: " + jsonConfirmacion);
Console.WriteLine(
    "Prueba confirmacion correcta: " + confirmacionCorrecta
);


Console.WriteLine("\n=== PRUEBA JSON LIBERAR RESERVA ===");

LiberarReserva liberacionOriginal = new LiberarReserva(
    "MSG-004",
    "LIB",
    "ALA",
    "SOL-001",
    DateTime.UtcNow,
    "RES-002"
);

string jsonLiberacion =
    JsonSerializer.Serialize(liberacionOriginal);

LiberarReserva? liberacionRecuperada =
    JsonSerializer.Deserialize<LiberarReserva>(jsonLiberacion);

bool liberacionCorrecta =
    liberacionRecuperada != null &&
    liberacionRecuperada.GetTipo() == "LiberarReserva" &&
    liberacionRecuperada.GetSedeOrigen() == "LIB" &&
    liberacionRecuperada.GetSedeDestino() == "ALA" &&
    liberacionRecuperada.GetCorrelationId() == "SOL-001" &&
    liberacionRecuperada.GetIdReserva() == "RES-002";

Console.WriteLine("JSON: " + jsonLiberacion);
Console.WriteLine(
    "Prueba liberacion correcta: " + liberacionCorrecta
);


Console.WriteLine("\n=== PRUEBA JSON ACTUALIZACION INVENTARIO ===");

ActualizacionInventario actualizacionOriginal =
    new ActualizacionInventario(
        "MSG-005",
        "LIB",
        "ALA",
        "SYNC-001",
        DateTime.UtcNow,
        "MON-001",
        15
    );

string jsonActualizacion =
    JsonSerializer.Serialize(actualizacionOriginal);

Console.WriteLine("JSON: " + jsonActualizacion);

ActualizacionInventario? actualizacionRecuperada =
    JsonSerializer.Deserialize<ActualizacionInventario>(
        jsonActualizacion
    );

bool actualizacionCorrecta =
    actualizacionRecuperada != null &&
    actualizacionRecuperada.GetTipo() == "ActualizacionInventario" &&
    actualizacionRecuperada.GetSedeOrigen() == "LIB" &&
    actualizacionRecuperada.GetSedeDestino() == "ALA" &&
    actualizacionRecuperada.GetCodigoProducto() == "MON-001" &&
    actualizacionRecuperada.GetDisponible() == 15;

Console.WriteLine(
    "Prueba actualizacion correcta: " + actualizacionCorrecta
);


Console.WriteLine("\n=== PRUEBA JSON SNAPSHOT INVENTARIO ===");

RegistroExistenciaGlobal[] existencias =
{
    new RegistroExistenciaGlobal(
        "MON-001", "LIB", 25, 5, 3
    ),
    new RegistroExistenciaGlobal(
        "MOU-001", "LIB", 20, 0, 2
    ),
    new RegistroExistenciaGlobal(
        "TEC-001", "LIB", 10, 2, 4
    )
};

SnapshotInventario snapshotOriginal =
    new SnapshotInventario(
        "MSG-007",
        "LIB",
        "LIM",
        "SYNC-002",
        DateTime.UtcNow,
        existencias
    );

string jsonSnapshot = JsonSerializer.Serialize(
    snapshotOriginal,
    new JsonSerializerOptions
    {
        WriteIndented = true
    }
);

Console.WriteLine(jsonSnapshot);

SnapshotInventario? snapshotRecuperado =
    JsonSerializer.Deserialize<SnapshotInventario>(
        jsonSnapshot
    );

bool snapshotCorrecto =
    snapshotRecuperado != null &&
    snapshotRecuperado.GetTipo() == "SnapshotInventario" &&
    snapshotRecuperado.GetExistencias().Length == 3 &&
    snapshotRecuperado.GetExistencias()[0].GetCodigoProducto() == "MON-001" &&
    snapshotRecuperado.GetExistencias()[0].GetCantidadDisponible() == 20 &&
    snapshotRecuperado.GetExistencias()[0].GetVersion() == 3;

Console.WriteLine(
    "\nPrueba snapshot correcta: " + snapshotCorrecto
);
