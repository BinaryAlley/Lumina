#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistEntriesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointSummary : Summary<AddPlaylistEntriesEndpoint, AddPlaylistEntriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistEntriesEndpointSummary()
    {
        Summary = "Adds several tracks to a playlist of the current user.";
        Description = "Adds several tracks to the end of the ordered entries of the playlist identified by its Id, when the playlist belongs to the currently authenticated user, in the order the request lists them. Every track must exist, and the media library of every track must be reachable by the current user; a missing track and an unreachable library are both reported as not found, so the response never reveals whether a track exists in another user's library. The playlist must accept music tracks, which is the only media kind exposed until video libraries are catalogued. The same track may appear more than once. The request is applied as a whole: when any track is missing or not reachable, no entry is added. The tracks are inserted in bounded batches, so a long queue is accepted.";

        ExampleRequest = new AddPlaylistEntriesRequest(
            TrackIds:
            [
                Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                Guid.Parse("8e1f2a3b-4c5d-4e6f-8a9b-0c1d2e3f4a5b")
            ]
        );
        RequestParam(request => request.TrackIds, "The Ids of the tracks to add, in the order they must take. Required.");

        ResponseParam<PlaylistEntriesAddedResponse>(response => response.PlaylistId, "The Id of the playlist the entries were added to.");
        ResponseParam<PlaylistEntriesAddedResponse>(response => response.AddedCount, "The number of entries that were appended, including repeated tracks.");
        ResponseParam<PlaylistEntriesAddedResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The number of added entries is returned with the playlist activity time.",
            example: new PlaylistEntriesAddedResponse(
                PlaylistId: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                AddedCount: 12,
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch"
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the playlist does not exist, does not belong to the current user, or a track does not exist or is not reachable.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaylistNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because the entries could not be stored, because of a conflicting row or because the database is busy.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "UniqueConstraintViolation",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-batch",
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
