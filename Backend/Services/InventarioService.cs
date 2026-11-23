using Backend.Models;
using Backend.Persistencia;

namespace Backend.Services;
public class InventarioService
{
    private Inventario inventario;
    private InventarioJson persistencia;
    private readonly object bloqueo = new object();

    public InventarioService(
        Inventario inventario,
        InventarioJson persistencia)
    {
        this.inventario = inventario ??
            throw new ArgumentNullException(nameof(inventario));

        this.persistencia = persistencia ??
            throw new ArgumentNullException(nameof(persistencia));
    }

    public bool AgregarProducto(Producto producto, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.AgregarProducto(
                producto, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public bool AgregarExistencias(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.AgregarExistencias(
                codigo, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public bool DescontarExistencias(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.DescontarExistencias(
                codigo, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public bool ReservarProducto(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.ReservarProducto(
                codigo, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public bool LiberarReserva(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.LiberarReserva(
                codigo, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public bool ConfirmarReserva(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.ConfirmarReserva(
                codigo, cantidad);

            if (resultado)
            {
                persistencia.Guardar(inventario);
            }

            return resultado;
        }
    }

    public RegistroInventarioJson? BuscarProducto(string codigo)
    {
        lock (bloqueo)
        {
            InventarioItem? item = inventario.BuscarProducto(codigo);

            if (item == null)
            {
                return null;
            }

            Producto producto = item.GetProducto();

            return new RegistroInventarioJson(
                producto.GetCodigo(),
                producto.GetNombre(),
                producto.GetDescripcion(),
                producto.GetCategoria(),
                producto.GetPrecio(),
                item.GetCantidad(),
                item.GetCantidadReservada()
            );
        }
    }
}
