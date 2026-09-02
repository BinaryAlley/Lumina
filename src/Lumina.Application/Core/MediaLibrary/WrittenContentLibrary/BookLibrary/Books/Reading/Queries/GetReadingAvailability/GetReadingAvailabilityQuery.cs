#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Query for checking the reading availability of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading availability is checked.</param>
public record GetReadingAvailabilityQuery(
    Guid BookId
) : IQuery;
