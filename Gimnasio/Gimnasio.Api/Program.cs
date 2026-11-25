using System.Text;
using Gimnasio.Datos;
using Gimnasio.Datos.Repositorios;
using Gimnasio.Negocio.Mapeos;
using Gimnasio.Negocio.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Contexto de Base de Datos
// Proveedor configurable (SqlServer en local, Postgres en la nube); la logica de la API no cambia.
var proveedorBd = builder.Configuration["Database:Provider"] ?? "SqlServer";
builder.Services.AddDbContext<GimnasioContext>(options =>
{
    var cadena = builder.Configuration.GetConnectionString("GimnasioDB");
    if (proveedorBd.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(cadena);
    else
        options.UseSqlServer(cadena);
});

// Inyección de Dependencias (Repositorios y Servicios)
builder.Services.AddScoped<IPlanRepositorio, PlanRepositorio>();
builder.Services.AddScoped<ISocioRepositorio, SocioRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

builder.Services.AddScoped<IPlanServicio, PlanServicio>();
builder.Services.AddScoped<ISocioServicio, SocioServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"],
    typeof(MappingProfile));

// Controladores
builder.Services.AddControllers();

// Configuración de Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clave secreta Jwt:Key no está configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Documentación OpenAPI con esquema Bearer para botón "Authorize" en Swagger UI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese su token JWT:"
        });

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };

        document.Security = new List<OpenApiSecurityRequirement> { requirement };

        return Task.CompletedTask;
    });
});

// CORS
const string CorsPolicy = "FrontendPolicy";
var origenes = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Gimnasio API v1");
    options.DocumentTitle = "Gimnasio API";
});

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

// UseAuthentication DEBE estar antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
