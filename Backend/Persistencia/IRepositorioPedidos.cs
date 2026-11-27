using Backend.Pedidos;

// Persistencia de los pedidos de la sede (archivo local).
public interface IRepositorioPedidos
{
    // Guarda el pedido; si ya existía uno con el mismo Id, lo reemplaza
    void Guardar(Pedido pedido);

    // Lee todos los pedidos guardados (lista vacía si no hay archivo)
    ListaEnlazada CargarTodos();
}
