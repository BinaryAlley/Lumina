#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface defining methods for handling file-related operations based on different sources.
/// </summary>
public interface IFileTypeService
{
    /// <summary>
    /// Determines if <paramref name="file"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="file">The file to determine if it is an image or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    Task<ErrorOr<ImageType>> GetImageTypeAsync(File file, CancellationToken cancellationToken);

    /// <summary>
    /// Determines if a file identified by <paramref name="path"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="path">The path of the file to determine if it is an image or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    Task<ErrorOr<ImageType>> GetImageTypeAsync(FileSystemPathId path, CancellationToken cancellationToken);
}
