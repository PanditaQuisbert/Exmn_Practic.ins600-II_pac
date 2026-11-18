namespace SistemaNotasColegio.DTOs.Auth;

public record UsuarioSesionDto(
    int Id,
    string Cedula,
    string Nombres,
    string Apellidos,
    string Correo,
    string NombreUsuario,
    int RolId,
    string Rol);
