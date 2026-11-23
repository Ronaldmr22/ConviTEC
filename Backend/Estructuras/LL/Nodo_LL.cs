using Microsoft.VisualBasic;

public class Nodo_LL
{
    private object? Data;
    private Nodo_LL? Next;

    public Nodo_LL(object data)
    {
        Data = data;
    }
    
    public object GetData()
    {
        return Data;
    }

    public Nodo_LL GetNext()
    {
        return Next;
    }

    public void SetData(object objeto)
    {
        Data = objeto;
    }

    public void SetNext(Nodo_LL? nodo)
    {
        Next = nodo;
    }
}