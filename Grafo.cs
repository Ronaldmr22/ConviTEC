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
            Vertice liberia = new Vertice("Liberia");
            Vertice canias = new Vertice("Cañas");
            Vertice alajuela = new Vertice("Alajuela");
            Vertice orotina = new Vertice("Orotina");
            Vertice puntarenas = new Vertice("Puntarenas");
            Vertice heredia = new Vertice("Heredia");
            Vertice sanJose = new Vertice("San José");
            Vertice cartago = new Vertice("Cartago");
            Vertice guapiles = new Vertice("Guapiles");
            Vertice turrialba = new Vertice("Turrialba");
            Vertice paraiso = new Vertice("Paraíso");
            Vertice siquirres = new Vertice("Siquirres");
            Vertice limon = new Vertice("Limón");


            AgregarAristaNoDirigida(liberia, canias, 45);
            AgregarAristaNoDirigida(alajuela, orotina, 49);
            AgregarAristaNoDirigida(alajuela, puntarenas, 105);
            AgregarAristaNoDirigida(alajuela, heredia, 15);
            AgregarAristaNoDirigida(alajuela, sanJose, 20);
            AgregarAristaNoDirigida(sanJose, heredia, 35);
            AgregarAristaNoDirigida(sanJose, guapiles, 126);
            AgregarAristaNoDirigida(guapiles, siquirres, 40);
            AgregarAristaNoDirigida(siquirres, limon, 90);
            AgregarAristaNoDirigida(sanJose, cartago, 40);
            AgregarAristaNoDirigida(cartago, paraiso, 20);
            AgregarAristaNoDirigida(paraiso, turrialba, 60);
            AgregarAristaNoDirigida(limon, turrialba, 130);



            return alajuela;
            
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
