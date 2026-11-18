using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Calificaciones;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Common;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class CalificacionService : ICalificacionService
{
    private readonly AppDbContext _dbContext;

    public CalificacionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CalificacionResponseDto>> GetAllAsync(
        int profesorId,
        int? estudianteId,
        int? materiaId,
        int? trimestre,
        CancellationToken cancellationToken = default)
    {
        var materiasAsignadas = _dbContext.AsignacionesMaterias
            .Where(asignacion => asignacion.ProfesorId == profesorId)
            .Select(asignacion => asignacion.MateriaId);

        var query = _dbContext.Calificaciones
            .AsNoTracking()
            .Include(calificacion => calificacion.Estudiante)
            .Include(calificacion => calificacion.Materia)
            .Include(calificacion => calificacion.RegistradoPor)
            .Where(calificacion => materiasAsignadas.Contains(calificacion.MateriaId));

        if (estudianteId.HasValue)
        {
            query = query.Where(calificacion => calificacion.EstudianteId == estudianteId.Value);
        }

        if (materiaId.HasValue)
        {
            query = query.Where(calificacion => calificacion.MateriaId == materiaId.Value);
        }

        if (trimestre.HasValue)
        {
            var trimestreFiltro = (Trimestre)trimestre.Value;
            query = query.Where(calificacion => calificacion.Trimestre == trimestreFiltro);
        }

        var calificaciones = await query
            .OrderBy(calificacion => calificacion.Estudiante.Apellidos)
            .ThenBy(calificacion => calificacion.Estudiante.Nombres)
            .ThenBy(calificacion => calificacion.Materia.Nombre)
            .ThenBy(calificacion => calificacion.Trimestre)
            .ToListAsync(cancellationToken);

        return calificaciones.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResult<CalificacionResponseDto>> GetByIdAsync(
        int profesorId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await FindWithDetailsAsync(id, cancellationToken);

        if (calificacion is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La calificación no existe.",
                ServiceErrorType.NotFound);
        }

        if (!await IsMateriaAsignadaAsync(profesorId, calificacion.MateriaId, cancellationToken))
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "No puede consultar calificaciones de materias que no tiene asignadas.",
                ServiceErrorType.Forbidden);
        }

        return ServiceResult<CalificacionResponseDto>.Ok(MapToResponse(calificacion));
    }

    public async Task<ServiceResult<CalificacionResponseDto>> CreateAsync(
        int profesorId,
        CrearCalificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Trimestre is < 1 or > 3)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "El trimestre debe ser 1, 2 o 3.");
        }

        if (request.Nota is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La nota es obligatoria.");
        }

        var nota = request.Nota.Value;

        if (nota < ReglasAcademicas.NotaMinima
            || nota > ReglasAcademicas.NotaMaxima)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La nota debe estar entre 0 y 100.");
        }

        var estudiante = await _dbContext.Estudiantes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                estudianteActual => estudianteActual.Id == request.EstudianteId,
                cancellationToken);

        if (estudiante is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "El estudiante no existe.",
                ServiceErrorType.NotFound);
        }

        if (!estudiante.Activo)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "El estudiante está inactivo y no puede recibir calificaciones.");
        }

        var materia = await _dbContext.Materias
            .AsNoTracking()
            .SingleOrDefaultAsync(
                materiaActual => materiaActual.Id == request.MateriaId,
                cancellationToken);

        if (materia is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La materia no existe.",
                ServiceErrorType.NotFound);
        }

        if (!await IsMateriaAsignadaAsync(profesorId, materia.Id, cancellationToken))
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "No puede registrar calificaciones en materias que no tiene asignadas.",
                ServiceErrorType.Forbidden);
        }

        if (!materia.Activo)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La materia está inactiva y no puede recibir calificaciones.");
        }

        var trimestre = (Trimestre)request.Trimestre;

        var existeCalificacion = await _dbContext.Calificaciones.AnyAsync(
            calificacion => calificacion.EstudianteId == request.EstudianteId
                && calificacion.MateriaId == request.MateriaId
                && calificacion.Trimestre == trimestre,
            cancellationToken);

        if (existeCalificacion)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "Ya existe una calificación para ese estudiante, materia y trimestre. " +
                "Use PUT /api/calificaciones/{id} para corregirla.",
                ServiceErrorType.Conflict);
        }

        var nuevaCalificacion = new Calificacion
        {
            EstudianteId = request.EstudianteId,
            MateriaId = request.MateriaId,
            Trimestre = trimestre,
            Nota = nota,
            RegistradoPorUsuarioId = profesorId
        };

        _dbContext.Calificaciones.Add(nuevaCalificacion);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "Ya existe una calificación para ese estudiante, materia y trimestre.",
                ServiceErrorType.Conflict);
        }

        var creada = await FindWithDetailsAsync(nuevaCalificacion.Id, cancellationToken);

        return ServiceResult<CalificacionResponseDto>.Ok(MapToResponse(creada!));
    }

    public async Task<ServiceResult<CalificacionResponseDto>> UpdateAsync(
        int profesorId,
        int id,
        ActualizarCalificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await _dbContext.Calificaciones
            .Include(calificacionActual => calificacionActual.Estudiante)
            .Include(calificacionActual => calificacionActual.Materia)
            .Include(calificacionActual => calificacionActual.RegistradoPor)
            .SingleOrDefaultAsync(
                calificacionActual => calificacionActual.Id == id,
                cancellationToken);

        if (calificacion is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La calificación no existe.",
                ServiceErrorType.NotFound);
        }

        if (!await IsMateriaAsignadaAsync(profesorId, calificacion.MateriaId, cancellationToken))
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "No puede modificar calificaciones de materias que no tiene asignadas.",
                ServiceErrorType.Forbidden);
        }

        if (calificacion.RegistradoPorUsuarioId != profesorId)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "No puede modificar una calificación registrada por otro profesor.",
                ServiceErrorType.Forbidden);
        }

        if (request.Nota is null)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La nota es obligatoria.");
        }

        var nota = request.Nota.Value;

        if (nota < ReglasAcademicas.NotaMinima
            || nota > ReglasAcademicas.NotaMaxima)
        {
            return ServiceResult<CalificacionResponseDto>.Fail(
                "La nota debe estar entre 0 y 100.");
        }

        calificacion.Nota = nota;
        calificacion.ActualizadoPorUsuarioId = profesorId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CalificacionResponseDto>.Ok(MapToResponse(calificacion));
    }

    private Task<bool> IsMateriaAsignadaAsync(
        int profesorId,
        int materiaId,
        CancellationToken cancellationToken)
    {
        return _dbContext.AsignacionesMaterias.AnyAsync(
            asignacion => asignacion.ProfesorId == profesorId
                && asignacion.MateriaId == materiaId,
            cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number is 2601 or 2627;
    }

    private Task<Calificacion?> FindWithDetailsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Calificaciones
            .AsNoTracking()
            .Include(calificacion => calificacion.Estudiante)
            .Include(calificacion => calificacion.Materia)
            .Include(calificacion => calificacion.RegistradoPor)
            .SingleOrDefaultAsync(
                calificacion => calificacion.Id == id,
                cancellationToken);
    }

    private static CalificacionResponseDto MapToResponse(Calificacion calificacion)
    {
        return new CalificacionResponseDto(
            calificacion.Id,
            calificacion.EstudianteId,
            calificacion.Estudiante.Cedula,
            $"{calificacion.Estudiante.Nombres} {calificacion.Estudiante.Apellidos}".Trim(),
            calificacion.MateriaId,
            calificacion.Materia.Codigo,
            calificacion.Materia.Nombre,
            (int)calificacion.Trimestre,
            calificacion.Nota,
            calificacion.RegistradoPorUsuarioId,
            $"{calificacion.RegistradoPor.Nombres} {calificacion.RegistradoPor.Apellidos}".Trim(),
            calificacion.FechaRegistro,
            calificacion.FechaActualizacion);
    }
}
