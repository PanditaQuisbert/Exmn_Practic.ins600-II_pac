using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Asignaciones;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/asignaciones")]
[Authorize(Policy = AuthorizationPolicies.Secretaria)]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionService _asignacionService;

    public AsignacionesController(IAsignacionService asignacionService)
    {
        _asignacionService = asignacionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AsignacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? profesorId,
        [FromQuery] int? materiaId,
        CancellationToken cancellationToken)
    {
        var asignaciones = await _asignacionService.GetAllAsync(
            profesorId,
            materiaId,
            cancellationToken);

        return Ok(asignaciones);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AsignacionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CrearAsignacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _asignacionService.CreateAsync(request, cancellationToken);

        if (!result.Success)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetAll), result.Data);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _asignacionService.DeleteAsync(id, cancellationToken);

        if (!result.Success)
        {
            return this.ToActionResult(result);
        }

        return NoContent();
    }
}
