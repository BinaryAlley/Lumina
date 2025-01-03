#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileSystemManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileSystemManagement;

/// <summary>
/// Controller for managing file system drives.
/// </summary>
[Authorize]
[Route("drives")]
public class DrivesController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrivesController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DrivesController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Controller action for getting the list of all file system drives.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-drives")]
    public async Task<IActionResult> GetDrives(CancellationToken cancellationToken)
    {
        FileSystemTreeNodeModel[] response = await _apiHttpClient.GetAsync<FileSystemTreeNodeModel[]>($"drives/get-drives", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = new { drives = response } });
    }
}
