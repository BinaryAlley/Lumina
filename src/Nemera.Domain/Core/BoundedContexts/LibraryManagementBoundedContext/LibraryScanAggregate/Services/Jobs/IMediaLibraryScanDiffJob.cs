namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for computing the differences between the files on disk and the media library scan snapshot of the previous scan.
/// </summary>
public interface IMediaLibraryScanDiffJob : IMediaLibraryScanJob
{
}
