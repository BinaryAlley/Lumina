#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents the request for displaying the reading view of a book.
/// </summary>
/// <param name="BookId">The unique identifier of the book to read. Required.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record ReadBookViewRequest(
    Guid BookId,
    Guid LibraryId
);
