using System;
using System.Collections.Generic;
using System.Text;

namespace Grafo
{
    public class Grafo
    {
        public List<Vertice> Vertices { get; set; }
        public Grafo()
        {
            this.Vertices = new List<Vertice>();
        }
    }
    public class Vertice
    {
        public String Nombre { get; set; }
        public List<Arista> Aristas { get; set; }
        public int Indice { get; set; }
        public String Tipo { get; set; }

        public Vertice(String nombre, int indice, String tipo)
        {
            this.Nombre = nombre;
            this.Indice = indice;
            this.Tipo = tipo;

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
    public class GrafoJsonDto
    {
        public List<VerticeJsonDto> Vertices { get; set; } = new();
        public List<AristaJsonDto> Aristas { get; set; } = new();
    }

    public class VerticeJsonDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Indice { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }

    public class AristaJsonDto
    {
        public int OrigenIndice { get; set; }
        public int DestinoIndice { get; set; }
        public int Peso { get; set; }
        public bool NoDirigida { get; set; } = true;
    }
}
