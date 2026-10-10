#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for the artwork of the music library items.
/// </summary>
internal sealed class MusicArtworkRepository : IMusicArtworkRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MusicArtworkRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a range of pieces of artwork of music library items.
    /// </summary>
    /// <param name="artwork">The pieces of artwork to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertRangeAsync(IReadOnlyCollection<MusicArtworkEntity> artwork, CancellationToken cancellationToken)
    {
        if (artwork.Count > 0)
            _luminaDbContext.MusicArtwork.AddRange(artwork);
        return Task.FromResult<Result<Created>>(Result.Created);
    }

    /// <summary>
    /// Gets the pieces of artwork owned by the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="MusicArtworkEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<MusicArtworkEntity>>> GetByOwnerAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        IReadOnlyList<MusicArtworkEntity> artwork = await _luminaDbContext.MusicArtwork
            .AsNoTracking()
            .Where(musicArtwork => musicArtwork.OwnerType == ownerType && musicArtwork.OwnerId == ownerId)
            .OrderBy(musicArtwork => musicArtwork.ArtworkType)
            .ThenBy(musicArtwork => musicArtwork.Ordinal)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From(artwork);
    }

    /// <summary>
    /// Deletes all the pieces of artwork owned by the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByOwnerAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        await _luminaDbContext.MusicArtwork
            .Where(musicArtwork => musicArtwork.OwnerType == ownerType && musicArtwork.OwnerId == ownerId)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        return Result.Deleted;
    }

    /// <summary>
    /// Resets the artwork enrichment status of all the pieces of artwork owned by the music library items of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        // The artwork table is shared by all the music library item types, so the owners of the artwork of this library are collected per type.
        List<Guid> artistIds = await _luminaDbContext.Artists
            .Where(artist => artist.LibraryId == libraryId)
            .Select(artist => artist.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        List<Guid> albumIds = await _luminaDbContext.Albums
            .Where(album => album.LibraryId == libraryId)
            .Select(album => album.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        List<Guid> trackIds = await _luminaDbContext.Tracks
            .Where(track => track.LibraryId == libraryId)
            .Select(track => track.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        await _luminaDbContext.MusicArtwork
            .Where(musicArtwork => (musicArtwork.OwnerType == MusicArtworkOwnerType.Artist && artistIds.Contains(musicArtwork.OwnerId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Album && albumIds.Contains(musicArtwork.OwnerId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Track && trackIds.Contains(musicArtwork.OwnerId)))
            .ExecuteUpdateAsync(setters => setters.SetProperty(musicArtwork => musicArtwork.Status, ArtworkStatus.Pending), cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }
}
