#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;

/// <summary>
/// Media library scanner for a music media library type.
/// </summary>
internal class MusicLibraryTypeScanner : IMusicLibraryTypeScanner
{
    private readonly IMediaLibraryScanJobFactory _mediaScanJobFactory;

    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    public LibraryType SupportedLibraryType { get; } = LibraryType.Music;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryTypeScanner"/> class.
    /// </summary>
    /// <param name="mediaScanJobFactory">Injected factory for creating media library scan jobs.</param>
    public MusicLibraryTypeScanner(IMediaLibraryScanJobFactory mediaScanJobFactory)
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
        // Declare the list of jobs that this scanner requires.
        IMusicFileSystemDiscoveryJob fileSystemDiscoveryJob = _mediaScanJobFactory.CreateJob<IMusicFileSystemDiscoveryJob>(libraryId);
        IMediaLibraryScanDiffJob mediaLibraryScanDiffJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanDiffJob>(libraryId);
        IMediaLibraryScanHashJob mediaLibraryScanHashJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanHashJob>(libraryId);
        IMusicMetadataExtractionJob musicMetadataExtractionJob = _mediaScanJobFactory.CreateJob<IMusicMetadataExtractionJob>(libraryId);
        IMediaLibraryScanResultsSaveJob mediaLibraryScanResultsSaveJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanResultsSaveJob>(libraryId);

        // Establish the hierarchical relationships between jobs.
        fileSystemDiscoveryJob.AddChild(mediaLibraryScanDiffJob);
        mediaLibraryScanDiffJob.AddParent(fileSystemDiscoveryJob);

        mediaLibraryScanDiffJob.AddChild(mediaLibraryScanHashJob);
        mediaLibraryScanHashJob.AddParent(mediaLibraryScanDiffJob);

        mediaLibraryScanHashJob.AddChild(musicMetadataExtractionJob);
        musicMetadataExtractionJob.AddParent(mediaLibraryScanHashJob);

        // The metadata extraction job reads the embedded tags of the discovered music files, falling back to the structure of the media library
        // on disk, and stages the extracted metadata, so that the results save job can materialize the artists, albums and tracks from it.
        musicMetadataExtractionJob.AddChild(mediaLibraryScanResultsSaveJob);
        mediaLibraryScanResultsSaveJob.AddParent(musicMetadataExtractionJob);

        // The enrichment of the media library items is split into independent, modular jobs that run sequentially: the provider configuration
        // invalidation job invalidates the enrichment state of the items whose metadata or artwork providers changed since the last scan, the
        // metadata enrichment job enriches the metadata and links the media contributors, and the artwork enrichment job resolves the artwork.
        // The artwork enrichment job is always the last job in the directed acyclic job graph, running after the metadata enrichment job.
        IMediaLibraryScanProviderConfigurationInvalidationJob mediaLibraryScanProviderConfigurationInvalidationJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanProviderConfigurationInvalidationJob>(libraryId);
        IMediaLibraryScanMetadataEnrichmentJob mediaLibraryScanMetadataEnrichmentJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
        IMediaLibraryScanArtworkEnrichmentJob mediaLibraryScanArtworkEnrichmentJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanArtworkEnrichmentJob>(libraryId);

        mediaLibraryScanResultsSaveJob.AddChild(mediaLibraryScanProviderConfigurationInvalidationJob);
        mediaLibraryScanProviderConfigurationInvalidationJob.AddParent(mediaLibraryScanResultsSaveJob);

        mediaLibraryScanProviderConfigurationInvalidationJob.AddChild(mediaLibraryScanMetadataEnrichmentJob);
        mediaLibraryScanMetadataEnrichmentJob.AddParent(mediaLibraryScanProviderConfigurationInvalidationJob);

        mediaLibraryScanMetadataEnrichmentJob.AddChild(mediaLibraryScanArtworkEnrichmentJob);
        mediaLibraryScanArtworkEnrichmentJob.AddParent(mediaLibraryScanMetadataEnrichmentJob);

        // Return the top level jobs that will be triggered when the scan will be started.
        yield return fileSystemDiscoveryJob;
    }
}
