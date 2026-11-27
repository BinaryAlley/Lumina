#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for a mood of a media element.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MoodDto
{
    /// <summary>
    /// Gets or sets the name of the mood.
    /// </summary>
    public string? Name { get; set; }
}
