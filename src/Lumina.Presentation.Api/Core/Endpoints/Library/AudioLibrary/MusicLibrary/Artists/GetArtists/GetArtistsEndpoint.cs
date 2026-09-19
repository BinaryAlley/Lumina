#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;
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

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;

/// <summary>
/// API endpoint for the <c>/artists</c> route.
/// </summary>
public class GetArtistsEndpoint : BaseEndpoint<GetArtistsRequest, IResult>
{
    private readonly IQueryHandler<GetArtistsQuery, Result<PaginatedResponse<ArtistResponse>>> _getArtistsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsEndpoint"/> class.
    /// </summary>
    /// <param name="getArtistsQueryHandler">Injected service for handling get artists queries.</param>
    public GetArtistsEndpoint(IQueryHandler<GetArtistsQuery, Result<PaginatedResponse<ArtistResponse>>> getArtistsQueryHandler)
    {
        _getArtistsQueryHandler = getArtistsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Artists.GET_ARTISTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the artists of the media library identified by the route.
    /// </summary>
    /// <param name="request">The request containing the pagination and filtering options of the artists to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtistsRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<PaginatedResponse<ArtistResponse>> result = await _getArtistsQueryHandler.HandleAsync(request.ToQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
