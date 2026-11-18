using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Models;

public class AsignacionMateria : IAuditableEntity
{
    public int Id { get; set; }

    public int ProfesorId { get; set; }

    public Usuario Profesor { get; set; } = null!;

    public int MateriaId { get; set; }

    public Materia Materia { get; set; } = null!;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
