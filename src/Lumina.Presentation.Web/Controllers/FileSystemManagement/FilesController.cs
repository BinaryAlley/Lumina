#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileSystemManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileSystemManagement;

/// <summary>
/// Controller for managing file system files.
/// </summary>
[Authorize]
[Route("files")]
public class FilesController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public FilesController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Gets the files of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the files.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-tree-files")]
    public async Task<IActionResult> GetTreeFiles([FromQuery] string path, [FromQuery] bool includeHiddenElements, CancellationToken cancellationToken)
    {
        IEnumerable<FileSystemTreeNodeModel> response = await _apiHttpClient.GetAsync<FileSystemTreeNodeModel[]>(
            $"files/get-tree-files?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the files of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the files.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-files")]
    public async Task<IActionResult> GetFiles([FromQuery] string path, [FromQuery] bool includeHiddenElements, CancellationToken cancellationToken)
    {
        IEnumerable<FileModel> response = await _apiHttpClient.GetAsync<FileModel[]>(
           $"files/get-files?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }
}
