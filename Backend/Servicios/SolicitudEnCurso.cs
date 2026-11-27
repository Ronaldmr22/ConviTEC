using Backend.Mensajes;
using Backend.Pedidos;

// Lleva la cuenta de las respuestas que un pedido está esperando de otras sedes.
//
// Cada línea remota del pedido se envía como una SolicitudReserva con su propio
// CorrelationId; cuando llega la RespuestaReserva con ese mismo CorrelationId,
// se sabe a qué línea corresponde y se le asigna el Id de la reserva.
//
// "Tarea" termina en true cuando todas aceptaron, o en false apenas una rechaza.
public class SolicitudEnCurso
{
    private readonly string idPedido;
    private readonly LineaPedido[] lineas;
    private readonly string[] correlationIds;
    private readonly bool[] respondidas;
    private int pendientes;
    private bool cerrada;
    private string motivoRechazo;
    private readonly object candado = new object();

    // RunContinuationsAsynchronously: quien espera la tarea continúa en otro hilo,
    // no dentro del hilo que está procesando el mensaje de RabbitMQ.
    private readonly TaskCompletionSource<bool> completado =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    public SolicitudEnCurso(string idPedido, LineaPedido[] lineas)
    {
        if (lineas == null || lineas.Length == 0)
        {
            throw new ArgumentException("La solicitud necesita al menos una línea.");
        }

        this.idPedido = idPedido;
        this.lineas = lineas;
        this.correlationIds = new string[lineas.Length];
        this.respondidas = new bool[lineas.Length];
        this.pendientes = lineas.Length;
        this.cerrada = false;
        this.motivoRechazo = "";

        for (int i = 0; i < lineas.Length; i++)
        {
            correlationIds[i] = idPedido + "|" + i + "|" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }

    public string GetIdPedido() { return idPedido; }
    public int GetCantidadLineas() { return lineas.Length; }
    public LineaPedido GetLinea(int indice) { return lineas[indice]; }
    public string GetCorrelationId(int indice) { return correlationIds[indice]; }
    public Task<bool> Tarea { get { return completado.Task; } }

    public string GetMotivoRechazo()
    {
        lock (candado) { return motivoRechazo; }
    }

    public bool Contiene(string correlationId)
    {
        return BuscarIndice(correlationId) >= 0;
    }

    // Devuelve false si la respuesta llegó tarde (solicitud ya cerrada) o no
    // pertenece a esta solicitud. En ese caso, si era una reserva aceptada,
    // quien llama debe liberarla.
    public bool RegistrarRespuesta(RespuestaReserva respuesta)
    {
        lock (candado)
        {
            int indice = BuscarIndice(respuesta.GetCorrelationId());
            if (indice < 0 || cerrada)
            {
                return false;
            }

            if (respondidas[indice])
            {
                return true;   // respuesta repetida: ya se tomó en cuenta
            }

            respondidas[indice] = true;
            pendientes--;

            if (respuesta.GetAceptada())
            {
                lineas[indice].SetIdReserva(respuesta.GetIdReserva());
            }
            else
            {
                motivoRechazo = respuesta.GetMotivo();
                completado.TrySetResult(false);
            }

            if (pendientes == 0)
            {
                completado.TrySetResult(true);   // no hace nada si ya terminó en false
            }
            return true;
        }
    }

    // Marca que un envío falló (por ejemplo, sin conexión)
    public void MarcarFallo(string motivo)
    {
        lock (candado)
        {
            motivoRechazo = motivo;
            completado.TrySetResult(false);
        }
    }

    // A partir de aquí, las respuestas que lleguen se consideran tardías
    public void Cerrar()
    {
        lock (candado)
        {
            cerrada = true;
            completado.TrySetResult(false);
        }
    }

    private int BuscarIndice(string correlationId)
    {
        for (int i = 0; i < correlationIds.Length; i++)
        {
            if (correlationIds[i] == correlationId)
            {
                return i;
            }
        }
        return -1;
    }
}
