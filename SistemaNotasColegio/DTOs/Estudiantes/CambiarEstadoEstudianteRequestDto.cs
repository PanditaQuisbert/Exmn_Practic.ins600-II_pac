using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Estudiantes;

public class CambiarEstadoEstudianteRequestDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Activo { get; set; }
}
