using System.Drawing;
using Backend.Estructuras.Arboles;

public class ListaEnlazada
{
    private Nodo_LL? Head;
    private Nodo_LL? Tail;
    private int Size = 0;

    public Nodo_LL GetHead()
    {
        return Head;
    }

    public Nodo_LL GetTail()
    {
        return Tail;
    }

    public int GetSize()
    {
        return Size;
    }

    public void SetHead(Nodo_LL nodo)
    {
        Head = nodo;
    }

    public void SetTail(Nodo_LL nodo)
    {
        Tail = nodo;
    }

    public object EncontrarDato(Func<object, bool> criterio)
    {
        for (Nodo_LL i = Head; i != null; i = i.GetNext())
        {
            if (criterio(i.GetData()))
            {
                return i.GetData();
            }
        }
        return null;
    }

    public void AgregarDato(object data)
    {
        Nodo_LL nuevo = new Nodo_LL(data);
        if (Size == 0)
        {
            Head = nuevo;
            Tail = nuevo;
            Size++;
        }
        else
        {
            Tail.SetNext(nuevo);
            Tail = nuevo;
            Size++;
        }
    }

    public bool EliminarDato(Func<object, bool> criterio)
    {
        if (Head == null) return false;
        if (criterio(Head.GetData()))
        {
            Head = Head.GetNext();
            if (Head == null)
            {
                Tail = null;
            }
            Size--;
            return true;
        }
        for (Nodo_LL i = Head; i != Tail; i = i.GetNext())
        {
            if (criterio(i.GetNext().GetData()))
            {
                if (i.GetNext() == Tail)
                {
                    Tail = i;
                    i.SetNext(null);
                }
                else
                {
                    i.SetNext(i.GetNext().GetNext());
                }
                Size--;
                return true;
            }
        }
        return false;
    }

    public void Recorrer(Action<object> accion)
    {
        for (Nodo_LL i = Head; i != null; i = i.GetNext())
        {
            accion(i.GetData());
        }
    }
}