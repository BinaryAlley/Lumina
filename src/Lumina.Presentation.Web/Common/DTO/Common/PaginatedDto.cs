#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for a page of elements together with its pagination metadata.
/// </summary>
/// <typeparam name="T">The type of the elements of the page.</typeparam>
/// <param name="Data">The elements of the requested page.</param>
/// <param name="CurrentPage">The one based number of the returned page.</param>
/// <param name="PerPage">The number of elements requested per page.</param>
/// <param name="Count">The total number of elements that match the filter, even when only a page of them is returned.</param>
/// <param name="NumberOfPages">The total number of pages that match the filter.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}; PerPage: {PerPage}; Count: {Count}")]
public sealed record PaginatedDto<T>(
    List<T> Data,
    int CurrentPage,
    int PerPage,
    int Count,
    int NumberOfPages
);
