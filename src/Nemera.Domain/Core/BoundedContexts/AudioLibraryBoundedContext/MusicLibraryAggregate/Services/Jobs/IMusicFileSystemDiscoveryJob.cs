#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

#endregion

namespace Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for discovering music file system items.
/// </summary>
public interface IMusicFileSystemDiscoveryJob : IMediaLibraryScanDiscoveryJob
{
}
