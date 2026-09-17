#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to check the reading availability of a book.
/// </summary>
/// <param name="BookId">The unique identifier of the book whose reading availability is checked. Required.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record GetBookReadingAvailabilityRequest(
    Guid BookId,
    Guid LibraryId
);
