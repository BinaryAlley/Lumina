#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get a book by the specified Id.
/// </summary>
/// <param name="Id">The Id of the book to retrieve.</param>
[DebuggerDisplay("Id: {Id}")]
public record GetBookRequest(
    string? Id
);
