#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
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

    /// <inheritdoc/>
    public LibraryType SupportedType { get; } = LibraryType.Book;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryTypeScanner"/> class.
    /// </summary>
    /// <param name="mediaScanJobFactory">Injected factory for creating media library scan jobs.</param>
    public BookLibraryTypeScanner(IMediaLibraryScanJobFactory mediaScanJobFactory)
    {
        _mediaScanJobFactory = mediaScanJobFactory;
    }

    /// <inheritdoc/>
    public IEnumerable<IMediaLibraryScanJob> CreateScanJobsForLibrary(LibraryId libraryId, bool downloadMedatadaFromWeb)
    {
        // declare the list of jobs that this scanner requires
        IFileSystemDiscoveryJob fileSystemDiscoveryJob = _mediaScanJobFactory.CreateJob<IFileSystemDiscoveryJob>(libraryId);
        IRepositoryMetadataDiscoveryJob repositoryMetadataDiscoveryJob = _mediaScanJobFactory.CreateJob<IRepositoryMetadataDiscoveryJob>(libraryId);
        IHashComparerJob hashComparerJob = _mediaScanJobFactory.CreateJob<IHashComparerJob>(libraryId);
        IGoodReadsMetadataScrapJob goodReadsMetadataScrapJob = _mediaScanJobFactory.CreateJob<IGoodReadsMetadataScrapJob>(libraryId);

        // establish the hierarchical relationships between jobs
        fileSystemDiscoveryJob.AddChild(hashComparerJob);
        repositoryMetadataDiscoveryJob.AddChild(hashComparerJob);

        hashComparerJob.AddParent(fileSystemDiscoveryJob);
        hashComparerJob.AddParent(repositoryMetadataDiscoveryJob);

        if (downloadMedatadaFromWeb)
        {
            hashComparerJob.AddChild(goodReadsMetadataScrapJob);
            goodReadsMetadataScrapJob.AddParent(hashComparerJob);
        }

        // return the top level jobs that will be triggered when the scan will be started
        yield return fileSystemDiscoveryJob;
        yield return repositoryMetadataDiscoveryJob;
    }
}
