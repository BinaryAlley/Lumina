#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.SaveMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="SaveMusicTrackEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicTrackEndpointSummary : Summary<SaveMusicTrackEndpoint, UpdateMusicTrackRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicTrackEndpointSummary"/> class.
    /// </summary>
    public SaveMusicTrackEndpointSummary()
    {
        Summary = "Updates a music track.";
        Description = "Updates the music track identified by the route, together with its metadata.";

        Response(200, "The music track was updated.");
    }
}
