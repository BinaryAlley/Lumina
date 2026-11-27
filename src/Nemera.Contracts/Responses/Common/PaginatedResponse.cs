#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.Responses.Common;

/// <summary>
/// Represents a paginated response containing a subset of data and pagination metadata for the current page.
/// </summary>
/// <typeparam name="TData">The type of the data items contained in the paginated response.</typeparam>
public sealed record PaginatedResponse<TData> 
{
    /// <summary>
    /// Gets the collection of data items associated with this instance.
    /// </summary>
    public required IReadOnlyList<TData> Data { get; init; }

    /// <summary>
    /// Gets the current page number in a paginated collection.
    /// </summary>
    public required int CurrentPage { get; init; }

    /// <summary>
    /// Gets or sets the number of items to include per page in paginated results.
    /// </summary>
    public required int PerPage { get; init; }

    /// <summary>
    /// Gets the total number of elements available in the paginated result set.
    /// </summary>
    public required int Count { get; init; }

    /// <summary>
    /// Gets the total number of pages based on elements available in the result set and per page.
    /// </summary>
    public required int NumberOfPages { get; init; }
}
