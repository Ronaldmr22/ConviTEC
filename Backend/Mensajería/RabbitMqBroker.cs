using Backend.Mensajes;
using Backend.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Implementación del broker con RabbitMQ.
//
// Topología:
//  - Exchange "convitec.sedes" (direct): mensajes punto a punto. Cada sede tiene
//    la cola "convitec.sede.{CODIGO}" enlazada con su código como routing key.
//  - Exchange "convitec.inventario" (fanout): difusión a todas las sedes. Cada
//    sede tiene la cola "convitec.sync.{CODIGO}" enlazada a este exchange.
//
// Las colas son durables y los mensajes persistentes: si una sede está apagada,
// sus mensajes la esperan en la cola. El ack se hace después de procesar.
public class RabbitMqBroker : IBrokerMensajeria, IAsyncDisposable
{
    public const string ExchangeSedes = "convitec.sedes";
    public const string ExchangeInventario = "convitec.inventario";

    private readonly ConfiguracionRabbit configuracion;
    private readonly string codigoSede;
    private readonly ILogger<RabbitMqBroker> logger;

    private IConnection? conexion;
    private IChannel? canalPublicacion;
    private IChannel? canalConsumo;

    // Un canal de RabbitMQ no admite publicaciones simultáneas desde varios hilos
    private readonly SemaphoreSlim candadoPublicacion = new SemaphoreSlim(1, 1);

    public RabbitMqBroker(IOptions<ConfiguracionRabbit> configuracion, ConfiguracionSede sede, ILogger<RabbitMqBroker> logger)
    {
        this.configuracion = configuracion.Value;
        this.codigoSede = sede.GetCodigo();
        this.logger = logger;
    }

    public bool EstaConectado
    {
        get { return conexion != null && conexion.IsOpen && canalPublicacion != null && canalPublicacion.IsOpen; }
    }

    private string ColaDirecta
    {
        get { return "convitec.sede." + codigoSede; }
    }

    private string ColaSincronizacion
    {
        get { return "convitec.sync." + codigoSede; }
    }

    public async Task ConectarAsync(CancellationToken cancelacion)
    {
        if (EstaConectado)
        {
            return;
        }

        ConnectionFactory fabrica = new ConnectionFactory
        {
            HostName = configuracion.Host,
            Port = configuracion.Puerto,
            UserName = configuracion.Usuario,
            Password = configuracion.Clave,
            VirtualHost = configuracion.VirtualHost,
            ClientProvidedName = "ConviTEC-" + codigoSede,
            // Si se cae la red, el cliente se reconecta y vuelve a declarar todo
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        conexion = await fabrica.CreateConnectionAsync(cancelacion);
        canalPublicacion = await conexion.CreateChannelAsync(cancellationToken: cancelacion);
        canalConsumo = await conexion.CreateChannelAsync(cancellationToken: cancelacion);

        // Exchanges (durables: sobreviven a un reinicio del servidor)
        await canalPublicacion.ExchangeDeclareAsync(ExchangeSedes, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancelacion);
        await canalPublicacion.ExchangeDeclareAsync(ExchangeInventario, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancelacion);

        // Colas propias de esta sede
        await canalPublicacion.QueueDeclareAsync(ColaDirecta, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancelacion);
        await canalPublicacion.QueueBindAsync(ColaDirecta, ExchangeSedes, routingKey: codigoSede, cancellationToken: cancelacion);

        await canalPublicacion.QueueDeclareAsync(ColaSincronizacion, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancelacion);
        await canalPublicacion.QueueBindAsync(ColaSincronizacion, ExchangeInventario, routingKey: "", cancellationToken: cancelacion);

        // Procesar de a un mensaje a la vez por cola
        await canalConsumo.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancelacion);

        logger.LogInformation("Conectado a RabbitMQ en {Host}:{Puerto} como sede {Sede}", configuracion.Host, configuracion.Puerto, codigoSede);
    }

    public Task PublicarAsync(Mensaje mensaje, string sedeDestino)
    {
        if (string.IsNullOrWhiteSpace(sedeDestino))
        {
            throw new ArgumentException("La sede destino es obligatoria.");
        }
        return EnviarAsync(ExchangeSedes, sedeDestino, mensaje);
    }

    public Task DifundirAsync(Mensaje mensaje)
    {
        return EnviarAsync(ExchangeInventario, "", mensaje);
    }

    private async Task EnviarAsync(string exchange, string routingKey, Mensaje mensaje)
    {
        IChannel? canal = canalPublicacion;
        if (canal == null || !EstaConectado)
        {
            throw new InvalidOperationException("No hay conexión con RabbitMQ.");
        }

        byte[] cuerpo = SerializadorMensajes.Serializar(mensaje);

        BasicProperties propiedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = mensaje.GetId(),
            CorrelationId = mensaje.GetCorrelationId(),
            Type = mensaje.GetTipo()
        };

        await candadoPublicacion.WaitAsync();
        try
        {
            await canal.BasicPublishAsync(exchange, routingKey, mandatory: false, basicProperties: propiedades, body: cuerpo);
        }
        finally
        {
            candadoPublicacion.Release();
        }

        logger.LogDebug("Enviado {Tipo} {Id} a {Destino}", mensaje.GetTipo(), mensaje.GetId(), routingKey == "" ? "todas las sedes" : routingKey);
    }

    public async Task SuscribirAsync(Func<Mensaje, Task> manejador, CancellationToken cancelacion)
    {
        IChannel? canal = canalConsumo;
        if (canal == null)
        {
            throw new InvalidOperationException("Primero hay que conectar con RabbitMQ.");
        }

        await ConsumirColaAsync(canal, ColaDirecta, manejador, cancelacion);
        await ConsumirColaAsync(canal, ColaSincronizacion, manejador, cancelacion);
    }

    private async Task ConsumirColaAsync(IChannel canal, string cola, Func<Mensaje, Task> manejador, CancellationToken cancelacion)
    {
        AsyncEventingBasicConsumer consumidor = new AsyncEventingBasicConsumer(canal);

        consumidor.ReceivedAsync += async (origen, entrega) =>
        {
            Mensaje mensaje;
            try
            {
                mensaje = SerializadorMensajes.Deserializar(entrega.Body.ToArray());
            }
            catch (Exception error)
            {
                // Un mensaje mal formado nunca se va a poder leer: se descarta
                logger.LogWarning(error, "Mensaje inválido descartado en {Cola}", cola);
                await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                await manejador(mensaje);
                await canal.BasicAckAsync(entrega.DeliveryTag, multiple: false);
            }
            catch (Exception error)
            {
                // No se reencola para no repetir el error en un ciclo infinito
                logger.LogError(error, "Error procesando {Tipo} {Id}", mensaje.GetTipo(), mensaje.GetId());
                await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await canal.BasicConsumeAsync(cola, autoAck: false, consumer: consumidor, cancellationToken: cancelacion);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (canalConsumo != null) { await canalConsumo.CloseAsync(); }
            if (canalPublicacion != null) { await canalPublicacion.CloseAsync(); }
            if (conexion != null) { await conexion.CloseAsync(); }
        }
        catch (Exception error)
        {
            logger.LogDebug(error, "Error cerrando la conexión con RabbitMQ");
        }
    }
}
