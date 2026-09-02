#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to check the reading availability of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading availability is checked. Required.</param>
[DebuggerDisplay("BookId: {BookId}")]
public sealed record GetReadingAvailabilityRequest(
    Guid BookId
);
