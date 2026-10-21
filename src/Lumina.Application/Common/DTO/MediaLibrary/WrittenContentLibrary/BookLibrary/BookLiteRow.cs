#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Lightweight read model of a book, containing only the fields needed to display the book in a card-based grid or a list.
/// </summary>
/// <param name="Id">The Id of the book.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="ReleaseYear">The release year of the book (re-release year, if available, or original release year), if known.</param>
/// <param name="CoverPath">The path of the image representing the cover of the book, if available.</param>
public sealed record BookLiteRow(
    Guid Id,
    string Title,
    int? ReleaseYear,
    string? CoverPath
);
