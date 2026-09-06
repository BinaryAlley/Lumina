#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get a book by the specified Id.
/// </summary>
/// <param name="Id">The unique identifier of the book to retrieve, taken from the route. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record GetBookRequest(
    string Id
);
