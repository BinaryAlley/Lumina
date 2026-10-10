#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an ISNI (International Standard Name Identifier) code of a music artist.
/// </summary>
/// <param name="Value">The value of the ISNI code.</param>
[DebuggerDisplay("Value: {Value}")]
public record ArtistIsniEntity(
    string Value
);
