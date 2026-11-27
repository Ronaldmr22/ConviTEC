// IMPLEMENTACIÓN TEMPORAL mientras Persona 2 termina el grafo con Dijkstra.
// Solo sirve para poder probar pedidos: decide si hay ruta revisando si el
// origen y el destino están en el mismo componente del grafo del enunciado
// (Liberia y Cañas forman un componente aislado).
// Cuando exista el planificador real, se cambia el registro en Program.cs.
public class PlanificadorRutasTemporal : IPlanificadorRutas
{
    private static readonly string[] ComponenteGuanacaste = { "LIB", "Liberia", "Cañas" };

    private static readonly string[] ComponenteCentral =
    {
        "ALA", "SJO", "LIM",
        "Alajuela", "San José", "Limón", "Puntarenas", "Orotina", "Heredia",
        "Cartago", "Paraíso", "Turrialba", "Guápiles", "Siquirres"
    };

    public bool ExisteRuta(string origen, string destino)
    {
        int componenteOrigen = Componente(origen);
        int componenteDestino = Componente(destino);
        return componenteOrigen != 0 && componenteOrigen == componenteDestino;
    }

    // 1 = Guanacaste, 2 = central, 0 = desconocido
    private static int Componente(string lugar)
    {
        if (Contiene(ComponenteGuanacaste, lugar)) { return 1; }
        if (Contiene(ComponenteCentral, lugar)) { return 2; }
        return 0;
    }

    private static bool Contiene(string[] lugares, string lugar)
    {
        for (int i = 0; i < lugares.Length; i++)
        {
            if (string.Equals(lugares[i], lugar?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
