namespace SistemaNotasColegio.DTOs.Materias;

public record MateriaResponseDto(
    int Id,
    string Codigo,
    string Nombre,
    bool Activo,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion);
