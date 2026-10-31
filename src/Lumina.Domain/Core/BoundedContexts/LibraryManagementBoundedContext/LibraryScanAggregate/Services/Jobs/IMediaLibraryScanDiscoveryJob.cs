namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job that discovers the file system items of a media library.
/// </summary>
/// <remarks>
/// This is always the only top level job of a media library scan: a scan cannot process anything before the files are discovered.
/// </remarks>
public interface IMediaLibraryScanDiscoveryJob : IMediaLibraryScanJob
{
}
