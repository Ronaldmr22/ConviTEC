using Backend.Models;

namespace Backend.Pedidos{
public class LineaPedido
{
    private Producto Producto;
    private int Cantidad;
    private decimal PrecioUnitario;
    private string SedeProveedora;
    private string IdReserva;

    public Producto GetProducto() {return Producto;}
    public int GetCantidad() {return Cantidad;}
    public decimal GetPrecioUnitario() {return PrecioUnitario;}
    public string GetSedeProveedora() {return SedeProveedora;}
    public string GetIdReserva() {return IdReserva;}

    public LineaPedido(Producto producto, string sedeproveedora, string idreserva)
    {
        this.Producto = producto;
        this.SedeProveedora = sedeproveedora;
        this.IdReserva = idreserva;
    }

    public decimal Subtotal()
    {
        return Cantidad * PrecioUnitario;
    }
}
}