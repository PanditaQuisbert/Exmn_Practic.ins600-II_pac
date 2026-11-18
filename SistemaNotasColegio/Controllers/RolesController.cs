using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.DTOs.Roles;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = AuthorizationPolicies.Secretaria)]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _rolService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }
}
