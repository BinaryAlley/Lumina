#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteMusicTrackEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicTrackEndpointSummary : Summary<DeleteMusicTrackEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicTrackEndpointSummary"/> class.
    /// </summary>
    public DeleteMusicTrackEndpointSummary()
    {
        Summary = "Deletes a music track.";
        Description = "Soft deletes the music track identified by the route, so its metadata, artwork and file path are kept and the track can be restored.";

        Response(200, "The music track was deleted.");
    }
}
