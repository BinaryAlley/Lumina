#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get the books of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose books are retrieved. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetBooksRequest(
    Guid LibraryId
);
