#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="MovePlaylistEntryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointSummary : Summary<MovePlaylistEntryEndpoint, MovePlaylistEntryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public MovePlaylistEntryEndpointSummary()
    {
        Summary = "Moves one entry of a playlist of the current user.";
        Description = "Moves the entry identified by its Id to the target zero based position in the ordered entries of the playlist identified by its Id, when the playlist belongs to the currently authenticated user. The request carries only the target position, so the move changes one sparse order key instead of rewriting the following entries. The moved entry and its new position are returned.";

        ExampleRequest = new MovePlaylistEntryRequest(
            TargetPosition: 3
        );
        RequestParam(request => request.TargetPosition, "The new zero based position of the entry. Required, zero or greater.");

        ResponseParam<PlaylistEntryMovedResponse>(response => response.PlaylistId, "The Id of the playlist whose entry was moved.");
        ResponseParam<PlaylistEntryMovedResponse>(response => response.EntryId, "The Id of the moved entry.");
        ResponseParam<PlaylistEntryMovedResponse>(response => response.Position, "The new zero based position of the entry.");
        ResponseParam<PlaylistEntryMovedResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated.");

        Response(200, "The moved entry and its new position are returned.",
            example: new PlaylistEntryMovedResponse(
                PlaylistId: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                EntryId: Guid.Parse("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"),
                Position: 3,
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move"
                }
            }
        );

        Response(404, "The request failed because the playlist or the entry does not exist, or the playlist does not belong to the current user.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaylistNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaylistEntryNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because another request changed the playlist after it was read, or because the new position could not be stored.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "UniqueConstraintViolation",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move",
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d/move",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistIdCannotBeEmpty",
                            "PlaylistEntryIdCannotBeEmpty",
                            "PlaylistMoveTargetPositionIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
