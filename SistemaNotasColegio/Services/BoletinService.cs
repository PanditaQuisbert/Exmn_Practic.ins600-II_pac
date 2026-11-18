using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Boletines;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class BoletinService : IBoletinService
{
    private readonly AppDbContext _dbContext;

    public BoletinService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Construye el boletín de un estudiante.
    /// Regla de negocio: los estudiantes inactivos sí pueden consultarse porque su
    /// historial académico debe permanecer disponible; solo un estudiante inexistente
    /// devuelve 404.
    /// </summary>
    public async Task<ServiceResult<BoletinEstudianteResponseDto>> GetByEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await _dbContext.Estudiantes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                estudianteActual => estudianteActual.Id == estudianteId,
                cancellationToken);

        if (estudiante is null)
        {
            return ServiceResult<BoletinEstudianteResponseDto>.Fail(
                "El estudiante no existe.",
                ServiceErrorType.NotFound);
        }

        var calificaciones = await _dbContext.Calificaciones
            .AsNoTracking()
            .Where(calificacion => calificacion.EstudianteId == estudianteId)
            .Select(calificacion => new
            {
                calificacion.MateriaId,
                calificacion.Trimestre,
                calificacion.Nota
            })
            .ToListAsync(cancellationToken);

        var materiasConNotas = calificaciones
            .Select(calificacion => calificacion.MateriaId)
            .Distinct()
            .ToList();

        var materias = await _dbContext.Materias
            .AsNoTracking()
            .Where(materia => materia.Activo || materiasConNotas.Contains(materia.Id))
            .OrderBy(materia => materia.Nombre)
            .Select(materia => new
            {
                materia.Id,
                materia.Codigo,
                materia.Nombre
            })
            .ToListAsync(cancellationToken);

        var notasPorMateria = calificaciones.ToLookup(calificacion => calificacion.MateriaId);
        var filas = new List<BoletinMateriaDto>(materias.Count);

        foreach (var materia in materias)
        {
            var notas = notasPorMateria[materia.Id]
                .ToDictionary(calificacion => (int)calificacion.Trimestre, calificacion => calificacion.Nota);

            decimal? primerTrimestre = notas.TryGetValue(1, out var notaPrimero) ? notaPrimero : null;
            decimal? segundoTrimestre = notas.TryGetValue(2, out var notaSegundo) ? notaSegundo : null;
            decimal? tercerTrimestre = notas.TryGetValue(3, out var notaTercero) ? notaTercero : null;

            var promedio = ReglasAcademicas.CalcularPromedioAnual(
                primerTrimestre,
                segundoTrimestre,
                tercerTrimestre);

            filas.Add(new BoletinMateriaDto(
                materia.Id,
                materia.Codigo,
                materia.Nombre,
                primerTrimestre,
                segundoTrimestre,
                tercerTrimestre,
                promedio,
                ReglasAcademicas.CalcularEstado(promedio)));
        }

        var response = new BoletinEstudianteResponseDto(
            estudiante.Id,
            estudiante.Cedula,
            estudiante.Nombres,
            estudiante.Apellidos,
            estudiante.Activo,
            filas);

        return ServiceResult<BoletinEstudianteResponseDto>.Ok(response);
    }
}
