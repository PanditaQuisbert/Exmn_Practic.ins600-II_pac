using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Materias;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class MateriaService : IMateriaService
{
    private readonly AppDbContext _dbContext;

    public MateriaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MateriaResponseDto>> GetAllAsync(
        bool incluirInactivas,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Materias
            .AsNoTracking()
            .AsQueryable();

        if (!incluirInactivas)
        {
            query = query.Where(materia => materia.Activo);
        }

        var materias = await query
            .OrderBy(materia => materia.Nombre)
            .ToListAsync(cancellationToken);

        return materias.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResult<MateriaResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var materia = await _dbContext.Materias
            .AsNoTracking()
            .SingleOrDefaultAsync(
                materiaActual => materiaActual.Id == id,
                cancellationToken);

        if (materia is null)
        {
            return ServiceResult<MateriaResponseDto>.Fail(
                "La materia no existe.",
                ServiceErrorType.NotFound);
        }

        return ServiceResult<MateriaResponseDto>.Ok(MapToResponse(materia));
    }

    public async Task<ServiceResult<MateriaResponseDto>> CreateAsync(
        CrearMateriaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var codigo = request.Codigo.Trim();

        if (await _dbContext.Materias.AnyAsync(
                materia => materia.Codigo == codigo,
                cancellationToken))
        {
            return ServiceResult<MateriaResponseDto>.Fail(
                "Ya existe una materia con ese código.",
                ServiceErrorType.Conflict);
        }

        var materia = new Materia
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Activo = true
        };

        _dbContext.Materias.Add(materia);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<MateriaResponseDto>.Ok(MapToResponse(materia));
    }

    public async Task<ServiceResult<MateriaResponseDto>> UpdateAsync(
        int id,
        ActualizarMateriaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var materia = await _dbContext.Materias
            .SingleOrDefaultAsync(
                materiaActual => materiaActual.Id == id,
                cancellationToken);

        if (materia is null)
        {
            return ServiceResult<MateriaResponseDto>.Fail(
                "La materia no existe.",
                ServiceErrorType.NotFound);
        }

        var codigo = request.Codigo.Trim();

        if (await _dbContext.Materias.AnyAsync(
                materiaActual => materiaActual.Codigo == codigo
                    && materiaActual.Id != id,
                cancellationToken))
        {
            return ServiceResult<MateriaResponseDto>.Fail(
                "Ya existe una materia con ese código.",
                ServiceErrorType.Conflict);
        }

        materia.Codigo = codigo;
        materia.Nombre = request.Nombre.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<MateriaResponseDto>.Ok(MapToResponse(materia));
    }

    public async Task<ServiceResult<MateriaResponseDto>> ChangeStatusAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        var materia = await _dbContext.Materias
            .SingleOrDefaultAsync(
                materiaActual => materiaActual.Id == id,
                cancellationToken);

        if (materia is null)
        {
            return ServiceResult<MateriaResponseDto>.Fail(
                "La materia no existe.",
                ServiceErrorType.NotFound);
        }

        materia.Activo = activo;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<MateriaResponseDto>.Ok(MapToResponse(materia));
    }

    private static MateriaResponseDto MapToResponse(Materia materia)
    {
        return new MateriaResponseDto(
            materia.Id,
            materia.Codigo,
            materia.Nombre,
            materia.Activo,
            materia.FechaRegistro,
            materia.FechaActualizacion);
    }
}
