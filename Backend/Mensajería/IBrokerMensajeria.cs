using Backend.Mensajes;

// Contrato del intermediario de mensajes.
// El resto del sistema solo conoce esta interfaz; RabbitMqBroker es la
// implementación real. Así se podría cambiar de tecnología o usar un broker
// falso en las pruebas sin tocar la lógica de pedidos.
public interface IBrokerMensajeria
{
    bool EstaConectado { get; }

    // Conecta con el servidor y declara exchanges y colas
    Task ConectarAsync(CancellationToken cancelacion);

    // Envía un mensaje a una sede específica (punto a punto)
    Task PublicarAsync(Mensaje mensaje, string sedeDestino);

    // Envía un mensaje a todas las sedes (difusión)
    Task DifundirAsync(Mensaje mensaje);

    // Empieza a recibir mensajes; el manejador se ejecuta por cada uno
    Task SuscribirAsync(Func<Mensaje, Task> manejador, CancellationToken cancelacion);
}
