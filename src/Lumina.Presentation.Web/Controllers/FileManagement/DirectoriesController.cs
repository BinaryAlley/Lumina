#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system directories.
/// </summary>
[Route("[controller]")]
public class DirectoriesController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoriesController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DirectoriesController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Gets the directories of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the directories.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-directories")]
    public async IAsyncEnumerable<DirectoryModel?> GetDirectories([FromQuery] string path, [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<DirectoryModel?> response = _apiHttpClient.GetAsyncEnumerable<DirectoryModel>(
            $"directories/get-directories?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        await foreach (DirectoryModel? directory in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return directory;
        }
    }

    /// <summary>
    /// Gets the directories of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the directories.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-tree-directories")]
    public async IAsyncEnumerable<FileSystemTreeNodeModel?> GetTreeDirectories([FromQuery] string path, [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<FileSystemTreeNodeModel?> response = _apiHttpClient.GetAsyncEnumerable<FileSystemTreeNodeModel>(
            $"directories/get-tree-directories?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        await foreach (FileSystemTreeNodeModel? directory in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return directory;
        }
    }

    /// <summary>
    /// Gets the tree of expanded directories leading up to <paramref name="path"/>, with the additional list of drives, and children of the last child directory.
    /// </summary>
    /// <param name="path">The path for which to get the directory tree.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-directory-tree")]
    public async Task<IActionResult> GetDirectoryTree([FromQuery] string path, [FromQuery] bool includeFiles, [FromQuery] bool includeHiddenElements, CancellationToken cancellationToken)
    {
        FileSystemTreeNodeModel[] response = await _apiHttpClient.GetAsync<FileSystemTreeNodeModel[]>($"directories/get-directory-tree?path={Uri.EscapeDataString(path)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        return Json(new { success = true, data = new { tree = response } });
    }
}
