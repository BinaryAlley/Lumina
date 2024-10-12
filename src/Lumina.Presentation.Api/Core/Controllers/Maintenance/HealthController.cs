#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Core.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.Maintenance;

/// <summary>
/// Controller for health checks.
/// </summary>
[Route("[controller]")]
public class HealthController : ApiController
{
    /// <summary>
    /// Checks if the application is healthy.
    /// </summary>
    [HttpGet("check-health")]
    public async Task<IActionResult> CheckHealth()
    {
        // TODO: to be implemented
        return await Task.FromResult(Ok());
    }
}
