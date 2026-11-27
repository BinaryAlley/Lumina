#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Command for updating the cover image of an existing book.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this book belongs to, taken from the route.</param>
/// <param name="BookId">The unique identifier of the book whose cover image is updated, taken from the route.</param>
/// <param name="Cover">The stream of the uploaded cover image file.</param>
/// <param name="FileName">The name of the uploaded cover image file.</param>
[DebuggerDisplay("BookId: {BookId} FileName: {FileName}")]
public record UpdateBookCoverCommand(
    string? LibraryId,
    string? BookId,
    Stream? Cover,
    string? FileName
) : ICommand;
