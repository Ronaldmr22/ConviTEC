using Backend.Models;
using Backend.Persistencia;

namespace Backend.Services;

public class InventarioService
{
    private Inventario inventario;
    private InventarioJson persistencia;
    private readonly object bloqueo = new object();

    // Nuevos atributos para sincronizar la copia global
    private InventarioGlobalService? servicioGlobal;
    private string? codigoSede;

    public InventarioService(
        Inventario inventario,
        InventarioJson persistencia)
    {
        this.inventario = inventario ??
            throw new ArgumentNullException(nameof(inventario));

        this.persistencia = persistencia ??
            throw new ArgumentNullException(nameof(persistencia));
    }

    // Conectar el servicio local con la copia global
    public void ConfigurarSincronizacionGlobal(
        InventarioGlobalService servicioGlobal,
        string codigoSede)
    {
        if (servicioGlobal == null)
        {
            throw new ArgumentNullException(nameof(servicioGlobal));
        }

        if (string.IsNullOrWhiteSpace(codigoSede))
        {
            throw new ArgumentException(
                "El codigo de la sede es obligatorio."
            );
        }

        lock (bloqueo)
        {
            this.servicioGlobal = servicioGlobal;
            this.codigoSede = codigoSede;
        }
    }

    // Actualizar el registro global de nuestra propia sede
    private void ActualizarCopiaGlobal(string codigo)
    {
        if (servicioGlobal == null || codigoSede == null)
        {
            return;
        }

        // Este metodo se ejecuta dentro del bloqueo local
        InventarioItem? item = inventario.BuscarProducto(codigo);

        if (item == null)
        {
            return;
        }

        RegistroExistenciaGlobal? anterior = servicioGlobal.Buscar(codigo, codigoSede);

        Console.WriteLine("=== SINCRONIZACION GLOBAL ===");

        Console.WriteLine("Producto: " + codigo);
        Console.WriteLine("Sede: " + codigoSede);

        Console.WriteLine(
            "Cantidad local: " + item.GetCantidad()
        );

        Console.WriteLine(
            "Cantidad global anterior: " +
            (anterior == null ? "No existe" : anterior.GetCantidad().ToString())
        );

        Console.WriteLine(
            "Version anterior: " +
            (anterior == null ? "No existe" : anterior.GetVersion().ToString())
        );

        int cantidad = item.GetCantidad();
        int cantidadReservada = item.GetCantidadReservada();

        // Si no hubo cambios, conservar la version
        if (anterior != null &&
            anterior.GetCantidad() == cantidad &&
            anterior.GetCantidadReservada() == cantidadReservada)
        {
            return;
        }

        int nuevaVersion = anterior == null? 1: checked(anterior.GetVersion() + 1);

        bool actualizado = servicioGlobal.ActualizarExistencias(
            codigo,
            codigoSede,
            cantidad,
            cantidadReservada,
            nuevaVersion
        );

        Console.WriteLine("Actualizacion realizada: " + actualizado);
        Console.WriteLine("Nueva version: " + nuevaVersion);

    }

    // Registrar un producto
    public bool AgregarProducto(Producto producto, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.AgregarProducto(
                producto, cantidad
            );

            if (resultado)
            {
                persistencia.Guardar(inventario);
                ActualizarCopiaGlobal(producto.GetCodigo());
            }

            return resultado;
        }
    }

    // Incorporar unidades
    public bool AgregarExistencias(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            Console.WriteLine("ENTRO A AGREGAR EXISTENCIAS");

            bool resultado = inventario.AgregarExistencias(
                codigo, cantidad
            );

            Console.WriteLine("Resultado: " + resultado);

            if (resultado)
            {
                persistencia.Guardar(inventario);

                Console.WriteLine("VOY A ACTUALIZAR LA COPIA GLOBAL");

                ActualizarCopiaGlobal(codigo);
            }

            return resultado;
        }
    }

    // Descontar unidades
    public bool DescontarExistencias(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.DescontarExistencias(
                codigo, cantidad
            );

            if (resultado)
            {
                persistencia.Guardar(inventario);
                ActualizarCopiaGlobal(codigo);
            }

            return resultado;
        }
    }

    // Reservar unidades
    public bool ReservarProducto(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.ReservarProducto(
                codigo, cantidad
            );

            if (resultado)
            {
                persistencia.Guardar(inventario);
                ActualizarCopiaGlobal(codigo);
            }

            return resultado;
        }
    }

    // Liberar unidades reservadas
    public bool LiberarReserva(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.LiberarReserva(
                codigo, cantidad
            );

            if (resultado)
            {
                persistencia.Guardar(inventario);
                ActualizarCopiaGlobal(codigo);
            }

            return resultado;
        }
    }

    // Confirmar unidades reservadas
    public bool ConfirmarReserva(string codigo, int cantidad)
    {
        lock (bloqueo)
        {
            bool resultado = inventario.ConfirmarReserva(
                codigo, cantidad
            );

            if (resultado)
            {
                persistencia.Guardar(inventario);
                ActualizarCopiaGlobal(codigo);
            }

            return resultado;
        }
    }

    // Buscar producto por codigo
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

    // Obtener todos los productos locales
    public RegistroInventarioJson[] ObtenerProductos()
    {
        lock (bloqueo)
        {
            InventarioItem[] items = inventario.ObtenerProductos();

            RegistroInventarioJson[] registros =
                new RegistroInventarioJson[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                Producto producto = items[i].GetProducto();

                registros[i] = new RegistroInventarioJson(
                    producto.GetCodigo(),
                    producto.GetNombre(),
                    producto.GetDescripcion(),
                    producto.GetCategoria(),
                    producto.GetPrecio(),
                    items[i].GetCantidad(),
                    items[i].GetCantidadReservada()
                );
            }

            return registros;
        }
    }
}
