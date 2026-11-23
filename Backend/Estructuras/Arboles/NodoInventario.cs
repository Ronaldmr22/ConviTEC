using Backend.Models;
namespace Backend.Estructuras.Arboles;
public class NodoInventario
{
    private InventarioItem item;
    private NodoInventario? izquierdo;
    private NodoInventario? derecho;

    public NodoInventario(InventarioItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        this.item = item;
        this.izquierdo = null;
        this.derecho = null;
    }

    // Getters
    public InventarioItem GetItem()
    {
        return item;
    }

    public NodoInventario? GetIzquierdo()
    {
        return izquierdo;
    }

    public NodoInventario? GetDerecho()
    {
        return derecho;
    }

    // Setters
    public void SetIzquierdo(NodoInventario? nodo)
    {
        this.izquierdo = nodo;
    }

    public void SetDerecho(NodoInventario? nodo)
    {
        this.derecho = nodo;
    }
}
