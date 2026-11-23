namespace Backend.Estructuras.Arboles;
public class ArbolSplayInventario
{
    private NodoInventario? raiz;

    public ArbolSplayInventario()
    {
        this.raiz = null;
    }

    public NodoInventario? GetRaiz()
    {
        return raiz;
    }

    // Rotación hacia la derecha
    private NodoInventario RotarDerecha(NodoInventario nodo)
    {
        NodoInventario? izquierdo = nodo.GetIzquierdo();

        if (izquierdo == null)
        {
            return nodo;
        }

        nodo.SetIzquierdo(izquierdo.GetDerecho());
        izquierdo.SetDerecho(nodo);

        return izquierdo;
    }

    // Rotación hacia la izquierda
    private NodoInventario RotarIzquierda(NodoInventario nodo)
    {
        NodoInventario? derecho = nodo.GetDerecho();

        if (derecho == null)
        {
            return nodo;
        }

        nodo.SetDerecho(derecho.GetIzquierdo());
        derecho.SetIzquierdo(nodo);

        return derecho;
    }
    private NodoInventario? Splay(NodoInventario? nodo, string codigo)
    {
        if (nodo == null)
        {
            return null;
        }

        string codigoActual = nodo.GetItem().GetProducto().GetCodigo();

        int comparacion = string.Compare(codigo, codigoActual, StringComparison.Ordinal);

        if (comparacion == 0)
        {
            return nodo;
        }

        // Buscar en el subárbol izquierdo
        if (comparacion < 0)
        {
            NodoInventario? izquierdo = nodo.GetIzquierdo();

            if (izquierdo == null)
            {
                return nodo;
            }

            string codigoIzquierdo = izquierdo.GetItem().GetProducto().GetCodigo();

            int comparacionHijo = string.Compare(
                codigo,
                codigoIzquierdo,
                StringComparison.Ordinal
            );

            if (comparacionHijo < 0)
            {
                // Zig-Zig izquierdo
                izquierdo.SetIzquierdo(Splay(izquierdo.GetIzquierdo(), codigo));
                nodo = RotarDerecha(nodo);
            }
            else if (comparacionHijo > 0)
            {
                // Zig-Zag izquierdo
                izquierdo.SetDerecho(Splay(izquierdo.GetDerecho(), codigo));

                if (izquierdo.GetDerecho() != null)
                {
                    nodo.SetIzquierdo(RotarIzquierda(izquierdo));
                }
            }

            if (nodo.GetIzquierdo() == null)
            {
                return nodo;
            }
            return RotarDerecha(nodo);
        }

        // Buscar en el subárbol derecho
        NodoInventario? derecho = nodo.GetDerecho();

        if (derecho == null)
        {
            return nodo;
        }

        string codigoDerecho = derecho.GetItem().GetProducto().GetCodigo();
        int comparacionDerecha = string.Compare(
            codigo,
            codigoDerecho,
            StringComparison.Ordinal
        );

        if (comparacionDerecha > 0)
        {
            // Zig-Zig derecho
            derecho.SetDerecho(Splay(derecho.GetDerecho(), codigo));
            nodo = RotarIzquierda(nodo);
        }
        else if (comparacionDerecha < 0)
        {
            // Zig-Zag derecho
            derecho.SetIzquierdo(Splay(derecho.GetIzquierdo(), codigo)
            );

            if (derecho.GetIzquierdo() != null)
            {
                nodo.SetDerecho(RotarDerecha(derecho));
            }
        }
        if (nodo.GetDerecho() == null)
        {
            return nodo;
        }
        return RotarIzquierda(nodo);
    }
}
