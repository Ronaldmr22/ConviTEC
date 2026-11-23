using System.Text.Json.Serialization;
namespace Backend.Persistencia;
public class RegistroInventarioJson
{
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string Categoria { get; set; }
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
    public int CantidadReservada { get; set; }

    [JsonConstructor]
    public RegistroInventarioJson(string codigo, string nombre, string descripcion, string categoria, decimal precio, int cantidad, int cantidadReservada)
    {
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
        Precio = precio;
        Cantidad = cantidad;
        CantidadReservada = cantidadReservada;
    }
}
