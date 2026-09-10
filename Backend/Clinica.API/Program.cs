using System.Text;
using System.Text.Json.Serialization;
using Clinica.API.Middleware;
using Clinica.API.Seguridad;
using Clinica.Aplicacion.Interfaces;
using Clinica.Aplicacion.Servicios;
using Clinica.Aplicacion.Validadores;
using Clinica.Infraestructura.Configuracion;
using Clinica.Infraestructura.Persistencia;
using Clinica.Infraestructura.Persistencia.Contexto;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios de aplicación (casos de uso) ----------
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IServicioPacientes, ServicioPacientes>();
builder.Services.AddScoped<IServicioProfesionales, ServicioProfesionales>();
builder.Services.AddScoped<IServicioTurnos, ServicioTurnos>();
builder.Services.AddScoped<IServicioPanel, ServicioPanel>();

// Registra todos los IValidator<T> definidos en Clinica.Aplicacion (un solo lugar,
// no hace falta agregar cada validador nuevo a mano).
builder.Services.AddValidatorsFromAssemblyContaining<CrearPacienteDtoValidador>();

// ---------- Infraestructura (DbContext, repositorios, hashing, JWT) ----------
builder.Services.AgregarInfraestructura(builder.Configuration);

// Adaptador web de IUsuarioActual (necesita acceso al HttpContext del request).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActualHttp>();

builder.Services.AddControllers().AddJsonOptions(opciones =>
{
    // Los estados de turno (y cualquier otro enum) viajan como texto en el JSON
    // ("Pendiente", no "1"): más legible y consistente con el resto de la API en español.
    opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ---------- CORS ----------
// El origen permitido se toma de configuración (variable de entorno ORIGEN_FRONTEND),
// nunca se hardcodea ni se usa un comodín: evita exponer la API a cualquier sitio.
var origenesFrontend = (builder.Configuration["ORIGEN_FRONTEND"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PoliticaFrontend", politica =>
    {
        if (origenesFrontend.Length > 0)
        {
            politica.WithOrigins(origenesFrontend).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ---------- Autenticación JWT ----------
var configuracionJwt = builder.Configuration.GetSection(ConfiguracionJwt.Seccion).Get<ConfiguracionJwt>()
    ?? throw new InvalidOperationException("No se configuró la sección 'Jwt'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracionJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = configuracionJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracionJwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Clínica API",
        Version = "v1",
        Description = "API del mini sistema de gestión de turnos de una clínica."
    });

    var esquemaJwt = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Pegar únicamente el token JWT (sin el prefijo \"Bearer\")."
    };
    opciones.AddSecurityDefinition("Bearer", esquemaJwt);
    opciones.AddSecurityRequirement(_ => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

var app = builder.Build();

// Middleware global de excepciones: debe ir primero para capturar errores de
// cualquier middleware/endpoint posterior en el pipeline.
app.UseMiddleware<ManejadorGlobalDeExcepciones>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PoliticaFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplica migraciones pendientes y carga datos de demostración al iniciar.
// Es seguro en cada arranque: las migraciones son idempotentes y el seed
// verifica primero si ya hay datos cargados.
using (var alcance = app.Services.CreateScope())
{
    var contexto = alcance.ServiceProvider.GetRequiredService<ClinicaDbContext>();
    await contexto.Database.MigrateAsync();

    var hasheador = alcance.ServiceProvider.GetRequiredService<IHasheadorContrasenas>();
    await SeedDeDatos.EjecutarAsync(contexto, hasheador, app.Configuration);
}

app.Run();

// Necesario para que WebApplicationFactory<Program> (tests de integración) encuentre la clase.
public partial class Program
{
}
