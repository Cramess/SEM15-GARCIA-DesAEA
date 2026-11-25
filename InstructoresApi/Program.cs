using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MICROSERVICIO 2: InstructoresApi. Proyecto independiente de Gimnasio.Api:
// su propio puerto, sus propios datos (en memoria) y sin compartir clases.

// Valida el token JWT con la MISMA clave, emisor y audiencia que Gimnasio.Api.
// La clave NO esta en el repositorio: llega por variable de entorno Jwt__Key.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clave Jwt:Key no esta configurada (variable de entorno Jwt__Key).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    });
builder.Services.AddAuthorization();

var instructores = new List<Instructor>
{
    new(1, "Marco Quispe Ramos", "Musculacion", "marco.quispe@gimnasio.com", "Manana"),
    new(2, "Lucia Fernandez Soto", "Yoga y Pilates", "lucia.fernandez@gimnasio.com", "Tarde"),
    new(3, "Diego Paredes Luna", "Crossfit", "diego.paredes@gimnasio.com", "Noche"),
    new(4, "Andrea Rojas Vega", "Spinning", "andrea.rojas@gimnasio.com", "Manana")
};

var app = builder.Build();

// Detras del proxy del proveedor (Render) la solicitud llega por http; se respeta X-Forwarded-*.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

// GET /api/instructores (requiere token)
app.MapGet("/api/instructores", () => Results.Ok(instructores)).RequireAuthorization();

// GET /api/instructores/{id} (requiere token)
app.MapGet("/api/instructores/{id:int}", (int id) =>
    instructores.FirstOrDefault(i => i.Id == id) is { } instructor
        ? Results.Ok(instructor)
        : Results.NotFound()).RequireAuthorization();

app.Run();

record Instructor(int Id, string Nombre, string Especialidad, string Correo, string Turno);
