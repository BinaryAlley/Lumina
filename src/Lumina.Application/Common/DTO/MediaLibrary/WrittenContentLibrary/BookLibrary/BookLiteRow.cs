#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Lightweight read model of a book, containing only the fields needed to display the book in a card-based grid or a list.
/// </summary>
public sealed record BookLiteRow
{
    /// <summary>
    /// Gets the Id of the book.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the book.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the release year of the book (re-release year, if available, or original release year), if known.
    /// </summary>
    public int? ReleaseYear { get; init; }

    /// <summary>
    /// Gets the path of the image representing the cover of the book, if available.
    /// </summary>
    public string? CoverPath { get; init; }
}
