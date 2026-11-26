namespace Backend.Mensajes;

public class LiberarReserva : Mensaje
{
    private string idReserva;

    public LiberarReserva(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha, string idReserva)
        : base(id, "LiberarReserva", sedeOrigen, sedeDestino, correlationId, fecha)
    {
        if (string.IsNullOrWhiteSpace(idReserva))
        {
            throw new ArgumentException("El identificador de la reserva es obligatorio.");
        }

        this.idReserva = idReserva;
    }

    public string GetIdReserva()
    {
        return idReserva;
    }
}
