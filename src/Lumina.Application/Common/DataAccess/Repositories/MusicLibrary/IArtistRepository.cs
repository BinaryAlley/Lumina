#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for artists.
/// </summary>
public interface IArtistRepository : IRepository<ArtistEntity>,
                                     IInsertRepositoryAction<ArtistEntity>,
                                     IUpdateRepositoryAction<ArtistEntity>,
                                     IGetByIdRepositoryAction<ArtistEntity, Guid>,
                                     IGetAllRepositoryAction<ArtistEntity>,
                                     IGetAllLiteRepositoryAction<ArtistEntity, ArtistLiteRow>,
                                     IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets the artist of the library identified by <paramref name="libraryId"/> that has the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose artist is retrieved.</param>
    /// <param name="name">The name of the artist to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="ArtistEntity"/> with the provided name, or an error.</returns>
    Task<Result<ArtistEntity?>> GetByNameAsync(Guid libraryId, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the artists of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet,
    /// together with their albums and tracks and the collections needed to enrich them, ordered by name, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are retrieved.</param>
    /// <param name="lastName">The name of the last retrieved artist, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of artists to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of artists needing their metadata enriched, or an error.</returns>
    Task<Result<IReadOnlyList<ArtistEntity>>> GetArtistsNeedingMetadataAsync(Guid libraryId, string? lastName, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of artists of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of artists needing their metadata enriched, or an error.</returns>
    Task<Result<int>> GetArtistsNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of artists of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of artists needing their artwork resolved, or an error.</returns>
    Task<Result<int>> GetArtistsNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the artists of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet,
    /// together with their albums and tracks, ordered by name, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are retrieved.</param>
    /// <param name="lastName">The name of the last retrieved artist, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of artists to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of artists needing their artwork resolved, or an error.</returns>
    Task<Result<IReadOnlyList<ArtistEntity>>> GetArtistsNeedingArtworkAsync(Guid libraryId, string? lastName, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the metadata enrichment status of all the artists of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-enriched, because the metadata provider configuration of the library changed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
}
