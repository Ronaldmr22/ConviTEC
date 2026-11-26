using System.Text.Json.Serialization;

namespace Backend.Mensajes;

public class SolicitudSnapshot : Mensaje
{
    [JsonConstructor]
    public SolicitudSnapshot(
        string id,
        string sedeOrigen,
        string sedeDestino,
        string correlationId,
        DateTime fecha)
        : base(
            id,
            "SolicitudSnapshot",
            sedeOrigen,
            sedeDestino,
            correlationId,
            fecha)
    {
    }
}
