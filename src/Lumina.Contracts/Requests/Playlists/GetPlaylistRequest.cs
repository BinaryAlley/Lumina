#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Playlists;

/// <summary>
/// Represents a request for getting a window of the entries of a playlist of the current user.
/// </summary>
/// <param name="Offset">The zero based position of the first entry to return. Optional; defaults to 0.</param>
/// <param name="Limit">The maximum number of entries to return, from 1 to 1000. Optional; defaults to 200.</param>
[DebuggerDisplay("Offset: {Offset}; Limit: {Limit}")]
public record GetPlaylistRequest(
    int? Offset,
    int? Limit
);
