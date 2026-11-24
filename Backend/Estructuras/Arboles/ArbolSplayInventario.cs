using Backend.Models;
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
    public bool Insertar(InventarioItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        string codigo = item.GetProducto().GetCodigo();

        // Caso 1: El árbol está vacío
        if (raiz == null)
        {
            raiz = new NodoInventario(item);
            return true;
        }

        // Reorganizar el árbol según el código
        raiz = Splay(raiz, codigo);

        string codigoRaiz = raiz!.GetItem().GetProducto().GetCodigo();

        int comparacion = string.Compare(
            codigo,
            codigoRaiz,
            StringComparison.Ordinal
        );

        // Caso 2: El producto ya existe
        if (comparacion == 0)
        {
            return false;
        }

        // Crear el nuevo nodo
        NodoInventario nuevo = new NodoInventario(item);

        // Caso 3: El código es menor
        if (comparacion < 0)
        {
            nuevo.SetIzquierdo(raiz.GetIzquierdo());
            nuevo.SetDerecho(raiz);
            raiz.SetIzquierdo(null);
        }
        // Caso 4: El código es mayor
        else
        {
            nuevo.SetDerecho(raiz.GetDerecho());
            nuevo.SetIzquierdo(raiz);
            raiz.SetDerecho(null);
        }

        raiz = nuevo;

        return true;
    }

    public InventarioItem? Buscar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código no puede estar vacío.");
        }

        if (raiz == null)
        {
            return null;
        }

        // Reorganizar el árbol
        raiz = Splay(raiz, codigo);

        string codigoRaiz = raiz!.GetItem().GetProducto().GetCodigo();

        // Verificar si encontramos el producto
        if (string.Equals(
            codigo,
            codigoRaiz,
            StringComparison.Ordinal))
        {
            return raiz.GetItem();
        }

        return null;
    }

    public InventarioItem[] RecorrerEnOrden()
    {
        InventarioItem[] productos = new InventarioItem[ContarNodos(raiz)];

        int indice = 0;

        RecorrerEnOrdenRecursivo(raiz, productos, ref indice);

        return productos;
    }

    private int ContarNodos(NodoInventario? nodo)
    {
        if (nodo == null)
        {
            return 0;
        }

        return 1 + ContarNodos(nodo.GetIzquierdo()) + ContarNodos(nodo.GetDerecho());
    }

    private void RecorrerEnOrdenRecursivo(
        NodoInventario? nodo,
        InventarioItem[] productos,
        ref int indice)
    {
        if (nodo == null)
        {
            return;
        }

        RecorrerEnOrdenRecursivo(nodo.GetIzquierdo(), productos, ref indice);

        productos[indice] = nodo.GetItem();
        indice++;

        RecorrerEnOrdenRecursivo(nodo.GetDerecho(), productos, ref indice);
    }
}
