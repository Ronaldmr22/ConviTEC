using Backend.Models;
using Backend.Estructuras.Arboles;
namespace Backend.Services;
public class Inventario
{
    private ArbolSplayInventario arbol;
    public Inventario()
    {
        this.arbol = new ArbolSplayInventario();
    }

    // Registrar un nuevo producto
    public bool AgregarProducto(Producto producto, int cantidad)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto));
        }

        InventarioItem item = new InventarioItem(producto, cantidad);

        return arbol.Insertar(item);
    }

    // Buscar un producto por código
    public InventarioItem? BuscarProducto(string codigo)
    {
        return arbol.Buscar(codigo);
    }

    // Agregar unidades a un producto existente
    public bool AgregarExistencias(string codigo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        InventarioItem? item = arbol.Buscar(codigo);

        if (item == null)
        {
            return false;
        }

        item.AgregarCantidad(cantidad);
        return true;
    }

    // Descontar unidades de un producto existente
    public bool DescontarExistencias(string codigo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.");
        }

        InventarioItem? item = arbol.Buscar(codigo);

        if (item == null)
        {
            return false;
        }

        item.DescontarCantidad(cantidad);
        return true;
    }

    // Consultar todos los productos del inventario
    public InventarioItem[] ObtenerProductos()
    {
        return arbol.RecorrerEnOrden();
    }

    public bool ReservarProducto(string codigo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }

        InventarioItem? item = arbol.Buscar(codigo);

        if (item == null)
        {
            return false;
        }

        item.ReservarCantidad(cantidad);

        return true;
    }

    public bool LiberarReserva(string codigo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }

        InventarioItem? item = arbol.Buscar(codigo);

        if (item == null)
        {
            return false;
        }

        item.LiberarReserva(cantidad);

        return true;
    }

    public bool ConfirmarReserva(string codigo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser positiva.");
        }

        InventarioItem? item = arbol.Buscar(codigo);

        if (item == null)
        {
            return false;
        }

        item.ConfirmarReserva(cantidad);

        return true;
    }
}