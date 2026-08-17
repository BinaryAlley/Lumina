#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a paginated collection of lightweight book elements, together with pagination metadata.
/// </summary>
[DebuggerDisplay("CurrentPage: {CurrentPage}; Count: {Count}")]
public class PaginatedBookLiteModel
{
    /// <summary>
    /// Gets or sets the collection of lightweight book elements associated with this instance.
    /// </summary>
    public IReadOnlyList<BookLiteModel> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the current page number in a paginated collection.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the number of items to include per page in paginated results.
    /// </summary>
    public int PerPage { get; set; }

    /// <summary>
    /// Gets or sets the total number of elements available in the paginated result set.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages based on elements available in the result set and per page.
    /// </summary>
    public int NumberOfPages { get; set; }
}
