namespace Backend.Models;
public class InventarioItem
{
    private Producto producto;
    private int cantidad;

    public InventarioItem(Producto producto, int cantidad)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto));
        }

        if (cantidad < 0)
        {
            throw new ArgumentException("La cantidad no puede ser negativa.");
        }

        this.producto = producto;
        this.cantidad = cantidad;
    }

    // Getters
    public Producto GetProducto()
    {
        return producto;
    }

    public int GetCantidad()
    {
        return cantidad;
    }

    // Métodos para modificar existencias
    public void AgregarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        this.cantidad += cantidad;
    }

    public void DescontarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        if (cantidad > this.cantidad)
        {
            throw new InvalidOperationException("Inventario insuficiente.");
        }

        this.cantidad -= cantidad;
    }
}
