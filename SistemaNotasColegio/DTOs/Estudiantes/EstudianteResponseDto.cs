namespace SistemaNotasColegio.DTOs.Estudiantes;

public record EstudianteResponseDto(
    int Id,
    string Cedula,
    string Nombres,
    string Apellidos,
    bool Activo,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion);
