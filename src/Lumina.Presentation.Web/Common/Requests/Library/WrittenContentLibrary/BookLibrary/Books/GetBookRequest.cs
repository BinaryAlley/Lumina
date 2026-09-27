#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get a book by the specified Id.
/// </summary>
/// <param name="BookId">The unique identifier of the book to retrieve, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record GetBookRequest(
    string BookId,
    Guid LibraryId
);
