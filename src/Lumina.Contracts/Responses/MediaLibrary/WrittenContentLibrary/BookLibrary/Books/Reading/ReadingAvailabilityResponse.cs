#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents the reading availability of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading availability is reported.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to.</param>
/// <param name="IsAvailable">Whether the book can be opened for reading.</param>
/// <param name="ErrorCode">The code of the error preventing the book from being read, when it cannot be read. Can be <see langword="null"/> when the book is available.</param>
[DebuggerDisplay("BookId: {BookId}, IsAvailable: {IsAvailable}")]
public sealed record ReadingAvailabilityResponse(
    Guid BookId,
    Guid LibraryId,
    bool IsAvailable,
    string? ErrorCode
);
