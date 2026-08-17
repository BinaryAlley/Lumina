#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents the request for displaying the books browsing view.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose books are browsed.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetBooksViewRequest(
    Guid? LibraryId
);
