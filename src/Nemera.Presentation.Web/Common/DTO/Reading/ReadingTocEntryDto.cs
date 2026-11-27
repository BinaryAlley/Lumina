#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Reading;

/// <summary>
/// Data transfer object for an entry of the table of contents of the reading manifest of a book.
/// </summary>
[DebuggerDisplay("Label: {Label}")]
public class ReadingTocEntryDto
{
    /// <summary>
    /// Gets or sets the label of the table of contents entry.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the opaque location reference of the reading section the entry points to.
    /// </summary>
    public string LocationRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child entries of the table of contents entry.
    /// </summary>
    public List<ReadingTocEntryDto> Children { get; set; } = [];
}
