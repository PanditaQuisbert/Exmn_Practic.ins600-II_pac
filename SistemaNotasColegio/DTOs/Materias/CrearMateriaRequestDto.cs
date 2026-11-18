using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Materias;

public class CrearMateriaRequestDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 20 caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z\-_\.]+$", ErrorMessage = "El código solo puede contener letras, números, guiones, guiones bajos y puntos.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
