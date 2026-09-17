#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbumTracks;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/{albumId}/tracks</c> route.
/// </summary>
public class GetAlbumTracksEndpoint : BaseEndpoint<GetAlbumTracksRequest, IResult>
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
    /// Gets the list of all the tracks of the album identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the id of the album whose tracks are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAlbumTracksRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<TrackResponse>> result = await _getAlbumTracksQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
