namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for invalidating the enrichment state of the media library items
/// whose metadata or artwork providers changed since the last scan.
/// </summary>
public interface IMediaLibraryScanProviderConfigurationInvalidationJob : IMediaLibraryScanJob
{
}
