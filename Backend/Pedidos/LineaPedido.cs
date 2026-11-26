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
    public string GetSedeProveedora() {return SedeProveedora;}
    public string GetIdReserva() {return IdReserva;}
    public void SetSedeProveedora(string nuevo) {this.SedeProveedora = nuevo;}

    public LineaPedido(Producto producto, int cantidad, int precioUnitario)
    {
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