#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for an ISRC (International Standard Recording Code) of a track.
/// </summary>
/// <param name="Value">The value of the ISRC.</param>
[DebuggerDisplay("Value: {Value}")]
public record IsrcDto(
    string? Value
);
