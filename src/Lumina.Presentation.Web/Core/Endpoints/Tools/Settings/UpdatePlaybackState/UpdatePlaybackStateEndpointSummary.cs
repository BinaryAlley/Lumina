#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdatePlaybackState;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdatePlaybackStateEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointSummary : Summary<UpdatePlaybackStateEndpoint, PlaybackStateDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointSummary"/> class.
    /// </summary>
    public UpdatePlaybackStateEndpointSummary()
    {
        Summary = "Updates the playback state the player reported for the current user.";
        Description = "Updates the last volume and mute state the player reported for the current user. This is written automatically by the player and is not part of the settings the user edits.";

        RequestParam(r => r.Volume, "The last volume the player reported, as a value between 0 and 1. Required.");
        RequestParam(r => r.IsMuted, "The last mute state the player reported. Required.");

        ExampleRequest = new PlaybackStateDto { Volume = 1.0, IsMuted = false };

        Response(200, "The playback state of the current user is updated.", example: new { success = true, data = new { isUpdated = true } });
    }
}
