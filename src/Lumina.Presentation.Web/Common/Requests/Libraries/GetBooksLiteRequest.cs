#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Libraries;

/// <summary>
/// Represents a request to get the lightweight details of the books of a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public class GetBooksLiteRequest
{
    /// <summary>
    /// Gets or sets the Id of the media library whose books are retrieved.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the page of results to retrieve.
    /// </summary>
    public int? CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of books to retrieve per page.
    /// </summary>
    public int? PerPage { get; set; }

    /// <summary>
    /// Gets or sets the search term used to filter results.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the alpha key used to filter the results by the first character of their title, for the alpha picker.
    /// Can be <see langword="null"/> to disable alpha filtering, a single ASCII letter (case-insensitive) to filter by that letter,
    /// "#" to filter by titles whose first character is a digit, or "*" to filter by titles whose first character is neither a letter nor a digit.
    /// </summary>
    public string? FilterAlphaKey { get; set; }

    /// <summary>
    /// Gets or sets whether the leading "The " prefix of a title is ignored when computing the alpha key, or not.
    /// </summary>
    public bool ShouldIgnoreThePrefixForAlphaPicker { get; set; }

    /// <summary>
    /// Gets or sets the name of the field by which to sort the results.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the direction in which to sort the results.
    /// </summary>
    public SortOrder? SortOrder { get; set; }
}
