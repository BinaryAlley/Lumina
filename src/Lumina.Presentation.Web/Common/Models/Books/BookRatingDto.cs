#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Enums;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Books;

/// <summary>
/// Represents a book rating.
/// </summary>
public class BookRatingDto : RatingDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the optional source of the rating (e.g., "Goodreads", "Amazon").
    /// </summary>
    public BookRatingSource? Source { get; set; }
    #endregion
}