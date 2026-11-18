using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Profesores;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/profesores")]
[Authorize(Policy = AuthorizationPolicies.Profesor)]
public class ProfesoresController : ControllerBase
{
    private readonly IProfesorService _profesorService;

    public ProfesoresController(IProfesorService profesorService)
    {
        _profesorService = profesorService;
    }

    [HttpGet("mis-asignaciones")]
    [ProducesResponseType(typeof(IReadOnlyList<MiAsignacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMisAsignaciones(CancellationToken cancellationToken)
    {
        var profesorId = User.GetUsuarioId();

        if (profesorId is null)
        {
            return Unauthorized(new { mensaje = "El token no contiene un usuario válido." });
        }

        var asignaciones = await _profesorService.GetMisAsignacionesAsync(
            profesorId.Value,
            cancellationToken);

        return Ok(asignaciones);
    }
}
