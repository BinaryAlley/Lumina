#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.GetMostPlayed;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMostPlayedEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostPlayedEndpointSummary : Summary<GetMostPlayedEndpoint, GetMostPlayedRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostPlayedEndpointSummary"/> class.
    /// </summary>
    public GetMostPlayedEndpointSummary()
    {
        Summary = "Gets the most played tracks of the current user.";
        Description = "Gets the tracks the current user played the most, from the most played. Only the plays that count as plays are included: a play counts when it completed or when the listened duration reached 30 seconds. Each track is returned once, with its counted plays, its completed plays, the duration the user listened to it and its last activity time. A track deleted from its library keeps its row, without display data. The response is private to the authenticated user.";

        ExampleRequest = new GetMostPlayedRequest(CurrentPage: 1, PerPage: 50);
        RequestParam(request => request.CurrentPage, "The one based page of tracks to return. Optional. Defaults to 1.");
        RequestParam(request => request.PerPage, "The number of tracks per page. Optional. Defaults to 50. When both pagination parameters are omitted, every matching track is returned.");

        ResponseParam<PaginatedResponse<PlayedTrackResponse>>(response => response.Data, "The most played tracks of the requested page, ordered by counted plays, listened duration, last activity, and track Id.");
        ResponseParam<PaginatedResponse<PlayedTrackResponse>>(response => response.CurrentPage, "The current page number.");
        ResponseParam<PaginatedResponse<PlayedTrackResponse>>(response => response.PerPage, "The number of tracks retrieved per page.");
        ResponseParam<PaginatedResponse<PlayedTrackResponse>>(response => response.Count, "The total number of tracks that match the filters.");
        ResponseParam<PaginatedResponse<PlayedTrackResponse>>(response => response.NumberOfPages, "The total number of pages.");
        ResponseParam<PlayedTrackResponse>(response => response.TrackId, "The Id of the track.");
        ResponseParam<PlayedTrackResponse>(response => response.LibraryId, "The Id of the media library that contains the track.");
        ResponseParam<PlayedTrackResponse>(response => response.AlbumId, "The Id of the album that contains the track.");
        ResponseParam<PlayedTrackResponse>(response => response.ArtistId, "The Id of the artist that produced the track.");
        ResponseParam<PlayedTrackResponse>(response => response.Title, "The title of the track, if it still exists and the user can access its library.");
        ResponseParam<PlayedTrackResponse>(response => response.AlbumTitle, "The title of the album that contains the track, if available.");
        ResponseParam<PlayedTrackResponse>(response => response.ArtistName, "The name of the artist that produced the track, if available.");
        ResponseParam<PlayedTrackResponse>(response => response.CoverPath, "The path of the cover art of the album, if available.");
        ResponseParam<PlayedTrackResponse>(response => response.LastPlayedOnUtc, "The date and time of the last recorded activity of the track's most recent play.");
        ResponseParam<PlayedTrackResponse>(response => response.PlayCount, "The number of plays of the track that count as plays.");
        ResponseParam<PlayedTrackResponse>(response => response.CompletedPlayCount, "The number of plays of the track that completed.");
        ResponseParam<PlayedTrackResponse>(response => response.ListenedDurationInSeconds, "The duration the user listened to the track, over all of its counted plays, in seconds.");

        Response(200, "The requested page of most played tracks is returned.",
            example: new PaginatedResponse<PlayedTrackResponse>
            {
                Data =
                [
                    new PlayedTrackResponse(
                        TrackId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                        LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                        AlbumId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                        ArtistId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                        Title: "Time",
                        AlbumTitle: "The Dark Side of the Moon",
                        ArtistName: "Pink Floyd",
                        CoverPath: "covers/dark-side-of-the-moon.jpg",
                        LastPlayedOnUtc: new DateTime(2026, 10, 20, 18, 30, 0, DateTimeKind.Utc),
                        PlayCount: 12,
                        CompletedPlayCount: 9,
                        ListenedDurationInSeconds: 4230.5)
                ],
                CurrentPage = 1,
                PerPage = 50,
                Count = 1,
                NumberOfPages = 1
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
                    instance = "/api/v1/users/me/music-play-history/most-played"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/music-play-history/most-played"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/music-play-history/most-played"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/me/music-play-history/most-played",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
