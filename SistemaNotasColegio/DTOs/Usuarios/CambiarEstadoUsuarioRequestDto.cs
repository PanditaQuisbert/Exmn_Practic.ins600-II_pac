using System.ComponentModel.DataAnnotations;

namespace SistemaNotasColegio.DTOs.Usuarios;

public class CambiarEstadoUsuarioRequestDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Activo { get; set; }
}
