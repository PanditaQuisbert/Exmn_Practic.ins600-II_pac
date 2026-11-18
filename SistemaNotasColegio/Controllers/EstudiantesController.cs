using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Estudiantes;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/estudiantes")]
[Authorize(Policy = AuthorizationPolicies.Secretaria)]
public class EstudiantesController : ControllerBase
{
    private readonly IEstudianteService _estudianteService;

    public EstudiantesController(IEstudianteService estudianteService)
    {
        _estudianteService = estudianteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstudianteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool incluirInactivos = false,
        CancellationToken cancellationToken = default)
    {
        var estudiantes = await _estudianteService.GetAllAsync(
            incluirInactivos,
            cancellationToken);

        return Ok(estudiantes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EstudianteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _estudianteService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EstudianteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CrearEstudianteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _estudianteService.CreateAsync(request, cancellationToken);

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
    [ProducesResponseType(typeof(EstudianteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarEstudianteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _estudianteService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(EstudianteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] CambiarEstadoEstudianteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _estudianteService.ChangeStatusAsync(
            id,
            request.Activo!.Value,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
