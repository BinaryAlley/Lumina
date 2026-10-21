#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemovePlaylistEntryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointSummary : Summary<RemovePlaylistEntryEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public RemovePlaylistEntryEndpointSummary()
    {
        Summary = "Removes an entry from a playlist of the current user.";
        Description = "Removes the entry identified by its Id from the playlist identified by its Id, when the playlist belongs to the currently authenticated user. The entries use sparse order keys, so only the removed row is written and no remaining entry needs to be repositioned. The referenced track is not deleted.";

        ResponseParam<PlaylistEntryRemovedResponse>(response => response.PlaylistId, "The Id of the playlist the entry was removed from.");
        ResponseParam<PlaylistEntryRemovedResponse>(response => response.RemovedEntryId, "The Id of the removed entry.");
        ResponseParam<PlaylistEntryRemovedResponse>(response => response.RepositionedCount, "Always zero, because sparse order keys need no remaining entries to be repositioned.");
        ResponseParam<PlaylistEntryRemovedResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated.");

        Response(200, "The removed entry and the number of repositioned entries are returned.",
            example: new PlaylistEntryRemovedResponse(
                PlaylistId: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                RemovedEntryId: Guid.Parse("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"),
                RepositionedCount: 0,
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaylistEntryNotFound",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because another request changed the playlist after it was read.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ConcurrencyConflict",
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d",
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries/2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistIdCannotBeEmpty",
                            "PlaylistEntryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
