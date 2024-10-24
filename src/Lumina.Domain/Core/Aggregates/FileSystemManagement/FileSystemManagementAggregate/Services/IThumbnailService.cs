#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling thumbnails.
/// </summary>
public interface IThumbnailService
{
    /// <summary>
    /// Gets the thumbnail of a file at the specified path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="quality">The quality of the thumbnail to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of bytes representing the thumbnail of the file at the specified path or an error.</returns>
    Task<ErrorOr<Thumbnail>> GetThumbnailAsync(string path, int quality, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the thumbnail of a file at the specified path.
    /// </summary>
    /// <param name="path">The path object.</param>
    /// <param name="quality">The quality of the thumbnail to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of bytes representing the thumbnail of the file at the specified path or an error.</returns>
    Task<ErrorOr<Thumbnail>> GetThumbnailAsync(FileSystemPathId path, int quality, CancellationToken cancellationToken);
}
