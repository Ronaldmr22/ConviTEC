using System;
using System.Collections.Generic;
using System.Text;

namespace Grafo
{
    public class Vertice
    {
        public int Valor { get; set; }
        public List<Arista> Aristas { get; set; }

        public Vertice(int Valor)
        {
            this.Valor = Valor;
            this.Aristas = new List<Arista>();
        }
    }
    public class Arista
    {
        public Vertice Vertice1 { get; set; }
        public Vertice Vertice2 { get; set; }
        public int Peso { get; set; }
        public Arista(Vertice vertice1, Vertice vertice2, int peso)
        {
            this.Vertice1 = vertice1;
            this.Vertice2 = vertice2;
            this.Peso = peso;
        }
    }
}
