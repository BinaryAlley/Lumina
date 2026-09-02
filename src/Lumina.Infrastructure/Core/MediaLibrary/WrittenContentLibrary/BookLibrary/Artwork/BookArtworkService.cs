#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;

/// <summary>
/// Stores the artwork of a book into the internal media directory, under a per-book directory, and returns its relative path.
/// </summary>
internal sealed class BookArtworkService : IBookArtworkService
{
    private const long MAX_ARTWORK_SIZE_BYTES = 10 * 1024 * 1024;

    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MediaSettingsDto _mediaSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookArtworkService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="httpClientFactory">Injected factory used to create the HTTP clients that download the remote artwork.</param>
    /// <param name="mediaSettingsOptions">Injected service for retrieving <see cref="MediaSettingsDto"/>.</param>
    public BookArtworkService(IEnvironmentContext environmentContext, IPathService pathService, IHttpClientFactory httpClientFactory, IOptions<MediaSettingsDto> mediaSettingsOptions)
    {
        _environmentContext = environmentContext;
        _pathService = pathService;
        _httpClientFactory = httpClientFactory;
        _mediaSettings = mediaSettingsOptions.Value;
    }

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
    public async Task<Result<string>> SaveBookArtworkAsync(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle, ArtworkDto artwork, CancellationToken cancellationToken)
    {
        // resolve the local file path of the artwork, downloading it to a temporary file when it is remote
        Result<ArtworkSourceResult> resolveSourceResult = await ResolveArtworkSourceAsync(artwork, cancellationToken).ConfigureAwait(false);
        if (resolveSourceResult.IsFailure)
            return resolveSourceResult.Errors;
        string sourcePath = resolveSourceResult.Value.Path;

        try
        {
            Result<string> storeResult = await StoreArtworkAsync(libraryId, bookId, libraryName, authorName, bookTitle, sourcePath, cancellationToken).ConfigureAwait(false);
            if (storeResult.IsFailure)
                return storeResult.Errors;
            return storeResult;
        }
        finally
        {
            // remove the temporary file downloaded for the remote artwork, never a local file of the user
            if (resolveSourceResult.Value.IsTemporary && File.Exists(sourcePath))
            {
                try
                {
                    File.Delete(sourcePath);
                }
                catch (IOException)
                {
                    // a failed cleanup of the temporary file must not mask the result of the operation
                }
                catch (UnauthorizedAccessException)
                {
                    // a failed cleanup of the temporary file must not mask the result of the operation
                }
            }
        }
    }

    /// <summary>
    /// Deletes the artwork of the book from the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> DeleteBookArtwork(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle)
    {
        Result<string> artworkDirectoryPathResult = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;
        DeleteCover(artworkDirectoryPathResult.Value);
        return Result.Deleted;
    }

