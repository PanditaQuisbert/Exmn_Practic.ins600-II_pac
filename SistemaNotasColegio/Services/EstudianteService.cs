using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Estudiantes;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class EstudianteService : IEstudianteService
{
    private readonly AppDbContext _dbContext;

    public EstudianteService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EstudianteResponseDto>> GetAllAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Estudiantes
            .AsNoTracking()
            .AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(estudiante => estudiante.Activo);
        }

        var estudiantes = await query
            .OrderBy(estudiante => estudiante.Apellidos)
            .ThenBy(estudiante => estudiante.Nombres)
            .ToListAsync(cancellationToken);

        return estudiantes.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResult<EstudianteResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await _dbContext.Estudiantes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                estudianteActual => estudianteActual.Id == id,
                cancellationToken);

        if (estudiante is null)
        {
            return ServiceResult<EstudianteResponseDto>.Fail(
                "El estudiante no existe.",
                ServiceErrorType.NotFound);
        }

        return ServiceResult<EstudianteResponseDto>.Ok(MapToResponse(estudiante));
    }

    public async Task<ServiceResult<EstudianteResponseDto>> CreateAsync(
        CrearEstudianteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var cedula = request.Cedula.Trim();

        if (await _dbContext.Estudiantes.AnyAsync(
                estudiante => estudiante.Cedula == cedula,
                cancellationToken))
        {
            return ServiceResult<EstudianteResponseDto>.Fail(
                "Ya existe un estudiante con esa cédula.",
                ServiceErrorType.Conflict);
        }

        var estudiante = new Estudiante
        {
            Cedula = cedula,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Activo = true
        };

        _dbContext.Estudiantes.Add(estudiante);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<EstudianteResponseDto>.Ok(MapToResponse(estudiante));
    }

    public async Task<ServiceResult<EstudianteResponseDto>> UpdateAsync(
        int id,
        ActualizarEstudianteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await _dbContext.Estudiantes
            .SingleOrDefaultAsync(
                estudianteActual => estudianteActual.Id == id,
                cancellationToken);

        if (estudiante is null)
        {
            return ServiceResult<EstudianteResponseDto>.Fail(
                "El estudiante no existe.",
                ServiceErrorType.NotFound);
        }

        var cedula = request.Cedula.Trim();

        if (await _dbContext.Estudiantes.AnyAsync(
                estudianteActual => estudianteActual.Cedula == cedula
                    && estudianteActual.Id != id,
                cancellationToken))
        {
            return ServiceResult<EstudianteResponseDto>.Fail(
                "Ya existe un estudiante con esa cédula.",
                ServiceErrorType.Conflict);
        }

        estudiante.Cedula = cedula;
        estudiante.Nombres = request.Nombres.Trim();
        estudiante.Apellidos = request.Apellidos.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<EstudianteResponseDto>.Ok(MapToResponse(estudiante));
    }

    public async Task<ServiceResult<EstudianteResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await _dbContext.Estudiantes
            .SingleOrDefaultAsync(
                estudianteActual => estudianteActual.Id == id,
                cancellationToken);

        if (estudiante is null)
        {
            return ServiceResult<EstudianteResponseDto>.Fail(
                "El estudiante no existe.",
                ServiceErrorType.NotFound);
        }

        estudiante.Activo = activo;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<EstudianteResponseDto>.Ok(MapToResponse(estudiante));
    }

    private static EstudianteResponseDto MapToResponse(Estudiante estudiante)
    {
        return new EstudianteResponseDto(
            estudiante.Id,
            estudiante.Cedula,
            estudiante.Nombres,
            estudiante.Apellidos,
            estudiante.Activo,
            estudiante.FechaRegistro,
            estudiante.FechaActualizacion);
    }
}
