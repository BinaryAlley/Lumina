#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.RestoreBook;

/// <summary>
/// Command for restoring a deleted book by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the deleted book to restore, taken from the route.</param>
[DebuggerDisplay("BookId: {BookId}")]
public record RestoreBookCommand(
    string? LibraryId,
    string? BookId
) : ICommand;
