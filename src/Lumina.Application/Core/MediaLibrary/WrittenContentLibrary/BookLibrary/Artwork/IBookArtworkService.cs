#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;

/// <summary>
/// Interface for the service for storing the artwork of a book into the internal media directory and serving its relative path.
/// </summary>
public interface IBookArtworkService
{
    /// <summary>
    /// Stores the <paramref name="artwork"/> of the book into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    Task<Result<string>> SaveBookArtworkAsync(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle, ArtworkDto artwork, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the artwork of the book from the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Result<Deleted> DeleteBookArtwork(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle);
}
