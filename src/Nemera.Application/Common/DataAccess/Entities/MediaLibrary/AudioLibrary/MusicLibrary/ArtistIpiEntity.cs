#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an IPI (Interested Party Information) code of a music artist.
/// </summary>
/// <param name="Value">The value of the IPI code.</param>
[DebuggerDisplay("Value: {Value}")]
public record ArtistIpiEntity(
    string Value
);
