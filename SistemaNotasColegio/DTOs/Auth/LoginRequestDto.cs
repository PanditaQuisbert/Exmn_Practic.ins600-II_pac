using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
