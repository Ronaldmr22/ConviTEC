using System.Text.Json;
using Backend.Models;
using Backend.Services;

namespace Backend.Persistencia;

public class PersistenciaInventarioGlobalJson
{
    private string rutaArchivo;

    public PersistenciaInventarioGlobalJson(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new ArgumentException(
                "La ruta del archivo es obligatoria."
            );
        }

        this.rutaArchivo = Path.GetFullPath(rutaArchivo);
    }

    // Guardar la copia del inventario global
    public void Guardar(InventarioGlobal inventarioGlobal)
    {
        if (inventarioGlobal == null)
        {
            throw new ArgumentNullException(nameof(inventarioGlobal));
        }

        RegistroExistenciaGlobal[] registros =
            inventarioGlobal.ObtenerRegistros();

        RegistroGlobalJson[] datos =
            new RegistroGlobalJson[registros.Length];

        for (int i = 0; i < registros.Length; i++)
        {
            datos[i] = new RegistroGlobalJson(
                registros[i].GetCodigoProducto(),
                registros[i].GetCodigoSede(),
                registros[i].GetCantidad(),
                registros[i].GetCantidadReservada(),
                registros[i].GetVersion()
            );
        }

        JsonSerializerOptions opciones =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        string json = JsonSerializer.Serialize(datos, opciones);

        string? carpeta = Path.GetDirectoryName(rutaArchivo);

        if (carpeta != null)
        {
            Directory.CreateDirectory(carpeta);
        }

        string rutaTemporal = rutaArchivo + ".tmp";

        File.WriteAllText(rutaTemporal, json);
        File.Move(rutaTemporal, rutaArchivo, true);
    }

    // Recuperar la copia del inventario global
    public InventarioGlobal Cargar()
    {
        InventarioGlobal inventarioGlobal =
            new InventarioGlobal();

        if (!File.Exists(rutaArchivo))
        {
            return inventarioGlobal;
        }

        string json = File.ReadAllText(rutaArchivo);

        RegistroGlobalJson[] datos =
            JsonSerializer.Deserialize<RegistroGlobalJson[]>(json)
            ?? throw new InvalidDataException(
                "El archivo del inventario global no es valido."
            );

        for (int i = 0; i < datos.Length; i++)
        {
            RegistroGlobalJson registro = datos[i];

            bool agregado = inventarioGlobal.ActualizarExistencias(
                registro.CodigoProducto,
                registro.CodigoSede,
                registro.Cantidad,
                registro.CantidadReservada,
                registro.Version
            );

            if (!agregado)
            {
                throw new InvalidDataException(
                    "Registro duplicado o version invalida."
                );
            }
        }

        return inventarioGlobal;
    }

    public string GetRutaArchivo()
    {
        return rutaArchivo;
    }
}
