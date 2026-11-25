using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// GATEWAY: unico punto de entrada. Las rutas se declaran en ocelot.json (local)
// u ocelot.Production.json (nube: hosts publicos https:443).
var archivoRutas = builder.Environment.IsProduction() ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration.AddJsonFile(archivoRutas, optional: false, reloadOnChange: true);

// El gateway valida el JWT ANTES de reenviar (misma clave, emisor y audiencia que los servicios).
// "Bearer" es el nombre que se usa en AuthenticationProviderKeys de ocelot.json.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clave Jwt:Key no esta configurada (variable de entorno Jwt__Key).");

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options => options.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
