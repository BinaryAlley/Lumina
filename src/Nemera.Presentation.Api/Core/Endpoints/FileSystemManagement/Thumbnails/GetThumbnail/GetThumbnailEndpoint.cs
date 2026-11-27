#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.FileSystemManagement.Files;
using Nemera.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Nemera.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Nemera.Contracts.Requests.FileSystemManagement.Thumbnails;
using Nemera.Contracts.Responses.FileSystemManagement.Thumbnails;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.MimeTypes;
using Nemera.Presentation.Api.Common.Routes.FileSystemManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

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
        return result.Match(success => TypedResults.File(success.Bytes, ImageMimeTypes.GetMimeType(success.Type)), Problem);
    }
}
