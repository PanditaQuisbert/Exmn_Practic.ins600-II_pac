namespace SistemaNotasColegio.DTOs.Asignaciones;

public record AsignacionResponseDto(
    int Id,
    int ProfesorId,
    string ProfesorNombre,
    string ProfesorNombreUsuario,
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    DateTime FechaRegistro);
