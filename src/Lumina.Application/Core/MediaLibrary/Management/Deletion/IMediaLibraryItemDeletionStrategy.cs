#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Deletion;

/// <summary>
/// Interface defining the strategy used to delete a media library item that is no longer present in the media library scan snapshot, for a specific media library type.
/// </summary>
public interface IMediaLibraryItemDeletionStrategy
{
    /// <summary>
    /// The media library type that this deletion strategy supports.
    /// </summary>
    LibraryType SupportedLibraryType { get; }

    /// <summary>
    /// Deletes the media library item stored at the provided <paramref name="path"/> in the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose item is deleted.</param>
    /// <param name="path">The file system path of the media library item to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> DeleteItemAsync(Guid libraryId, string path, CancellationToken cancellationToken);
}
