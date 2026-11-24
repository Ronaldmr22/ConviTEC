namespace Backend.Models;

public class RegistroGlobalRespuesta
{
    public string CodigoProducto { get; }
    public string CodigoSede { get; }
    public int Cantidad { get; }
    public int CantidadReservada { get; }
    public int CantidadDisponible { get; }
    public long Version { get; }

    public RegistroGlobalRespuesta(
        string codigoProducto,
        string codigoSede,
        int cantidad,
        int cantidadReservada,
        long version)
    {
        CodigoProducto = codigoProducto;
        CodigoSede = codigoSede;
        Cantidad = cantidad;
        CantidadReservada = cantidadReservada;
        CantidadDisponible = cantidad - cantidadReservada;
        Version = version;
    }
}