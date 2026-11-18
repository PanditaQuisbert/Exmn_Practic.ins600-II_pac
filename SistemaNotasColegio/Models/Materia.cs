using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Models;

public class Materia : IAuditableEntity
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<AsignacionMateria> Asignaciones { get; set; } = new List<AsignacionMateria>();

    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
