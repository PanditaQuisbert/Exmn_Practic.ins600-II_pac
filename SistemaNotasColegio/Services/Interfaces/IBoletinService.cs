using SistemaNotasColegio.DTOs.Boletines;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IBoletinService
{
    Task<ServiceResult<BoletinEstudianteResponseDto>> GetByEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);
}
