using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

namespace Nemera.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for discovering books file system items.
/// </summary>
public interface IBooksFileSystemDiscoveryJob : IMediaLibraryScanDiscoveryJob
{
}
