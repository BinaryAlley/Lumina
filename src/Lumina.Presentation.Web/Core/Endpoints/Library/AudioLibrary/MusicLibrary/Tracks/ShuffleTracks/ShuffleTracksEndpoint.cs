#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/tracks/api-shuffle-tracks</c> route.
/// </summary>
public class ShuffleTracksEndpoint : BaseEndpoint<ShuffleTracksRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ShuffleTracksEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Music.SHUFFLE_TRACKS);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves a random selection of tracks of the library identified in the request.
    /// </summary>
    /// <param name="request">The request containing the selection size, the optional scope and the tracks to exclude.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ShuffleTracksRequest request, CancellationToken cancellationToken)
    {
        List<MusicShuffleTrackDto> response = await _apiHttpClient.PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(
            BuildEndpoint(request.LibraryId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve a random selection of tracks.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the tracks are sampled from.</param>
    /// <returns>The API endpoint to which the retrieval request is sent.</returns>
    private static string BuildEndpoint(Guid libraryId)
    {
        // The strongly typed Id is formatted as a Guid, so the substituted route value can never escape its URL segment.
        return ApiRoutes.Music.SHUFFLE_TRACKS.Replace("{libraryId}", libraryId.ToString());
    }
}
