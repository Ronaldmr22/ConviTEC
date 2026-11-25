using Backend.Models;
using Backend.Pedidos;

public class ServicioPedidos
{
    private IPlanificadorRutas Planificador;
    private CoordinadorReservas Coordinador;
    private IRepositorioPedidos Repositorio;
    private Cola Pendientes;
    private ListaEnlazada Historial;
    private InventarioGlobal InventarioGlobal;

    public ServicioPedidos(IPlanificadorRutas planificador, CoordinadorReservas coordinador, IRepositorioPedidos repositorio, InventarioGlobal inventarioGlobal)
    {
        this.Planificador = planificador;
        this.Coordinador = coordinador;
        this.Repositorio = repositorio;
        this.Pendientes = new Cola();
        this.Historial = new ListaEnlazada();
        this.InventarioGlobal = inventarioGlobal;
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
            Coordinador.LiberarPedido(pedido);
            return Rechazar(pedido, EstadoPedido.Cancelado, "Una sede rechazó la reserva o no respondió a tiempo");
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
        ListaEnlazada existencias = InventarioGlobal.GetExistencias();
        for (Nodo i = lista.GetHead(); i != null; i = i.GetNext())
        {
            if (InventarioLocal.Disponible(((LineaPedido)i.GetData()).GetProducto().GetCodigo()) >= ((LineaPedido)i.GetData()).GetCantidad())
            {
                ((LineaPedido)i.GetData()).SetSedeProveedora(pedido.GetSedeOrigen());
                continue;
            }

            object proveedora = existencias.EncontrarDato(data => 
            ((ExistenciaRemota)data).GetCodigoProducto() == ((LineaPedido)i.GetData()).GetProducto().GetCodigo() && 
            ((ExistenciaRemota)data).GetDisponible() >= ((LineaPedido)i.GetData()).GetCantidad());
            if (proveedora == null)
            {
                return ((LineaPedido)i.GetData()).GetProducto().GetNombre();
            }
            ((LineaPedido)i.GetData()).SetSedeProveedora(((ExistenciaRemota)proveedora).GetCodigoSede());
        }
        return null;
    }
}