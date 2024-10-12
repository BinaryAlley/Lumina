#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.FileManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
#endregion

namespace Lumina.Presentation.Web.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system files.
/// </summary>
[Route("[controller]")]
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
    public async IAsyncEnumerable<FileSystemTreeNodeModel?> GetTreeFiles([FromQuery] string path, [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<FileSystemTreeNodeModel?> response = _apiHttpClient.GetAsyncEnumerable<FileSystemTreeNodeModel>(
            $"files/get-tree-files?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        await foreach (FileSystemTreeNodeModel? file in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return file;
        }
    }

    /// <summary>
    /// Gets the files of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the files.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-files")]
    public async IAsyncEnumerable<FileModel?> GetFiles([FromQuery] string path, [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<FileModel?> response = _apiHttpClient.GetAsyncEnumerable<FileModel>(
           $"files/get-files?path={Uri.EscapeDataString(path)}&includeHiddenElements={includeHiddenElements}", cancellationToken: cancellationToken);
        await foreach (FileModel? file in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return file;
        }
    }
}
