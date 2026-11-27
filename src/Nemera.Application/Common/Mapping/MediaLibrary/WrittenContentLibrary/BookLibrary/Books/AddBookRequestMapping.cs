#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="AddBookRequest"/>.
/// </summary>
public static class AddBookRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddBookCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the book is added to, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddBookCommand ToCommand(this AddBookRequest request, string? libraryId)
    {
        return new AddBookCommand(
            libraryId,
            request.Path,
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
