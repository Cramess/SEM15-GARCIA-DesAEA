using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gimnasio.Entidades.Dtos;
using Gimnasio.Negocio.Excepciones;
using Gimnasio.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _authServicio;

    public AuthController(IAuthServicio authServicio)
    {
        _authServicio = authServicio;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<PerfilDto>> Registrar(UsuarioRegistroDto dto)
    {
        try
        {
            var creado = await _authServicio.RegistrarAsync(dto);
            return StatusCode(201, creado);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDto>> Login(UsuarioLoginDto dto)
    {
        var resultado = await _authServicio.LoginAsync(dto);
        if (resultado == null)
            return Unauthorized(new { mensaje = "Credenciales inválidas." });

        return Ok(resultado);
    }

    [HttpGet("perfil")]
    [Authorize]
    public ActionResult<PerfilDto> Perfil()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                  ?? User.FindFirst("email")?.Value
                  ?? User.Identity?.Name;

        var rol = User.FindFirstValue(ClaimTypes.Role)
               ?? User.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(correo))
            return Unauthorized(new { mensaje = "No se encontraron los datos del usuario en el token." });

        return Ok(new PerfilDto
        {
            Correo = correo,
            Rol = rol ?? string.Empty
        });
    }
}
