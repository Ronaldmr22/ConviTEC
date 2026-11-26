namespace Backend.Pedidos{
public class Pedido
{
    private string Id;
    private DateTime FechaCreación;
    private EstadoPedido Estado;
    private string SedeOrigen;
    private string Destino;
    private ListaEnlazada Lineas;
    private string MotivoRechazo;

    public Pedido(string sedeOrigen, string destino)
    {
        this.Id = sedeOrigen + "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        this.FechaCreación = DateTime.Now;
        this.Estado = EstadoPedido.Registrado;
        this.SedeOrigen = sedeOrigen;
        this.Destino = destino;
        this.Lineas = new ListaEnlazada();
        this.MotivoRechazo = "";
    }

    public string GetId() {return Id;}
    public DateTime GetFechaCreación() {return FechaCreación;}
    public EstadoPedido GetEstado() {return Estado;}
    public string GetSedeOrigen() {return SedeOrigen;}
    public string GetDestino() {return Destino;}
    public ListaEnlazada GetLineas() {return Lineas;}
    public string GetMotivoRechazo() {return MotivoRechazo;}

    public void SetID(string id) {Id = id;}
    public void SetEstado(EstadoPedido estado) {Estado = estado;}
    public void SetMotivoRechazo(string motivoRechazo) {MotivoRechazo = motivoRechazo;}


    public void AgregarLinea(LineaPedido linea)
    {
        Lineas.AgregarDato(linea);
    }

    public decimal CalcularTotal()
    {
        decimal total = 0;
        Lineas.Recorrer(dato => total += ((LineaPedido)dato).Subtotal());
        return total;
    }

    public ListaEnlazada ObtenerSedesParticipantes()
    {
        ListaEnlazada sedes = new ListaEnlazada();
        Lineas.Recorrer(dato =>
        {
            string sede = ((LineaPedido)dato).GetSedeProveedora();
            if (sedes.EncontrarDato(s => (string)s == sede) == null)
            {
            sedes.AgregarDato(sede);
            }
        });
        return sedes;
    }

    public void CambiarEstado(EstadoPedido Nuevo)
    {
        if (Estado == EstadoPedido.Entregado || Estado == EstadoPedido.Cancelado || Estado == EstadoPedido.Rechazado)
        {
            throw new InvalidOperationException($"El pedido ya está {Estado}");
        }
        Estado = Nuevo;
    }
}
}