#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbumTracks;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks</c> route.
/// </summary>
public class GetAlbumTracksEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetAlbumTracksQuery, Result<IReadOnlyList<TrackResponse>>> _getAlbumTracksQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksEndpoint"/> class.
    /// </summary>
    /// <param name="getAlbumTracksQueryHandler">Injected service for handling get album tracks queries.</param>
    public GetAlbumTracksEndpoint(IQueryHandler<GetAlbumTracksQuery, Result<IReadOnlyList<TrackResponse>>> getAlbumTracksQueryHandler)
    {
        _getAlbumTracksQueryHandler = getAlbumTracksQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_ALBUM_TRACKS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the tracks of the album identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        GetAlbumTracksQuery query = new(libraryId, artistId, albumId);
        Result<IReadOnlyList<TrackResponse>> result = await _getAlbumTracksQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
