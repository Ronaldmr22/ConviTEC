using System.Text;
using System.Text.Json;
using Backend.Mensajes;

// Convierte mensajes a JSON (bytes) para enviarlos por RabbitMQ y de vuelta.
// El campo "Tipo" de cada mensaje indica qué clase hay que reconstruir.
public static class SerializadorMensajes
{
    public static byte[] Serializar(Mensaje mensaje)
    {
        if (mensaje == null)
        {
            throw new ArgumentNullException(nameof(mensaje));
        }

        // GetType() para que se incluyan los campos de la clase hija
        string json = JsonSerializer.Serialize(mensaje, mensaje.GetType());
        return Encoding.UTF8.GetBytes(json);
    }

    public static Mensaje Deserializar(byte[] cuerpo)
    {
        string json = Encoding.UTF8.GetString(cuerpo);

        string? tipo;
        using (JsonDocument documento = JsonDocument.Parse(json))
        {
            if (!documento.RootElement.TryGetProperty("Tipo", out JsonElement elementoTipo))
            {
                throw new InvalidDataException("El mensaje no indica su tipo.");
            }
            tipo = elementoTipo.GetString();
        }

        Type clase = ObtenerClase(tipo);

        object? resultado = JsonSerializer.Deserialize(json, clase);
        if (resultado == null)
        {
            throw new InvalidDataException("El mensaje está vacío.");
        }

        return (Mensaje)resultado;
    }

    private static Type ObtenerClase(string? tipo)
    {
        switch (tipo)
        {
            case "SolicitudReserva": return typeof(SolicitudReserva);
            case "RespuestaReserva": return typeof(RespuestaReserva);
            case "ConfirmarReserva": return typeof(ConfirmarReserva);
            case "LiberarReserva": return typeof(LiberarReserva);
            case "ActualizacionInventario": return typeof(ActualizacionInventario);
            case "SolicitudSnapshot": return typeof(SolicitudSnapshot);
            case "SnapshotInventario": return typeof(SnapshotInventario);
            default:
                throw new InvalidDataException("Tipo de mensaje desconocido: " + tipo);
        }
    }
}
