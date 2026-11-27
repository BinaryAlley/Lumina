#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtist;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}</c> route.
/// </summary>
public class GetArtistEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetArtistQuery, Result<ArtistResponse>> _getArtistQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistEndpoint"/> class.
    /// </summary>
    /// <param name="getArtistQueryHandler">Injected service for handling get artist queries.</param>
    public GetArtistEndpoint(IQueryHandler<GetArtistQuery, Result<ArtistResponse>> getArtistQueryHandler)
    {
        _getArtistQueryHandler = getArtistQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Artists.GET_ARTIST_BY_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets an artist by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        GetArtistQuery query = new(libraryId, artistId);
        Result<ArtistResponse> result = await _getArtistQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
