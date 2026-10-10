#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an ISWC (International Standard Musical Work Code) of the work a track is a recording of.
/// </summary>
/// <param name="Value">The value of the ISWC.</param>
[DebuggerDisplay("Value: {Value}")]
public record TrackWorkIswcEntity(
    string Value
);
