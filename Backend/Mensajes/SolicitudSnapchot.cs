namespace Backend.Mensajes;

public class SolicitudSnapshot : Mensaje
{
    public SolicitudSnapshot(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha)
        : base(id, "SolicitudSnapshot", sedeOrigen, sedeDestino, correlationId, fecha)
    {
    }
}
