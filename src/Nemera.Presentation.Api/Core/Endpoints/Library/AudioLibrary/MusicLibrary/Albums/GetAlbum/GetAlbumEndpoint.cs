#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbum;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}</c> route.
/// </summary>
public class GetAlbumEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetAlbumQuery, Result<AlbumResponse>> _getAlbumQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="getAlbumQueryHandler">Injected service for handling get album queries.</param>
    public GetAlbumEndpoint(IQueryHandler<GetAlbumQuery, Result<AlbumResponse>> getAlbumQueryHandler)
    {
        _getAlbumQueryHandler = getAlbumQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_ALBUM_BY_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets an album by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        GetAlbumQuery query = new(libraryId, artistId, albumId);
        Result<AlbumResponse> result = await _getAlbumQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
