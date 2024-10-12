#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using Lumina.Presentation.Web.Common.Models.Common;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a book rating.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
public class BookRatingModel : RatingModel
{
    /// <summary>
    /// Gets or sets the source of the book rating (e.g., a specific website or platform).
    /// </summary>
    public BookRatingSource? Source { get; set; }
}