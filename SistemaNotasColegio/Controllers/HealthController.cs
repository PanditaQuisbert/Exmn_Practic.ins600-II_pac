using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Data;

namespace SistemaNotasColegio.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var databaseAvailable = await _dbContext.Database.CanConnectAsync(cancellationToken);

        var response = new
        {
            status = databaseAvailable ? "Healthy" : "Unhealthy",
            database = databaseAvailable ? "Connected" : "Unavailable",
            timestamp = DateTime.UtcNow
        };

        return databaseAvailable
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
