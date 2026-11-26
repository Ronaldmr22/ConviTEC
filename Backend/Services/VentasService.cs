using Backend.Models;
using Backend.Persistencia;

namespace Backend.Services;

public class VentasService
{
    private readonly VentasJson persistencia;
    private readonly object bloqueo = new();

    public VentasService(VentasJson persistencia)
    {
        this.persistencia = persistencia;
    }

    public Venta[] ObtenerHistorial()
    {
        lock (bloqueo)
        {
            return persistencia.Cargar();
        }
    }

    public bool RegistrarVenta(Venta venta)
    {
        lock (bloqueo)
        {
            Venta[] historial = persistencia.Cargar();

            // Evita contar dos veces el mismo pedido.
            foreach (Venta registrada in historial)
            {
                if (registrada.IdPedido == venta.IdPedido)
                {
                    return false;
                }
            }

            Array.Resize(ref historial, historial.Length + 1);
            historial[^1] = venta;
            persistencia.Guardar(historial);

            return true;
        }
    }

    public EstadisticasVentas ObtenerEstadisticas()
    {
        Venta[] historial = ObtenerHistorial();
        decimal ingresosTotales = 0;
        ProductoEstrella[] ingresosPorProducto = [];
        RendimientoPedido[] rendimientos = new RendimientoPedido[historial.Length];
        int cantidadRendimientos = 0;

        foreach (Venta venta in historial)
        {
            decimal monto = venta.MontoTotal;
            ingresosTotales += monto;

            // Una entrega de cero minutos no tiene un rendimiento calculable.
            if (venta.MinutosTraslado > 0)
            {
                rendimientos[cantidadRendimientos++] = new RendimientoPedido(
                    venta.IdPedido, monto, venta.MinutosTraslado,
                    monto / venta.MinutosTraslado);
            }

            foreach (LineaVenta linea in venta.Productos)
            {
                int indice = 0;

                while (indice < ingresosPorProducto.Length &&
                    ingresosPorProducto[indice].CodigoProducto != linea.CodigoProducto)
                {
                    indice++;
                }

                if (indice == ingresosPorProducto.Length)
                {
                    Array.Resize(ref ingresosPorProducto, indice + 1);
                    ingresosPorProducto[indice] = new ProductoEstrella(
                        linea.CodigoProducto, linea.NombreProducto, linea.Subtotal);
                }
                else
                {
                    ProductoEstrella acumulado = ingresosPorProducto[indice];
                    ingresosPorProducto[indice] = acumulado with
                    {
                        Ingresos = acumulado.Ingresos + linea.Subtotal
                    };
                }
            }
        }

        ProductoEstrella? estrella = null;

        foreach (ProductoEstrella producto in ingresosPorProducto)
        {
            if (estrella is null || producto.Ingresos > estrella.Ingresos)
            {
                estrella = producto;
            }
        }

        Array.Resize(ref rendimientos, cantidadRendimientos);

        // Ordenamiento por inserción, de menor a mayor rendimiento.
        for (int i = 1; i < rendimientos.Length; i++)
        {
            RendimientoPedido actual = rendimientos[i];
            int anterior = i - 1;

            while (anterior >= 0 &&
                rendimientos[anterior].ColonesPorMinuto > actual.ColonesPorMinuto)
            {
                rendimientos[anterior + 1] = rendimientos[anterior];
                anterior--;
            }

            rendimientos[anterior + 1] = actual;
        }

        return new EstadisticasVentas(ingresosTotales, estrella, rendimientos);
    }
}
