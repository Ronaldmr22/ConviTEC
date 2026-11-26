using System.Text.Json;
using Backend.Models;
using Backend.Servicios;

namespace Backend.Persistencia;

public class InventarioJson
{
    private string rutaArchivo;

    public InventarioJson(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new ArgumentException("La ruta no puede estar vacía.");
        }

        this.rutaArchivo = Path.GetFullPath(rutaArchivo);
    }

    // Guardar el inventario en un archivo JSON
    public void Guardar(Inventario inventario)
    {
        if (inventario == null)
        {
            throw new ArgumentNullException(nameof(inventario));
        }

        InventarioItem[] items = inventario.ObtenerProductos();

        RegistroInventarioJson[] registros = new RegistroInventarioJson[items.Length];

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

        JsonSerializerOptions opciones = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(registros, opciones);

        string? carpeta = Path.GetDirectoryName(rutaArchivo);

        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        // Archivo temporal para no sobrescribir directamente
        string temporal = rutaArchivo + ".tmp";

        File.WriteAllText(temporal, json);
        File.Move(temporal, rutaArchivo, true);
    }

    // Recuperar el inventario desde JSON
    public Inventario Cargar()
    {
        Inventario inventario = new Inventario();

        if (!File.Exists(rutaArchivo))
        {
            return inventario;
        }

        string json = File.ReadAllText(rutaArchivo);

        RegistroInventarioJson[] registros = JsonSerializer.Deserialize<RegistroInventarioJson[]>(json)
            ?? throw new InvalidDataException("El archivo de inventario no contiene un arreglo válido.");

        foreach (RegistroInventarioJson registro in registros)
        {
            Producto producto = new Producto(
                registro.Codigo,
                registro.Nombre,
                registro.Descripcion,
                registro.Categoria,
                registro.Precio
            );

            bool agregado = inventario.AgregarProducto(
                producto,
                registro.Cantidad
            );

            if (!agregado)
            {
                throw new InvalidDataException(
                    "Código duplicado: " + registro.Codigo
                );
            }

            if (registro.CantidadReservada < 0 ||
                registro.CantidadReservada > registro.Cantidad)
            {
                throw new InvalidDataException(
                    "Cantidad reservada inválida: " + registro.Codigo
                );
            }

            if (registro.CantidadReservada > 0)
            {
                inventario.ReservarProducto(
                    registro.Codigo,
                    registro.CantidadReservada
                );
            }
        }

        return inventario;
    }
    public string GetRutaArchivo()
    {
        return rutaArchivo;
    }
}
