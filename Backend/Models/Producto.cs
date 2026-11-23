namespace Backend.Models;
public class Producto
{
    private string codigo;
    private string nombre;
    private decimal precio;
    private string categoria;
    private string descripcion;
    public Producto(string codigo, string nombre, string descripcion, string categoria, decimal precio)
    {
        this.codigo = codigo;
        this.nombre = nombre;
        this.descripcion = descripcion;
        this.categoria = categoria;
        this.precio = precio;
    }

    // Getters
    public string GetCodigo() { return codigo; }
    public string GetNombre() { return nombre; }
    public decimal GetPrecio() { return precio; }
    public string GetCategoria() { return categoria; }
    public string GetDescripcion() { return descripcion; }

    // Setters
    public void SetNombre(string nombre) { this.nombre = nombre; }
    public void SetPrecio(decimal precio) { this.precio = precio; }
    public void SetCategoria(string categoria) { this.categoria = categoria; }
    public void SetDescripcion(string descripcion) { this.descripcion = descripcion; }
}