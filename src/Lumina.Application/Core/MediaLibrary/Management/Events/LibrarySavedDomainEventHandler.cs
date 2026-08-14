#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library is created or updated.
/// </summary>
public class LibrarySavedDomainEventHandler : IDomainEventHandler<LibrarySavedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;
    private readonly MediaSettingsDto _mediaSettingsModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrarySavedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="mediaSettingsModelOptions">Injected service for retrieving <see cref="MediaSettingsDto"/>.</param>
    public LibrarySavedDomainEventHandler(
        IUnitOfWork unitOfWork,
        IEnvironmentContext environmentContext,
        IPathService pathService,
        IOptions<MediaSettingsDto> mediaSettingsModelOptions)
    {
        _unitOfWork = unitOfWork;
        _environmentContext = environmentContext;
        _pathService = pathService;
        _mediaSettingsModel = mediaSettingsModelOptions.Value;
    }

    /// <summary>
    /// Handles the event raised when a media library is created or updated.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibrarySavedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.Library.CoverImage is not null)
        {
            // attempt to copy the image from the original location provided by the user to the internal location for media library files
            Result<string> saveCoverImageResult = await SaveCoverImageToMediaDirectoryAsync(domainEvent.Library.Id.Value, domainEvent.Library.CoverImage, cancellationToken).ConfigureAwait(false);
            if (saveCoverImageResult.IsFailure)
                throw new EventualConsistencyException(saveCoverImageResult.FirstError, saveCoverImageResult.Errors);
            domainEvent.Library.SetInternalLibraryCoverImagePath(saveCoverImageResult.Value);
            // update the media library with the new cover location
            ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
            Result<Updated> updateLibraryResult = await libraryRepository.UpdateAsync(domainEvent.Library.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateLibraryResult.IsFailure)
                throw new EventualConsistencyException(updateLibraryResult.FirstError, updateLibraryResult.Errors);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else // no cover image is provided, delete any cover image that might exist in the internal location for media library files
        {
            Result<string> libraryPathResult = GetLibraryPath(domainEvent.Library.Id.Value);
            if (libraryPathResult.IsFailure)
                throw new EventualConsistencyException(libraryPathResult.FirstError, libraryPathResult.Errors);
            DeleteCoverImageFromMediaDirectory(libraryPathResult.Value);
        }
    }

    /// <summary>
    /// Gets the file system path of a media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The id of the media library for which to regtrieve the file system path.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the file system path of the media library, or an error message.
    /// </returns>
    private Result<string> GetLibraryPath(Guid libraryId)
    {
        // root directory for media
        Result<string> rootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettingsModel.RootDirectory); 
        if (rootPathResult.IsFailure)
            return rootPathResult.Errors;

        // libraries directory
        Result<string> librariesPathResult = _pathService.CombinePath(rootPathResult.Value, _mediaSettingsModel.LibrariesDirectory);
        if (librariesPathResult.IsFailure)
            return librariesPathResult.Errors;

        // this new particular library' directory
        Result<string> libraryPathResult = _pathService.CombinePath(librariesPathResult.Value, libraryId.ToString());
        if (libraryPathResult.IsFailure)
            return libraryPathResult.Errors;
        return libraryPathResult.Value;
    }

    /// <summary>
    /// Saves the cover image of a media library to the internal media directory.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to save the cover image.</param>
    /// <param name="imagePath">The file path of the image file that will be copied to the media directory.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the location of the new cover image, or an error message.
    /// </returns>
    private async Task<Result<string>> SaveCoverImageToMediaDirectoryAsync(Guid libraryId, string imagePath, CancellationToken cancellationToken)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(imagePath);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;

        Result<bool> fileExistsResult = _environmentContext.FileProviderService.FileExists(fileSystemPathIdResult.Value);
        if (fileExistsResult.IsFailure)
            return fileExistsResult.Errors;
        if (!fileExistsResult.Value)
            return Errors.FileSystemManagement.FileNotFound;

        // make sure the file is an actual supported image
        Result<ImageType> imageCheckResult = await _environmentContext.FileTypeService.GetImageTypeAsync(fileSystemPathIdResult.Value, cancellationToken).ConfigureAwait(false);
        if (imageCheckResult.IsFailure)
            return imageCheckResult.Errors;
        if (imageCheckResult.Value == ImageType.None)
            return Errors.Library.CoverFileMustBeAnImage;

        // the provided cover image path exists and is a valid image, store it in the media directory
        Result<string> rootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettingsModel.RootDirectory); // root directory for media
        if (rootPathResult.IsFailure)
            return rootPathResult.Errors;

        Result<string> librariesPathResult = _pathService.CombinePath(rootPathResult.Value, _mediaSettingsModel.LibrariesDirectory); // libraries directory
        if (librariesPathResult.IsFailure)
            return librariesPathResult.Errors;

        Result<FileSystemPathId> rootPathIdResult = FileSystemPathId.Create(rootPathResult.Value);
        if (rootPathIdResult.IsFailure)
            return rootPathIdResult.Errors;

        Result<FileSystemPathId> librariesPathIdResult = FileSystemPathId.Create(librariesPathResult.Value);
        if (librariesPathIdResult.IsFailure)
            return librariesPathIdResult.Errors;

        // create the path of the new library
        Result<string> libraryPathResult = GetLibraryPath(libraryId);
        if (libraryPathResult.IsFailure)
            return libraryPathResult.Errors;

        Result<FileSystemPathId> newLibraryPathIdResult = FileSystemPathId.Create(libraryPathResult.Value);
        if (newLibraryPathIdResult.IsFailure)
            return newLibraryPathIdResult.Errors;

        // check if it doesn't already exist, and if so, create it
        Result<bool> directoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(newLibraryPathIdResult.Value);
        if (directoryExistsResult.IsFailure)
            return directoryExistsResult.Errors;

        if (!directoryExistsResult.Value)
        {
            Result<FileSystemPathId> createDirectoryResult = _environmentContext.DirectoryProviderService.CreateDirectory(librariesPathIdResult.Value, libraryId.ToString());
            if (createDirectoryResult.IsFailure)
                return createDirectoryResult.Errors;
        }
        else
        {
            // delete previous library covers that might exist
            DeleteCoverImageFromMediaDirectory(libraryPathResult.Value);

            Result<Deleted> deleteExistingCoverImageResult = DeleteCoverImageFromMediaDirectory(libraryPathResult.Value);
            if (deleteExistingCoverImageResult.IsFailure)
                return deleteExistingCoverImageResult.Errors;
        }
        // copy the new cover file from the location provided by the user
        Result<FileSystemPathId> copyFileResult = _environmentContext.FileProviderService.CopyFile(fileSystemPathIdResult.Value, newLibraryPathIdResult.Value, true);
        if (copyFileResult.IsFailure)
            return copyFileResult.Errors;

        // rename the new cover file to the standard naming
        Result<FileSystemPathId> renameFileResult = _environmentContext.FileProviderService.RenameFile(copyFileResult.Value, $"cover.{imageCheckResult.Value.ToString().ToLower()}");
        if (renameFileResult.IsFailure)
            return renameFileResult.Errors;

        // get the internal relative path for the copied file
        string relativePath = renameFileResult.Value.Path[AppContext.BaseDirectory.Length..];
        if (!relativePath.StartsWith(_pathService.PathSeparator))
            relativePath = $"{_pathService.PathSeparator}{relativePath}";
        return relativePath;
    }

    /// <summary>
    /// Deletes the cover image from the media directory.
    /// </summary>
    /// <param name="libraryPath">The path where the library content is located.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Deleted> DeleteCoverImageFromMediaDirectory(string libraryPath)
    {

        Result<FileSystemPathId> newLibraryPathIdResult = FileSystemPathId.Create(libraryPath);
        if (newLibraryPathIdResult.IsFailure)
            return newLibraryPathIdResult.Errors;

        // get existing files of this media library's directory, and delete previous cover files, if they are found
        Result<IEnumerable<FileSystemPathId>> getExistingLibraryFilesResult = _environmentContext.FileProviderService.GetFilePaths(newLibraryPathIdResult.Value, true);
        if (getExistingLibraryFilesResult.IsFailure)
            return getExistingLibraryFilesResult.Errors;

        foreach (FileSystemPathId filePathId in getExistingLibraryFilesResult.Value)
            if (Regex.IsMatch(filePathId.Path, @"[\\/]cover\.[^.\\/]+$", RegexOptions.IgnoreCase))
                _environmentContext.FileProviderService.DeleteFile(filePathId);

        return Result.Deleted;
    }
}
