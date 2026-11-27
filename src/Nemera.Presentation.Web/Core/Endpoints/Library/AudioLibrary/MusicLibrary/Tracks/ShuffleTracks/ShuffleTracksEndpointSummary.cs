#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Class used for providing a textual description for the <see cref="ShuffleTracksEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointSummary : Summary<ShuffleTracksEndpoint, ShuffleTracksRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointSummary"/> class.
    /// </summary>
    public ShuffleTracksEndpointSummary()
    {
        Summary = "Draws a random selection of tracks from a media library, an artist or an album.";
        Description = "Returns uniformly random tracks from the whole library, one artist, or one album, excluding the tracks the caller already queued. The tracks are selected by the API database, so a large library does not have to be enumerated by the browser.";

        Response(200, "The random selection of tracks is returned.", example: new[] { new MusicShuffleTrackDto
        {
            TrackId = Guid.NewGuid(),
            Title = "Bohemian Rhapsody",
            DurationInSeconds = 354,
            AlbumId = Guid.NewGuid(),
            AlbumTitle = "A Night at the Opera",
            ArtistId = Guid.NewGuid(),
            CoverPath = "/music/queen/a-night-at-the-opera/cover.jpg"
        }});
    }
}
