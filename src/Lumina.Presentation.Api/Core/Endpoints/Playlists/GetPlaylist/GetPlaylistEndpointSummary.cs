#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaylistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointSummary : Summary<GetPlaylistEndpoint, GetPlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointSummary"/> class.
    /// </summary>
    public GetPlaylistEndpointSummary()
    {
        Summary = "Gets a playlist of the current user.";
        Description = "Gets the playlist identified by its Id when it belongs to the currently authenticated user, together with a page of its ordered entries. The currentPage and perPage query parameters select the page, and entries.count reports the total number of entries. When neither parameter is provided, every entry is returned in a single page. Each entry carries the media item Id and the media item kind it references, and a music track also carries the display details needed to play it: the library, album and artist Ids, the title, the album title, the artist name, the duration and the cover art. An entry whose media item was deleted is still returned, with empty display details.";

        ExampleRequest = new GetPlaylistRequest(
            CurrentPage: 1,
            PerPage: 200
        );
        RequestParam(request => request.CurrentPage, "The one based page of entries to return. Optional; defaults to 1 when a page size is provided.");
        RequestParam(request => request.PerPage, "The number of entries per page. Optional; defaults to 200 when a page is provided. When both pagination parameters are omitted, every entry is returned.");

        ResponseParam<PlaylistResponse>(response => response.Id, "The Id of the playlist.");
        ResponseParam<PlaylistResponse>(response => response.Name, "The name of the playlist.");
        ResponseParam<PlaylistResponse>(response => response.Description, "The description of the playlist, if applicable.");
        ResponseParam<PlaylistResponse>(response => response.Type, "The kind of media the playlist holds.");
        ResponseParam<PlaylistResponse>(response => response.Entries, "The page of ordered entries of the playlist, with its pagination metadata.");
        ResponseParam<PlaylistResponse>(response => response.CreatedOnUtc, "The date and time when the playlist was created.");
        ResponseParam<PlaylistResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The playlist and its ordered entries are returned.",
            example: new PlaylistResponse(
                Id: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                Name: "Evening listening",
                Description: "Quiet tracks for the end of the day.",
                Type: PlaylistType.Music,
                Entries: new PaginatedResponse<PlaylistEntryResponse>
                {
                    Data =
                    [
                        new PlaylistEntryResponse(
                            Id: Guid.Parse("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"),
                            MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                            MediaItemType: PlaylistMediaItemType.MusicTrack,
                            Position: 0,
                            LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                            CollectionId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                            CreatorId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                            MediaItemTitle: "The Show Must Go On",
                            CollectionTitle: "Innuendo",
                            CreatorName: "Queen",
                            DurationInSeconds: 262,
                            CoverPath: "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg"
                        )
                    ],
                    CurrentPage = 1,
                    PerPage = 200,
                    Count = 1,
                    NumberOfPages = 1
                },
                CreatedOnUtc: new DateTime(2026, 10, 19, 18, 0, 0, DateTimeKind.Utc),
                UpdatedOnUtc: new DateTime(2026, 10, 19, 19, 30, 0, DateTimeKind.Utc)
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
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"
                }
            }
        );

        Response(404, "The request failed because the playlist does not exist, or it does not belong to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "PlaylistNotFound",
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b",
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
                instance = "/api/v1/users/me/playlists/1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
