#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Infrastructure.Common.Utilities;
using Lumina.Infrastructure.Common.Networking;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Stores the artwork of an album into the internal media directory, under a per-album directory, and returns its relative path.
/// </summary>
internal sealed class MusicArtworkService : IMusicArtworkService
{
    private const long MAX_ARTWORK_SIZE_BYTES = 10 * 1024 * 1024;

    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MediaSettingsDto _mediaSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="httpClientFactory">Injected factory used to create the HTTP clients that download the remote artwork.</param>
    /// <param name="mediaSettingsOptions">Injected service for retrieving <see cref="MediaSettingsDto"/>.</param>
    public MusicArtworkService(IEnvironmentContext environmentContext, IPathService pathService, IHttpClientFactory httpClientFactory, IOptions<MediaSettingsDto> mediaSettingsOptions)
    {
        _environmentContext = environmentContext;
        _pathService = pathService;
        _httpClientFactory = httpClientFactory;
        _mediaSettings = mediaSettingsOptions.Value;
    }

    /// <summary>
    /// Stores the <paramref name="artwork"/> of the album into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <param name="releaseTypeName">The name of the release type directory the album is stored under on disk, used to group the artwork of the different releases of an artist. When omitted, no release type segment is added.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    public async Task<Result<string>> SaveAlbumArtworkAsync(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, ArtworkDto artwork, CancellationToken cancellationToken, string? releaseTypeName = null)
    {
        Result<string> artworkDirectoryPathResult = BuildAlbumArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle, releaseTypeName);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;
        return await SaveArtworkAsync(artworkDirectoryPathResult.Value, artwork, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stores the <paramref name="artwork"/> of the artist into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    public async Task<Result<string>> SaveArtistArtworkAsync(Guid libraryId, string libraryName, string artistName, ArtworkDto artwork, CancellationToken cancellationToken)
    {
        Result<string> artworkDirectoryPathResult = BuildArtistArtworkDirectoryPath(libraryId, libraryName, artistName);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;
        return await SaveArtworkAsync(artworkDirectoryPathResult.Value, artwork, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the local source of the <paramref name="artwork"/>, stores it into <paramref name="artworkDirectoryPath"/>, and cleans up the temporary download of a remote artwork.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory the artwork is stored into.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    private async Task<Result<string>> SaveArtworkAsync(string artworkDirectoryPath, ArtworkDto artwork, CancellationToken cancellationToken)
    {
        // Resolve the local file path of the artwork, downloading it to a temporary file when it is remote.
        Result<ArtworkSourceResult> resolveSourceResult = await ResolveArtworkSourceAsync(artwork, cancellationToken).ConfigureAwait(false);
        if (resolveSourceResult.IsFailure)
            return resolveSourceResult.Errors;
        string sourcePath = resolveSourceResult.Value.Path;

        try
        {
            return await StoreArtworkAsync(artworkDirectoryPath, artwork.Type, artwork.Ordinal, sourcePath, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Remove the temporary source of the artwork, whether it was downloaded for a remote artwork or produced by a provider, never a file of the user.
            if (resolveSourceResult.Value.IsTemporary && File.Exists(sourcePath))
            {
                try
                {
                    File.Delete(sourcePath);
                }
                catch (IOException)
                {
                    // A failed cleanup of the temporary file must not mask the result of the operation.
                }
                catch (UnauthorizedAccessException)
                {
                    // A failed cleanup of the temporary file must not mask the result of the operation.
                }
            }
        }
    }

    /// <summary>
    /// Deletes the stored artwork of the album from the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="releaseTypeName">The name of the release type directory the album is stored under on disk, used to group the artwork of the different releases of an artist. When omitted, no release type segment is added.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> DeleteAlbumArtwork(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, string? releaseTypeName = null)
    {
        Result<string> artworkDirectoryPathResult = BuildAlbumArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle, releaseTypeName);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;
        return DeleteArtworkInDirectory(artworkDirectoryPathResult.Value);
    }

    /// <summary>
    /// Deletes the stored artwork of the artist from the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> DeleteArtistArtwork(Guid libraryId, string libraryName, string artistName)
    {
        Result<string> artworkDirectoryPathResult = BuildArtistArtworkDirectoryPath(libraryId, libraryName, artistName);
        if (artworkDirectoryPathResult.IsFailure)
            return artworkDirectoryPathResult.Errors;
        return DeleteArtworkInDirectory(artworkDirectoryPathResult.Value);
    }

    /// <summary>
    /// Deletes the files directly contained in <paramref name="artworkDirectoryPath"/>, leaving the subdirectories, such as the album directories of an artist, untouched.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory whose files are deleted.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Deleted> DeleteArtworkInDirectory(string artworkDirectoryPath)
    {
        Result<FileSystemPathId> directoryPathIdResult = FileSystemPathId.Create(artworkDirectoryPath);
        if (directoryPathIdResult.IsFailure)
            return directoryPathIdResult.Errors;

        // When nothing was stored for the owner yet, its directory does not exist, so there is nothing to delete.
        Result<bool> directoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(directoryPathIdResult.Value);
        if (directoryExistsResult.IsFailure)
            return directoryExistsResult.Errors;
        if (!directoryExistsResult.Value)
            return Result.Deleted;

        Result<IEnumerable<FileSystemPathId>> getFilesResult = _environmentContext.FileProviderService.GetFilePaths(directoryPathIdResult.Value, true);
        if (getFilesResult.IsFailure)
            return getFilesResult.Errors;

        foreach (FileSystemPathId filePathId in getFilesResult.Value)
            _environmentContext.FileProviderService.DeleteFile(filePathId);

        return Result.Deleted;
    }

    /// <summary>
    /// Stores the artwork file at <paramref name="sourcePath"/> into <paramref name="artworkDirectoryPath"/>.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory the artwork is stored into.</param>
    /// <param name="artworkType">The type of the artwork being stored.</param>
    /// <param name="ordinal">The ordinal of the artwork within its type.</param>
    /// <param name="sourcePath">The file system path of the artwork file to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    private async Task<Result<string>> StoreArtworkAsync(string artworkDirectoryPath, ArtworkType artworkType, int ordinal, string sourcePath, CancellationToken cancellationToken)
    {
        Result<FileSystemPathId> sourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (sourcePathIdResult.IsFailure)
            return sourcePathIdResult.Errors;

        Result<bool> fileExistsResult = _environmentContext.FileProviderService.FileExists(sourcePathIdResult.Value);
        if (fileExistsResult.IsFailure)
            return fileExistsResult.Errors;
        if (!fileExistsResult.Value)
            return Errors.FileSystemManagement.FileNotFound;

        // Reject the artwork when it is a symbolic link or a reparse point, which could point to any file of the machine.
        if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.ReparsePoint))
            return Errors.FileSystemManagement.InvalidPath;

        // Reject the artwork when it exceeds the maximum allowed size.
        FileInfo sourceFileInfo = new(sourcePath);
        if (sourceFileInfo.Length > MAX_ARTWORK_SIZE_BYTES)
            return Errors.FileSystemManagement.FileTooLarge;

        // Make sure the file is an actual supported image.
        Result<ImageType> imageTypeResult = await _environmentContext.FileTypeService.GetImageTypeAsync(sourcePathIdResult.Value, cancellationToken).ConfigureAwait(false);
        if (imageTypeResult.IsFailure)
            return imageTypeResult.Errors;
        if (imageTypeResult.Value == ImageType.None)
            return Errors.Library.CoverFileMustBeAnImage;

        Result<Success> ensureDirectoryResult = EnsureDirectory(artworkDirectoryPath);
        if (ensureDirectoryResult.IsFailure)
            return ensureDirectoryResult.Errors;

        Result<FileSystemPathId> artworkDirectoryPathIdResult = FileSystemPathId.Create(artworkDirectoryPath);
        if (artworkDirectoryPathIdResult.IsFailure)
            return artworkDirectoryPathIdResult.Errors;

        // Copy the artwork file from the source location.
        Result<FileSystemPathId> copyFileResult = _environmentContext.FileProviderService.CopyFile(sourcePathIdResult.Value, artworkDirectoryPathIdResult.Value, true);
        if (copyFileResult.IsFailure)
            return copyFileResult.Errors;

        // Rename the copied file to the standard naming, derived from the type and the ordinal of the artwork.
        Result<FileSystemPathId> renameFileResult = _environmentContext.FileProviderService.RenameFile(copyFileResult.Value, $"{BuildArtworkFileStem(artworkType, ordinal)}.{imageTypeResult.Value.ToFileExtension()}");
        if (renameFileResult.IsFailure)
            return renameFileResult.Errors;

        // Get the internal relative path for the copied file.
        string relativePath = renameFileResult.Value.Path[AppContext.BaseDirectory.Length..];
        if (!relativePath.StartsWith(_pathService.PathSeparator))
            relativePath = $"{_pathService.PathSeparator}{relativePath}";
        return relativePath;
    }

    /// <summary>
    /// Builds the file name stem of an artwork, derived from its type and ordinal.
    /// </summary>
    /// <param name="artworkType">The type of the artwork.</param>
    /// <param name="ordinal">The ordinal of the artwork within its type.</param>
    /// <returns>The file name stem of the artwork.</returns>
    private static string BuildArtworkFileStem(ArtworkType artworkType, int ordinal)
    {
        string stem = artworkType switch
        {
            ArtworkType.Cover => "cover",
            ArtworkType.Front => "front",
            ArtworkType.Back => "back",
            ArtworkType.Booklet => "booklet",
            ArtworkType.Medium => "medium",
            ArtworkType.Tray => "tray",
            ArtworkType.Spine => "spine",
            ArtworkType.Obi => "obi",
            ArtworkType.Sticker => "sticker",
            ArtworkType.Poster => "poster",
            ArtworkType.Liner => "liner",
            ArtworkType.Watermark => "watermark",
            ArtworkType.Backdrop => "backdrop",
            ArtworkType.Banner => "banner",
            ArtworkType.Logo => "logo",
            ArtworkType.Thumb => "thumb",
            _ => "other"
        };
        return ordinal > 0 ? $"{stem}-{ordinal}" : stem;
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
            return new ArtworkSourceResult(artwork.LocalPath, IsTemporary: artwork.IsTemporary);

        if (string.IsNullOrWhiteSpace(artwork.RemoteUrl))
            return Errors.FileSystemManagement.FileNotFound;

        // Download the remote artwork into a temporary file, aborting when it exceeds the allowed size.
        using HttpClient httpClient = _httpClientFactory.CreateClient(NamedHttpClients.ARTWORK_DOWNLOAD);
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
            // Remove the partially downloaded temporary file when the download failed.
            if (!downloaded && File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Builds the file system path of the directory of the album artwork, under the internal media directory.
    /// The artwork of the album is stored under the directory of its release type, mirroring the structure of the scanned files.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="releaseTypeName">The name of the release type directory the album is stored under on disk, or <see langword="null"/> to store the artwork directly under the artist directory.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of the album artwork directory, or an error.</returns>
    private Result<string> BuildAlbumArtworkDirectoryPath(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, string? releaseTypeName)
    {
        Result<string> artistPathResult = BuildArtistArtworkDirectoryPath(libraryId, libraryName, artistName);
        if (artistPathResult.IsFailure)
            return artistPathResult.Errors;

        // The release type directory groups the releases of an artist that share a title but are different releases, like the album and the single of the same songs.
        Result<string> albumParentPathResult = artistPathResult;
        if (!string.IsNullOrWhiteSpace(releaseTypeName))
        {
            Result<PathSegment> releaseTypeSegmentResult = _pathService.SanitizeSegment(releaseTypeName);
            if (releaseTypeSegmentResult.IsFailure)
                return releaseTypeSegmentResult.Errors;
            Result<string> releaseTypePathResult = _pathService.CombinePath(artistPathResult.Value, releaseTypeSegmentResult.Value.Name);
            if (releaseTypePathResult.IsFailure)
                return releaseTypePathResult.Errors;
            albumParentPathResult = releaseTypePathResult;
        }

        Result<PathSegment> albumSegmentResult = _pathService.SanitizeSegment($"{albumTitle}-{albumId}");
        if (albumSegmentResult.IsFailure)
            return albumSegmentResult.Errors;
        return _pathService.CombinePath(albumParentPathResult.Value, albumSegmentResult.Value.Name);
    }

    /// <summary>
    /// Builds the file system path of the directory of the artist artwork, under the internal media directory.
    /// The artwork of the albums of the artist is stored in the subdirectories of this directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of the artist artwork directory, or an error.</returns>
    private Result<string> BuildArtistArtworkDirectoryPath(Guid libraryId, string libraryName, string artistName)
    {
        Result<string> libraryPathResult = BuildLibraryArtworkDirectoryPath(libraryId, libraryName);
        if (libraryPathResult.IsFailure)
            return libraryPathResult.Errors;

        Result<PathSegment> artistSegmentResult = _pathService.SanitizeSegment(string.IsNullOrWhiteSpace(artistName) ? "Unknown" : artistName);
        if (artistSegmentResult.IsFailure)
            return artistSegmentResult.Errors;
        return _pathService.CombinePath(libraryPathResult.Value, artistSegmentResult.Value.Name);
    }

    /// <summary>
    /// Builds the file system path of the directory of a music media library, under the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="libraryName">The name of the media library.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of the media library directory, or an error.</returns>
    private Result<string> BuildLibraryArtworkDirectoryPath(Guid libraryId, string libraryName)
    {
        Result<string> mediaRootPathResult = _pathService.CombinePath(AppContext.BaseDirectory, _mediaSettings.RootDirectory);
        if (mediaRootPathResult.IsFailure)
            return mediaRootPathResult.Errors;
        Result<string> musicPathResult = _pathService.CombinePath(mediaRootPathResult.Value, _mediaSettings.MusicDirectory);
        if (musicPathResult.IsFailure)
            return musicPathResult.Errors;

        Result<PathSegment> librarySegmentResult = _pathService.SanitizeSegment($"{libraryName}-{libraryId}");
        if (librarySegmentResult.IsFailure)
            return librarySegmentResult.Errors;
        return _pathService.CombinePath(musicPathResult.Value, librarySegmentResult.Value.Name);
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
    /// Describes the resolved local source of the artwork.
    /// </summary>
    /// <param name="Path">The file system path of the resolved source.</param>
    /// <param name="IsTemporary">Whether the resolved source is a temporary file that must be cleaned up after it is stored.</param>
    private sealed record ArtworkSourceResult(string Path, bool IsTemporary);
}
