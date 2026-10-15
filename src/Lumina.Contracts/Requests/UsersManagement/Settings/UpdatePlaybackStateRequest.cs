#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.UsersManagement.Settings;

/// <summary>
/// Represents a request to report the playback state of the current user. The player sends this automatically, and these values are not part of the settings the user edits.
/// </summary>
/// <param name="Volume">The last volume the player reported, as a value between 0 and 1. Required.</param>
/// <param name="IsMuted">The last mute state the player reported. Required.</param>
[DebuggerDisplay("Volume: {Volume}; IsMuted: {IsMuted}")]
public sealed record UpdatePlaybackStateRequest(
    double? Volume,
    bool? IsMuted
);
