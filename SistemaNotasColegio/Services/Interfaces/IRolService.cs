using SistemaNotasColegio.DTOs.Roles;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IRolService
{
    Task<IReadOnlyList<RolResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
