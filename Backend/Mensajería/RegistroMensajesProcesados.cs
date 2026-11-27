// Recuerda los Id de los últimos mensajes procesados para ignorar duplicados.
// RabbitMQ puede entregar dos veces el mismo mensaje (por ejemplo, si la sede
// se cae antes de confirmar el ack), y un "ConfirmarReserva" repetido no debe
// descontar stock dos veces.
//
// Se usa un arreglo circular de tamaño fijo: cuando se llena, el Id más viejo
// se sobrescribe. Con 1000 posiciones alcanza de sobra para este proyecto.
public class RegistroMensajesProcesados
{
    private readonly string?[] ids;
    private int siguiente;
    private int cantidad;
    private readonly object candado = new object();

    public RegistroMensajesProcesados() : this(1000)
    {
    }

    public RegistroMensajesProcesados(int capacidad)
    {
        if (capacidad <= 0)
        {
            throw new ArgumentException("La capacidad debe ser mayor que cero.");
        }
        ids = new string?[capacidad];
        siguiente = 0;
        cantidad = 0;
    }

    public bool YaProcesado(string id)
    {
        lock (candado)
        {
            for (int i = 0; i < cantidad; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public void Marcar(string id)
    {
        lock (candado)
        {
            ids[siguiente] = id;
            siguiente = (siguiente + 1) % ids.Length;
            if (cantidad < ids.Length)
            {
                cantidad++;
            }
        }
    }
}
