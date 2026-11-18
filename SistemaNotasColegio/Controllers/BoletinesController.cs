using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Boletines;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/boletines")]
[Authorize(Policy = AuthorizationPolicies.Secretaria)]
public class BoletinesController : ControllerBase
{
    private readonly IBoletinService _boletinService;

    public BoletinesController(IBoletinService boletinService)
    {
        _boletinService = boletinService;
    }

    [HttpGet("estudiantes/{estudianteId:int}")]
    [ProducesResponseType(typeof(BoletinEstudianteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEstudiante(
        int estudianteId,
        CancellationToken cancellationToken)
    {
        var result = await _boletinService.GetByEstudianteAsync(
            estudianteId,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
