#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Common.Utilities;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// API endpoint for the <c>/thumbnails/get-thumbnail</c> route.
/// </summary>
public class GetThumbnailEndpoint : BaseEndpoint<GetThumbnailRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetThumbnailEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Thumbnails.GET_THUMBNAIL);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the thumbnail of the file located at the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path of the file for which to get the thumbnail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThumbnailRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<ThumbnailResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.File(success.Bytes, MimeTypes.GetMimeType(success.Type)), Problem);
    }
}
