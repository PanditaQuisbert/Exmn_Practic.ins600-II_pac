using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Calificaciones;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/calificaciones")]
[Authorize(Policy = AuthorizationPolicies.Profesor)]
public class CalificacionesController : ControllerBase
{
    private readonly ICalificacionService _calificacionService;

    public CalificacionesController(ICalificacionService calificacionService)
    {
        _calificacionService = calificacionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CalificacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery, Range(1, int.MaxValue)] int? estudianteId,
        [FromQuery, Range(1, int.MaxValue)] int? materiaId,
        [FromQuery, Range(1, 3)] int? trimestre,
        CancellationToken cancellationToken)
    {
        var profesorId = User.GetUsuarioId();

        if (profesorId is null)
        {
            return Unauthorized(new { mensaje = "El token no contiene un usuario válido." });
        }

        var calificaciones = await _calificacionService.GetAllAsync(
            profesorId.Value,
            estudianteId,
            materiaId,
            trimestre,
            cancellationToken);

        return Ok(calificaciones);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CalificacionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var profesorId = User.GetUsuarioId();

        if (profesorId is null)
        {
            return Unauthorized(new { mensaje = "El token no contiene un usuario válido." });
        }

        var result = await _calificacionService.GetByIdAsync(
            profesorId.Value,
            id,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CalificacionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CrearCalificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var profesorId = User.GetUsuarioId();

        if (profesorId is null)
        {
            return Unauthorized(new { mensaje = "El token no contiene un usuario válido." });
        }

        var result = await _calificacionService.CreateAsync(
            profesorId.Value,
            request,
            cancellationToken);

        if (!result.Success)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CalificacionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarCalificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var profesorId = User.GetUsuarioId();

        if (profesorId is null)
        {
            return Unauthorized(new { mensaje = "El token no contiene un usuario válido." });
        }

        var result = await _calificacionService.UpdateAsync(
            profesorId.Value,
            id,
            request,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
