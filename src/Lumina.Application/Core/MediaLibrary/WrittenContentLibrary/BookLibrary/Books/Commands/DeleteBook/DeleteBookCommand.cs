#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Command for deleting a book by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book to delete, taken from the route.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record DeleteBookCommand(
    string? LibraryId,
    string? BookId
) : ICommand;
