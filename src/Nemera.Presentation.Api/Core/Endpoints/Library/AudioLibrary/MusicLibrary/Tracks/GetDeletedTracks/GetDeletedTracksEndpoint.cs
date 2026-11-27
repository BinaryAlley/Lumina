#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetDeletedTracks;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/tracks/deleted-items</c> route.
/// </summary>
public class GetDeletedTracksEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetDeletedTracksQuery, Result<IReadOnlyList<DeletedTrackResponse>>> _getDeletedTracksQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksEndpoint"/> class.
    /// </summary>
    /// <param name="getDeletedTracksQueryHandler">Injected service for handling get deleted tracks queries.</param>
    public GetDeletedTracksEndpoint(IQueryHandler<GetDeletedTracksQuery, Result<IReadOnlyList<DeletedTrackResponse>>> getDeletedTracksQueryHandler)
    {
        _getDeletedTracksQueryHandler = getDeletedTracksQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Tracks.GET_DELETED_TRACKS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the deleted tracks of the library identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier of the library from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<IReadOnlyList<DeletedTrackResponse>> result = await _getDeletedTracksQueryHandler.HandleAsync(new GetDeletedTracksQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
