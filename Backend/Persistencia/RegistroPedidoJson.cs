namespace Backend.Persistencia;

// Forma en que se guarda un pedido en pedidos.json
public class RegistroPedidoJson
{
    public string Id { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = "";
    public string SedeOrigen { get; set; } = "";
    public string Destino { get; set; } = "";
    public string MotivoRechazo { get; set; } = "";
    public RegistroLineaPedidoJson[] Lineas { get; set; } = new RegistroLineaPedidoJson[0];
}

// Forma en que se guarda cada línea dentro del pedido
public class RegistroLineaPedidoJson
{
    public string CodigoProducto { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public string? SedeProveedora { get; set; }
    public string? IdReserva { get; set; }
}
