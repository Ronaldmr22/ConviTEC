public class ResultadoPedido
{
    private bool aceptado;
    private string IdPedido;
    private string Mensaje;

    private ResultadoPedido(bool aceptado, string Idpedido, string Mensaje)
    {
        this.aceptado = aceptado;
        this.IdPedido = Idpedido;
        this.Mensaje = Mensaje;
    }

    public static ResultadoPedido Aceptado(string idPedido, string mensaje)
    {
        return new ResultadoPedido(true, idPedido, mensaje);
    }

    public static ResultadoPedido Rechazado(string idPedido, string mensaje)
    {
        return new ResultadoPedido(false, idPedido, mensaje);
    }

    public bool GetAceptado() {return aceptado;}
    public string GetIdPedido() {return IdPedido;}
    public string GetMensaje() {return Mensaje;}
}