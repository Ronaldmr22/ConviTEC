namespace Grafo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Vertice inicial = CrearGrafo();
            ImprimirGrafo( inicial);

        }
        public static Vertice CrearGrafo()
        {
            Vertice vertice1 = new Vertice(1);
            Vertice vertice2 = new Vertice(2);
            Vertice vertice3 = new Vertice(3);
            Vertice vertice4 = new Vertice(4);
            Vertice vertice5 = new Vertice(5);
            Vertice vertice6 = new Vertice(6);

            vertice6.Aristas.Add(new Arista(vertice6, vertice4, 1));
            vertice4.Aristas.Add(new Arista(vertice4, vertice5, 2));
            vertice4.Aristas.Add(new Arista(vertice4, vertice3, 4));
            vertice3.Aristas.Add(new Arista(vertice3, vertice2, 5));
            vertice5.Aristas.Add(new Arista(vertice5, vertice2, 4));
            vertice5.Aristas.Add(new Arista(vertice5, vertice1, 3));
            vertice2.Aristas.Add(new Arista(vertice2, vertice1, 2));

            return vertice6;
        }
        public static void ImprimirGrafo(Vertice? vertice, HashSet<Vertice>? visitados = null)
        {
            if (vertice == null)
                return;

            visitados ??= new HashSet<Vertice>();

            if (!visitados.Add(vertice))
                return;

            Console.WriteLine($"Vértice: {vertice.Valor}");

            if (vertice.Aristas.Count == 0)
            {
                Console.WriteLine("  (Sin aristas salientes)");
            }
            else
            {
                foreach (var arista in vertice.Aristas)
                {
                    Console.WriteLine($"  -> Vértice {arista.Vertice2.Valor} (Peso: {arista.Peso})");
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
