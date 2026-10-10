#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for the music metadata staged during a media library scan.
/// </summary>
internal sealed class MusicLibraryScanItemMetadataRepository : IMusicLibraryScanItemMetadataRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryScanItemMetadataRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MusicLibraryScanItemMetadataRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a range of staged music metadata items.
    /// </summary>
    /// <param name="items">The staged music metadata items to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertRangeAsync(IReadOnlyCollection<MusicLibraryScanItemMetadataEntity> items, CancellationToken cancellationToken)
    {
        if (items.Count > 0)
            _luminaDbContext.MusicLibraryScanItemMetadata.AddRange(items);
        return Task.FromResult<Result<Created>>(Result.Created);
    }

    /// <summary>
    /// Gets all the staged music metadata items of the media library scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the media library scan whose staged music metadata is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="MusicLibraryScanItemMetadataEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<MusicLibraryScanItemMetadataEntity>>> GetByScanIdAsync(Guid scanId, CancellationToken cancellationToken)
    {
        IReadOnlyList<MusicLibraryScanItemMetadataEntity> items = await _luminaDbContext.MusicLibraryScanItemMetadata
            .AsNoTracking()
            .Where(musicMetadata => musicMetadata.LibraryScanId == scanId)
            .OrderBy(musicMetadata => musicMetadata.Path)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From(items);
    }

    /// <summary>
    /// Deletes all the staged music metadata items of the media library scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the media library scan whose staged music metadata is deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByScanIdAsync(Guid scanId, CancellationToken cancellationToken)
    {
        await _luminaDbContext.MusicLibraryScanItemMetadata
            .Where(musicMetadata => musicMetadata.LibraryScanId == scanId)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        return Result.Deleted;
    }
}
