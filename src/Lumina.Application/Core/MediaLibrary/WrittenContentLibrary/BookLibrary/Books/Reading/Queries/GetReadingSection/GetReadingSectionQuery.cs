#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Query for getting the content of a reading section of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading section is retrieved.</param>
/// <param name="LocationRef">The opaque location reference of the reading section.</param>
public record GetReadingSectionQuery(
    Guid BookId,
    string LocationRef
) : IQuery;
