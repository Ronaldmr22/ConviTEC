using System.Text.Json.Serialization;
namespace Backend.Persistencia;

public class RegistroGlobalJson
{
    public string CodigoProducto { get; }
    public string CodigoSede { get; }
    public int Cantidad { get; }
    public int CantidadReservada { get; }
    public int Version { get; }

    [JsonConstructor]
    public RegistroGlobalJson(
        string codigoProducto,
        string codigoSede,
        int cantidad,
        int cantidadReservada,
        int version)
    {
        CodigoProducto = codigoProducto;
        CodigoSede = codigoSede;
        Cantidad = cantidad;
        CantidadReservada = cantidadReservada;
        Version = version;
    }
}