using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Auth;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Usuario o contraseña incorrectos.";

    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var nombreUsuario = request.NombreUsuario.Trim();

        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuarioActual => usuarioActual.Rol)
            .SingleOrDefaultAsync(
                usuarioActual => usuarioActual.NombreUsuario == nombreUsuario,
                cancellationToken);

        if (usuario is null
            || !usuario.Activo
            || !_passwordService.Verify(request.Password, usuario.PasswordHash))
        {
            return ServiceResult<LoginResponseDto>.Fail(
                InvalidCredentialsMessage,
                ServiceErrorType.Unauthorized);
        }

        var (token, expiresAt) = _jwtTokenService.CreateToken(usuario);

        var response = new LoginResponseDto(
            token,
            "Bearer",
            expiresAt,
            new UsuarioSesionDto(
                usuario.Id,
                usuario.Cedula,
                usuario.Nombres,
                usuario.Apellidos,
                usuario.Correo,
                usuario.NombreUsuario,
                usuario.RolId,
                usuario.Rol.Nombre));

        return ServiceResult<LoginResponseDto>.Ok(response);
    }
}
