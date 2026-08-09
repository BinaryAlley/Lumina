#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Hooks;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;

/// <summary>
/// Media library scanner for a books media library type.
/// </summary>
internal class BookLibraryTypeScanner : IBookLibraryTypeScanner
{
    private readonly IMediaLibraryScanJobFactory _mediaScanJobFactory;
    private readonly IScanJobRegistry _scanJobRegistry;

    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    public LibraryType SupportedType { get; } = LibraryType.Book;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryTypeScanner"/> class.
    /// </summary>
    /// <param name="mediaScanJobFactory">Injected factory for creating media library scan jobs.</param>
    /// <param name="scanJobRegistry">Injected registry of the media library scan jobs injected by plugins at the defined hook points.</param>
    public BookLibraryTypeScanner(IMediaLibraryScanJobFactory mediaScanJobFactory, IScanJobRegistry scanJobRegistry)
    {
        _mediaScanJobFactory = mediaScanJobFactory;
        _scanJobRegistry = scanJobRegistry;
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
        IGoodReadsMetadataScrapJob goodReadsMetadataScrapJob = _mediaScanJobFactory.CreateJob<IGoodReadsMetadataScrapJob>(libraryId);
        IMediaLibraryScanResultsSaveJob mediaLibraryScanResultsSaveJob = _mediaScanJobFactory.CreateJob<IMediaLibraryScanResultsSaveJob>(libraryId);

        // establish the hierarchical relationships between jobs, splicing the plugin jobs at the defined hook points
        IMediaLibraryScanJob previousJob = fileSystemDiscoveryJob;
        previousJob = SpliceJobsAtHook(previousJob, ScanJobHooks.AFTER_FILE_SYSTEM_DISCOVERY, libraryId);
        previousJob.AddChild(mediaLibraryScanDiffJob);
        mediaLibraryScanDiffJob.AddParent(previousJob);

        previousJob = SpliceJobsAtHook(mediaLibraryScanDiffJob, ScanJobHooks.AFTER_SCAN_DIFF, libraryId);
        previousJob.AddChild(mediaLibraryScanHashJob);
        mediaLibraryScanHashJob.AddParent(previousJob);

        previousJob = SpliceJobsAtHook(mediaLibraryScanHashJob, ScanJobHooks.BEFORE_METADATA_ENRICHMENT, libraryId);
        if (downloadMetadataFromWeb)
        {
            previousJob.AddChild(goodReadsMetadataScrapJob);
            goodReadsMetadataScrapJob.AddParent(previousJob);
            previousJob = goodReadsMetadataScrapJob;
        }
        previousJob = SpliceJobsAtHook(previousJob, ScanJobHooks.AFTER_METADATA_ENRICHMENT, libraryId);
        previousJob = SpliceJobsAtHook(previousJob, ScanJobHooks.BEFORE_SCAN_RESULTS_SAVE, libraryId);
        previousJob.AddChild(mediaLibraryScanResultsSaveJob);
        mediaLibraryScanResultsSaveJob.AddParent(previousJob);

        // return the top level jobs that will be triggered when the scan will be started
        yield return fileSystemDiscoveryJob;
    }

    /// <summary>
    /// Splices the plugin jobs registered at the provided <paramref name="hookName"/> after the <paramref name="previousJob"/>, and returns the last job of the extended chain.
    /// </summary>
    /// <param name="previousJob">The job after which the plugin jobs are spliced.</param>
    /// <param name="hookName">The name of the hook point at which the plugin jobs are injected.</param>
    /// <param name="libraryId">The unique identifier of the media library upon which the scan is performed.</param>
    /// <returns>The last job of the extended chain, which can be used as the parent for the next job.</returns>
    private IMediaLibraryScanJob SpliceJobsAtHook(IMediaLibraryScanJob previousJob, string hookName, LibraryId libraryId)
    {
        IMediaLibraryScanJob currentJob = previousJob;
        foreach (IMediaLibraryScanJob pluginJob in _scanJobRegistry.GetJobsForHook(hookName, libraryId))
        {
            currentJob.AddChild(pluginJob);
            pluginJob.AddParent(currentJob);
            currentJob = pluginJob;
        }
        return currentJob;
    }
}
