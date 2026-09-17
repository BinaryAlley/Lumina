#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistAlbums;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums</c> route.
/// </summary>
public class GetArtistAlbumsEndpoint : BaseEndpoint<GetArtistAlbumsRequest, IResult>
{
    private readonly IQueryHandler<GetArtistAlbumsQuery, Result<IReadOnlyList<AlbumResponse>>> _getArtistAlbumsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsEndpoint"/> class.
    /// </summary>
    /// <param name="getArtistAlbumsQueryHandler">Injected service for handling get artist albums queries.</param>
    public GetArtistAlbumsEndpoint(IQueryHandler<GetArtistAlbumsQuery, Result<IReadOnlyList<AlbumResponse>>> getArtistAlbumsQueryHandler)
    {
        _getArtistAlbumsQueryHandler = getArtistAlbumsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_ARTIST_ALBUMS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the albums of the artist identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the id of the artist whose albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtistAlbumsRequest request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<AlbumResponse>> result = await _getArtistAlbumsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
