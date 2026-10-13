#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Service that removes the artwork records and the stored artwork files of a music library item, together with the artwork of its children.
/// The artwork table is shared by all the music library item types and references its owners by a plain id, so the artwork of an item does not
/// cascade when the item is removed, and is removed explicitly instead, from the innermost children outwards.
/// </summary>
internal sealed class MusicArtworkCleanupService : IMusicArtworkCleanupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMusicArtworkService _musicArtworkService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkCleanupService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="musicArtworkService">Injected service for removing the stored artwork files of a music library item.</param>
    public MusicArtworkCleanupService(IUnitOfWork unitOfWork, IMusicArtworkService musicArtworkService)
    {
        _unitOfWork = unitOfWork;
        _musicArtworkService = musicArtworkService;
    }

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the artist identified by <paramref name="artistId"/>, together with the artwork of its albums and tracks.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteArtistArtworkAsync(Guid artistId, CancellationToken cancellationToken)
    {
        // The albums and the tracks of the artist are removed by the database through the cascading foreign keys, so only the artwork of the
        // children has to be collected here, and it has to be removed before the artwork of the artist itself, from the innermost children outwards.
        Result<IReadOnlyList<AlbumEntity>> getAlbumsResult = await _unitOfWork.AlbumRepository.GetByArtistIdAsync(artistId, cancellationToken).ConfigureAwait(false);
        if (getAlbumsResult.IsFailure)
            return getAlbumsResult.Errors;

        foreach (AlbumEntity album in getAlbumsResult.Value)
        {
            Result<Deleted> deleteAlbumArtworkResult = await DeleteAlbumArtworkAsync(album.Id, cancellationToken).ConfigureAwait(false);
            if (deleteAlbumArtworkResult.IsFailure)
                return deleteAlbumArtworkResult.Errors;
        }

        return await DeleteOwnerArtworkAsync(MusicArtworkOwnerType.Artist, artistId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the album identified by <paramref name="albumId"/>, together with the artwork of its tracks.
    /// </summary>
    /// <param name="albumId">The Id of the album whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteAlbumArtworkAsync(Guid albumId, CancellationToken cancellationToken)
    {
        // The tracks of the album are removed by the database through the cascading foreign keys, so only their artwork has to be removed here,
        // before the artwork of the album itself.
        Result<IReadOnlyList<TrackEntity>> getTracksResult = await _unitOfWork.TrackRepository.GetByAlbumIdAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (getTracksResult.IsFailure)
            return getTracksResult.Errors;

        foreach (TrackEntity track in getTracksResult.Value)
        {
            Result<Deleted> deleteTrackArtworkResult = await DeleteTrackArtworkAsync(track.Id, cancellationToken).ConfigureAwait(false);
            if (deleteTrackArtworkResult.IsFailure)
                return deleteTrackArtworkResult.Errors;
        }

        return await DeleteOwnerArtworkAsync(MusicArtworkOwnerType.Album, albumId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the track identified by <paramref name="trackId"/>.
    /// </summary>
    /// <param name="trackId">The Id of the track whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Deleted>> DeleteTrackArtworkAsync(Guid trackId, CancellationToken cancellationToken)
    {
        return DeleteOwnerArtworkAsync(MusicArtworkOwnerType.Track, trackId, cancellationToken);
    }

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the single music library item identified by <paramref name="ownerType"/> and <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item whose artwork is removed.</param>
    /// <param name="ownerId">The Id of the music library item whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Deleted>> DeleteOwnerArtworkAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository.GetByOwnerAsync(ownerType, ownerId, cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;

        // The stored files do not cascade from their records either, so every stored file is removed before its record is; the artwork whose file has not been resolved yet has no file to remove.
        foreach (MusicArtworkEntity artwork in getArtworkResult.Value)
        {
            if (string.IsNullOrWhiteSpace(artwork.FileName))
                continue;
            Result<Deleted> deleteStoredArtworkResult = _musicArtworkService.DeleteStoredArtwork(artwork.FileName);
            if (deleteStoredArtworkResult.IsFailure)
                return deleteStoredArtworkResult.Errors;
        }

        return await _unitOfWork.MusicArtworkRepository.DeleteByOwnerAsync(ownerType, ownerId, cancellationToken).ConfigureAwait(false);
    }
}
