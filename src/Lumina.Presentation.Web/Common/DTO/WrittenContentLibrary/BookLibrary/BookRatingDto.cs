#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System.Diagnostics;
using Lumina.Presentation.Web.Common.DTO.Common;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for a book rating.
/// </summary>
[DebuggerDisplay("Value: {Value}, MaxValue: {MaxValue}")]
public class BookRatingDto : RatingDto
{
    /// <summary>
    /// Gets or sets the source of the book rating (e.g., a specific website or platform).
    /// </summary>
    public BookRatingSource? Source { get; set; }
}
