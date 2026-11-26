using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

public record ProductoPedidoRequest(
    [Required] string CodigoProducto,
    [Range(1, int.MaxValue)] int Cantidad);

public record RegistrarPedidoRequest(
    [Required] string Destino,
    [Required, MinLength(1)] ProductoPedidoRequest[] Productos);

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    [HttpPost]
    public IActionResult Registrar([FromBody] RegistrarPedidoRequest solicitud)
    {
        // El servicio de reservas se conectará aquí cuando esté integrado.
        return StatusCode(503, new
        {
            aceptado = false,
            mensaje = "El registro de pedidos todavía no está disponible en esta sede."
        });
    }
}