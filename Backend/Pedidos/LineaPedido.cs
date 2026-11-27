using Backend.Models;

namespace Backend.Pedidos{
public class LineaPedido
{
    private Producto Producto;
    private int Cantidad;
    private decimal PrecioUnitario;
    private string? SedeProveedora;
    private string? IdReserva;

    public Producto GetProducto() {return Producto;}
    public int GetCantidad() {return Cantidad;}
    public decimal GetPrecioUnitario() {return PrecioUnitario;}
    public string? GetSedeProveedora() {return SedeProveedora;}
    public string? GetIdReserva() {return IdReserva;}
    public void SetSedeProveedora(string nuevo) {this.SedeProveedora = nuevo;}
    public void SetIdReserva(string? idReserva) {this.IdReserva = idReserva;}

    public LineaPedido(Producto producto, int cantidad, decimal precioUnitario)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto));
        }
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        this.Producto = producto;
        this.Cantidad = cantidad;
        this.PrecioUnitario = precioUnitario;
        SedeProveedora = null;
        this.IdReserva = null;
    }

    public decimal Subtotal()
    {
        return Cantidad * PrecioUnitario;
    }
}
}
