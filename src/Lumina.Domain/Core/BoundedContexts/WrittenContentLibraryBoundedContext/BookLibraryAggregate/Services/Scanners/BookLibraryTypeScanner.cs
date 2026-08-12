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
    /// <param name="downloadMetadataFromWeb">Whether the library permits downloading data from the web, or not.</param>
    /// <returns>A collection of media library scan jobs.</returns>
    public IEnumerable<IMediaLibraryScanJob> CreateScanJobsForLibrary(LibraryId libraryId, bool downloadMetadataFromWeb)
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

        // when the library permits downloading data from the web, the metadata enrichment job is the last job in the directed acyclic job graph,
        // running after the scan results save job, so that the books are materialized before their metadata is enriched
        if (downloadMetadataFromWeb)
        {
            IMediaLibraryScanMetadataEnrichmentJob mediaLibraryScanMetadataEnrichmentJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
            mediaLibraryScanResultsSaveJob.AddChild(mediaLibraryScanMetadataEnrichmentJob);
            mediaLibraryScanMetadataEnrichmentJob.AddParent(mediaLibraryScanResultsSaveJob);
        }

        // return the top level jobs that will be triggered when the scan will be started
        yield return fileSystemDiscoveryJob;
    }
}
