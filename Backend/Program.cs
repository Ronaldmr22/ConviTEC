var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ── Configuración (no es AddSingleton, pero también hay una sola) ──
builder.Services.Configure<ConfiguracionSede>(builder.Configuration.GetSection("Sede"));
builder.Services.Configure<ConfiguracionRabbit>(builder.Configuration.GetSection("RabbitMQ"));

// ── Mensajería ──
builder.Services.AddSingleton<IBrokerMensajeria, RabbitMqBroker>();
builder.Services.AddSingleton<RegistroMensajesProcesados>();
builder.Services.AddHostedService<DespachadorMensajes>();

// ── Inventario global y sincronización ──
builder.Services.AddSingleton<InventarioGlobal>();
builder.Services.AddSingleton<ServicioSincronizacion>();

// ── Reservas y pedidos ──
builder.Services.AddSingleton<GestorReservasLocales>();
builder.Services.AddSingleton<CoordinadorReservas>();
builder.Services.AddSingleton<ServicioPedidos>();

// ── Contratos con las otras áreas (interfaz → clase que la implementa) ──
builder.Services.AddSingleton<IInventarioLocal, InventarioLocal>();        
builder.Services.AddSingleton<IPlanificadorRutas, PlanificadorRutas>();  
builder.Services.AddSingleton<IRepositorioPedidos, RepositorioPedidosJson>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
