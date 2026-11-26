using Backend.Models;

namespace Backend.Servicios;

public class InventarioGlobal
{
    private RegistroExistenciaGlobal[] registros;
    private int cantidadRegistros;
    private readonly object bloqueo = new object();

    public InventarioGlobal()
    {
        registros = new RegistroExistenciaGlobal[8];
        cantidadRegistros = 0;
    }

    // Buscar la posicion de un producto en una sede
    private int BuscarIndice(string codigoProducto, string codigoSede)
    {
        for (int i = 0; i < cantidadRegistros; i++)
        {
            if (registros[i].GetCodigoProducto() == codigoProducto &&
                registros[i].GetCodigoSede() == codigoSede)
            {
                return i;
            }
        }

        return -1;
    }

    // Ampliar el arreglo cuando ya no tenga espacio
    private void AmpliarCapacidad()
    {
        RegistroExistenciaGlobal[] nuevo = new RegistroExistenciaGlobal[registros.Length * 2];

        for (int i = 0; i < cantidadRegistros; i++)
        {
            nuevo[i] = registros[i];
        }

        registros = nuevo;
    }

    // Registrar una nueva version de las existencias
    public bool ActualizarExistencias(
        string codigoProducto,
        string codigoSede,
        int cantidad,
        int cantidadReservada,
        int version)
    {
        RegistroExistenciaGlobal nuevo =
            new RegistroExistenciaGlobal(
                codigoProducto,
                codigoSede,
                cantidad,
                cantidadReservada,
                version
            );

        lock (bloqueo)
        {
            int indice = BuscarIndice(codigoProducto, codigoSede);

            if (indice >= 0)
            {
                // Ignorar mensajes repetidos o antiguos
                if (version <= registros[indice].GetVersion())
                {
                    return false;
                }

                registros[indice] = nuevo;
                return true;
            }

            // Si el producto de esa sede no existe, agregarlo
            if (cantidadRegistros == registros.Length)
            {
                AmpliarCapacidad();
            }

            registros[cantidadRegistros] = nuevo;
            cantidadRegistros++;

            return true;
        }
    }

    // Consultar un producto en una sede especifica
    public RegistroExistenciaGlobal? Buscar(
        string codigoProducto,
        string codigoSede)
    {
        lock (bloqueo)
        {
            int indice = BuscarIndice(codigoProducto, codigoSede);

            if (indice == -1)
            {
                return null;
            }

            return registros[indice];
        }
    }

    // Obtener todos los registros conocidos
    public RegistroExistenciaGlobal[] ObtenerRegistros()
    {
        lock (bloqueo)
        {
            RegistroExistenciaGlobal[] resultado =
                new RegistroExistenciaGlobal[cantidadRegistros];

            for (int i = 0; i < cantidadRegistros; i++)
            {
                resultado[i] = registros[i];
            }

            return resultado;
        }
    }
}
