using SistemaNotasColegio.DTOs.Usuarios;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioResponseDto>> GetAllAsync(
        int? rolId,
        bool incluirInactivos,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UsuarioResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UsuarioResponseDto>> CreateAsync(
        CrearUsuarioRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UsuarioResponseDto>> UpdateAsync(
        int id,
        ActualizarUsuarioRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UsuarioResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default);
}
