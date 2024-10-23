#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Requests.WrittenContentLibrary.BookLibrary.Books;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="AddBookRequest"/>.
/// </summary>
public static class AddBookRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddBookCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static AddBookCommand ToCommand(this AddBookRequest request)
    {
        return new AddBookCommand(
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
