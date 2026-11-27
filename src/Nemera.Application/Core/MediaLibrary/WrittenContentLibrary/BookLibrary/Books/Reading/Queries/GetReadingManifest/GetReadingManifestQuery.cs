#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Query for getting the reading manifest of a book.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book whose reading manifest is retrieved, taken from the route.</param>
public record GetReadingManifestQuery(
    string? LibraryId,
    string? BookId
) : IQuery;
