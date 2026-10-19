#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.StartPlay;

/// <summary>
/// Class used for providing a textual description for the <see cref="StartPlayEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartPlayEndpointSummary : Summary<StartPlayEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartPlayEndpointSummary"/> class.
    /// </summary>
    public StartPlayEndpointSummary()
    {
        Summary = "Records the start of a play of a track by the current user.";
        Description = "Records the start of a play of a track by the current user, so that the play history, statistics and recommendations can be built from it. The track must belong to the named library, album and artist. The play record is returned with its identifier, which the player uses to report its progress, completion or stop. The play record belongs to the current user, and it never stores a queue, a current index, a repeat or shuffle state, or a playback session.";

        ResponseParam<PlayRecordResponse>(r => r.Id, "The Id of the play record.");
        ResponseParam<PlayRecordResponse>(r => r.TrackId, "The Id of the track that was played.");
        ResponseParam<PlayRecordResponse>(r => r.LibraryId, "The Id of the media library that contains the track.");
        ResponseParam<PlayRecordResponse>(r => r.AlbumId, "The Id of the album that contains the track.");
        ResponseParam<PlayRecordResponse>(r => r.ArtistId, "The Id of the artist that produced the track.");
        ResponseParam<PlayRecordResponse>(r => r.Status, "The state of the recorded play.");
        ResponseParam<PlayRecordResponse>(r => r.LastPositionInSeconds, "The last position reported by the player, in seconds.");
        ResponseParam<PlayRecordResponse>(r => r.ListenedDurationInSeconds, "The duration the user has listened to, in seconds.");
        ResponseParam<PlayRecordResponse>(r => r.StartedOnUtc, "The date and time when the play started.");
        ResponseParam<PlayRecordResponse>(r => r.EndedOnUtc, "The date and time when the play completed or stopped, if it has ended.");
        ResponseParam<PlayRecordResponse>(r => r.UpdatedOnUtc, "The date and time when the play record was last updated, if applicable.");

        Response(200, "The play was recorded and its play record is returned.",
            example: new PlayRecordResponse(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                TrackId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                AlbumId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                ArtistId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                Status: PlaybackRecordStatus.InProgress,
                LastPositionInSeconds: 0,
                ListenedDurationInSeconds: 0,
                StartedOnUtc: new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc),
                EndedOnUtc: null,
                UpdatedOnUtc: null
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the album or the track does not exist, or they do not belong to the library, artist and album named by the request.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "AlbumNotFound",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "TrackIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
