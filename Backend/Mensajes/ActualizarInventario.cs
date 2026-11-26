namespace Backend.Mensajes;

public class ActualizacionInventario : Mensaje
{
    private string codigoProducto;
    private int disponible;

    public ActualizacionInventario(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha, string codigoProducto, int disponible)
        : base(id, "ActualizacionInventario", sedeOrigen, sedeDestino, correlationId, fecha)
    {
        if (string.IsNullOrWhiteSpace(codigoProducto))
        {
            throw new ArgumentException("El codigo del producto es obligatorio.");
        }

        if (disponible < 0)
        {
            throw new ArgumentException("La cantidad disponible no puede ser negativa.");
        }

        this.codigoProducto = codigoProducto;
        this.disponible = disponible;
    }

    public string GetCodigoProducto()
    {
        return codigoProducto;
    }

    public int GetDisponible()
    {
        return disponible;
    }
}
