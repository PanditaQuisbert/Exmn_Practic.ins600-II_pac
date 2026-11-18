using SistemaNotasColegio.DTOs.Profesores;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IProfesorService
{
    Task<IReadOnlyList<MiAsignacionResponseDto>> GetMisAsignacionesAsync(
        int profesorId,
        CancellationToken cancellationToken = default);
}
