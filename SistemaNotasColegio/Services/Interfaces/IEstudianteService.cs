using SistemaNotasColegio.DTOs.Estudiantes;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IEstudianteService
{
    Task<IReadOnlyList<EstudianteResponseDto>> GetAllAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<EstudianteResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<EstudianteResponseDto>> CreateAsync(
        CrearEstudianteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<EstudianteResponseDto>> UpdateAsync(
        int id,
        ActualizarEstudianteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<EstudianteResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default);
}
