// Lo que el módulo de pedidos necesita del grafo de distribución (Persona 2).
// "origen" es el código de la sede (LIB, ALA, SJO, LIM) y "destino" el nombre
// del punto de entrega tal como aparece en el grafo (Cañas, Heredia, ...).
public interface IPlanificadorRutas
{
    bool ExisteRuta(string origen, string destino);
}
