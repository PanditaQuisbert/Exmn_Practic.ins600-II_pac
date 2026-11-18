using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Usuarios;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;

    public UsuarioService(
        AppDbContext dbContext,
        IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task<IReadOnlyList<UsuarioResponseDto>> GetAllAsync(
        int? rolId,
        bool incluirInactivos,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Rol)
            .AsQueryable();

        if (rolId.HasValue)
        {
            query = query.Where(usuario => usuario.RolId == rolId.Value);
        }

        if (!incluirInactivos)
        {
            query = query.Where(usuario => usuario.Activo);
        }

        var usuarios = await query
            .OrderBy(usuario => usuario.Apellidos)
            .ThenBy(usuario => usuario.Nombres)
            .ToListAsync(cancellationToken);

        return usuarios.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResult<UsuarioResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuarioActual => usuarioActual.Rol)
            .SingleOrDefaultAsync(usuarioActual => usuarioActual.Id == id, cancellationToken);

        if (usuario is null)
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "El usuario no existe.",
                ServiceErrorType.NotFound);
        }

        return ServiceResult<UsuarioResponseDto>.Ok(MapToResponse(usuario));
    }

    public async Task<ServiceResult<UsuarioResponseDto>> CreateAsync(
        CrearUsuarioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var rol = await _dbContext.Roles
            .SingleOrDefaultAsync(rolActual => rolActual.Id == request.RolId, cancellationToken);

        if (rol is null || !IsAllowedRole(rol.Id))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "El rol indicado no es válido. Solo se permite Secretaría o Profesor.");
        }

        var cedula = request.Cedula.Trim();
        var correo = request.Correo.Trim();
        var nombreUsuario = request.NombreUsuario.Trim();

        if (!Configuration.PasswordPolicy.IsValid(request.Password))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                $"La contraseña debe tener al menos {Configuration.PasswordPolicy.MinimumLength} caracteres.");
        }

        if (await _dbContext.Usuarios.AnyAsync(usuario => usuario.Cedula == cedula, cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con esa cédula.",
                ServiceErrorType.Conflict);
        }

        if (await _dbContext.Usuarios.AnyAsync(
                usuario => usuario.Correo == correo,
                cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con ese correo.",
                ServiceErrorType.Conflict);
        }

        if (await _dbContext.Usuarios.AnyAsync(
                usuario => usuario.NombreUsuario == nombreUsuario,
                cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con ese nombre de usuario.",
                ServiceErrorType.Conflict);
        }

        var usuario = new Usuario
        {
            Cedula = cedula,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Correo = correo,
            NombreUsuario = nombreUsuario,
            PasswordHash = _passwordService.Hash(request.Password),
            RolId = rol.Id,
            Rol = rol,
            Activo = true
        };

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UsuarioResponseDto>.Ok(MapToResponse(usuario));
    }

    public async Task<ServiceResult<UsuarioResponseDto>> UpdateAsync(
        int id,
        ActualizarUsuarioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(usuarioActual => usuarioActual.Rol)
            .SingleOrDefaultAsync(usuarioActual => usuarioActual.Id == id, cancellationToken);

        if (usuario is null)
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "El usuario no existe.",
                ServiceErrorType.NotFound);
        }

        var rol = await _dbContext.Roles
            .SingleOrDefaultAsync(rolActual => rolActual.Id == request.RolId, cancellationToken);

        if (rol is null || !IsAllowedRole(rol.Id))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "El rol indicado no es válido. Solo se permite Secretaría o Profesor.");
        }

        var cedula = request.Cedula.Trim();
        var correo = request.Correo.Trim();
        var nombreUsuario = request.NombreUsuario.Trim();

        if (await _dbContext.Usuarios.AnyAsync(
                usuarioActual => usuarioActual.Cedula == cedula && usuarioActual.Id != id,
                cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con esa cédula.",
                ServiceErrorType.Conflict);
        }

        if (await _dbContext.Usuarios.AnyAsync(
                usuarioActual => usuarioActual.Correo == correo && usuarioActual.Id != id,
                cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con ese correo.",
                ServiceErrorType.Conflict);
        }

        if (await _dbContext.Usuarios.AnyAsync(
                usuarioActual => usuarioActual.NombreUsuario == nombreUsuario && usuarioActual.Id != id,
                cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "Ya existe un usuario con ese nombre de usuario.",
                ServiceErrorType.Conflict);
        }

        if (usuario.RolId == Roles.SecretariaId
            && rol.Id != Roles.SecretariaId
            && await IsLastActiveSecretariaAsync(usuario.Id, cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "No se puede cambiar el rol de la única Secretaría activa.",
                ServiceErrorType.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (!Configuration.PasswordPolicy.IsValid(request.Password))
            {
                return ServiceResult<UsuarioResponseDto>.Fail(
                    $"La contraseña debe tener al menos {Configuration.PasswordPolicy.MinimumLength} caracteres.");
            }

            usuario.PasswordHash = _passwordService.Hash(request.Password);
        }

        usuario.Cedula = cedula;
        usuario.Nombres = request.Nombres.Trim();
        usuario.Apellidos = request.Apellidos.Trim();
        usuario.Correo = correo;
        usuario.NombreUsuario = nombreUsuario;
        usuario.RolId = rol.Id;
        usuario.Rol = rol;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UsuarioResponseDto>.Ok(MapToResponse(usuario));
    }

    public async Task<ServiceResult<UsuarioResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(usuarioActual => usuarioActual.Rol)
            .SingleOrDefaultAsync(usuarioActual => usuarioActual.Id == id, cancellationToken);

        if (usuario is null)
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "El usuario no existe.",
                ServiceErrorType.NotFound);
        }

        if (!activo
            && usuario.RolId == Roles.SecretariaId
            && await IsLastActiveSecretariaAsync(usuario.Id, cancellationToken))
        {
            return ServiceResult<UsuarioResponseDto>.Fail(
                "No se puede desactivar la única Secretaría activa.",
                ServiceErrorType.Conflict);
        }

        usuario.Activo = activo;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UsuarioResponseDto>.Ok(MapToResponse(usuario));
    }

    private async Task<bool> IsLastActiveSecretariaAsync(
        int usuarioId,
        CancellationToken cancellationToken)
    {
        var otherActiveSecretarias = await _dbContext.Usuarios
            .CountAsync(
                usuario => usuario.RolId == Roles.SecretariaId
                    && usuario.Activo
                    && usuario.Id != usuarioId,
                cancellationToken);

        return otherActiveSecretarias == 0;
    }

    private static bool IsAllowedRole(int rolId)
    {
        return rolId is Roles.SecretariaId or Roles.ProfesorId;
    }

    private static UsuarioResponseDto MapToResponse(Usuario usuario)
    {
        return new UsuarioResponseDto(
            usuario.Id,
            usuario.Cedula,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Correo,
            usuario.NombreUsuario,
            usuario.RolId,
            usuario.Rol.Nombre,
            usuario.Activo,
            usuario.FechaRegistro,
            usuario.FechaActualizacion);
    }
}
