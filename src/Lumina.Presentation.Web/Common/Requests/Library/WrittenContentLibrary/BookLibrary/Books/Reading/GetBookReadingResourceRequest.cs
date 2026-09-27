#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get a resource of a book, for reading.
/// </summary>
/// <param name="BookId">The unique identifier of the book whose resource is retrieved. Required.</param>
/// <param name="ResourceKey">The opaque resource key of the resource. Required.</param>
/// <param name="LibraryId">The Id of the media library the book belongs to. Required.</param>
[DebuggerDisplay("BookId: {BookId}, ResourceKey: {ResourceKey}")]
public record GetBookReadingResourceRequest(
    Guid BookId,
    string ResourceKey,
    Guid LibraryId
);
