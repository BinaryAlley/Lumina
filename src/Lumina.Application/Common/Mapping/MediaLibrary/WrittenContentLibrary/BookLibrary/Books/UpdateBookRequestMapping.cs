#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="UpdateBookRequest"/>.
/// </summary>
public static class UpdateBookRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateBookCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the book belongs to, taken from the route.</param>
    /// <param name="bookId">The Id of the book to update, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static UpdateBookCommand ToCommand(this UpdateBookRequest request, string? libraryId, string? bookId)
    {
        return new UpdateBookCommand(
            libraryId,
            bookId,
            request.Metadata,
            request.Format,
            request.Edition,
            request.VolumeNumber,
            request.Series,
            request.ASIN,
            request.GoodreadsId,
            request.LCCN,
            request.OCLCNumber,
            request.OpenLibraryId,
            request.LibraryThingId,
            request.GoogleBooksId,
            request.BarnesAndNobleId,
            request.AppleBooksId,
            request.ISBNs,
            request.Contributors,
            request.Ratings
        );
    }
}
