#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdatePlaylistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointSummary : Summary<UpdatePlaylistEndpoint, UpdatePlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointSummary"/> class.
    /// </summary>
    public UpdatePlaylistEndpointSummary()
    {
        Summary = "Updates the details of a playlist of the current user.";
        Description = "Updates the name and the description of the playlist identified by its Id when it belongs to the currently authenticated user, leaving its ordered entries untouched. The playlist is identified by the route value, not by any id posted in the body.";

        ExampleRequest = new UpdatePlaylistRequest(
            Name: "Evening listening",
            Description: "Quiet tracks for the end of the day."
        );
        RequestParam(request => request.Name, "The name of the playlist. Required, maximum 255 characters.");
        RequestParam(request => request.Description, "The description of the playlist. Optional, maximum 2000 characters.");

        ResponseParam<PlaylistDetailsResponse>(response => response.Id, "The Id of the playlist.");
        ResponseParam<PlaylistDetailsResponse>(response => response.Name, "The name of the playlist.");
        ResponseParam<PlaylistDetailsResponse>(response => response.Description, "The description of the playlist, if applicable.");
        ResponseParam<PlaylistDetailsResponse>(response => response.Type, "The kind of media the playlist holds.");
        ResponseParam<PlaylistDetailsResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The updated playlist scalars are returned.",
            example: new PlaylistDetailsResponse(
                Id: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                Name: "Evening listening",
                Description: "Quiet tracks for the end of the day.",
                Type: PlaylistType.Music,
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

        Response(409, "The request failed because another request changed the playlist after it was read.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ConcurrencyConflict",
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
                            "PlaylistIdCannotBeEmpty",
                            "PlaylistNameCannotBeEmpty",
                            "PlaylistNameMustBeMaximum255CharactersLong",
                            "PlaylistDescriptionMustBeMaximum2000CharactersLong"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
