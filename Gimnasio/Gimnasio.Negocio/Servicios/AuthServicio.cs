using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Gimnasio.Datos.Repositorios;
using Gimnasio.Entidades;
using Gimnasio.Entidades.Dtos;
using Gimnasio.Negocio.Excepciones;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gimnasio.Negocio.Servicios;

public interface IAuthServicio
{
    Task<PerfilDto> RegistrarAsync(UsuarioRegistroDto dto);
    Task<TokenDto?> LoginAsync(UsuarioLoginDto dto);
}

public class AuthServicio : IAuthServicio
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IConfiguration _configuration;

    public const string RolPorDefecto = "Recepcionista";

    public AuthServicio(IUsuarioRepositorio usuarios, IConfiguration configuration)
    {
        _usuarios = usuarios;
        _configuration = configuration;
    }

    public async Task<PerfilDto> RegistrarAsync(UsuarioRegistroDto dto)
    {
        if (await _usuarios.ExisteCorreoAsync(dto.Correo))
            throw new ReglaNegocioException($"Ya existe un usuario registrado con el correo '{dto.Correo}'.");

        var claveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave);

        var usuario = new Usuario
        {
            Correo = dto.Correo,
            ClaveHash = claveHash,
            Rol = RolPorDefecto
        };

        await _usuarios.AgregarAsync(usuario);

        return new PerfilDto
        {
            Correo = usuario.Correo,
            Rol = usuario.Rol
        };
    }

    public async Task<TokenDto?> LoginAsync(UsuarioLoginDto dto)
    {
        var usuario = await _usuarios.BuscarPorCorreoAsync(dto.Correo);
        if (usuario == null)
            return null;

        bool claveValida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash);
        if (!claveValida)
            return null;

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("La clave secreta Jwt:Key no está configurada.");
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var duracionMinutos = double.TryParse(_configuration["Jwt:DuracionMinutos"], out var minutos) ? minutos : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol),
            new("role", usuario.Rol),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracion = DateTime.UtcNow.AddMinutes(duracionMinutos);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiracion,
            signingCredentials: creds
        );

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiracion
        };
    }
}
