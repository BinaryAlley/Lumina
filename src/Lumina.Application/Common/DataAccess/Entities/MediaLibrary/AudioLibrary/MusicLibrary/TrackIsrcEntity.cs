#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an ISRC (International Standard Recording Code) of a track.
/// </summary>
/// <param name="Value">The value of the ISRC.</param>
[DebuggerDisplay("Value: {Value}")]
public record TrackIsrcEntity(
    string Value
);
