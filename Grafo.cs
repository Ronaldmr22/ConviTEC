using System.Diagnostics.Contracts;
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
            AgregarAristaNoDirigida(convitec.Vertices[3], convitec.Vertices[4], 40);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[3], 49);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[4], 105);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[5], 15);
            AgregarAristaNoDirigida(convitec.Vertices[2], convitec.Vertices[6], 20);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[5], 35);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[7], 40);
            AgregarAristaNoDirigida(convitec.Vertices[6], convitec.Vertices[8], 70);
            AgregarAristaNoDirigida(convitec.Vertices[8], convitec.Vertices[9], 90);
            AgregarAristaNoDirigida(convitec.Vertices[8], convitec.Vertices[11], 35);
            AgregarAristaNoDirigida(convitec.Vertices[7], convitec.Vertices[10], 40);
            AgregarAristaNoDirigida(convitec.Vertices[10], convitec.Vertices[9], 20);
            AgregarAristaNoDirigida(convitec.Vertices[11], convitec.Vertices[12], 60);
            AgregarAristaNoDirigida(convitec.Vertices[9], convitec.Vertices[12], 130);

           Django(convitec, convitec.Vertices[0]);
          
        }

        public static void Django(Grafo grafo, Vertice origen)
        {
            if (grafo == null || origen == null || grafo.Vertices.Count == 0)
                return;

            int cantidadVertices = grafo.Vertices.Count;
            int[] distancias = new int[cantidadVertices];
            bool[] procesados = new bool[cantidadVertices];
            Vertice[] anteriores = new Vertice[cantidadVertices];

            for (int i = 0; i < cantidadVertices; i++)
            {
                distancias[i] = int.MaxValue;
                procesados[i] = false;
                anteriores[i] = null!;
            }

            distancias[origen.Indice] = 0;

            for (int count = 0; count < cantidadVertices; count++)
            {
                Vertice? actual = null;
                int menorDistancia = int.MaxValue;

                // Buscar el vértice no procesado con menor distancia acumulada
                for (int i = 0; i < cantidadVertices; i++)
                {
                    Vertice candidato = grafo.Vertices[i];
                    if (!procesados[candidato.Indice] && distancias[candidato.Indice] <= menorDistancia)
                    {
                        menorDistancia = distancias[candidato.Indice];
                        actual = candidato;
                    }
                }

                if (actual == null || distancias[actual.Indice] == int.MaxValue)
                    break;

                procesados[actual.Indice] = true;

               
                // Relajar las aristas salientes del vértice actual
                for (int j = 0; j < actual.Aristas.Count; j++)
                {
                    Arista arista = actual.Aristas[j];
                    Vertice vecino = arista.Vertice2;

                    if (!procesados[vecino.Indice] && arista.Peso >= 0)
                    {
                        int nuevaDistancia = distancias[actual.Indice] + arista.Peso;
                        if (nuevaDistancia < distancias[vecino.Indice])
                        {
                            distancias[vecino.Indice] = nuevaDistancia;
                            anteriores[vecino.Indice] = actual;
                        }
                    }
                }
            }

            
            Console.WriteLine($"\n--- Distancias mínimas desde {origen.Nombre} (Dijkstra) ---");
            for (int i = 0; i < cantidadVertices; i++)
            {
                Vertice v = grafo.Vertices[i];
                if (distancias[v.Indice] == int.MaxValue)
                {
                    Console.WriteLine($"Hacia {v.Nombre}: No alcanzable");
                }
                else
                {
                    Console.WriteLine($"Hacia {v.Nombre}: {distancias[v.Indice]}");
                }
            }
        }


        public static void AgregarAristaNoDirigida(Vertice origen, Vertice destino, int peso)
        {
            origen.Aristas.Add(new Arista(origen, destino, peso));
            destino.Aristas.Add(new Arista(destino, origen, peso));
        }
        
       
        
    }
}
