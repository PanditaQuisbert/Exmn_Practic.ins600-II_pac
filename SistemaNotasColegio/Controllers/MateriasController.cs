using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Materias;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/materias")]
[Authorize(Policy = AuthorizationPolicies.Secretaria)]
public class MateriasController : ControllerBase
{
    private readonly IMateriaService _materiaService;

    public MateriasController(IMateriaService materiaService)
    {
        _materiaService = materiaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MateriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool incluirInactivas = false,
        CancellationToken cancellationToken = default)
    {
        var materias = await _materiaService.GetAllAsync(
            incluirInactivas,
            cancellationToken);

        return Ok(materias);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MateriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _materiaService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MateriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CrearMateriaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _materiaService.CreateAsync(request, cancellationToken);

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
    [ProducesResponseType(typeof(MateriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarMateriaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _materiaService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(MateriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] CambiarEstadoMateriaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _materiaService.ChangeStatusAsync(
            id,
            request.Activo!.Value,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
