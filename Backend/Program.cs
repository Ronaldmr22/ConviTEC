using Backend.Models;
using Backend.Services;
using Backend.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Registrar controladores
builder.Services.AddControllers();

// Leer los datos de la sede
string codigo = builder.Configuration["Sede:Codigo"]
    ?? throw new InvalidOperationException("Falta el código de sede.");

string nombre = builder.Configuration["Sede:Nombre"]
    ?? throw new InvalidOperationException("Falta el nombre de sede.");

string rutaDatos = builder.Configuration["Sede:RutaDatos"]
    ?? throw new InvalidOperationException("Falta la ruta de datos.");

// Crear configuración de la sede
ConfiguracionSede sede = new ConfiguracionSede(
    codigo,
    nombre,
    rutaDatos
);

// Registrar la configuración de esta instancia
builder.Services.AddSingleton(sede);

// Crear la ruta del archivo de inventario
string rutaInventario = Path.Combine(
    builder.Environment.ContentRootPath,
    sede.GetRutaDatos(),
    "inventario.json"
);

// Crear la persistencia
InventarioJson persistencia =
    new InventarioJson(rutaInventario);

// Recuperar el inventario local
Inventario inventario = persistencia.Cargar();

// Registrar los objetos para usarlos desde controladores
builder.Services.AddSingleton(persistencia);
builder.Services.AddSingleton(inventario);

var app = builder.Build();

// Habilitar frontend estático
app.UseDefaultFiles();
app.UseStaticFiles();

// Habilitar rutas de la API
app.MapControllers();

app.Run();
