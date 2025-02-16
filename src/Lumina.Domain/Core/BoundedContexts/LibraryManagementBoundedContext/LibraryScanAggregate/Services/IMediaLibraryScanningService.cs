#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;

/// <summary>
/// Interface for the service used to scan a media library.
/// </summary>
public interface IMediaLibraryScanningService
{
    /// <summary>
    /// Starts <paramref name="scan"/>.
    /// </summary>
    /// <param name="scan">The media library scan to start.</param>
    /// <param name="libraryType">The type of the media library to be scanned.</param>
    /// <param name="downloadMedatadaFromWeb">Whether the library permits downloading data from the web, or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Success>> StartScanAsync(LibraryScan scan, LibraryType libraryType, bool downloadMedatadaFromWeb, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels <paramref name="scan"/>.
    /// </summary>
    /// <param name="scan">The scan to cancel.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    ErrorOr<Success> CancelScan(LibraryScan scan);
}
