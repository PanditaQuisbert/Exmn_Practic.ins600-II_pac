using SistemaNotasColegio.DTOs.Auth;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}
