using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Profesores;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class ProfesorService : IProfesorService
{
    private readonly AppDbContext _dbContext;

    public ProfesorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MiAsignacionResponseDto>> GetMisAsignacionesAsync(
        int profesorId,
        CancellationToken cancellationToken = default)
    {
        var asignaciones = await _dbContext.AsignacionesMaterias
            .AsNoTracking()
            .Include(asignacion => asignacion.Materia)
            .Where(asignacion => asignacion.ProfesorId == profesorId)
            .OrderBy(asignacion => asignacion.Materia.Nombre)
            .ToListAsync(cancellationToken);

        return asignaciones
            .Select(asignacion => new MiAsignacionResponseDto(
                asignacion.Id,
                asignacion.MateriaId,
                asignacion.Materia.Codigo,
                asignacion.Materia.Nombre,
                asignacion.Materia.Activo,
                asignacion.FechaRegistro))
            .ToList();
    }
}
