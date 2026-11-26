
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Servicios;

namespace Backend.Controllers;

[ApiController]
[Route("api/inventario-global")]
public class InventarioGlobalController : ControllerBase
{
    private readonly InventarioGlobalService inventarioGlobalService;

    public InventarioGlobalController(
        InventarioGlobalService inventarioGlobalService)
    {
        this.inventarioGlobalService = inventarioGlobalService;
    }

    // Consultar todos los registros globales conocidos
    [HttpGet]
    public ActionResult<RegistroExistenciaGlobal[]> ObtenerInventarioGlobal()
    {
        RegistroExistenciaGlobal[] registros =
            inventarioGlobalService.ObtenerRegistros();

        return Ok(ConvertirRegistros(registros));
    }

    // Consultar las existencias de un producto en todas las sedes conocidas
    [HttpGet("{codigoProducto}")]
    public ActionResult<RegistroGlobalRespuesta[]> BuscarProductoGlobal(
        string codigoProducto)
    {
        RegistroExistenciaGlobal[] registros =
            inventarioGlobalService.ObtenerRegistros();

        int cantidad = 0;

        // Contar cuantos registros corresponden al producto
        for (int i = 0; i < registros.Length; i++)
        {
            if (registros[i].GetCodigoProducto() == codigoProducto)
            {
                cantidad++;
            }
        }

        if (cantidad == 0)
        {
            return NotFound("No se encontraron registros del producto.");
        }

        RegistroGlobalRespuesta[] resultado = new RegistroGlobalRespuesta[cantidad];

        int indice = 0;

        for (int i = 0; i < registros.Length; i++)
        {
            if (registros[i].GetCodigoProducto() == codigoProducto)
            {
                resultado[indice] = ConvertirRegistro(registros[i]);
                indice++;
            }
        }

        return Ok(resultado);
    }

    // Consultar un producto en una sede especifica
    [HttpGet("{codigoProducto}/{codigoSede}")]
    public ActionResult<RegistroGlobalRespuesta> BuscarProductoPorSede(
        string codigoProducto,
        string codigoSede)
    {
        RegistroExistenciaGlobal? registro =
            inventarioGlobalService.Buscar(
                codigoProducto,
                codigoSede
            );

        if (registro == null)
        {
            return NotFound("No se encontro el registro solicitado.");
        }

        return Ok(ConvertirRegistro(registro));
    }

    // Convertir un registro interno a una respuesta HTTP
    private RegistroGlobalRespuesta ConvertirRegistro(RegistroExistenciaGlobal registro)
    {
        return new RegistroGlobalRespuesta(
            registro.GetCodigoProducto(),
            registro.GetCodigoSede(),
            registro.GetCantidad(),
            registro.GetCantidadReservada(),
            registro.GetVersion()
        );
    }

    // Convertir un arreglo de registros
    private RegistroGlobalRespuesta[] ConvertirRegistros(
        RegistroExistenciaGlobal[] registros)
    {
        RegistroGlobalRespuesta[] resultado = new RegistroGlobalRespuesta[registros.Length];

        for (int i = 0; i < registros.Length; i++)
        {
            resultado[i] = ConvertirRegistro(registros[i]);
        }

        return resultado;
    }
}
