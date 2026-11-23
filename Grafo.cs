using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Grafo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            {
                string rutaJson = args.Length > 0 ? args[0] : "vertices.json";
                Grafo convitec = CargarGrafoDesdeJson(rutaJson);
                Django(convitec, convitec.Vertices[7]);
              
            }
        }

        
        public static Grafo CargarGrafoDesdeJson(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"No se encontró el archivo JSON: {rutaArchivo}");

            string json = File.ReadAllText(rutaArchivo);

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            GrafoJsonDto? datos = JsonSerializer.Deserialize<GrafoJsonDto>(json, opciones);
            if (datos == null)
                throw new InvalidDataException("El contenido del JSON no es válido.");

            Grafo grafo = new Grafo();

            // 1. Crear vértices y mapearlos por Indice
            var verticesPorIndice = new Dictionary<int, Vertice>();
            foreach (var vDto in datos.Vertices)
            {
                if (verticesPorIndice.ContainsKey(vDto.Indice))
                    throw new InvalidDataException($"Índice de vértice duplicado: {vDto.Indice}");

                var vertice = new Vertice(vDto.Nombre, vDto.Indice);
                grafo.Vertices.Add(vertice);
                verticesPorIndice[vDto.Indice] = vertice;
            }

            // 2. Crear aristas conectando los vértices correspondientes
            foreach (var aDto in datos.Aristas)
            {
                if (!verticesPorIndice.TryGetValue(aDto.OrigenIndice, out Vertice? origen) ||
                    !verticesPorIndice.TryGetValue(aDto.DestinoIndice, out Vertice? destino))
                {
                    throw new InvalidDataException(
                        $"Arista con índice inexistente ({aDto.OrigenIndice} -> {aDto.DestinoIndice}).");
                }

                if (aDto.NoDirigida)
                {
                    AgregarAristaNoDirigida(origen, destino, aDto.Peso);
                }
                else
                {
                    origen.Aristas.Add(new Arista(origen, destino, aDto.Peso));
                }
            }

            return grafo;
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
