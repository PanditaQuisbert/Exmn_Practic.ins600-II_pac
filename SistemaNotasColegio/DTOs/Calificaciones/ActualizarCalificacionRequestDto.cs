using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Calificaciones;

public class ActualizarCalificacionRequestDto
{
    [Required(ErrorMessage = "La nota es obligatoria.")]
    [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
    public decimal? Nota { get; set; }
}
