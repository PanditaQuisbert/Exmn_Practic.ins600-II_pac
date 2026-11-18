namespace SistemaNotasColegio.DTOs.Usuarios;

public record UsuarioResponseDto(
    int Id,
    string Cedula,
    string Nombres,
    string Apellidos,
    string Correo,
    string NombreUsuario,
    int RolId,
    string Rol,
    bool Activo,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion);
