using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Persistencia;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly InventarioService inventarioService;

    public InventarioController(
        InventarioService inventarioService)
    {
        this.inventarioService = inventarioService;
    }

    // Consultar las existencias locales
    [HttpGet]
    public ActionResult<RegistroInventarioJson[]> ObtenerInventario()
    {
        return Ok(inventarioService.ObtenerProductos());
    }

    // Incorporar unidades al inventario local
    [HttpPatch("{codigo}/agregar")]
    public IActionResult AgregarExistencias(
        string codigo,
        [FromBody] CantidadRequest? solicitud)
    {
        if (solicitud == null || solicitud.Cantidad <= 0)
        {
            return BadRequest("La cantidad debe ser positiva.");
        }

        try
        {
            bool resultado = inventarioService.AgregarExistencias(
                codigo,
                solicitud.Cantidad
            );

            if (!resultado)
            {
                return NotFound("Producto no encontrado.");
            }

            return Ok(inventarioService.BuscarProducto(codigo));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (OverflowException)
        {
            return BadRequest("La cantidad supera el limite permitido.");
        }
    }

    // Descontar unidades del inventario local
    [HttpPatch("{codigo}/descontar")]
    public IActionResult DescontarExistencias(
        string codigo,
        [FromBody] CantidadRequest? solicitud)
    {
        if (solicitud == null || solicitud.Cantidad <= 0)
        {
            return BadRequest("La cantidad debe ser positiva.");
        }

        try
        {
            bool resultado = inventarioService.DescontarExistencias(
                codigo,
                solicitud.Cantidad
            );

            if (!resultado)
            {
                return NotFound("Producto no encontrado.");
            }

            return Ok(inventarioService.BuscarProducto(codigo));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (OverflowException)
        {
            return BadRequest(
                "La cantidad supera el limite permitido."
            );
        }
    }
}