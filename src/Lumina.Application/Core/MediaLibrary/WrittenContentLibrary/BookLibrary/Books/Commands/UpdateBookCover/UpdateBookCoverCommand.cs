#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Command for updating the cover image of an existing book.
/// </summary>
/// <param name="BookId">The Id of the book whose cover image is updated.</param>
/// <param name="Cover">The stream of the uploaded cover image file.</param>
/// <param name="FileName">The name of the uploaded cover image file.</param>
[DebuggerDisplay("BookId: {BookId} FileName: {FileName}")]
public record UpdateBookCoverCommand(
    Guid BookId,
    Stream? Cover,
    string? FileName
) : ICommand;
