using Backend.Models;
using Backend.Persistencia;

namespace Backend.Services;

public class InventarioGlobalService
{
    private InventarioGlobal inventarioGlobal;
    private PersistenciaInventarioGlobalJson persistencia;
    private readonly object bloqueo = new object();

    public InventarioGlobalService(
        InventarioGlobal inventarioGlobal,
        PersistenciaInventarioGlobalJson persistencia)
    {
        this.inventarioGlobal = inventarioGlobal ??
            throw new ArgumentNullException(nameof(inventarioGlobal));

        this.persistencia = persistencia ??
            throw new ArgumentNullException(nameof(persistencia));
    }

    public bool ActualizarExistencias(
        string codigoProducto,
        string codigoSede,
        int cantidad,
        int cantidadReservada,
        int version)
    {
        lock (bloqueo)
        {
            bool actualizado = inventarioGlobal.ActualizarExistencias(
                codigoProducto,
                codigoSede,
                cantidad,
                cantidadReservada,
                version
            );

            if (actualizado)
            {
                persistencia.Guardar(inventarioGlobal);
            }

            return actualizado;
        }
    }

    public RegistroExistenciaGlobal? Buscar(
        string codigoProducto,
        string codigoSede)
    {
        lock (bloqueo)
        {
            return inventarioGlobal.Buscar(
                codigoProducto,
                codigoSede
            );
        }
    }

    public RegistroExistenciaGlobal[] ObtenerRegistros()
    {
        lock (bloqueo)
        {
            return inventarioGlobal.ObtenerRegistros();
        }
    }

    public void SincronizarInventarioLocal(
        InventarioService inventarioLocal,
        string codigoSede)
    {
        if (inventarioLocal == null)
        {
            throw new ArgumentNullException(nameof(inventarioLocal));
        }

        if (string.IsNullOrWhiteSpace(codigoSede))
        {
            throw new ArgumentException(
                "El codigo de la sede es obligatorio."
            );
        }

        // Obtener los productos del inventario fisico
        RegistroInventarioJson[] productos =
            inventarioLocal.ObtenerProductos();

        for (int i = 0; i < productos.Length; i++)
        {
            RegistroInventarioJson producto = productos[i];

            // Consultar el registro global de esta sede
            RegistroExistenciaGlobal? anterior = Buscar(
                producto.Codigo,
                codigoSede
            );

            // Si las cantidades siguen iguales,
            // no necesitamos generar otra version.
            if (anterior != null &&
                anterior.GetCantidad() == producto.Cantidad &&
                anterior.GetCantidadReservada() == producto.CantidadReservada)
            {
                continue;
            }

            // Crear una version mayor que la anterior
            int nuevaVersion = anterior == null
                ? 1
                : checked(anterior.GetVersion() + 1);

            // Actualizar y guardar la copia global
            ActualizarExistencias(
                producto.Codigo,
                codigoSede,
                producto.Cantidad,
                producto.CantidadReservada,
                nuevaVersion
            );
        }
    }

}