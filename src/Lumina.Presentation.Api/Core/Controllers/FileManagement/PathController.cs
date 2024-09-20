#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileManagement.Paths.Commands;
using Lumina.Application.Core.FileManagement.Paths.Queries;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.ModelBinders;
using Lumina.Presentation.Api.Core.Controllers.Common;
using MapsterMapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system paths.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PathController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="PathController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public PathController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion
    
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the root of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-root")]
    public async Task<IActionResult> GetPathRoot([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, CancellationToken cancellationToken)
    {
        ErrorOr<PathSegmentResponse> result = await _mediator.Send(new GetPathRootQuery(path), cancellationToken).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }

    /// <summary>
    /// Gets the path separator character of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-separator")]
    public async Task<IActionResult> GetPathSeparator(CancellationToken cancellationToken)
    {
        PathSeparatorResponse result = await _mediator.Send(new GetPathSeparatorQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Gets the path separator character of the file system.
    /// </summary>
    /// <param name="path">The path for which to get the parent path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-path-parent")]
    public async Task<IActionResult> GetPathParent([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _mediator.Send(new GetPathParentQuery(path), cancellationToken).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }

    /// <summary>
    /// Combines <paramref name="originalPath"/> with <paramref name="newPath"/>.
    /// </summary>
    /// <param name="originalPath">The path to combine to.</param>
    /// <param name="newPath">The path to combine with.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("combine")]
    public async Task<IActionResult> CombinePath([FromQuery, ModelBinder(typeof(UrlStringBinder))] string originalPath, [FromQuery, ModelBinder(typeof(UrlStringBinder))] string newPath, CancellationToken cancellationToken)
    {
        ErrorOr<PathSegmentResponse> result = await _mediator.Send(new CombinePathCommand(originalPath, newPath), cancellationToken).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }

    /// <summary>
    /// Gets the path components of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the path segments.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("split")]
    public async Task<IActionResult> SplitPath([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _mediator.Send(new SplitPathCommand(path), cancellationToken).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }

    /// <summary>
    /// Validates the validity of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to be validated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidatePath([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, CancellationToken cancellationToken)
    {
        PathValidResponse result = await _mediator.Send(new ValidatePathQuery(path), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Checks whether <paramref name="path"/> exists or not.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("check-path-exists")]
    public async Task<IActionResult> CheckPathExists([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, CancellationToken cancellationToken)
    {
        PathExistsResponse result = await _mediator.Send(new CheckPathExistsQuery(path), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
    #endregion
}