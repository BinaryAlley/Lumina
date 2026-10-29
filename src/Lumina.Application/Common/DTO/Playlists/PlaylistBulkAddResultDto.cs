#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for the result of appending entries to a playlist in one bulk operation.
/// </summary>
/// <param name="FirstPosition">The sparse order key of the first appended entry.</param>
/// <param name="AddedCount">The number of entries appended.</param>
[DebuggerDisplay("FirstPosition: {FirstPosition}; AddedCount: {AddedCount}")]
public sealed record PlaylistBulkAddResultDto(
    long FirstPosition,
    int AddedCount
);
