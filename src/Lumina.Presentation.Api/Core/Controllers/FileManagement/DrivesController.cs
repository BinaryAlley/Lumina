#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileManagement.Drives.Queries;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system drives.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DrivesController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DrivesController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public DrivesController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Controller action for getting the list of all file system drives.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-drives")]
    public async Task<IActionResult> GetDrives(CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _mediator.Send(new GetDrivesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }
    #endregion
}