#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the response of an update music cover operation.
/// </summary>
[DebuggerDisplay("CoverPath: {CoverPath}")]
public class UpdateMusicCoverDto
{
    /// <summary>
    /// Gets or sets the relative path of the stored cover image.
    /// </summary>
    public string? CoverPath { get; set; }
}
