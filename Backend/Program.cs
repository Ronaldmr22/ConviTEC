
using Backend.Models;
using Backend.Servicios;
using Backend.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Leer configuración de la sede
string codigo = builder.Configuration["Sede:Codigo"]
    ?? throw new InvalidOperationException("Falta el código.");

string nombre = builder.Configuration["Sede:Nombre"]
    ?? throw new InvalidOperationException("Falta el nombre.");

string rutaDatos = builder.Configuration["Sede:RutaDatos"]
    ?? throw new InvalidOperationException("Falta la ruta.");

ConfiguracionSede sede = new ConfiguracionSede(
    codigo, nombre, rutaDatos
);

builder.Services.AddSingleton(sede);

// Ruta del inventario de esta sede
string rutaInventario = Path.Combine(
    builder.Environment.ContentRootPath,
    sede.GetRutaDatos(),
    "inventario.json"
);

// Crear persistencia y cargar inventario
InventarioJson persistencia =new InventarioJson(rutaInventario);

Inventario inventario = persistencia.Cargar();

// Registrar servicios
builder.Services.AddSingleton(persistencia);
builder.Services.AddSingleton(inventario);

// ESTA PARTE ES FUNDAMENTAL
InventarioService inventarioService = new InventarioService(inventario, persistencia);
builder.Services.AddSingleton(inventarioService);

string rutaInventarioGlobal = Path.Combine(
    builder.Environment.ContentRootPath,
    sede.GetRutaDatos(),
    "inventarioGlobal.json"
);

PersistenciaInventarioGlobalJson persistenciaGlobal = new PersistenciaInventarioGlobalJson(rutaInventarioGlobal);
InventarioGlobal inventarioGlobal = persistenciaGlobal.Cargar();

builder.Services.AddSingleton(persistenciaGlobal);
builder.Services.AddSingleton(inventarioGlobal);

InventarioGlobalService inventarioGlobalService = new InventarioGlobalService(inventarioGlobal, persistenciaGlobal );
builder.Services.AddSingleton(inventarioGlobalService);

inventarioGlobalService.SincronizarInventarioLocal(inventarioService, sede.GetCodigo());
inventarioService.ConfigurarSincronizacionGlobal(inventarioGlobalService, sede.GetCodigo( ));


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

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
