using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Usuarios;

public class ActualizarUsuarioRequestDto
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La cédula no puede superar los 20 caracteres.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden superar los 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(100, ErrorMessage = "La nueva contraseña no puede superar los 100 caracteres.")]
    public string? Password { get; set; }

    [Range(1, 2, ErrorMessage = "El rol debe ser Secretaría (1) o Profesor (2).")]
    public int RolId { get; set; }
}
