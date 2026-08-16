#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a lightweight book response, containing only the properties needed by the client for card-style navigation.
/// </summary>
/// <param name="Id">The Id of the book.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="ReleaseYear">The release year of the book (re-release year, if available, or original release year), if known.</param>
/// <param name="CoverPath">The path of the image representing the cover of the book, if available.</param>
[DebuggerDisplay("Title: {Title}")]
public record BookLiteResponse(
    Guid Id,
    string Title,
    int? ReleaseYear,
    string? CoverPath
);