    /// <summary>
    /// Stores the artwork file at <paramref name="sourcePath"/> into the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <param name="sourcePath">The file system path of the artwork file to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    private async Task<Result<string>> StoreArtworkAsync(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle, string sourcePath, CancellationToken cancellationToken)
    {
        Result<FileSystemPathId> sourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (sourcePathIdResult.IsFailure)
            return sourcePathIdResult.Errors;

        Result<bool> fileExistsResult = _environmentContext.FileProviderService.FileExists(sourcePathIdResult.Value);
        if (fileExistsResult.IsFailure)
            return fileExistsResult.Errors;
        if (!fileExistsResult.Value)
            return Errors.FileSystemManagement.FileNotFound;

        // reject the artwork when it is a symbolic link or a reparse point, which could point to any file of the machine
        if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.ReparsePoint))
            return Errors.FileSystemManagement.InvalidPath;

        // reject the artwork when it exceeds the maximum allowed size
        FileInfo sourceFileInfo = new(sourcePath);
        if (sourceFileInfo.Length > MAX_ARTWORK_SIZE_BYTES)
            return Errors.FileSystemManagement.FileTooLarge;

        // make sure the file is an actual supported image
        Result<ImageType> imageTypeResult = await _environmentContext.FileTypeService.GetImageTypeAsync(sourcePathIdResult.Value, cancellationToken).ConfigureAwait(false);
        if (imageTypeResult.IsFailure)
            return imageTypeResult.Errors;
        if (imageTypeResult.Value == ImageType.None)
            return Errors.Library.CoverFileMustBeAnImage;

        Result<string> artworkDirectoryPathResult = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;

        Result<Success> ensureDirectoryResult = EnsureDirectory(artworkDirectoryPathResult.Value);
        if (ensureDirectoryResult.IsFailure)
            return ensureDirectoryResult.Errors;

        // delete the previous cover of the book, so that a changed artwork does not leave orphaned files behind
        DeleteCover(artworkDirectoryPathResult.Value);

        Result<FileSystemPathId> artworkDirectoryPathIdResult = FileSystemPathId.Create(artworkDirectoryPathResult.Value);
        if (artworkDirectoryPathIdResult.IsFailure)
            return artworkDirectoryPathIdResult.Errors;

        // copy the artwork file from the source location
        Result<FileSystemPathId> copyFileResult = _environmentContext.FileProviderService.CopyFile(sourcePathIdResult.Value, artworkDirectoryPathIdResult.Value, true);
        if (copyFileResult.IsFailure)
            return copyFileResult.Errors;

        // rename the copied file to the standard naming
        Result<FileSystemPathId> renameFileResult = _environmentContext.FileProviderService.RenameFile(copyFileResult.Value, $"cover.{imageTypeResult.Value.ToString().ToLowerInvariant()}");
        if (renameFileResult.IsFailure)
            return renameFileResult.Errors;

        // get the internal relative path for the copied file
        string relativePath = renameFileResult.Value.Path[AppContext.BaseDirectory.Length..];
        if (!relativePath.StartsWith(_pathService.PathSeparator))
            relativePath = $"{_pathService.PathSeparator}{relativePath}";
        return relativePath;
    }

    /// <summary>
    /// Resolves the local file system path of the <paramref name="artwork"/>, downloading it to a temporary file when it is remote.
    /// </summary>
    /// <param name="artwork">The artwork whose source path is resolved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the resolved local source of the artwork, or an error.</returns>
    private async Task<Result<ArtworkSourceResult>> ResolveArtworkSourceAsync(ArtworkDto artwork, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(artwork.LocalPath))
            return new ArtworkSourceResult(artwork.LocalPath, IsTemporary: false);

        if (string.IsNullOrWhiteSpace(artwork.RemoteUrl))
            return Errors.FileSystemManagement.FileNotFound;

        // download the remote artwork into a temporary file, aborting when it exceeds the maximum allowed size
        using HttpClient httpClient = _httpClientFactory.CreateClient();
        string tempPath = Path.Combine(Path.GetTempPath(), $"lumina-artwork-{Guid.NewGuid():N}");
        bool downloaded = false;
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(artwork.RemoteUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Errors.FileSystemManagement.FileNotFound;

            await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);

            byte[] buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;
            while ((bytesRead = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                totalRead += bytesRead;
                if (totalRead > MAX_ARTWORK_SIZE_BYTES)
                    return Errors.FileSystemManagement.FileTooLarge;
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            }
            downloaded = true;
            return new ArtworkSourceResult(tempPath, IsTemporary: true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Errors.FileSystemManagement.FileNotFound;
        }
        finally
        {
            // remove the partially downloaded temporary file when the download failed
            if (!downloaded && File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Builds the file system path of the directory of the book artwork, under the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of the book artwork directory, or an error.</returns>
    private Result<string> BuildArtworkDirectoryPath(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle)
    {
        Result<string> mediaRootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettings.RootDirectory);
        if (mediaRootPathResult.IsFailure)
            return mediaRootPathResult.Errors;
        Result<string> booksPathResult = _pathService.CombinePath(mediaRootPathResult.Value, _mediaSettings.BooksDirectory);
        if (booksPathResult.IsFailure)
            return booksPathResult.Errors;

        Result<PathSegment> librarySegmentResult = _pathService.SanitizeSegment($"{libraryName}-{libraryId}");
        if (librarySegmentResult.IsFailure)
            return librarySegmentResult.Errors;
        Result<PathSegment> authorSegmentResult = _pathService.SanitizeSegment(string.IsNullOrWhiteSpace(authorName) ? "Unknown" : authorName);
        if (authorSegmentResult.IsFailure)
            return authorSegmentResult.Errors;
        Result<PathSegment> bookSegmentResult = _pathService.SanitizeSegment($"{bookTitle}-{bookId}");
        if (bookSegmentResult.IsFailure)
            return bookSegmentResult.Errors;

        Result<string> libraryPathResult = _pathService.CombinePath(booksPathResult.Value, librarySegmentResult.Value.Name);
        if (libraryPathResult.IsFailure)
            return libraryPathResult.Errors;
        Result<string> authorPathResult = _pathService.CombinePath(libraryPathResult.Value, authorSegmentResult.Value.Name);
        if (authorPathResult.IsFailure)
            return authorPathResult.Errors;
        return _pathService.CombinePath(authorPathResult.Value, bookSegmentResult.Value.Name);
    }

    /// <summary>
    /// Creates the directory at <paramref name="directoryPath"/>, along with its missing parents.
    /// </summary>
    /// <param name="directoryPath">The file system path of the directory to create.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Success> EnsureDirectory(string directoryPath)
    {
        Result<FileSystemPathId> directoryPathIdResult = FileSystemPathId.Create(directoryPath);
        if (directoryPathIdResult.IsFailure)
            return directoryPathIdResult.Errors;

        Result<bool> directoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(directoryPathIdResult.Value);
        if (directoryExistsResult.IsFailure)
            return directoryExistsResult.Errors;
        if (directoryExistsResult.Value)
            return Result.Success;

        string? parentDirectoryPath = Path.GetDirectoryName(directoryPath);
        if (!string.IsNullOrEmpty(parentDirectoryPath) && !string.Equals(parentDirectoryPath, directoryPath, StringComparison.Ordinal))
        {
            Result<Success> createParentResult = EnsureDirectory(parentDirectoryPath);
            if (createParentResult.IsFailure)
                return createParentResult.Errors;
        }

        Result<FileSystemPathId> parentDirectoryPathIdResult = FileSystemPathId.Create(parentDirectoryPath ?? directoryPath);
        if (parentDirectoryPathIdResult.IsFailure)
            return parentDirectoryPathIdResult.Errors;

        Result<FileSystemPathId> createDirectoryResult = _environmentContext.DirectoryProviderService.CreateDirectory(parentDirectoryPathIdResult.Value, Path.GetFileName(directoryPath));
        if (createDirectoryResult.IsFailure)
            return createDirectoryResult.Errors;
        return Result.Success;
    }

    /// <summary>
    /// Deletes the cover files of the book from the <paramref name="artworkDirectoryPath"/>.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory of the book artwork.</param>
    private void DeleteCover(string artworkDirectoryPath)
    {
        Result<FileSystemPathId> directoryPathIdResult = FileSystemPathId.Create(artworkDirectoryPath);
        if (directoryPathIdResult.IsFailure)
            return;

        Result<IEnumerable<FileSystemPathId>> getFilesResult = _environmentContext.FileProviderService.GetFilePaths(directoryPathIdResult.Value, true);
        if (getFilesResult.IsFailure)
            return;

        foreach (FileSystemPathId filePathId in getFilesResult.Value)
            if (Regex.IsMatch(filePathId.Path, @"[\\/]cover\.[^.\\/]+$", RegexOptions.IgnoreCase))
                _environmentContext.FileProviderService.DeleteFile(filePathId);
    }

    /// <summary>
    /// Describes the resolved local source of the artwork.
    /// </summary>
    private sealed record ArtworkSourceResult(string Path, bool IsTemporary);
}
