using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Asignaciones;

public class CrearAsignacionRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El profesor es obligatorio.")]
    public int ProfesorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La materia es obligatoria.")]
    public int MateriaId { get; set; }
}
