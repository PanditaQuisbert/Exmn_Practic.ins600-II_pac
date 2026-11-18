using SistemaNotasColegio.DTOs.Asignaciones;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IAsignacionService
{
    Task<IReadOnlyList<AsignacionResponseDto>> GetAllAsync(
        int? profesorId,
        int? materiaId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AsignacionResponseDto>> CreateAsync(
        CrearAsignacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
