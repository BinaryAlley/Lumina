#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistScopeEntries;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistScopeEntriesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesEndpointSummary : Summary<AddPlaylistScopeEntriesEndpoint, AddPlaylistScopeEntriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistScopeEntriesEndpointSummary()
    {
        Summary = "Adds every track of a library, artist or album to a playlist of the current user.";
        Description = "Appends every track of the music library identified by libraryId, optionally limited to an artist or an album, to the end of the ordered entries of the playlist identified by its Id. The server resolves the scope in catalogue order with one set-based statement, so a whole library is appended without materializing its track Ids, sorting them in memory or sending them to the data access layer. The playlist must belong to the currently authenticated user, and the library must be reachable by the current user. An empty scope appends nothing and reports a zero count without an activity time. The same track may appear in a playlist more than once.";

        ExampleRequest = new AddPlaylistScopeEntriesRequest(
            LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
            ArtistId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
            AlbumId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f")
        );
        RequestParam(request => request.LibraryId, "The Id of the music library the scope belongs to. Required.");
        RequestParam(request => request.ArtistId, "The Id of the artist the scope is limited to. Optional.");
        RequestParam(request => request.AlbumId, "The Id of the album the scope is limited to. Optional.");

        ResponseParam<PlaylistEntriesAddedResponse>(response => response.PlaylistId, "The Id of the playlist the entries were added to.");
        ResponseParam<PlaylistEntriesAddedResponse>(response => response.AddedCount, "The number of entries that were appended, including repeated tracks.");
        ResponseParam<PlaylistEntriesAddedResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The scope was appended to the playlist.",
            example: new PlaylistEntriesAddedResponse(
                PlaylistId: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                AddedCount: 120,
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope"
                }
            }
        );

        Response(403, "The request failed because the library is not accessible to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the playlist does not exist, or it does not belong to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "PlaylistNotFound",
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope",
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b/entries-scope",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistIdCannotBeEmpty",
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
