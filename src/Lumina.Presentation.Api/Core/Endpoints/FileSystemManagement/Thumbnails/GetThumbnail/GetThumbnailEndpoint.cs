#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Common.Utilities;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly IQueryHandler<GetThumbnailQuery, Result<ThumbnailResponse>> _getThumbnailQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpoint"/> class.
    /// </summary>
    /// <param name="getThumbnailQueryHandler">Injected service for handling get thumbnail queries.</param>
    public GetThumbnailEndpoint(IQueryHandler<GetThumbnailQuery, Result<ThumbnailResponse>> getThumbnailQueryHandler)
    {
        _getThumbnailQueryHandler = getThumbnailQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Thumbnails.GET_THUMBNAIL);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the thumbnail of the file located at the path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path of the file for which to get the thumbnail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThumbnailRequest request, CancellationToken cancellationToken)
    {
        Result<ThumbnailResponse> result = await _getThumbnailQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.File(success.Bytes, MimeTypes.GetMimeType(success.Type)), Problem);
    }
}
