#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="UpdateBookCoverCommand"/>.
/// </summary>
public static class UpdateBookCoverCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to the cover artwork of the book identified by <paramref name="bookId"/>, replacing the
    /// file of <paramref name="existingCover"/> when the book already has a cover, or creating a new cover artwork when it does not.
    /// </summary>
    /// <param name="command">The command whose cover image was stored.</param>
    /// <param name="existingCover">The existing cover artwork of the book, or <see langword="null"/> when the book has none.</param>
    /// <param name="bookId">The Id of the book the cover belongs to.</param>
    /// <param name="storedCoverPath">The relative path of the stored cover image.</param>
    /// <param name="userId">The Id of the user that stored the cover image.</param>
    /// <returns>The cover artwork of the book, carrying the stored cover image.</returns>
    public static BookArtworkEntity ToRepositoryEntity(this UpdateBookCoverCommand command, BookArtworkEntity? existingCover, Guid bookId, string storedCoverPath, Guid userId)
    {
        if (existingCover is null)
        {
            return new BookArtworkEntity
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                ArtworkType = ArtworkType.Cover,
                Ordinal = 0,
                FileName = storedCoverPath,
                Status = ArtworkStatus.Enriched,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = null
            };
        }

        existingCover.FileName = storedCoverPath;
        existingCover.Status = ArtworkStatus.Enriched;
        existingCover.LastUpdateUtc = DateTime.UtcNow;
        existingCover.UpdatedOnUtc = DateTime.UtcNow;
        existingCover.UpdatedBy = userId;
        return existingCover;
    }
}
