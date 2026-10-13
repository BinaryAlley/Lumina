#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Common.Events;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library is deleted.
/// </summary>
public class LibraryDeletedDomainEventHandler : IDomainEventHandler<LibraryDeletedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;
    private readonly MediaSettingsDto _mediaSettingsModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDeletedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="mediaSettingsModelOptions">Injected service for retrieving <see cref="MediaSettingsDto"/>.</param>
    public LibraryDeletedDomainEventHandler(IUnitOfWork unitOfWork, IEnvironmentContext environmentContext, IPathService pathService, IOptions<MediaSettingsDto> mediaSettingsModelOptions)
    {
        _unitOfWork = unitOfWork;
        _environmentContext = environmentContext;
        _pathService = pathService;
        _mediaSettingsModel = mediaSettingsModelOptions.Value;
    }

    /// <summary>
    /// Handles the event raised when a media library is deleted.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibraryDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid libraryId = domainEvent.Library.Id.Value;

        // The media items of the library and their artwork do not cascade from the library row, because they reference it by a plain id,
        // so they are removed first, otherwise they would be orphaned in the database. The artwork is removed before its owners, so that
        // its owners are still present when the artwork is looked up.
        Result<Deleted> deleteMusicArtworkResult = await _unitOfWork.MusicArtworkRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (deleteMusicArtworkResult.IsFailure)
            throw new EventualConsistencyException(deleteMusicArtworkResult.FirstError, deleteMusicArtworkResult.Errors);

        // Deleting the artists removes their albums, tracks and their related data, through the cascading foreign keys.
        Result<Deleted> deleteArtistsResult = await _unitOfWork.ArtistRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (deleteArtistsResult.IsFailure)
            throw new EventualConsistencyException(deleteArtistsResult.FirstError, deleteArtistsResult.Errors);

        // Deleting the books removes their artwork and their related data, through the cascading foreign keys.
        Result<Deleted> deleteBooksResult = await _unitOfWork.BookRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (deleteBooksResult.IsFailure)
            throw new EventualConsistencyException(deleteBooksResult.FirstError, deleteBooksResult.Errors);

        // The staged music metadata of the library does not cascade from the library row either, so it is removed as well.
        Result<Deleted> deleteStagedMusicMetadataResult = await _unitOfWork.MusicLibraryScanItemMetadataRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (deleteStagedMusicMetadataResult.IsFailure)
            throw new EventualConsistencyException(deleteStagedMusicMetadataResult.FirstError, deleteStagedMusicMetadataResult.Errors);

        // The cover image of the library is stored under the libraries directory, under a directory named after the library id,
        // while the artwork of the media items of the library is stored under the per-media-type directories, under directories
        // named after the library, so all of them must be removed when the library is deleted. All the media item files of a library
        // live inside these directories, so removing them removes the library's stored media without touching the user's own files.
        DeleteLibraryDirectory(_mediaSettingsModel.LibrariesDirectory, libraryId.ToString());

        Result<PathSegment> librarySegmentResult = _pathService.SanitizeSegment($"{domainEvent.Library.Title}-{libraryId}");
        if (librarySegmentResult.IsFailure)
            throw new EventualConsistencyException(librarySegmentResult.FirstError, librarySegmentResult.Errors);
        DeleteLibraryDirectory(_mediaSettingsModel.BooksDirectory, librarySegmentResult.Value.Name);
        DeleteLibraryDirectory(_mediaSettingsModel.MusicDirectory, librarySegmentResult.Value.Name);
    }

    /// <summary>
    /// Deletes the directory of a media library from the internal media location of the provided media type.
    /// </summary>
    /// <param name="mediaTypeDirectory">The name of the media type subdirectory the library directory is stored under.</param>
    /// <param name="libraryDirectoryName">The name of the directory of the media library.</param>
    private void DeleteLibraryDirectory(string mediaTypeDirectory, string libraryDirectoryName)
    {
        // Root directory for media.
        Result<string> rootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettingsModel.RootDirectory);
        if (rootPathResult.IsFailure)
            throw new EventualConsistencyException(rootPathResult.FirstError, rootPathResult.Errors);

        // Media type directory.
        Result<string> mediaTypePathResult = _pathService.CombinePath(rootPathResult.Value, mediaTypeDirectory);
        if (mediaTypePathResult.IsFailure)
            throw new EventualConsistencyException(mediaTypePathResult.FirstError, mediaTypePathResult.Errors);

        // This particular library's directory.
        Result<string> libraryPathResult = _pathService.CombinePath(mediaTypePathResult.Value, libraryDirectoryName);
        if (libraryPathResult.IsFailure)
            throw new EventualConsistencyException(libraryPathResult.FirstError, libraryPathResult.Errors);

        Result<FileSystemPathId> libraryPathIdResult = FileSystemPathId.Create(libraryPathResult.Value);
        if (libraryPathIdResult.IsFailure)
            throw new EventualConsistencyException(libraryPathIdResult.FirstError, libraryPathIdResult.Errors);

        // The directory only exists when the library actually stored something under this media type, so a missing directory is not an error.
        Result<bool> directoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(libraryPathIdResult.Value);
        if (directoryExistsResult.IsFailure)
            throw new EventualConsistencyException(directoryExistsResult.FirstError, directoryExistsResult.Errors);
        if (!directoryExistsResult.Value)
            return;

        Result<Deleted> deleteLibraryDirectoryResult = _environmentContext.DirectoryProviderService.DeleteDirectory(libraryPathIdResult.Value);
        if (deleteLibraryDirectoryResult.IsFailure)
            throw new EventualConsistencyException(deleteLibraryDirectoryResult.FirstError, deleteLibraryDirectoryResult.Errors);
    }
}
