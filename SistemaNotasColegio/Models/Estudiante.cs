using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Models;

public class Estudiante : IAuditableEntity
{
    public int Id { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
