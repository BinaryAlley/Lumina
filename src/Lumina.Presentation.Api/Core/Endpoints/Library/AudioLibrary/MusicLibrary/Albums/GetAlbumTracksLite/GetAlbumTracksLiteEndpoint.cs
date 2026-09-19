#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbumTracksLite;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/{albumId}/tracks/lite</c> route.
/// </summary>
public class GetAlbumTracksLiteEndpoint : BaseEndpoint<GetAlbumTracksLiteRequest, IResult>
{
    private readonly IQueryHandler<GetAlbumTracksLiteQuery, Result<PaginatedResponse<TrackLiteResponse>>> _getAlbumTracksLiteQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksLiteEndpoint"/> class.
    /// </summary>
    /// <param name="getAlbumTracksLiteQueryHandler">Injected service for handling get album tracks lite queries.</param>
    public GetAlbumTracksLiteEndpoint(IQueryHandler<GetAlbumTracksLiteQuery, Result<PaginatedResponse<TrackLiteResponse>>> getAlbumTracksLiteQueryHandler)
    {
        _getAlbumTracksLiteQueryHandler = getAlbumTracksLiteQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_ALBUM_TRACKS_LITE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the lightweight read models of all the tracks of the album identified by the route.
    /// </summary>
    /// <param name="request">The request containing the pagination options of the tracks to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAlbumTracksLiteRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        Result<PaginatedResponse<TrackLiteResponse>> result = await _getAlbumTracksLiteQueryHandler.HandleAsync(request.ToQuery(libraryId, artistId, albumId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
