public class ResultadoPedido
{
    private bool aceptado;
    private string IdPedido;
    private string Mensaje;

    public static ResultadoPedido Aceptado(string idPedido, string mensaje)
    {
        
    }

    public static ResultadoPedido Rechazado(string idPedido, string mensaje)
    {
        
    }

    public bool GetAceptado() {return aceptado;}
    public string GetIdPedido() {return IdPedido;}
    public string GetMensaje() {return Mensaje;}
}