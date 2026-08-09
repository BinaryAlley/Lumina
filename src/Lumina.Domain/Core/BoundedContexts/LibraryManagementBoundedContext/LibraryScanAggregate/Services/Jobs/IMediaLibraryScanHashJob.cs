namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for computing the content hashes of the files that changed since the previous scan.
/// </summary>
public interface IMediaLibraryScanHashJob : IMediaLibraryScanJob
{
}
