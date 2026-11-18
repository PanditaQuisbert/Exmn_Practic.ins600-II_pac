using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Models;

public class Calificacion : IAuditableEntity
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }

    public Estudiante Estudiante { get; set; } = null!;

    public int MateriaId { get; set; }

    public Materia Materia { get; set; } = null!;

    public Trimestre Trimestre { get; set; }

    public decimal Nota { get; set; }

    public int RegistradoPorUsuarioId { get; set; }

    public Usuario RegistradoPor { get; set; } = null!;

    public int? ActualizadoPorUsuarioId { get; set; }

    public Usuario? ActualizadoPor { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
