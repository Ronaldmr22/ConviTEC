using System.Text.Json.Serialization;
namespace Backend.Mensajes;

public class RespuestaReserva : Mensaje
{
    private bool aceptada;
    private string idReserva;
    private string motivo;
    public bool Aceptada => aceptada;
    public string IdReserva => idReserva;
    public string Motivo => motivo;

    [JsonConstructor]
    public RespuestaReserva(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha, bool aceptada, string idReserva, string motivo)
        : base(id, "RespuestaReserva", sedeOrigen, sedeDestino, correlationId, fecha)
    {
        if (aceptada && string.IsNullOrWhiteSpace(idReserva))
        {
            throw new ArgumentException("Una reserva aceptada debe tener un identificador.");
        }

        this.aceptada = aceptada;
        this.idReserva = idReserva;
        this.motivo = motivo;
    }

    public bool GetAceptada()
    {
        return aceptada;
    }

    public string GetIdReserva()
    {
        return idReserva;
    }

    public string GetMotivo()
    {
        return motivo;
    }
}