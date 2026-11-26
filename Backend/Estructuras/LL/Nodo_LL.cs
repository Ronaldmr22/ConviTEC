using Microsoft.VisualBasic;

public class Nodo
{
    private object? Data;
    private Nodo? Next;

    public Nodo(object data)
    {
        Data = data;
    }
    
    public object GetData()
    {
        return Data;
    }

    public Nodo GetNext()
    {
        return Next;
    }

    public void SetData(object objeto)
    {
        Data = objeto;
    }

    public void SetNext(Nodo? nodo)
    {
        Next = nodo;
    }
}