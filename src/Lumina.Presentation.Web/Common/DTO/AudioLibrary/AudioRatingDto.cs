#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary;

/// <summary>
/// Data transfer object for the rating of an audio media element.
/// </summary>
[DebuggerDisplay("Value: {Value}, MaxValue: {MaxValue}")]
public class AudioRatingDto : RatingDto
{
    /// <summary>
    /// Gets or sets the source of the audio rating (e.g., a specific website or platform).
    /// </summary>
    public AudioRatingSource? Source { get; set; }
}
