#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get the reading manifest of a book.
/// </summary>
/// <param name="BookId">The unique identifier of the book whose reading manifest is retrieved. Required.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record GetBookReadingManifestRequest(
    Guid BookId,
    Guid LibraryId
);
