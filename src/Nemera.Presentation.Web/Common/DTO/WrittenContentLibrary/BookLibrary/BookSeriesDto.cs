#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for a request to get book series information.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class BookSeriesDto
{
    /// <summary>
    /// Gets or sets the title of the book series.
    /// </summary>
    public string? Title { get; set; }
}
