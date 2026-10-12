#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Interface for the service for storing the artwork of a music library item, an album or an artist, into the internal media directory and serving its relative path.
/// </summary>
public interface IMusicArtworkService
{
    /// <summary>
    /// Stores the <paramref name="artwork"/> of the album into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="releaseTypeName">The name of the release type directory the album is stored under on disk, used to group the artwork of the different releases of an artist. When <see langword="null"/>, no release type segment is added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    Task<Result<string>> SaveAlbumArtworkAsync(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, ArtworkDto artwork, string? releaseTypeName, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the uploaded <paramref name="artwork"/> of the album into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="artwork">The stream of the uploaded artwork to store.</param>
    /// <param name="fileName">The name of the uploaded artwork file.</param>
    /// <param name="releaseTypeName">The name of the release type directory the album is stored under on disk, used to group the artwork of the different releases of an artist. When <see langword="null"/>, no release type segment is added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    Task<Result<string>> SaveAlbumArtworkAsync(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, Stream artwork, string fileName, string? releaseTypeName, CancellationToken cancellationToken);

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
    Result<Deleted> DeleteAlbumArtwork(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle, string? releaseTypeName = null);

    /// <summary>
    /// Stores the <paramref name="artwork"/> of the artist into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <param name="artwork">The artwork to store.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    Task<Result<string>> SaveArtistArtworkAsync(Guid libraryId, string libraryName, string artistName, ArtworkDto artwork, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the uploaded <paramref name="artwork"/> of the artist into the internal media directory, and returns the relative path of the stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <param name="artwork">The stream of the uploaded artwork to store.</param>
    /// <param name="fileName">The name of the uploaded artwork file.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the relative path of the stored artwork, or an error.</returns>
    Task<Result<string>> SaveArtistArtworkAsync(Guid libraryId, string libraryName, string artistName, Stream artwork, string fileName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the stored artwork of the artist from the internal media directory.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Result<Deleted> DeleteArtistArtwork(Guid libraryId, string libraryName, string artistName);

    /// <summary>
    /// Deletes the stored artwork file at <paramref name="artworkRelativePath"/> from the internal media directory, when it still exists.
    /// </summary>
    /// <param name="artworkRelativePath">The relative path of the stored artwork file.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Result<Deleted> DeleteStoredArtwork(string artworkRelativePath);
}
