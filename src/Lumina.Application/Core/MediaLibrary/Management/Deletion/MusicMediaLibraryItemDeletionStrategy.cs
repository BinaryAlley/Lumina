#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Deletion;

/// <summary>
/// Deletion strategy for the media library items of the music media library type. The track stored at the deleted path is removed, together with its stored artwork,
/// and the album and the artist that are left with no children after the removal are removed as well.
/// </summary>
internal sealed class MusicMediaLibraryItemDeletionStrategy : IMediaLibraryItemDeletionStrategy
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMusicArtworkService _musicArtworkService;
    private readonly ILogger<MusicMediaLibraryItemDeletionStrategy> _logger;

    /// <summary>
    /// The media library type that this deletion strategy supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Music;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryItemDeletionStrategy"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="musicArtworkService">Injected service for deleting the stored artwork files of the music library items.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while deleting the media library item.</param>
    public MusicMediaLibraryItemDeletionStrategy(IUnitOfWork unitOfWork, IMusicArtworkService musicArtworkService, ILogger<MusicMediaLibraryItemDeletionStrategy> logger)
    {
        _unitOfWork = unitOfWork;
        _musicArtworkService = musicArtworkService;
        _logger = logger;
    }

    /// <summary>
    /// Deletes the track stored at the provided <paramref name="path"/> in the media library identified by <paramref name="libraryId"/>, together with its stored artwork,
    /// and the album and the artist that are left with no children after the removal.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose track is deleted.</param>
    /// <param name="path">The file system path of the track to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> DeleteItemAsync(Guid libraryId, string path, CancellationToken cancellationToken)
    {
        // Load the track stored at the deleted path, which might have been already removed.
        // Soft deleted tracks are included on purpose: when their file disappears from disk, they must be removed permanently too.
        Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository.GetByPathAsync(libraryId, path, cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        TrackEntity? track = getTrackResult.Value;
        if (track is null)
            return Result.Success;

        Guid trackId = track.Id;
        Guid albumId = track.AlbumId;

        // Delete the stored artwork of the track, which has no cascade relationship because the artwork table is shared by all the music library item types.
        Result<Deleted> deleteTrackArtworkResult = await DeleteStoredArtworkAndRowsAsync(MusicArtworkOwnerType.Track, trackId, cancellationToken).ConfigureAwait(false);
        if (deleteTrackArtworkResult.IsFailure)
            return deleteTrackArtworkResult.Errors;

        Result<Deleted> deleteTrackResult = await _unitOfWork.TrackRepository.DeleteByIdAsync(trackId, cancellationToken).ConfigureAwait(false);
        if (deleteTrackResult.IsFailure)
            return deleteTrackResult.Errors;

        // Load the album the deleted track belonged to, whether the album itself is soft deleted or not, to determine whether it is left with no children.
        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository.GetIncludingDeletedByIdAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        AlbumEntity? album = getAlbumResult.Value;
        if (album is null)
            return await SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Guid artistId = album.ArtistId;

        // Soft deleted tracks are counted as well, because their files still exist and the album must survive until the last one of them disappears.
        Result<IReadOnlyList<TrackEntity>> getRemainingTracksResult = await _unitOfWork.TrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (getRemainingTracksResult.IsFailure)
            return getRemainingTracksResult.Errors;
        // The deleted track is still tracked by the change tracker until the changes are saved, so it is excluded explicitly.
        if (getRemainingTracksResult.Value.All(remainingTrack => remainingTrack.Id == trackId))
        {
            Result<Deleted> deleteAlbumArtworkResult = await DeleteStoredArtworkAndRowsAsync(MusicArtworkOwnerType.Album, albumId, cancellationToken).ConfigureAwait(false);
            if (deleteAlbumArtworkResult.IsFailure)
                return deleteAlbumArtworkResult.Errors;

            Result<Deleted> deleteAlbumResult = await _unitOfWork.AlbumRepository.DeleteByIdAsync(albumId, cancellationToken).ConfigureAwait(false);
            if (deleteAlbumResult.IsFailure)
                return deleteAlbumResult.Errors;

            // Soft deleted albums are counted as well, because their tracks still exist and the artist must survive until the last one of them disappears.
            Result<IReadOnlyList<AlbumEntity>> getRemainingAlbumsResult = await _unitOfWork.AlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, cancellationToken).ConfigureAwait(false);
            if (getRemainingAlbumsResult.IsFailure)
                return getRemainingAlbumsResult.Errors;
            // The deleted album is still tracked by the change tracker until the changes are saved, so it is excluded explicitly.
            if (getRemainingAlbumsResult.Value.All(remainingAlbum => remainingAlbum.Id == albumId))
            {
                Result<Deleted> deleteArtistArtworkResult = await DeleteStoredArtworkAndRowsAsync(MusicArtworkOwnerType.Artist, artistId, cancellationToken).ConfigureAwait(false);
                if (deleteArtistArtworkResult.IsFailure)
                    return deleteArtistArtworkResult.Errors;

                Result<Deleted> deleteArtistResult = await _unitOfWork.ArtistRepository.DeleteByIdAsync(artistId, cancellationToken).ConfigureAwait(false);
                if (deleteArtistResult.IsFailure)
                    return deleteArtistResult.Errors;
            }
        }

        return await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the stored artwork files of the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>,
    /// and then deletes the artwork rows of that item.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Deleted>> DeleteStoredArtworkAndRowsAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository
            .GetByOwnerAsync(ownerType, ownerId, cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;

        // The stored files are deleted best-effort, since a missing or locked file must not prevent the permanent removal of the media library item.
        foreach (MusicArtworkEntity artwork in getArtworkResult.Value)
        {
            if (string.IsNullOrWhiteSpace(artwork.FileName))
                continue;

            Result<Deleted> deleteStoredArtworkResult = _musicArtworkService.DeleteStoredArtwork(artwork.FileName);
            if (deleteStoredArtworkResult.IsFailure)
                _logger.LogWarning("Failed to delete the stored {OwnerType} artwork at '{ArtworkPath}' of the music library item with Id '{OwnerId}', the file might remain orphaned.", ownerType, artwork.FileName, ownerId);
        }

        return await _unitOfWork.MusicArtworkRepository.DeleteByOwnerAsync(ownerType, ownerId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Persists the changes made to the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return saveChangesResult.IsFailure ? saveChangesResult.Errors : Result.Success;
    }
}
