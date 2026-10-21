#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Playlists;

/// <summary>
/// Represents a request for creating a playlist of the current user.
/// </summary>
/// <param name="Name">The name of the playlist. Required.</param>
/// <param name="Description">The description of the playlist. Optional.</param>
/// <param name="Type">The kind of media the playlist holds. Required.</param>
[DebuggerDisplay("Name: {Name}")]
public record CreatePlaylistRequest(
    string? Name,
    string? Description,
    PlaylistType? Type
);
