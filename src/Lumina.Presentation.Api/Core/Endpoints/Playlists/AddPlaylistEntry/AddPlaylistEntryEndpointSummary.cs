#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistEntryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointSummary : Summary<AddPlaylistEntryEndpoint, AddPlaylistEntryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistEntryEndpointSummary()
    {
        Summary = "Adds a track to a playlist of the current user.";
        Description = "Adds a track to the end of the ordered entries of the playlist identified by its Id, when the playlist belongs to the currently authenticated user. The track must exist, and its media library must be reachable by the current user; a missing track and an unreachable library are both reported as not found, so the response never reveals whether a track exists in another user's library. The playlist must accept music tracks, which is the only media kind exposed until video libraries are catalogued. The same track may appear in a playlist more than once.";

        ExampleRequest = new AddPlaylistEntryRequest(
            TrackId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")
        );
        RequestParam(request => request.TrackId, "The Id of the track to add. Required.");

        ResponseParam<PlaylistEntryAddedResponse>(response => response.PlaylistId, "The Id of the playlist the entry was added to.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry, "The added entry.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.Id, "The Id of the added entry.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.MediaItemId, "The Id of the media item the entry references.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.MediaItemType, "The kind of media item the entry references.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.Position, "The zero based position of the entry in the playlist.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.LibraryId, "The Id of the media library that contains the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.CollectionId, "The Id of the collection that contains the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.CreatorId, "The Id of the creator of the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.MediaItemTitle, "The title of the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.CollectionTitle, "The title of the collection that contains the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.CreatorName, "The name of the creator of the media item.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.DurationInSeconds, "The duration of the media item in seconds.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.Entry.CoverPath, "The path of the cover art of the collection, if available.");
        ResponseParam<PlaylistEntryAddedResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated.");

        Response(200, "The added entry is returned with the playlist activity time.",
            example: new PlaylistEntryAddedResponse(
                PlaylistId: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                Entry: new PlaylistEntryResponse(
                    Id: Guid.Parse("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"),
                    MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                    MediaItemType: PlaylistMediaItemType.MusicTrack,
                    Position: 11,
                    LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                    CollectionId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                    CreatorId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                    MediaItemTitle: "The Show Must Go On",
                    CollectionTitle: "Innuendo",
                    CreatorName: "Queen",
                    DurationInSeconds: 262,
                    CoverPath: "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg"
                ),
                UpdatedOnUtc: new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc)
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries"
                }
            }
        );

        Response(403, "The request failed because the playlist does not accept music tracks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Forbidden",
                status = 403,
                detail = "PlaylistDoesNotAcceptMediaItems",
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the playlist does not exist, does not belong to the current user, or the track does not exist or is not reachable.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaylistNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because the entry could not be stored, because of a conflicting row or because the database is busy.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "UniqueConstraintViolation",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistIdCannotBeEmpty",
                            "TrackIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
