#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;

/// <summary>
/// Query for getting a resource of a book, for reading.
/// </summary>
/// <param name="BookId">The Id of the book whose resource is retrieved.</param>
/// <param name="ResourceKey">The opaque resource key of the resource.</param>
public record GetReadingResourceQuery(
    Guid BookId,
    string ResourceKey
) : IQuery;
