namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for persisting data of the current scan. This should always be the last job in the directed acyclic job graph.
/// </summary>
public interface IRepositoryMetadataSaveJob : IMediaLibraryScanJob
{

}
