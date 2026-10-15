#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.UsersManagement.Settings;

/// <summary>
/// Represents the playback state of a user, as last reported by the player. The player writes these values automatically, and they are not part of the settings the user edits.
/// </summary>
/// <param name="Volume">The last volume the player reported, as a value between 0 and 1.</param>
/// <param name="IsMuted">The last mute state the player reported.</param>
[DebuggerDisplay("Volume: {Volume}; IsMuted: {IsMuted}")]
public sealed record PlaybackStateResponse(
    double Volume,
    bool IsMuted
);
