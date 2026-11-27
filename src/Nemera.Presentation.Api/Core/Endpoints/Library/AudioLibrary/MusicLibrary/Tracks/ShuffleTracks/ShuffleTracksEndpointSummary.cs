#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Class used for providing a textual description for the <see cref="ShuffleTracksEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointSummary : Summary<ShuffleTracksEndpoint, ShuffleTracksRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointSummary"/> class.
    /// </summary>
    public ShuffleTracksEndpointSummary()
    {
        Summary = "Draws a random selection of tracks from a media library, an artist or an album.";
        Description = "Returns uniformly random tracks from every track of the whole library, of one artist, or of one album, selected by the database itself. The client can pass the tracks it already queued to exclude them, so consecutive batches of a shuffle do not repeat a track. The request is returned to an Admin, who can shuffle all libraries, or to the owner of the library.";

        ExampleRequest = new ShuffleTracksRequest(
            Count: 100,
            ArtistId: null,
            AlbumId: null,
            ExcludedTrackIds: [Guid.NewGuid(), Guid.NewGuid()]
        );

        RequestParam(r => r.Count, "The maximum number of tracks to return, between 1 and 500.");
        RequestParam(r => r.ArtistId, "The artist whose tracks are sampled. Optional.");
        RequestParam(r => r.AlbumId, "The album whose tracks are sampled. Optional.");
        RequestParam(r => r.ExcludedTrackIds, "The tracks that must not be returned. Optional.");

        ResponseParam<ShuffleTrackResponse>(r => r.TrackId, "The Id of the track.");
        ResponseParam<ShuffleTrackResponse>(r => r.Title, "The title of the track.");
        ResponseParam<ShuffleTrackResponse>(r => r.DurationInSeconds, "The duration of the track in seconds.");
        ResponseParam<ShuffleTrackResponse>(r => r.AlbumId, "The Id of the album the track belongs to.");
        ResponseParam<ShuffleTrackResponse>(r => r.AlbumTitle, "The title of the album the track belongs to.");
        ResponseParam<ShuffleTrackResponse>(r => r.ArtistId, "The Id of the artist the album belongs to.");
        ResponseParam<ShuffleTrackResponse>(r => r.CoverPath, "The path of the cover of the album, if available.");

        Response(200, "The random selection of tracks is returned.",
            example: new List<ShuffleTrackResponse>
            {
                new(
                    TrackId: Guid.NewGuid(),
                    Title: "Bohemian Rhapsody",
                    DurationInSeconds: 354,
                    AlbumId: Guid.NewGuid(),
                    AlbumTitle: "A Night at the Opera",
                    ArtistId: Guid.NewGuid(),
                    CoverPath: "/music/queen/a-night-at-the-opera/cover.jpg"
                )
            }
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "Authentication failed",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/tracks/shuffle"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/tracks/shuffle",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/tracks/shuffle",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ShuffleTrackCountMustBeGreaterThanZero",
                            "ShuffleTrackCountMustBeAtMost500"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
