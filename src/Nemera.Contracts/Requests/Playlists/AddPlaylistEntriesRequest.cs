#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Playlists;

/// <summary>
/// Represents a request for adding several tracks to a playlist of the current user, in order.
/// </summary>
/// <param name="TrackIds">The Ids of the tracks to add, in the order they must take. Required.</param>
[DebuggerDisplay("TrackCount: {TrackIds.Count}")]
public record AddPlaylistEntriesRequest(
    List<Guid>? TrackIds
);
