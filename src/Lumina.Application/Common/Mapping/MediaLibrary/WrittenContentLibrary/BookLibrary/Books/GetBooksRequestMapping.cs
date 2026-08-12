#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetBooksRequest"/>.
/// </summary>
public static class GetBooksRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetBooksQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetBooksQuery ToQuery(this GetBooksRequest request)
    {
        return new GetBooksQuery(request.LibraryId);
    }
}
