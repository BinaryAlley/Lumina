#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Query for getting all the books of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose books are retrieved.</param>
public record GetBooksQuery(
    Guid LibraryId
) : IQuery;
