#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Query for checking the reading availability of a book.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book whose reading availability is checked, taken from the route.</param>
public record GetReadingAvailabilityQuery(
    string? LibraryId,
    string? BookId
) : IQuery;
