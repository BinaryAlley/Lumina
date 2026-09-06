#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="UpdateBookCommand"/>.
/// </summary>
public static class UpdateBookCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to <see cref="BookMetadataDto"/>, so that the edited data can be applied to the domain book.
    /// </summary>
    /// <param name="command">The command containing the edited data of the book.</param>
    /// <returns>The built metadata.</returns>
    public static BookMetadataDto ToBookMetadataDto(this UpdateBookCommand command)
    {
        return new BookMetadataDto(
            Title: command.Metadata!.Title,
            OriginalTitle: command.Metadata.OriginalTitle,
            Description: command.Metadata.Description,
            ReleaseInfo: command.Metadata.ReleaseInfo,
            Genres: command.Metadata.Genres,
            Tags: command.Metadata.Tags,
            Language: command.Metadata.Language,
            OriginalLanguage: command.Metadata.OriginalLanguage,
            Publisher: command.Metadata.Publisher,
            PageCount: command.Metadata.PageCount,
            Format: command.Format,
            Edition: command.Edition,
            VolumeNumber: command.VolumeNumber,
            Series: command.Series,
            ASIN: command.ASIN,
            GoodreadsId: command.GoodreadsId,
            LCCN: command.LCCN,
            OCLCNumber: command.OCLCNumber,
            OpenLibraryId: command.OpenLibraryId,
            LibraryThingId: command.LibraryThingId,
            GoogleBooksId: command.GoogleBooksId,
            BarnesAndNobleId: command.BarnesAndNobleId,
            AppleBooksId: command.AppleBooksId,
            Isbns: command.ISBNs,
            Contributors: command.Contributors,
            Ratings: command.Ratings,
            CoverImagePath: null
        );
    }
}
