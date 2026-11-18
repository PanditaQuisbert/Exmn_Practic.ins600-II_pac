using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Materias;

public class CambiarEstadoMateriaRequestDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Activo { get; set; }
}
