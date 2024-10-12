#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to get book series information.
/// </summary>
[DebuggerDisplay("{Title}")]
public class BookSeriesModel
{
    /// <summary>
    /// Gets or sets the title of the book series.
    /// </summary>
    public string? Title { get; set; }
}