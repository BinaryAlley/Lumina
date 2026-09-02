#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Reading;

/// <summary>
/// Data transfer object for the content of a reading section of a book.
/// </summary>
[DebuggerDisplay("LocationRef: {LocationRef}")]
public class ReadingSectionDto
{
    /// <summary>
    /// Gets or sets the opaque location reference of the reading section.
    /// </summary>
    public string LocationRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the reading section, if known.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sanitized HTML content of the reading section, ready to be rendered by the client.
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;
}
