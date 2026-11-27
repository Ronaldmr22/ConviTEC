using Backend.Models;
using Backend.Pedidos;
using Backend.Servicios;
using Backend.Persistencia;

public class ServicioPedidos
{
    private IPlanificadorRutas Planificador;
    private CoordinadorReservas Coordinador;
    private IRepositorioPedidos Repositorio;
    private Cola Pendientes;
    private ListaEnlazada Historial;
    private InventarioGlobal InventarioGlobal;
    private InventarioService InventarioLocal;

    public ServicioPedidos(IPlanificadorRutas planificador, CoordinadorReservas coordinador, IRepositorioPedidos repositorio, InventarioGlobal inventarioGlobal, InventarioService inventarioLocal)
    {
        this.Planificador = planificador;
        this.Coordinador = coordinador;
        this.Repositorio = repositorio;
        this.Pendientes = new Cola();
        this.Historial = new ListaEnlazada();
        this.InventarioGlobal = inventarioGlobal;
        this.InventarioLocal = inventarioLocal;

        CargarDesdeArchivo();
    }


    public async Task<ResultadoPedido> RegistrarPedido(Pedido pedido)
    {
        if (pedido == null || pedido.GetLineas().GetSize() == 0)
        {
            return ResultadoPedido.Rechazado("", "El pedido no tiene productos");
        }

        if (!Planificador.ExisteRuta(pedido.GetSedeOrigen(), pedido.GetDestino()))
        {
            return Rechazar(pedido, EstadoPedido.Rechazado, $"No existe ruta desde {pedido.GetSedeOrigen()} hasta {pedido.GetDestino()}");
        }

        string? sinStock = AsignarProveedores(pedido);
        if (sinStock != null)
        {
            return Rechazar(pedido, EstadoPedido.Rechazado, $"No hay existencias de {sinStock}");
        }

        pedido.CambiarEstado(EstadoPedido.EnReserva);
        bool reservado = await Coordinador.ReservarPedido(pedido);

        if (!reservado)
        {
            await Coordinador.LiberarPedido(pedido);
            string motivo = pedido.GetMotivoRechazo() != "" ? pedido.GetMotivoRechazo() : "Una sede rechazó la reserva o no respondió a tiempo";
            return Rechazar(pedido, EstadoPedido.Cancelado, motivo);
        }

        await Coordinador.ConfirmarPedido(pedido);
        pedido.CambiarEstado(EstadoPedido.Confirmado);
        Repositorio.Guardar(pedido);
        Historial.AgregarDato(pedido);
        Pendientes.Enqueue(pedido);

        return ResultadoPedido.Aceptado(pedido.GetId(), "Pedido Confirmado");
    }

    private ResultadoPedido Rechazar(Pedido pedido, EstadoPedido estado, string motivo)
    {
        pedido.CambiarEstado(estado);
        pedido.SetMotivoRechazo(motivo);
        Repositorio.Guardar(pedido);
        Historial.AgregarDato(pedido);
        return ResultadoPedido.Rechazado(pedido.GetId(), motivo);
    }

    private void CargarDesdeArchivo()
    {
        Historial = Repositorio.CargarTodos();
        Historial.Recorrer(dato =>
        {
            Pedido p = (Pedido)dato;
            if (p.GetEstado() == EstadoPedido.Confirmado)
            {
                Pendientes.Enqueue(p);
            }
        });
    }

    public ListaEnlazada ObtenerHistorial()
    {
        return Historial;
    }

    public Cola ObtenerPendientes()
    {
        return Pendientes;
    }

    private string? AsignarProveedores(Pedido pedido)
    {
        ListaEnlazada lista = pedido.GetLineas();

        RegistroExistenciaGlobal[] existencias = InventarioGlobal.ObtenerRegistros();

        for (Nodo i = lista.GetHead(); i != null; i = i.GetNext())
        {
            LineaPedido linea = (LineaPedido)i.GetData();

            string codigoProducto = linea.GetProducto().GetCodigo();
            int cantidad = linea.GetCantidad();

            // Primero buscar en el inventario local
            RegistroInventarioJson? productoLocal = InventarioLocal.BuscarProducto(codigoProducto);

            if (productoLocal != null && productoLocal.Cantidad - productoLocal.CantidadReservada >= cantidad)
            {
                linea.SetSedeProveedora(pedido.GetSedeOrigen());
                continue;
            }

            // Si no alcanza, buscar en el inventario global
            RegistroExistenciaGlobal? proveedora = null;

            for (int j = 0; j < existencias.Length; j++)
            {
                if (existencias[j].GetCodigoProducto() == codigoProducto &&
                    existencias[j].GetCodigoSede() != pedido.GetSedeOrigen() &&
                    existencias[j].GetCantidadDisponible() >= cantidad)
                {
                    proveedora = existencias[j];
                    break;
                }
            }

            if (proveedora == null)
            {
                return linea.GetProducto().GetNombre();
            }

            linea.SetSedeProveedora(proveedora.GetCodigoSede());
        }

        return null;
    }

}