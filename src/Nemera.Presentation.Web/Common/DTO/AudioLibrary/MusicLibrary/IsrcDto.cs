#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for an ISRC (International Standard Recording Code) of a track.
/// </summary>
[DebuggerDisplay("Value: {Value}")]
public class IsrcDto
{
    /// <summary>
    /// Gets or sets the value of the ISRC.
    /// </summary>
    public string? Value { get; set; }
}
