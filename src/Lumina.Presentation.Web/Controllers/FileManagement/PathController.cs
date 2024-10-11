#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system paths.
/// </summary>
[Route("[controller]")]
public class PathController : Controller
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IApiHttpClient _apiHttpClient;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="PathController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public PathController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the root of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-root")]
    public async Task<IActionResult> GetPathRoot([FromQuery] string path, CancellationToken cancellationToken)
    {
        PathSegmentModel response = await _apiHttpClient.GetAsync<PathSegmentModel>($"path/get-path-root?path={Uri.EscapeDataString(path)}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { root = response } });
    }

    /// <summary>
    /// Gets the path separator character of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-separator")]
    public async Task<IActionResult> GetPathSeparator(CancellationToken cancellationToken)
    {
        PathSeparatorModel response = await _apiHttpClient.GetAsync<PathSeparatorModel>($"path/get-path-separator", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { pathSeparator = response.Separator } });
    }

    /// <summary>
    /// Gets the path separator character of the file system.
    /// </summary>
    /// <param name="path">The path for which to get the parent path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-parent")]
    public async Task<IActionResult> GetPathParent([FromQuery] string path, CancellationToken cancellationToken)
    {
        PathSegmentModel[] response = await _apiHttpClient.GetAsync<PathSegmentModel[]>($"path/get-path-parent?path={Uri.EscapeDataString(path)}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { pathSegments = response } });
    }

    /// <summary>
    /// Gets the path components of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the path segments.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("split")]
    public async Task<IActionResult> SplitPath([FromQuery] string path, CancellationToken cancellationToken)
    {
        PathSegmentModel[] response = await _apiHttpClient.GetAsync<PathSegmentModel[]>($"path/split?path={Uri.EscapeDataString(path)}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { pathSegments = response } });
    }

    /// <summary>
    /// Validates the validity of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to be validated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidatePath([FromQuery] string path, CancellationToken cancellationToken)
    {
        PathValidModel response = await _apiHttpClient.GetAsync<PathValidModel>($"path/validate?path={Uri.EscapeDataString(path)}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { isValid = response.IsValid } });
    }

    /// <summary>
    /// Checks whether <paramref name="path"/> exists or not.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("check-path-exists")]
    public async Task<IActionResult> CheckPathExists([FromQuery] string path, [FromQuery] bool includeHiddenElements, CancellationToken cancellationToken)
    {
        PathExistsModel response = await _apiHttpClient.GetAsync<PathExistsModel>($"path/check-path-exists?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { exists = response.Exists } });
    }
    #endregion
}
