#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a response of an update artist cover operation.
/// </summary>
/// <param name="CoverPath">The relative path of the stored cover image.</param>
[DebuggerDisplay("CoverPath: {CoverPath}")]
public record UpdateArtistCoverResponse(
    string CoverPath
);
