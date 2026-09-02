#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get the reading manifest of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading manifest is retrieved. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public sealed record GetReadingManifestRequest(
    Guid BookId
);
