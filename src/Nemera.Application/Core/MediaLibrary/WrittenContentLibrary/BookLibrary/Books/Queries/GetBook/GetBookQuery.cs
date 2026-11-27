#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Query for getting a book by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book to get, taken from the route.</param>
public record GetBookQuery(
    string? LibraryId,
    string? BookId
) : IQuery;
