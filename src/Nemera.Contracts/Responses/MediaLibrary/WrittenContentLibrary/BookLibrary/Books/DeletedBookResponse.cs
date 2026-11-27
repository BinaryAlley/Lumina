#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a book that the user deleted and can restore.
/// </summary>
/// <param name="Id">The unique identifier of the book.</param>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="OriginalTitle">The original title of the book, when it differs from the current one.</param>
/// <param name="Path">The file system path of the book.</param>
/// <param name="DeletedOnUtc">The date and time when the book was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the book, when it has one.</param>
public record DeletedBookResponse(
    Guid Id,
    Guid LibraryId,
    string Title,
    string? OriginalTitle,
    string Path,
    DateTime DeletedOnUtc,
    string? CoverPath
);
