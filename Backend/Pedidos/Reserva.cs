namespace Backend.Pedidos{
public class Reserva
{
    private string Id;
    private string IdPedido;
    private string CodigoProducto;
    private int Cantidad;
    private string SedeSolicitante;
    private DateTime FechaExpiracion;
    private EstadoReserva estado;

    public Reserva(string idpedido, string codigoproducto, int cantidad, string sedesolicitante)
    {
        this.Id = Guid.NewGuid().ToString();
        this.IdPedido = idpedido;
        this.CodigoProducto = codigoproducto;
        this.Cantidad = cantidad;
        this.SedeSolicitante = sedesolicitante;
        this.estado = EstadoReserva.Activa;
        this.FechaExpiracion = DateTime.Now.AddMinutes(2);
    }

    public bool EstaVencida()
        {
            return estado == EstadoReserva.Activa && DateTime.Now > FechaExpiracion;
        }
}
}