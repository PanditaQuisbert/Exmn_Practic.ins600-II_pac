using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Asignaciones;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class AsignacionService : IAsignacionService
{
    private readonly AppDbContext _dbContext;

    public AsignacionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AsignacionResponseDto>> GetAllAsync(
        int? profesorId,
        int? materiaId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AsignacionesMaterias
            .AsNoTracking()
            .Include(asignacion => asignacion.Profesor)
            .Include(asignacion => asignacion.Materia)
            .AsQueryable();

        if (profesorId.HasValue)
        {
            query = query.Where(asignacion => asignacion.ProfesorId == profesorId.Value);
        }

        if (materiaId.HasValue)
        {
            query = query.Where(asignacion => asignacion.MateriaId == materiaId.Value);
        }

        var asignaciones = await query
            .OrderBy(asignacion => asignacion.Profesor.Apellidos)
            .ThenBy(asignacion => asignacion.Materia.Nombre)
            .ToListAsync(cancellationToken);

        return asignaciones.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResult<AsignacionResponseDto>> CreateAsync(
        CrearAsignacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var profesor = await _dbContext.Usuarios
            .Include(usuario => usuario.Rol)
            .SingleOrDefaultAsync(
                usuario => usuario.Id == request.ProfesorId,
                cancellationToken);

        if (profesor is null)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "El profesor no existe.",
                ServiceErrorType.NotFound);
        }

        if (profesor.RolId != Roles.ProfesorId)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "El usuario indicado no tiene el rol de Profesor.");
        }

        if (!profesor.Activo)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "El profesor está inactivo y no puede recibir asignaciones.");
        }

        var materia = await _dbContext.Materias
            .SingleOrDefaultAsync(
                materiaActual => materiaActual.Id == request.MateriaId,
                cancellationToken);

        if (materia is null)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "La materia no existe.",
                ServiceErrorType.NotFound);
        }

        if (!materia.Activo)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "La materia está inactiva y no puede asignarse.");
        }

        var exists = await _dbContext.AsignacionesMaterias.AnyAsync(
            asignacion => asignacion.ProfesorId == profesor.Id
                && asignacion.MateriaId == materia.Id,
            cancellationToken);

        if (exists)
        {
            return ServiceResult<AsignacionResponseDto>.Fail(
                "El profesor ya tiene asignada esa materia.",
                ServiceErrorType.Conflict);
        }

        var nuevaAsignacion = new AsignacionMateria
        {
            ProfesorId = profesor.Id,
            Profesor = profesor,
            MateriaId = materia.Id,
            Materia = materia
        };

        _dbContext.AsignacionesMaterias.Add(nuevaAsignacion);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AsignacionResponseDto>.Ok(MapToResponse(nuevaAsignacion));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await _dbContext.AsignacionesMaterias
            .SingleOrDefaultAsync(
                asignacionActual => asignacionActual.Id == id,
                cancellationToken);

        if (asignacion is null)
        {
            return ServiceResult<bool>.Fail(
                "La asignación no existe.",
                ServiceErrorType.NotFound);
        }

        var tieneCalificaciones = await _dbContext.Calificaciones.AnyAsync(
            calificacion => calificacion.MateriaId == asignacion.MateriaId
                && calificacion.RegistradoPorUsuarioId == asignacion.ProfesorId,
            cancellationToken);

        if (tieneCalificaciones)
        {
            return ServiceResult<bool>.Fail(
                "No se puede eliminar la asignación porque el profesor ya registró calificaciones en esa materia.",
                ServiceErrorType.Conflict);
        }

        _dbContext.AsignacionesMaterias.Remove(asignacion);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(true);
    }

    private static AsignacionResponseDto MapToResponse(AsignacionMateria asignacion)
    {
        return new AsignacionResponseDto(
            asignacion.Id,
            asignacion.ProfesorId,
            $"{asignacion.Profesor.Nombres} {asignacion.Profesor.Apellidos}".Trim(),
            asignacion.Profesor.NombreUsuario,
            asignacion.MateriaId,
            asignacion.Materia.Codigo,
            asignacion.Materia.Nombre,
            asignacion.FechaRegistro);
    }
}
