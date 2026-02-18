using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "weatherapp-api"
        });
    }

    /// <summary>
    /// Detailed health check with database connectivity
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetailed([FromServices] Data.AppDbContext dbContext)
    {
        var healthStatus = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "weatherapp-api",
            checks = new Dictionary<string, object>()
        };

        try
        {
            // Check database connectivity
            var canConnect = await dbContext.Database.CanConnectAsync();
            healthStatus.checks.Add("database", new
            {
                status = canConnect ? "healthy" : "unhealthy",
                message = canConnect ? "Database connection successful" : "Database connection failed"
            });

            if (!canConnect)
            {
                return StatusCode(503, healthStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            healthStatus.checks.Add("database", new
            {
                status = "unhealthy",
                message = $"Database check failed: {ex.Message}"
            });
            return StatusCode(503, healthStatus);
        }

        return Ok(healthStatus);
    }
}
