#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.WrittenContent.Books;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.WrittenContent;

/// <summary>
/// Media library scanner for a books media library type.
/// </summary>
internal class BookLibraryTypeScanner : IBookLibraryTypeScanner
{
    private readonly IMediaLibraryScanJobFactory _mediaScanJobFactory;

    /// <inheritdoc/>
    public LibraryType SupportedType { get; } = LibraryType.Book;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScannerFactory"/> class.
    /// </summary>
    /// <param name="mediaScanJobFactory">Injected factory for creating media library scan jobs.</param>
    public BookLibraryTypeScanner(IMediaLibraryScanJobFactory mediaScanJobFactory)
    {
        _mediaScanJobFactory = mediaScanJobFactory;
    }

    /// <inheritdoc/>
    public IEnumerable<MediaLibraryScanJob> CreateScanJobsForLibrary(Library library)
    {
        // declare the list of jobs that this scanner requires
        FileSystemDiscoveryJob fileSystemDiscoveryJob = _mediaScanJobFactory.CreateJob<FileSystemDiscoveryJob>(library); 
        RepositoryMetadataDiscoveryJob repositoryMetadataDiscoveryJob = _mediaScanJobFactory.CreateJob<RepositoryMetadataDiscoveryJob>(library);
        HashComparerJob hashComparerJob = _mediaScanJobFactory.CreateJob<HashComparerJob>(library);
        GoodReadsMetadataScrapJob goodReadsMetadataScrapJob = _mediaScanJobFactory.CreateJob<GoodReadsMetadataScrapJob>(library);

        // establish the hierarchical relationships between jobs
        fileSystemDiscoveryJob.AddChild(hashComparerJob);
        repositoryMetadataDiscoveryJob.AddChild(hashComparerJob);

        hashComparerJob.AddParent(fileSystemDiscoveryJob);
        hashComparerJob.AddParent(repositoryMetadataDiscoveryJob);

        if (library.DownloadMedatadaFromWeb)
        {
            hashComparerJob.AddChild(goodReadsMetadataScrapJob);
            goodReadsMetadataScrapJob.AddParent(hashComparerJob);
        }

        // return the top level jobs that will be triggered when the scan will be started
        yield return fileSystemDiscoveryJob;
        yield return repositoryMetadataDiscoveryJob;
    }
}
