#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistsLite;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/lite</c> route.
/// </summary>
public class GetArtistsLiteEndpoint : BaseEndpoint<GetArtistsLiteRequest, IResult>
{
    private readonly IQueryHandler<GetArtistsLiteQuery, Result<PaginatedResponse<ArtistLiteResponse>>> _getArtistsLiteQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsLiteEndpoint"/> class.
    /// </summary>
    /// <param name="getArtistsLiteQueryHandler">Injected service for handling get artists lite queries.</param>
    public GetArtistsLiteEndpoint(IQueryHandler<GetArtistsLiteQuery, Result<PaginatedResponse<ArtistLiteResponse>>> getArtistsLiteQueryHandler)
    {
        _getArtistsLiteQueryHandler = getArtistsLiteQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Artists.GET_ARTISTS_LITE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the lightweight read models of all the artists of the media library identified by the route.
    /// </summary>
    /// <param name="request">The request containing the pagination and filtering options of the artists to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtistsLiteRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<PaginatedResponse<ArtistLiteResponse>> result = await _getArtistsLiteQueryHandler.HandleAsync(request.ToQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
