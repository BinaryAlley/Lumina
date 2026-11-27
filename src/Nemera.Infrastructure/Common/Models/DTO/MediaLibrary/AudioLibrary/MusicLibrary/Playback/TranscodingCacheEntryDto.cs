#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents a single entry of the normalized audio cache, used to enforce the cache size limit.
/// </summary>
/// <param name="Path">The absolute path of the directory that holds the entry.</param>
/// <param name="Size">The total size of the entry, in bytes.</param>
/// <param name="LastUsedUtc">The date and time, in UTC, when the entry was last produced or served.</param>
/// <param name="ProducedAtUtc">The date and time, in UTC, when the entry was produced or, for an entry found on disk, the earliest representable time.</param>
[DebuggerDisplay("Path: {Path} Size: {Size}")]
internal sealed record TranscodingCacheEntryDto(
    string Path,
    long Size,
    DateTime LastUsedUtc,
    DateTime ProducedAtUtc
);
