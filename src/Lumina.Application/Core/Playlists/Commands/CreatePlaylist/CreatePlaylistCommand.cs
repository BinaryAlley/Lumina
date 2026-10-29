#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Command for creating a playlist of the current user.
/// </summary>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The optional description of the playlist.</param>
/// <param name="Type">The kind of media the playlist holds.</param>
[DebuggerDisplay("Name: {Name}")]
public record CreatePlaylistCommand(
    string? Name,
    string? Description,
    PlaylistType? Type
) : ICommand;
