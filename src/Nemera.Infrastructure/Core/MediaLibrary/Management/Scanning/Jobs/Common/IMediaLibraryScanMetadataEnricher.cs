#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Interface defining the metadata enrichment of the media library items of a specific media library type.
/// </summary>
internal interface IMediaLibraryScanMetadataEnricher
{
    /// <summary>
    /// The media library type that this metadata enricher supports.
    /// </summary>
    LibraryType SupportedLibraryType { get; }

    /// <summary>
    /// Enriches the metadata of the media library items of the provided media library, using the metadata providers configured for it.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose items are enriched.</param>
    /// <param name="scanId">The unique identifier of the media library scan.</param>
    /// <param name="userId">The unique identifier of the user that initiated the media library scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnrichAsync(LibraryId libraryId, ScanId scanId, UserId userId, CancellationToken cancellationToken);
}
