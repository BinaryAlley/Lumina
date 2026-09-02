#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Interface for the service for reading books, using the book reader plugins configured for their media library.
/// </summary>
public interface IBookReadingService
{
    /// <summary>
    /// Gets the reading manifest of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the reading manifest of the book, or an error.</returns>
    Task<Result<ReadingManifestResponse>> GetManifestAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, bool shouldRenderPdfAsImages, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the content of the reading section identified by <paramref name="locationRef"/> of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="locationRef">The opaque location reference of the reading section.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="shouldPreserveStyles">Whether the styles of the section content are preserved when it is sanitized, or stripped.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the content of the reading section, or an error.</returns>
    Task<Result<ReadingSectionDto>> GetSectionAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, string locationRef, bool shouldRenderPdfAsImages, bool shouldPreserveStyles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the resource identified by <paramref name="resourceKey"/> of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="resourceKey">The opaque resource key of the resource.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the resource, or an error.</returns>
    Task<Result<ReadingResourceDataDto>> GetResourceAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, string resourceKey, bool shouldRenderPdfAsImages, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the book stored at <paramref name="path"/> can be opened for reading, resolving the book reader configured
    /// for its media library and verifying that the reader is enabled, without extracting the book.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the reading availability of the book, or an error.</returns>
    Task<Result<ReadingAvailabilityResponse>> GetAvailabilityAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, CancellationToken cancellationToken);
}
