using System.IO;
using System.Text.Json;
using Backend.Models;

namespace Backend.Persistencia;

public class VentasJson
{
    private readonly string ruta;
    private readonly object bloqueo = new();

    public VentasJson(string ruta)
    {
        this.ruta = Path.GetFullPath(ruta);
    }

    public Venta[] Cargar()
    {
        lock (bloqueo)
        {
            if (!File.Exists(ruta))
            {
                return [];
            }

            using var archivo = File.OpenRead(ruta);
            return JsonSerializer.Deserialize<Venta[]>(archivo)
                ?? throw new InvalidDataException("El historial de ventas debe ser un arreglo JSON.");
        }
    }

    public void Guardar(Venta[] ventas)
    {
        lock (bloqueo)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            string rutaTemporal = ruta + ".tmp";

            // Escribe el historial completo antes de reemplazar el archivo anterior.
            try
            {
                using (var archivo = File.Create(rutaTemporal))
                {
                    JsonSerializer.Serialize(archivo, ventas);
                }

                File.Move(rutaTemporal, ruta, overwrite: true);
            }
            finally
            {
                File.Delete(rutaTemporal);
            }
        }
    }
}
