#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileManagement.Files.Queries;
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
    public FilesController(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the files of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the files.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-files")]
    public async IAsyncEnumerable<FileSystemTreeNodeResponse> GetFiles([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFilesQuery(path), cancellationToken).ConfigureAwait(false);
        foreach (var file in result.Value)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return file;
        }
    }
    #endregion
}