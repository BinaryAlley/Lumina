#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileManagement.Files.Queries.GetFiles;
using Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.ModelBinders;
using Lumina.Presentation.Api.Core.Controllers.Common;
using MapsterMapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system files.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FilesController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public FilesController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the files of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the files.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-tree-files")]
    public async IAsyncEnumerable<FileSystemTreeNodeResponse> GetTreeFiles([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, 
        [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _mediator.Send(new GetTreeFilesQuery(path, includeHiddenElements), cancellationToken).ConfigureAwait(false);
        foreach (FileSystemTreeNodeResponse file in result.Value)
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
    public async IAsyncEnumerable<FileResponse> GetFiles([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, 
        [FromQuery] bool includeHiddenElements, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileResponse>> result = await _mediator.Send(new GetFilesQuery(path, includeHiddenElements), cancellationToken).ConfigureAwait(false);
        if (!result.IsError)
        {
            foreach (FileResponse file in result.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                yield return file;
            }
        }
    }
    #endregion
}
