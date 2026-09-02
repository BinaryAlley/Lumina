#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Reading;

/// <summary>
/// Data transfer object for an item of the spine of the reading manifest of a book.
/// </summary>
[DebuggerDisplay("LocationRef: {LocationRef}")]
public class ReadingSpineItemDto
{
    /// <summary>
    /// Gets or sets the opaque location reference of the reading section.
    /// </summary>
    public string LocationRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the reading section, if known.
    /// </summary>
    public string? Title { get; set; }
}
