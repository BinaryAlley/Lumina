#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/tracks/shuffle</c> route.
/// </summary>
public class ShuffleTracksEndpoint : BaseEndpoint<ShuffleTracksRequest, IResult>
{
    private readonly IQueryHandler<ShuffleTracksQuery, Result<IReadOnlyList<ShuffleTrackResponse>>> _shuffleTracksQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpoint"/> class.
    /// </summary>
    /// <param name="shuffleTracksQueryHandler">Injected service for handling shuffle tracks queries.</param>
    public ShuffleTracksEndpoint(IQueryHandler<ShuffleTracksQuery, Result<IReadOnlyList<ShuffleTrackResponse>>> shuffleTracksQueryHandler)
    {
        _shuffleTracksQueryHandler = shuffleTracksQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Tracks.SHUFFLE_TRACKS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Draws a random selection of tracks of the library identified by the route.
    /// </summary>
    /// <param name="request">The request containing the selection size, the optional scope and the tracks to exclude.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ShuffleTracksRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<IReadOnlyList<ShuffleTrackResponse>> result = await _shuffleTracksQueryHandler.HandleAsync(request.ToQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
