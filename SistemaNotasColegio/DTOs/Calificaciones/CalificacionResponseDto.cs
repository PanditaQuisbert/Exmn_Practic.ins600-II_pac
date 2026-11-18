namespace SistemaNotasColegio.DTOs.Calificaciones;

public record CalificacionResponseDto(
    int Id,
    int EstudianteId,
    string EstudianteCedula,
    string EstudianteNombreCompleto,
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    int Trimestre,
    decimal Nota,
    int RegistradoPorUsuarioId,
    string RegistradoPorNombreCompleto,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion);
