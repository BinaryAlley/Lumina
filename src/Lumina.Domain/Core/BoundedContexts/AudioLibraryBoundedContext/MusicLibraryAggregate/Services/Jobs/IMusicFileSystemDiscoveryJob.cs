#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for discovering music file system items.
/// </summary>
public interface IMusicFileSystemDiscoveryJob : IMediaLibraryScanJob
{
}
