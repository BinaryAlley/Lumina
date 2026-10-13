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
        // The artwork table is shared by all the music library item types, so the owner of a piece of artwork is matched by its type against the
        // corresponding table, and the database resolves the membership of the owner in the library by a correlated subquery. Collecting the ids
        // of every artist, album and track of the library in memory first would make the cost of the operation grow with the size of the library.
        await _luminaDbContext.MusicArtwork
            .Where(musicArtwork => (musicArtwork.OwnerType == MusicArtworkOwnerType.Artist
                    && _luminaDbContext.Artists.Any(artist => artist.Id == musicArtwork.OwnerId && artist.LibraryId == libraryId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Album
                    && _luminaDbContext.Albums.Any(album => album.Id == musicArtwork.OwnerId && album.LibraryId == libraryId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Track
                    && _luminaDbContext.Tracks.Any(track => track.Id == musicArtwork.OwnerId && track.LibraryId == libraryId)))
            .ExecuteUpdateAsync(setters => setters.SetProperty(musicArtwork => musicArtwork.Status, ArtworkStatus.Pending), cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }

    /// <summary>
    /// Deletes all the pieces of artwork owned by the artists, albums and tracks of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork is deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        // The artwork table is shared by all the music library item types, so the owner of a piece of artwork is matched by its type against the
        // corresponding table, and the database resolves the membership of the owner in the library by a correlated subquery. Collecting the ids
        // of every artist, album and track of the library in memory first would make the cost of the operation grow with the size of the library.
        await _luminaDbContext.MusicArtwork
            .Where(musicArtwork => (musicArtwork.OwnerType == MusicArtworkOwnerType.Artist
                    && _luminaDbContext.Artists.Any(artist => artist.Id == musicArtwork.OwnerId && artist.LibraryId == libraryId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Album
                    && _luminaDbContext.Albums.Any(album => album.Id == musicArtwork.OwnerId && album.LibraryId == libraryId))
                || (musicArtwork.OwnerType == MusicArtworkOwnerType.Track
                    && _luminaDbContext.Tracks.Any(track => track.Id == musicArtwork.OwnerId && track.LibraryId == libraryId)))
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        return Result.Deleted;
    }

    /// <summary>
    /// Stores the cover image of the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="fileName">The relative file name of the stored cover image.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertCoverAsync(MusicArtworkOwnerType ownerType, Guid ownerId, string fileName, CancellationToken cancellationToken)
    {
        // The existing cover is matched the same way the callers that resolve it do, by owner and artwork type only, so that a cover stored
        // with an ordinal other than zero is replaced in place instead of a second cover row being inserted next to it.
        MusicArtworkEntity? existingCover = await _luminaDbContext.MusicArtwork
            .FirstOrDefaultAsync(musicArtwork => musicArtwork.OwnerType == ownerType
                && musicArtwork.OwnerId == ownerId
                && musicArtwork.ArtworkType == ArtworkType.Cover, cancellationToken).ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        if (existingCover is null)
        {
            _luminaDbContext.MusicArtwork.Add(new MusicArtworkEntity
            {
                Id = Guid.NewGuid(),
                OwnerType = ownerType,
                OwnerId = ownerId,
                ArtworkType = ArtworkType.Cover,
                Ordinal = 0,
                FileName = fileName,
                Status = ArtworkStatus.Enriched,
                CreatedOnUtc = now,
                CreatedBy = Guid.Empty,
                // The audit columns are owned by the auditing interceptor, which stamps only the rows that actually changed, so they are never mapped here.
                UpdatedOnUtc = null,
                UpdatedBy = null
            });
        }
        else
        {
            existingCover.FileName = fileName;
            existingCover.Status = ArtworkStatus.Enriched;
            existingCover.LastUpdateUtc = now;
        }
        return Result.Updated;
    }
}
