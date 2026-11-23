namespace Backend.Models;
public class ConfiguracionSede
{
    private string codigo;
    private string nombre;
    private string rutaDatos;

    public ConfiguracionSede(
        string codigo,
        string nombre,
        string rutaDatos)
    {
        if (string.IsNullOrWhiteSpace(codigo) ||
            string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(rutaDatos))
        {
            throw new ArgumentException(
                "La configuración de la sede está incompleta."
            );
        }

        this.codigo = codigo;
        this.nombre = nombre;
        this.rutaDatos = rutaDatos;
    }

    public string GetCodigo()
    {
        return codigo;
    }

    public string GetNombre()
    {
        return nombre;
    }

    public string GetRutaDatos()
    {
        return rutaDatos;
    }
}
