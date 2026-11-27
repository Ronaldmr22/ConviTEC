namespace Backend.Persistencia;

// Forma en que se guarda una reserva en reservas.json
public class RegistroReservaJson
{
    public string Id { get; set; } = "";
    public string IdPedido { get; set; } = "";
    public string CodigoProducto { get; set; } = "";
    public int Cantidad { get; set; }
    public string SedeSolicitante { get; set; } = "";
    public DateTime FechaExpiracion { get; set; }
    public string Estado { get; set; } = "";
}
