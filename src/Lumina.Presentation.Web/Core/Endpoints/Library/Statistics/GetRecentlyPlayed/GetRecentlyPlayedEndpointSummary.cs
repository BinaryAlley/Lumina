#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.PlayHistory;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetRecentlyPlayed;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRecentlyPlayedEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyPlayedEndpointSummary : Summary<GetRecentlyPlayedEndpoint, GetRecentlyPlayedRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyPlayedEndpointSummary"/> class.
    /// </summary>
    public GetRecentlyPlayedEndpointSummary()
    {
        Summary = "Gets the recently played tracks of the current user.";
        Description = "Proxies the request that gets a page of the recently played tracks of the current user to the API server. Each track is returned once, from the most recently played, with its counted plays, its completed plays and the duration the user listened to it.";

        ExampleRequest = new GetRecentlyPlayedRequest(CurrentPage: 1, PerPage: 10);
        RequestParam(request => request.CurrentPage, "The one based page of tracks to return. Optional. Defaults to 1.");
        RequestParam(request => request.PerPage, "The number of tracks per page. Optional. Defaults to 10.");

        ResponseParam<SuccessResponse<PaginatedDto<PlayedTrackDto>>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<PaginatedDto<PlayedTrackDto>>(response => response.Data, "The recently played tracks of the requested page, from the most recently played.");
        ResponseParam<PaginatedDto<PlayedTrackDto>>(response => response.CurrentPage, "The current page number.");
        ResponseParam<PaginatedDto<PlayedTrackDto>>(response => response.PerPage, "The number of tracks retrieved per page.");
        ResponseParam<PaginatedDto<PlayedTrackDto>>(response => response.Count, "The total number of tracks that match the filters.");
        ResponseParam<PaginatedDto<PlayedTrackDto>>(response => response.NumberOfPages, "The total number of pages.");
        ResponseParam<PlayedTrackDto>(response => response.TrackId, "The Id of the track.");
        ResponseParam<PlayedTrackDto>(response => response.LibraryId, "The Id of the media library that contains the track.");
        ResponseParam<PlayedTrackDto>(response => response.AlbumId, "The Id of the album that contains the track.");
        ResponseParam<PlayedTrackDto>(response => response.ArtistId, "The Id of the artist that produced the track.");
        ResponseParam<PlayedTrackDto>(response => response.Title, "The title of the track, if it still exists and the user can access its library.");
        ResponseParam<PlayedTrackDto>(response => response.AlbumTitle, "The title of the album that contains the track, if available.");
        ResponseParam<PlayedTrackDto>(response => response.ArtistName, "The name of the artist that produced the track, if available.");
        ResponseParam<PlayedTrackDto>(response => response.CoverPath, "The path of the cover art of the album, if available.");
        ResponseParam<PlayedTrackDto>(response => response.LastPlayedOnUtc, "The date and time of the last recorded activity of the track's most recent play.");
        ResponseParam<PlayedTrackDto>(response => response.PlayCount, "The number of plays of the track that count as plays.");
        ResponseParam<PlayedTrackDto>(response => response.CompletedPlayCount, "The number of plays of the track that completed.");
        ResponseParam<PlayedTrackDto>(response => response.ListenedDurationInSeconds, "The duration the user listened to the track, over all of its counted plays, in seconds.");

        Response(200, "The requested page of recently played tracks is returned.",
            example: new SuccessResponse<PaginatedDto<PlayedTrackDto>>(true, new PaginatedDto<PlayedTrackDto>(
                Data:
                [
                    new PlayedTrackDto(
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
                CurrentPage: 1,
                PerPage: 10,
                Count: 1,
                NumberOfPages: 1))
        );
    }
}
