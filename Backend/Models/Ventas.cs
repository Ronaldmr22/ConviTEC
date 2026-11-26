namespace Backend.Models;

public record LineaVenta(
    string CodigoProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario)
{
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public record Venta(
    string IdPedido,
    string CodigoSede,
    DateTimeOffset FechaEntrega,
    int MinutosTraslado,
    LineaVenta[] Productos)
{
    public decimal MontoTotal
    {
        get
        {
            decimal total = 0;

            foreach (LineaVenta producto in Productos)
            {
                total += producto.Subtotal;
            }

            return total;
        }
    }
}
