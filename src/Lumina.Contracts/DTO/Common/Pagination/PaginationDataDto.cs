#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.DTO.Common.Pagination;

/// <summary>
/// Data transfer object for pagination metadata.
/// </summary>
public sealed record PaginationDataDto
{
    private int perPage = 200;
    private int currentPage = 1;

    /// <summary>
    /// Gets the current page number in a paginated collection.
    /// </summary>
    public int CurrentPage
    {
        get { return currentPage; }
        set
        {
            currentPage = Math.Max(value, 1);
        }
    }

    /// <summary>
    /// Gets or sets the number of items to include per page in paginated results.
    /// </summary>
    public int PerPage
    {
        get { return perPage; }
        set
        {
            perPage = Math.Max(value, 1);
        }
    }
}
