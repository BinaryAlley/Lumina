#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;

/// <summary>
/// Command for updating the playback state the player reports for the current user automatically. These values are not part of the settings the user edits.
/// </summary>
/// <param name="Volume">The last volume the player reported, as a value between 0 and 1.</param>
/// <param name="IsMuted">The last mute state the player reported.</param>
public record UpdatePlaybackStateCommand(
    double? Volume,
    bool? IsMuted
) : ICommand;
