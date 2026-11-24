using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Persistencia;

namespace Backend.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly InventarioService inventarioService;

    public ProductosController(InventarioService inventarioService)
    {
        this.inventarioService = inventarioService;
    }

    // Consultar todos los productos del catálogo local
    [HttpGet]
    public ActionResult<RegistroInventarioJson[]> ObtenerProductos()
    {
        return Ok(inventarioService.ObtenerProductos());
    }

    // Buscar un producto por su código
    [HttpGet("{codigo}")]
    public ActionResult<RegistroInventarioJson> BuscarProducto(
        string codigo)
    {
        RegistroInventarioJson? producto =
            inventarioService.BuscarProducto(codigo);

        if (producto == null)
        {
            return NotFound("Producto no encontrado.");
        }

        return Ok(producto);
    }
}
