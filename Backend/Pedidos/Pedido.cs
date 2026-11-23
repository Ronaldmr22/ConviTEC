namespace Backend.Pedidos{
public class Pedido
{
    private string Id;
    private DateTime FechaCreación;
    private EstadoPedido estado;
    private string SedeOrigen;
    private string Destino;
    private ListaEnlazada lineas;
    private string MotivoRechazo;

    public Pedido(string sedeOrigen, string destino)
    {
        this.Id = SedeOrigen + "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        this.FechaCreación = DateTime.Now;
        this.estado = EstadoPedido.Registrado;
        this.SedeOrigen = sedeOrigen;
        this.Destino = destino;
        this.lineas = new ListaEnlazada();
        this.MotivoRechazo = "";
    }

    public void AgregarLinea(LineaPedido linea)
    {
        lineas.AgregarDato(linea);
    }

    public decimal CalcularTotal()
    {
        decimal total = 0;
        lineas.Recorrer(dato => total += ((LineaPedido)dato).Subtotal());
        return total;
    }

    public ListaEnlazada ObtenerSedesParticipantes()
    {
        ListaEnlazada sedes = new ListaEnlazada();
        lineas.Recorrer(dato =>
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
        if (estado == EstadoPedido.Entregado || estado == EstadoPedido.Cancelado || estado == EstadoPedido.Rechazado)
        {
            throw new InvalidOperationException($"El pedido ya está {estado}");
        }
        estado = Nuevo;
    }
}
}