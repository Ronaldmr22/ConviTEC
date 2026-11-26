using System.Drawing;
using Backend.Estructuras.Arboles;

public class Cola
{
    private Nodo? Head;
    private Nodo? Tail;
    private int Size = 0;

    public Nodo GetHead()
    {
        return Head;
    }

    public Nodo GetTail()
    {
        return Tail;
    }

    public int GetSize()
    {
        return Size;
    }

    public void SetHead(Nodo nodo)
    {
        Head = nodo;
    }

    public void SetTail(Nodo nodo)
    {
        Tail = nodo;
    }

    public void Enqueue(object data)
    {
        Nodo nuevo = new Nodo(data);
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

    public object Dequeue()
    {
        if (Head == null) {return null;}
        object dato = Head.GetData();
        Head = Head.GetNext();
        if (Head == null) {Tail = null;}
        Size--;
        return dato;
    }
}