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

    public Reserva(string idpedido, string codigoproducto, int cantidad, string sedesolicitante, int minutosExpiracion = 2)
    {
        this.Id = Guid.NewGuid().ToString();
        this.IdPedido = idpedido;
        this.CodigoProducto = codigoproducto;
        this.Cantidad = cantidad;
        this.SedeSolicitante = sedesolicitante;
        this.estado = EstadoReserva.Activa;
        this.FechaExpiracion = DateTime.Now.AddMinutes(minutosExpiracion);
    }

    // Constructor para reconstruir una reserva guardada en archivo
    public Reserva(string id, string idpedido, string codigoproducto, int cantidad, string sedesolicitante, DateTime fechaExpiracion, EstadoReserva estado)
    {
        this.Id = id;
        this.IdPedido = idpedido;
        this.CodigoProducto = codigoproducto;
        this.Cantidad = cantidad;
        this.SedeSolicitante = sedesolicitante;
        this.FechaExpiracion = fechaExpiracion;
        this.estado = estado;
    }

    public string GetId() {return Id;}
    public string GetIdPedido() {return IdPedido;}
    public string GetCodigoProducto() {return CodigoProducto;}
    public int GetCantidad() {return Cantidad;}
    public string GetSedeSolicitante() {return SedeSolicitante;}
    public DateTime GetFechaExpiracion() {return FechaExpiracion;}
    public EstadoReserva GetEstado() {return estado;}

    public bool EstaActiva()
    {
        return estado == EstadoReserva.Activa;
    }

    public bool EstaVencida()
    {
        return estado == EstadoReserva.Activa && DateTime.Now > FechaExpiracion;
    }

    public void Confirmar() {CambiarEstado(EstadoReserva.Confirmada);}
    public void Liberar() {CambiarEstado(EstadoReserva.Liberada);}
    public void MarcarVencida() {CambiarEstado(EstadoReserva.Vencida);}

    // Solo una reserva activa puede cambiar de estado
    private void CambiarEstado(EstadoReserva nuevo)
    {
        if (estado != EstadoReserva.Activa)
        {
            throw new InvalidOperationException($"La reserva ya está {estado}");
        }
        estado = nuevo;
    }
}
}
