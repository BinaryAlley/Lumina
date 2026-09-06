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
    /// <returns>The converted command.</returns>
    public static UpdateBookCommand ToCommand(this UpdateBookRequest request)
    {
        return new UpdateBookCommand(
            Guid.TryParse(request.Id, out Guid bookId) ? bookId : Guid.Empty,
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
