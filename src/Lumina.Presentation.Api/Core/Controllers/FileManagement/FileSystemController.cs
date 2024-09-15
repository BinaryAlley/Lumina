#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileManagement.FileSystem.Queries;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file systems.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-system")]
public class FileSystemController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public FileSystemController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the type of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-type")]
    public async Task<IActionResult> GetType(CancellationToken cancellationToken)
    {
        var platformType = await _mediator.Send(new GetFileSystemQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(platformType);
    }
    #endregion
}