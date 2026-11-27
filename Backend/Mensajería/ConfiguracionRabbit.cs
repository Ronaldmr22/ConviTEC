// Datos de conexión con el servidor RabbitMQ.
// Se llenan desde la sección "RabbitMQ" de appsettings.json o desde variables
// de entorno (por ejemplo RabbitMQ__Host=192.168.1.50, RabbitMQ__Clave=...).
// Si no se configura nada, se usan los valores por defecto de abajo.
public class ConfiguracionRabbit
{
    public string Host { get; set; } = "localhost";
    public int Puerto { get; set; } = 5672;
    public string Usuario { get; set; } = "guest";
    public string Clave { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    // Segundos que el coordinador espera las respuestas de reserva
    public int TimeoutReservaSegundos { get; set; } = 10;

    // Minutos que una reserva puede quedar activa sin confirmarse
    public int MinutosExpiracionReserva { get; set; } = 2;
}
