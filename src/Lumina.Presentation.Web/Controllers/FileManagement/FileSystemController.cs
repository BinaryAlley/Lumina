#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileManagement;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileManagement;

/// <summary>
/// Controller for managing file systems.
/// </summary>
[Route("/file-system")]
public class FileSystemController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public FileSystemController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Gets the type of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-type")]
    public async Task<IActionResult> GetType(CancellationToken cancellationToken)
    {
        FileSystemTypeModel response = await _apiHttpClient.GetAsync<FileSystemTypeModel>($"file-system/get-type", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { platformType = response.PlatformType.ToString() } });
    }
}
