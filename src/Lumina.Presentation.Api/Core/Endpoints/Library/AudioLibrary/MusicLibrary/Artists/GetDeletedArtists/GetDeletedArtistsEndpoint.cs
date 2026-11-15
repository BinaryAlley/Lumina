#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetDeletedArtists;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/deleted-items</c> route.
/// </summary>
public class GetDeletedArtistsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetDeletedArtistsQuery, Result<IReadOnlyList<DeletedArtistResponse>>> _getDeletedArtistsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedArtistsEndpoint"/> class.
    /// </summary>
    /// <param name="getDeletedArtistsQueryHandler">Injected service for handling get deleted artists queries.</param>
    public GetDeletedArtistsEndpoint(IQueryHandler<GetDeletedArtistsQuery, Result<IReadOnlyList<DeletedArtistResponse>>> getDeletedArtistsQueryHandler)
    {
        _getDeletedArtistsQueryHandler = getDeletedArtistsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Artists.GET_DELETED_ARTISTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the deleted artists of the library identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier of the library from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<IReadOnlyList<DeletedArtistResponse>> result = await _getDeletedArtistsQueryHandler.HandleAsync(new GetDeletedArtistsQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
