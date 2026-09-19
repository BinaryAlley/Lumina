#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistAlbumsLite;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/lite</c> route.
/// </summary>
public class GetArtistAlbumsLiteEndpoint : BaseEndpoint<GetArtistAlbumsLiteRequest, IResult>
{
    private readonly IQueryHandler<GetArtistAlbumsLiteQuery, Result<PaginatedResponse<AlbumLiteResponse>>> _getArtistAlbumsLiteQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsLiteEndpoint"/> class.
    /// </summary>
    /// <param name="getArtistAlbumsLiteQueryHandler">Injected service for handling get artist albums lite queries.</param>
    public GetArtistAlbumsLiteEndpoint(IQueryHandler<GetArtistAlbumsLiteQuery, Result<PaginatedResponse<AlbumLiteResponse>>> getArtistAlbumsLiteQueryHandler)
    {
        _getArtistAlbumsLiteQueryHandler = getArtistAlbumsLiteQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_ARTIST_ALBUMS_LITE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the lightweight read models of all the albums of the artist identified by the route.
    /// </summary>
    /// <param name="request">The request containing the pagination options of the albums to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtistAlbumsLiteRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _getArtistAlbumsLiteQueryHandler.HandleAsync(request.ToQuery(libraryId, artistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
