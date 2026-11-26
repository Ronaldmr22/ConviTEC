using System.Text.Json.Serialization;

namespace Backend.Persistencia;

public class ProductoInicialJson
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Descripcion { get; }
    public string Categoria { get; }
    public decimal Precio { get; }

    [JsonConstructor]
    public ProductoInicialJson(
        string codigo,
        string nombre,
        string descripcion,
        string categoria,
        decimal precio)
    {
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
        Precio = precio;
    }
}