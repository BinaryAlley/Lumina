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

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="CreatePlaylistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointSummary : Summary<CreatePlaylistEndpoint, CreatePlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointSummary"/> class.
    /// </summary>
    public CreatePlaylistEndpointSummary()
    {
        Summary = "Creates a playlist of the current user.";
        Description = "Creates a playlist that belongs to the currently authenticated user. The name is required, while the description is optional. The type decides which media items the playlist accepts. Only the music type is exposed, because video libraries are not catalogued yet; the entry model and the storage already carry a generic media item kind, so exposing video playlists later does not require a schema change. The new playlist starts empty.";

        ExampleRequest = new CreatePlaylistRequest(
            Name: "Evening listening",
            Description: "Quiet tracks for the end of the day.",
            Type: PlaylistType.Music
        );
        RequestParam(request => request.Name, "The name of the playlist. Required, maximum 255 characters.");
        RequestParam(request => request.Description, "The description of the playlist. Optional, maximum 2000 characters.");
        RequestParam(request => request.Type, "The kind of media the playlist holds. Required. Only Music is currently supported.");

        ResponseParam<PlaylistResponse>(response => response.Id, "The Id of the playlist.");
        ResponseParam<PlaylistResponse>(response => response.Name, "The name of the playlist.");
        ResponseParam<PlaylistResponse>(response => response.Description, "The description of the playlist, if applicable.");
        ResponseParam<PlaylistResponse>(response => response.Type, "The kind of media the playlist holds.");
        ResponseParam<PlaylistResponse>(response => response.Entries, "The page of ordered entries of the playlist, with its pagination metadata.");
        ResponseParam<PlaylistResponse>(response => response.CreatedOnUtc, "The date and time when the playlist was created.");
        ResponseParam<PlaylistResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The playlist was created and is returned.",
            example: new PlaylistResponse(
                Id: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                Name: "Evening listening",
                Description: "Quiet tracks for the end of the day.",
                Type: PlaylistType.Music,
                Entries: new PaginatedResponse<PlaylistEntryResponse>
                {
                    Data = [],
                    CurrentPage = 1,
                    PerPage = 200,
                    Count = 0,
                    NumberOfPages = 0
                },
                CreatedOnUtc: new DateTime(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc),
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
                    instance = "/api/v1/users/me/playlists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/playlists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/playlists"
                }
            }
        );

        Response(409, "The request failed because the playlist could not be stored, because of a conflicting row or because the database is busy.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "UniqueConstraintViolation",
                    instance = "/api/v1/users/me/playlists",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/playlists",
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
                instance = "/api/v1/users/me/playlists",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlaylistNameCannotBeEmpty",
                            "PlaylistNameMustBeMaximum255CharactersLong",
                            "PlaylistDescriptionMustBeMaximum2000CharactersLong",
                            "PlaylistTypeIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
