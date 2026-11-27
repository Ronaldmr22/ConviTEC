using System.Text.Json;
using Backend.Models;
using Backend.Pedidos;
using Backend.Persistencia;

// Guarda los pedidos de la sede en {RutaDatos}/pedidos.json.
// Sigue el mismo patrón que InventarioJson: se convierte cada objeto a un
// registro simple, se escribe a un archivo temporal y luego se reemplaza.
public class RepositorioPedidosJson : IRepositorioPedidos
{
    private readonly string rutaArchivo;
    private readonly object candado = new object();

    public RepositorioPedidosJson(ConfiguracionSede sede, IWebHostEnvironment entorno)
    {
        rutaArchivo = Path.Combine(entorno.ContentRootPath, sede.GetRutaDatos(), "pedidos.json");
    }

    public void Guardar(Pedido pedido)
    {
        if (pedido == null)
        {
            throw new ArgumentNullException(nameof(pedido));
        }

        lock (candado)
        {
            RegistroPedidoJson[] registros = LeerArchivo();
            RegistroPedidoJson nuevo = ARegistro(pedido);

            // Si ya existe un pedido con ese Id, se reemplaza
            for (int i = 0; i < registros.Length; i++)
            {
                if (registros[i].Id == nuevo.Id)
                {
                    registros[i] = nuevo;
                    EscribirArchivo(registros);
                    return;
                }
            }

            // Si no existe, se agrega al final
            RegistroPedidoJson[] ampliado = new RegistroPedidoJson[registros.Length + 1];
            for (int i = 0; i < registros.Length; i++)
            {
                ampliado[i] = registros[i];
            }
            ampliado[registros.Length] = nuevo;
            EscribirArchivo(ampliado);
        }
    }

    public ListaEnlazada CargarTodos()
    {
        lock (candado)
        {
            RegistroPedidoJson[] registros = LeerArchivo();
            ListaEnlazada pedidos = new ListaEnlazada();

            for (int i = 0; i < registros.Length; i++)
            {
                pedidos.AgregarDato(APedido(registros[i]));
            }
            return pedidos;
        }
    }

    private RegistroPedidoJson[] LeerArchivo()
    {
        if (!File.Exists(rutaArchivo))
        {
            return new RegistroPedidoJson[0];
        }

        RegistroPedidoJson[]? registros = JsonSerializer.Deserialize<RegistroPedidoJson[]>(File.ReadAllText(rutaArchivo));
        if (registros == null)
        {
            throw new InvalidDataException("El archivo de pedidos no contiene un arreglo válido.");
        }
        return registros;
    }

    private void EscribirArchivo(RegistroPedidoJson[] registros)
    {
        string json = JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true });

        string? carpeta = Path.GetDirectoryName(rutaArchivo);
        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        string temporal = rutaArchivo + ".tmp";
        File.WriteAllText(temporal, json);
        File.Move(temporal, rutaArchivo, true);
    }

    private static RegistroPedidoJson ARegistro(Pedido pedido)
    {
        ListaEnlazada lineas = pedido.GetLineas();
        RegistroLineaPedidoJson[] registrosLineas = new RegistroLineaPedidoJson[lineas.GetSize()];
        int i = 0;
        lineas.Recorrer(dato =>
        {
            LineaPedido linea = (LineaPedido)dato;
            Producto producto = linea.GetProducto();
            registrosLineas[i] = new RegistroLineaPedidoJson
            {
                CodigoProducto = producto.GetCodigo(),
                Nombre = producto.GetNombre(),
                Descripcion = producto.GetDescripcion(),
                Categoria = producto.GetCategoria(),
                PrecioUnitario = linea.GetPrecioUnitario(),
                Cantidad = linea.GetCantidad(),
                SedeProveedora = linea.GetSedeProveedora(),
                IdReserva = linea.GetIdReserva()
            };
            i++;
        });

        return new RegistroPedidoJson
        {
            Id = pedido.GetId(),
            FechaCreacion = pedido.GetFechaCreación(),
            Estado = pedido.GetEstado().ToString(),
            SedeOrigen = pedido.GetSedeOrigen(),
            Destino = pedido.GetDestino(),
            MotivoRechazo = pedido.GetMotivoRechazo(),
            Lineas = registrosLineas
        };
    }

    private static Pedido APedido(RegistroPedidoJson registro)
    {
        EstadoPedido estado;
        if (!Enum.TryParse(registro.Estado, out estado))
        {
            throw new InvalidDataException("Estado de pedido inválido: " + registro.Estado);
        }

        Pedido pedido = new Pedido(registro.Id, registro.FechaCreacion, estado,
            registro.SedeOrigen, registro.Destino, registro.MotivoRechazo);

        for (int i = 0; i < registro.Lineas.Length; i++)
        {
            RegistroLineaPedidoJson r = registro.Lineas[i];
            Producto producto = new Producto(r.CodigoProducto, r.Nombre, r.Descripcion, r.Categoria, r.PrecioUnitario);

            LineaPedido linea = new LineaPedido(producto, r.Cantidad, r.PrecioUnitario);
            if (r.SedeProveedora != null)
            {
                linea.SetSedeProveedora(r.SedeProveedora);
            }
            linea.SetIdReserva(r.IdReserva);
            pedido.AgregarLinea(linea);
        }
        return pedido;
    }
}
