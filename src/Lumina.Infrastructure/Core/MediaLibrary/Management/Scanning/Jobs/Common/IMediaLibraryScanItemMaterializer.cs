#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Interface defining the materialization of the media library items of a specific media library type from the results of a media library scan.
/// </summary>
internal interface IMediaLibraryScanItemMaterializer
{
    /// <summary>
    /// The media library type that this materializer supports.
    /// </summary>
    LibraryType SupportedLibraryType { get; }

    /// <summary>
    /// Resets the enrichment state of the media library items stored at the provided <paramref name="paths"/>, so that they are re-enriched, because their content changed since the last scan.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose items are reset.</param>
    /// <param name="paths">The file system paths of the media library items whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetEnrichmentStateForChangedPathsAsync(IUnitOfWork unitOfWork, Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Materializes the media library items of the media library from the paths of the media library scan snapshot.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose items are materialized.</param>
    /// <param name="scanId">The Id of the media library scan whose results are materialized.</param>
    /// <param name="paths">The file system paths of the media library scan snapshot.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> MaterializeItemsAsync(IUnitOfWork unitOfWork, Guid libraryId, Guid scanId, IReadOnlyList<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the metadata enrichment status of all the media library items of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose items are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the artwork enrichment status of all the media library items of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetArtworkStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken);
}
