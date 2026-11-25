using System.ComponentModel.DataAnnotations;

namespace Gimnasio.Entidades.Dtos;

public class UsuarioRegistroDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100 caracteres.")]
    public string Clave { get; set; } = string.Empty;
}

public class UsuarioLoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    public string Clave { get; set; } = string.Empty;
}

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
}

public class PerfilDto
{
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
