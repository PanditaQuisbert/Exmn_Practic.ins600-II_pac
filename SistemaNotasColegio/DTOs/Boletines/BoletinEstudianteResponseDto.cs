namespace SistemaNotasColegio.DTOs.Boletines;

public record BoletinEstudianteResponseDto(
    int EstudianteId,
    string Cedula,
    string Nombres,
    string Apellidos,
    bool Activo,
    IReadOnlyList<BoletinMateriaDto> Materias);
