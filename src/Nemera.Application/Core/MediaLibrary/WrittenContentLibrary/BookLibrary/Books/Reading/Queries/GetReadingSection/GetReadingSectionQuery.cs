#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Query for getting the content of a reading section of a book.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book whose reading section is retrieved, taken from the route.</param>
/// <param name="LocationRef">The opaque location reference of the reading section, taken from the route.</param>
public record GetReadingSectionQuery(
    string? LibraryId,
    string? BookId,
    string? LocationRef
) : IQuery;
