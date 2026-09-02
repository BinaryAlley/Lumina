#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get the content of a reading section of a book.
/// </summary>
/// <param name="BookId">The unique identifier of the book whose reading section is retrieved. Required.</param>
/// <param name="LocationRef">The opaque location reference of the reading section. Required.</param>
[DebuggerDisplay("BookId: {BookId}, LocationRef: {LocationRef}")]
public record GetBookReadingSectionRequest(
    Guid BookId,
    string LocationRef
);
