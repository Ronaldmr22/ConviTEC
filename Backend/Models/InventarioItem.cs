namespace Backend.Models;
public class InventarioItem
{
    private Producto producto;
    private int cantidad;
    private int cantidadReservada;

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
        this.cantidadReservada = 0;
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

    public int GetCantidadReservada()
    {
        return cantidadReservada;
    }

    public int GetCantidadDisponible()
    {
        return cantidad - cantidadReservada;
    }

    // Agregar unidades al inventario
    public void AgregarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }
        this.cantidad = checked(this.cantidad + cantidad);
    }

    // Descontar unidades que no estén reservadas
    public void DescontarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }

        if (cantidad > GetCantidadDisponible())
        {
            throw new InvalidOperationException("Inventario disponible insuficiente.");
        }
        this.cantidad -= cantidad;
    }

    // Apartar productos para un pedido
    public void ReservarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }

        if (cantidad > GetCantidadDisponible())
        {
            throw new InvalidOperationException("No hay suficientes unidades disponibles.");
        }
        this.cantidadReservada += cantidad;
    }

    // Cancelar una reserva
    public void LiberarReserva(int cantidad)
    {
        if (cantidad <= 0 || cantidad > cantidadReservada)
        {
            throw new ArgumentException("Cantidad de reserva inválida.");
        }
        this.cantidadReservada -= cantidad;
    }

    // Confirmar la salida de productos reservados
    public void ConfirmarReserva(int cantidad)
    {
        if (cantidad <= 0 || cantidad > cantidadReservada)
        {
            throw new ArgumentException("Cantidad de reserva inválida.");
        }
        this.cantidadReservada -= cantidad;
        this.cantidad -= cantidad;
    }
}