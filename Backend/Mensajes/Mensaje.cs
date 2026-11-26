
namespace Backend.Mensajes;

public abstract class Mensaje
{
    private string id;
    private string tipo;
    private string sedeOrigen;
    private string sedeDestino;
    private string correlationId;
    private DateTime fecha;

    protected Mensaje(string id, string tipo, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(tipo) ||
            string.IsNullOrWhiteSpace(sedeOrigen) ||
            string.IsNullOrWhiteSpace(sedeDestino) ||
            string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Los datos del mensaje son obligatorios.");
        }

        this.id = id;
        this.tipo = tipo;
        this.sedeOrigen = sedeOrigen;
        this.sedeDestino = sedeDestino;
        this.correlationId = correlationId;
        this.fecha = fecha;
    }

    public string GetId()
    {
        return id;
    }

    public string GetTipo()
    {
        return tipo;
    }

    public string GetSedeOrigen()
    {
        return sedeOrigen;
    }

    public string GetSedeDestino()
    {
        return sedeDestino;
    }

    public string GetCorrelationId()
    {
        return correlationId;
    }

    public DateTime GetFecha()
    {
        return fecha;
    }
}
