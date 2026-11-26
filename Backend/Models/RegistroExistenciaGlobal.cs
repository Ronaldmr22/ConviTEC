using System.Text.Json.Serialization;
namespace Backend.Models;

public class RegistroExistenciaGlobal
{
    private string codigoProducto;
    private string codigoSede;
    private int cantidad;
    private int cantidadReservada;
    private int version;
    // Propiedades para JSON
    public string CodigoProducto => codigoProducto;
    public string CodigoSede => codigoSede;
    public int Cantidad => cantidad;
    public int CantidadReservada => cantidadReservada;
    public int Version => version;
    // Propiedad calculada
    public int CantidadDisponible => cantidad - cantidadReservada;

    [JsonConstructor]
    public RegistroExistenciaGlobal(
        string codigoProducto,
        string codigoSede,
        int cantidad,
        int cantidadReservada,
        int version)
    {
        if (string.IsNullOrWhiteSpace(codigoProducto) ||
            string.IsNullOrWhiteSpace(codigoSede))
        {
            throw new ArgumentException(
                "El producto y la sede son obligatorios."
            );
        }

        if (cantidad < 0 ||
            cantidadReservada < 0 ||
            cantidadReservada > cantidad)
        {
            throw new ArgumentException(
                "Las cantidades del registro son invalidas."
            );
        }

        if (version < 0)
        {
            throw new ArgumentException(
                "La version no puede ser negativa."
            );
        }

        this.codigoProducto = codigoProducto;
        this.codigoSede = codigoSede;
        this.cantidad = cantidad;
        this.cantidadReservada = cantidadReservada;
        this.version = version;
    }

    public string GetCodigoProducto()
    {
        return codigoProducto;
    }

    public string GetCodigoSede()
    {
        return codigoSede;
    }

    public int GetCantidad()
    {
        return cantidad;
    }

    public int GetCantidadReservada()
    {
        return cantidadReservada;
    }

    public int GetCantidadDisponible()
    {
        return cantidad - cantidadReservada;
    }

    public int GetVersion()
    {
        return version;
    }
}
