#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a preview of important properties of a book.
/// </summary>
[DebuggerDisplay("{Title}")]
public class LightBookModel
{
    /// <summary>
    /// Gets the Id of the media item.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the media item.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the optional release year (either re-release year, if available, or original release year, if known) of the media item.
    /// </summary>
    public int? ReleaseYear { get; init; }

    /// <summary>
    /// Gets the optional path of the image representing the cover of this book.
    /// </summary>
    public string? CoverPath { get; init; }
}
