#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Playlists;

/// <summary>
/// Represents a request for moving one entry of a playlist of the current user to a new position.
/// </summary>
/// <param name="TargetPosition">The new zero based position of the entry. Required.</param>
[DebuggerDisplay("TargetPosition: {TargetPosition}")]
public record MovePlaylistEntryRequest(
    int? TargetPosition
);
