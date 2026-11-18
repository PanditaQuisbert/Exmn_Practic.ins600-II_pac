using SistemaNotasColegio.DTOs.Materias;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IMateriaService
{
    Task<IReadOnlyList<MateriaResponseDto>> GetAllAsync(
        bool incluirInactivas,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MateriaResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MateriaResponseDto>> CreateAsync(
        CrearMateriaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MateriaResponseDto>> UpdateAsync(
        int id,
        ActualizarMateriaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MateriaResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default);
}
