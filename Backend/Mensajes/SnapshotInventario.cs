using System.Text.Json.Serialization;
using Backend.Models;
namespace Backend.Mensajes;

public class SnapshotInventario : Mensaje
{
    private RegistroExistenciaGlobal[] existencias;
    // Propiedad para JSON
    public RegistroExistenciaGlobal[] Existencias => GetExistencias();

    [JsonConstructor]
    public SnapshotInventario(string id, string sedeOrigen, string sedeDestino, string correlationId, DateTime fecha, RegistroExistenciaGlobal[] existencias)
        : base(id, "SnapshotInventario", sedeOrigen, sedeDestino, correlationId, fecha)
    {
        if (existencias == null)
        {
            throw new ArgumentNullException(nameof(existencias));
        }

        this.existencias = new RegistroExistenciaGlobal[existencias.Length];

        for (int i = 0; i < existencias.Length; i++)
        {
            if (existencias[i] == null)
            {
                throw new ArgumentException("El snapshot no puede contener registros nulos.");
            }

            this.existencias[i] = existencias[i];
        }
    }

    public RegistroExistenciaGlobal[] GetExistencias()
    {
        RegistroExistenciaGlobal[] copia = new RegistroExistenciaGlobal[existencias.Length];

        for (int i = 0; i < existencias.Length; i++)
        {
            copia[i] = existencias[i];
        }

        return copia;
    }
}
