using Backend.Pedidos;

public class CoordinadorReservas
{
    private IBrokerMensajeria Broker;
    private GestorReservasLocales GestorLocal;
    private ListaEnlazada EnCurso;
    private int TimeoutSegundos;

    public bool ReservarPedido(Pedido pedido)
    {
        return;    
    }

    public void ConfirmarPedido(Pedido pedido)
    {
        
    }

    public void LiberarPedido(Pedido pedido)
    {
        
    }

    public void RecibirRespuesta(RespuestaReserva respuesta)
    {
        
    }
}