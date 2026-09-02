#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get the content of a reading section of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading section is retrieved. Required.</param>
/// <param name="LocationRef">The opaque location reference of the reading section. Required.</param>
[DebuggerDisplay("BookId: {BookId}, LocationRef: {LocationRef}")]
public sealed record GetReadingSectionRequest(
    Guid BookId,
    string LocationRef
);
