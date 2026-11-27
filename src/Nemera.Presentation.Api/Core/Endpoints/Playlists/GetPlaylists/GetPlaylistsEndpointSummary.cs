#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaylistsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointSummary : Summary<GetPlaylistsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointSummary"/> class.
    /// </summary>
    public GetPlaylistsEndpointSummary()
    {
        Summary = "Gets the playlists of the current user.";
        Description = "Gets the playlists that belong to the currently authenticated user, ordered by their most recent activity. Each playlist is returned without its entries, together with the number of entries it holds. A playlist of another user is never returned.";

        ResponseParam<PlaylistSummaryResponse>(response => response.Id, "The Id of the playlist.");
        ResponseParam<PlaylistSummaryResponse>(response => response.Name, "The name of the playlist.");
        ResponseParam<PlaylistSummaryResponse>(response => response.Description, "The description of the playlist, if applicable.");
        ResponseParam<PlaylistSummaryResponse>(response => response.Type, "The kind of media the playlist holds.");
        ResponseParam<PlaylistSummaryResponse>(response => response.EntryCount, "The number of entries of the playlist.");
        ResponseParam<PlaylistSummaryResponse>(response => response.CreatedOnUtc, "The date and time when the playlist was created.");
        ResponseParam<PlaylistSummaryResponse>(response => response.UpdatedOnUtc, "The date and time when the playlist was last updated, if applicable.");

        Response(200, "The playlists of the current user are returned.",
            example: new[]
            {
                new PlaylistSummaryResponse(
                    Id: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                    Name: "Evening listening",
                    Description: "Quiet tracks for the end of the day.",
                    Type: PlaylistType.Music,
                    EntryCount: 12,
                    CreatedOnUtc: new DateTime(2026, 10, 19, 18, 0, 0, DateTimeKind.Utc),
                    UpdatedOnUtc: new DateTime(2026, 10, 19, 19, 30, 0, DateTimeKind.Utc)
                )
            }
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
    }
}
