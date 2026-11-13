#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;

/// <summary>
/// Query for getting the deleted books of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose deleted books are retrieved, taken from the route.</param>
public record GetDeletedBooksQuery(
    string? LibraryId
) : IQuery;
