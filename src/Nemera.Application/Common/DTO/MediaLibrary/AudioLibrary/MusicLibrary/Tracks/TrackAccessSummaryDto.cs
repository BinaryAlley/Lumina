#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for the access information of a set of existing tracks.
/// </summary>
/// <param name="TrackCount">The number of distinct requested tracks that exist.</param>
/// <param name="LibraryIds">The distinct Ids of the media libraries that contain those tracks.</param>
[DebuggerDisplay("TrackCount: {TrackCount}; Libraries: {LibraryIds.Count}")]
public sealed record TrackAccessSummaryDto(
    int TrackCount,
    IReadOnlyList<Guid> LibraryIds
);
