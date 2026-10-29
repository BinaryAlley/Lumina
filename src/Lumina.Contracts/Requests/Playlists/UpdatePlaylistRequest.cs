#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Playlists;

/// <summary>
/// Represents a request for updating the details of a playlist of the current user.
/// </summary>
/// <param name="Name">The name of the playlist. Required.</param>
/// <param name="Description">The description of the playlist. Optional.</param>
[DebuggerDisplay("Name: {Name}")]
public record UpdatePlaylistRequest(
    string? Name,
    string? Description
);
