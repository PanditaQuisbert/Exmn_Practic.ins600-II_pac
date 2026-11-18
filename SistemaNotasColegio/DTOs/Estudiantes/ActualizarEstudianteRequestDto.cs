using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Estudiantes;

public class ActualizarEstudianteRequestDto
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "La cédula debe tener entre 5 y 20 caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z\-]+$", ErrorMessage = "La cédula solo puede contener letras, números y guiones.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[\p{L}\s'\-\.]+$", ErrorMessage = "Los nombres solo pueden contener letras, espacios, apóstrofos, puntos y guiones.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[\p{L}\s'\-\.]+$", ErrorMessage = "Los apellidos solo pueden contener letras, espacios, apóstrofos, puntos y guiones.")]
    public string Apellidos { get; set; } = string.Empty;
}
