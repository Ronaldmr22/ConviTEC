using System.Runtime.InteropServices;

namespace Grafo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Grafo convitec = new Grafo();

            convitec.Vertices.Add(new Vertice("Liberia", 0));
            convitec.Vertices.Add(new Vertice("Cañas", 1));
            convitec.Vertices.Add(new Vertice("Alajuela", 2));
            convitec.Vertices.Add(new Vertice("Orotina", 3));
            convitec.Vertices.Add(new Vertice("Puntarenas", 4));
            convitec.Vertices.Add(new Vertice("Heredia", 5));
            convitec.Vertices.Add(new Vertice("San José", 6));
            convitec.Vertices.Add(new Vertice("Cartago", 7));
            convitec.Vertices.Add(new Vertice("Guapiles", 8));
            convitec.Vertices.Add(new Vertice("Turrialba", 9));
            convitec.Vertices.Add(new Vertice("Paraíso", 10));
            convitec.Vertices.Add(new Vertice("Siquirres", 11));
            convitec.Vertices.Add(new Vertice("Limón", 12));
        


            AgregarAristaNoDirigida(convitec.Vertices[0], convitec.Vertices[1], 45);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[3], 49);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[4], 105);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[5], 15);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[6], 20);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[5], 35);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[7], 126);
            AgregarAristaNoDirigida(convitec.Vertices[7], convitec.Vertices[8], 40);
            AgregarAristaNoDirigida(convitec.Vertices[8], convitec.Vertices[9], 90);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[10], 40);
            AgregarAristaNoDirigida(convitec.Vertices[10], convitec.Vertices[11], 20);
            AgregarAristaNoDirigida(convitec.Vertices[11], convitec.Vertices[12], 60);
            AgregarAristaNoDirigida(convitec.Vertices[9], convitec.Vertices[12], 130);




           ImprimirGrafo(convitec.Vertices[0]);
           ImprimirGrafo(convitec.Vertices[2]);

        }

        
        public static void AgregarAristaNoDirigida(Vertice origen, Vertice destino, int peso)
        {
            origen.Aristas.Add(new Arista(origen, destino, peso));
            destino.Aristas.Add(new Arista(destino, origen, peso));
        }
        
       
        public static void ImprimirGrafo(Vertice? vertice, HashSet<Vertice>? visitados = null)
        {
            if (vertice == null)
                return;

            visitados ??= new HashSet<Vertice>();

            if (!visitados.Add(vertice))
                return;

            Console.WriteLine($"Vértice: {vertice.Nombre}");

            if (vertice.Aristas.Count == 0)
            {
                Console.WriteLine("  (Sin aristas salientes)");
            }
            else
            {
                foreach (var arista in vertice.Aristas)
                {
                    Console.WriteLine($"  -> Vértice {arista.Vertice2.Nombre} (Peso: {arista.Peso})");
                }
            }

            Console.WriteLine("_______________");

            foreach (var arista in vertice.Aristas)
            {
                ImprimirGrafo(arista.Vertice2, visitados);
            }
        }
    }
}
