#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Playlists;

/// <summary>
/// Represents a request for adding a track to a playlist of the current user.
/// </summary>
/// <param name="TrackId">The Id of the track to add. Required.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record AddPlaylistEntryRequest(
    Guid? TrackId
);
