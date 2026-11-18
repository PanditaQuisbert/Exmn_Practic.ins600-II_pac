using SistemaNotasColegio.DTOs.Calificaciones;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface ICalificacionService
{
    Task<IReadOnlyList<CalificacionResponseDto>> GetAllAsync(
        int profesorId,
        int? estudianteId,
        int? materiaId,
        int? trimestre,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CalificacionResponseDto>> GetByIdAsync(
        int profesorId,
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CalificacionResponseDto>> CreateAsync(
        int profesorId,
        CrearCalificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CalificacionResponseDto>> UpdateAsync(
        int profesorId,
        int id,
        ActualizarCalificacionRequestDto request,
        CancellationToken cancellationToken = default);
}
