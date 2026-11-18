using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Calificaciones;

public class CrearCalificacionRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El estudiante es obligatorio.")]
    public int EstudianteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La materia es obligatoria.")]
    public int MateriaId { get; set; }

    [Range(1, 3, ErrorMessage = "El trimestre debe ser 1, 2 o 3.")]
    public int Trimestre { get; set; }

    [Required(ErrorMessage = "La nota es obligatoria.")]
    [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
    public decimal? Nota { get; set; }
}
