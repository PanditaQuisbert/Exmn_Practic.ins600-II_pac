namespace SistemaNotasColegio.DTOs.Profesores;

public record MiAsignacionResponseDto(
    int AsignacionId,
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    bool MateriaActiva,
    DateTime FechaRegistro);
