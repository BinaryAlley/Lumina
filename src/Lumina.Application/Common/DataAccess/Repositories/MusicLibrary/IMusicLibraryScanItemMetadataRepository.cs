#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for the music metadata staged during a media library scan.
/// </summary>
public interface IMusicLibraryScanItemMetadataRepository : IRepository<MusicLibraryScanItemMetadataEntity>,
                                                           IInsertRangeRepositoryAction<MusicLibraryScanItemMetadataEntity>
{
    /// <summary>
    /// Gets all the staged music metadata items of the media library scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the media library scan whose staged music metadata is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="MusicLibraryScanItemMetadataEntity"/>, or an error.</returns>
    Task<Result<IReadOnlyList<MusicLibraryScanItemMetadataEntity>>> GetByScanIdAsync(Guid scanId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the staged music metadata items of the media library scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the media library scan whose staged music metadata is deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByScanIdAsync(Guid scanId, CancellationToken cancellationToken);
}
