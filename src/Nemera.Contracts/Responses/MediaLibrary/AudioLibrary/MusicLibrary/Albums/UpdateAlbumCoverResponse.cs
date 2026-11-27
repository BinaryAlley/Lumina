#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a response of an update album cover operation.
/// </summary>
/// <param name="CoverPath">The relative path of the stored cover image.</param>
[DebuggerDisplay("CoverPath: {CoverPath}")]
public record UpdateAlbumCoverResponse(
    string CoverPath
);
