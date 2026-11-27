#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetPlaybackState;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaybackStateEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointSummary : Summary<GetPlaybackStateEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointSummary"/> class.
    /// </summary>
    public GetPlaybackStateEndpointSummary()
    {
        Summary = "Retrieves the playback state the player reported for the current user.";
        Description = "Retrieves the last volume and mute state the player reported for the current user. These values are not part of the settings the user edits.";

        Response(200, "The playback state of the current user is returned.", example: new SuccessResponse<PlaybackStateDto>(true, default));
    }
}
