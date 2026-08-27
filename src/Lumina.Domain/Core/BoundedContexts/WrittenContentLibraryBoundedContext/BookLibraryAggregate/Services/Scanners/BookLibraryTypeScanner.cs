#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;

/// <summary>
/// Media library scanner for a books media library type.
/// </summary>
internal class BookLibraryTypeScanner : IBookLibraryTypeScanner
{
    private readonly IMediaLibraryScanJobFactory _mediaScanJobFactory;

    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    public LibraryType SupportedLibraryType { get; } = LibraryType.Book;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryTypeScanner"/> class.
    /// </summary>
    /// <param name="mediaScanJobFactory">Injected factory for creating media library scan jobs.</param>
    public BookLibraryTypeScanner(IMediaLibraryScanJobFactory mediaScanJobFactory)
    {
        _mediaScanJobFactory = mediaScanJobFactory;
    }

    /// <summary>
    /// Creates the media library scan jobs for the provided media library.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library for which to create the media library scan jobs.</param>
    /// <returns>A collection of media library scan jobs.</returns>
    public IEnumerable<IMediaLibraryScanJob> CreateScanJobsForLibrary(LibraryId libraryId)
    {
        // declare the list of jobs that this scanner requires
        IBooksFileSystemDiscoveryJob fileSystemDiscoveryJob = _mediaScanJobFactory.CreateJob<IBooksFileSystemDiscoveryJob>(libraryId);
        IMediaLibraryScanDiffJob mediaLibraryScanDiffJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanDiffJob>(libraryId);
        IMediaLibraryScanHashJob mediaLibraryScanHashJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanHashJob>(libraryId);
        IMediaLibraryScanResultsSaveJob mediaLibraryScanResultsSaveJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanResultsSaveJob>(libraryId);

        // establish the hierarchical relationships between jobs
        fileSystemDiscoveryJob.AddChild(mediaLibraryScanDiffJob);
        mediaLibraryScanDiffJob.AddParent(fileSystemDiscoveryJob);

        mediaLibraryScanDiffJob.AddChild(mediaLibraryScanHashJob);
        mediaLibraryScanHashJob.AddParent(mediaLibraryScanDiffJob);

        mediaLibraryScanHashJob.AddChild(mediaLibraryScanResultsSaveJob);
        mediaLibraryScanResultsSaveJob.AddParent(mediaLibraryScanHashJob);

        // the enrichment of the media library items is split into independent, modular jobs that run sequentially: the provider configuration
        // invalidation job invalidates the enrichment state of the items whose metadata or artwork providers changed since the last scan, the
        // metadata enrichment job enriches the metadata and links the media contributors, and the artwork enrichment job resolves the artwork.
        // The artwork enrichment job is always the last job in the directed acyclic job graph, running after the metadata enrichment job, so that
        // the artwork is resolved after the metadata, and thus after the contributors that provide the author names used to build the artwork directories.
        IMediaLibraryScanProviderConfigurationInvalidationJob mediaLibraryScanProviderConfigurationInvalidationJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanProviderConfigurationInvalidationJob>(libraryId);
        IMediaLibraryScanMetadataEnrichmentJob mediaLibraryScanMetadataEnrichmentJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
        IMediaLibraryScanArtworkEnrichmentJob mediaLibraryScanArtworkEnrichmentJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanArtworkEnrichmentJob>(libraryId);

        mediaLibraryScanResultsSaveJob.AddChild(mediaLibraryScanProviderConfigurationInvalidationJob);
        mediaLibraryScanProviderConfigurationInvalidationJob.AddParent(mediaLibraryScanResultsSaveJob);

        mediaLibraryScanProviderConfigurationInvalidationJob.AddChild(mediaLibraryScanMetadataEnrichmentJob);
        mediaLibraryScanMetadataEnrichmentJob.AddParent(mediaLibraryScanProviderConfigurationInvalidationJob);

        mediaLibraryScanMetadataEnrichmentJob.AddChild(mediaLibraryScanArtworkEnrichmentJob);
        mediaLibraryScanArtworkEnrichmentJob.AddParent(mediaLibraryScanMetadataEnrichmentJob);

        // return the top level jobs that will be triggered when the scan will be started
        yield return fileSystemDiscoveryJob;
    }
}
