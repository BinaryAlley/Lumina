using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for discovering books file system items.
/// </summary>
public interface IBooksFileSystemDiscoveryJob : IMediaLibraryScanJob
{
}
