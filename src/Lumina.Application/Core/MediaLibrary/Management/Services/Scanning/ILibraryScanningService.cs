#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;

/// <summary>
/// Interface for the service used to scan a media library.
/// </summary>
public interface ILibraryScanningService
{
    /// <summary>
    /// Starts the scan of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library to scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task StartScanAsync(Library library, CancellationToken cancellationToken);
}
