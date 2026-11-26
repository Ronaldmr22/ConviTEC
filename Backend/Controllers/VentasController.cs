using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly VentasService ventasService;

    public VentasController(VentasService ventasService)
    {
        this.ventasService = ventasService;
    }

    [HttpGet]
    public ActionResult<Venta[]> ObtenerHistorial()
    {
        return Ok(ventasService.ObtenerHistorial());
    }

    [HttpGet("estadisticas")]
    public ActionResult<EstadisticasVentas> ObtenerEstadisticas()
    {
        return Ok(ventasService.ObtenerEstadisticas());
    }
}