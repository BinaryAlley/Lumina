#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;

/// <summary>
/// Query for getting a resource of a book, for reading.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book whose resource is retrieved, taken from the route.</param>
/// <param name="ResourceKey">The opaque resource key of the resource, taken from the route.</param>
public record GetReadingResourceQuery(
    string? LibraryId,
    string? BookId,
    string? ResourceKey
) : IQuery;
