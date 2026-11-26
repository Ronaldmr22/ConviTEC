namespace Backend.Models;

public record ProductoEstrella(
    string CodigoProducto,
    string NombreProducto,
    decimal Ingresos);

public record RendimientoPedido(
    string IdPedido,
    decimal MontoTotal,
    int MinutosTraslado,
    decimal ColonesPorMinuto);

public record EstadisticasVentas(
    decimal IngresosTotales,
    ProductoEstrella? ProductoEstrella,
    RendimientoPedido[] PedidosPorRendimiento);