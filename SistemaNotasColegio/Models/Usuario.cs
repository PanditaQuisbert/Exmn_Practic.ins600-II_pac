using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Models;

public class Usuario : IAuditableEntity
{
    public int Id { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<AsignacionMateria> AsignacionesComoProfesor { get; set; } = new List<AsignacionMateria>();
}
