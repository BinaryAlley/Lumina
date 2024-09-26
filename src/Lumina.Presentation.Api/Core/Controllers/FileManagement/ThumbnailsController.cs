#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.ModelBinders;
using Lumina.Presentation.Api.Common.Utilities;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system items thumbnails.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ThumbnailsController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailsController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public ThumbnailsController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the thumbnail of a file identified by of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path of the file for which to get the thumbnail.</param>
    /// <param name="quality">The quality to use for the thumbnail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-thumbnail")]
    public async Task<IActionResult> GetThumbnail([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, [FromQuery] int quality, CancellationToken cancellationToken)
    {
        ErrorOr<ThumbnailResponse> getResult = await _mediator.Send(new GetThumbnailQuery(path, quality), cancellationToken);
        return getResult.Match(result => File(result.Bytes, MimeTypes.GetMimeType(result.Type)), errors => Problem(errors));
    }
    #endregion
}
