
namespace Backend.Mensajes;

public class SolicitudReserva : Mensaje
{
    private string idPedido;
    private string codigoProducto;
    private int cantidad;

    public SolicitudReserva(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha, string idPedido, string codigoProducto, int cantidad)
    : base(id, "SolicitudReserva", sedeOrigen, sedeDestino, correlationId, fecha)
    {
        if (string.IsNullOrWhiteSpace(idPedido))
        {
            throw new ArgumentException("El identificador del pedido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(codigoProducto))
        {
            throw new ArgumentException("El codigo del producto es obligatorio.");
        }

        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        this.idPedido = idPedido;
        this.codigoProducto = codigoProducto;
        this.cantidad = cantidad;
    }

    public string GetIdPedido()
    {
        return idPedido;
    }

    public string GetCodigoProducto()
    {
        return codigoProducto;
    }

    public int GetCantidad()
    {
        return cantidad;
    }
}