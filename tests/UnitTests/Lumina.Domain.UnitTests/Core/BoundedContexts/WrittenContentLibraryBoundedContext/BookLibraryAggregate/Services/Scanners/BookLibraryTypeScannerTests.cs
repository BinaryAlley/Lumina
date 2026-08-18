#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;

/// <summary>
/// Contains unit tests for the <see cref="BookLibraryTypeScanner"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLibraryTypeScannerTests
{
    private readonly IMediaLibraryScanJobFactory _mockJobFactory = Substitute.For<IMediaLibraryScanJobFactory>();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void SupportedLibraryType_ShouldBeBook()
    {
        // Arrange
        BookLibraryTypeScanner sut = new(_mockJobFactory);

        // Act & Assert
        Assert.Equal(LibraryType.Book, sut.SupportedLibraryType);
    }

    [Fact]
    public void CreateScanJobsForLibrary_WhenDownloadMetadataFromWebIsFalse_ShouldCreateJobChainWithoutMetadataEnrichment()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        TestScanJob discoveryJob = CreateJob<IBooksFileSystemDiscoveryJob>();
        TestScanJob diffJob = CreateJob<IMediaLibraryScanDiffJob>();
        TestScanJob hashJob = CreateJob<IMediaLibraryScanHashJob>();
        TestScanJob saveJob = CreateJob<IMediaLibraryScanResultsSaveJob>();
        BookLibraryTypeScanner sut = new(_mockJobFactory);

        // Act
        List<IMediaLibraryScanJob> rootJobs = [.. sut.CreateScanJobsForLibrary(libraryId, downloadMetadataFromWeb: false)];

        // Assert
        IMediaLibraryScanJob rootJob = Assert.Single(rootJobs);
        Assert.Same(discoveryJob, rootJob);
        Assert.Contains(diffJob, discoveryJob.Children);
        Assert.Contains(discoveryJob, diffJob.Parents);
        Assert.Contains(hashJob, diffJob.Children);
        Assert.Contains(diffJob, hashJob.Parents);
        Assert.Contains(saveJob, hashJob.Children);
        Assert.Contains(hashJob, saveJob.Parents);
        _mockJobFactory.DidNotReceive().CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(Arg.Any<LibraryId>());
    }

    [Fact]
    public void CreateScanJobsForLibrary_WhenDownloadMetadataFromWebIsTrue_ShouldCreateJobChainWithMetadataEnrichment()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        TestScanJob discoveryJob = CreateJob<IBooksFileSystemDiscoveryJob>();
        TestScanJob diffJob = CreateJob<IMediaLibraryScanDiffJob>();
        TestScanJob hashJob = CreateJob<IMediaLibraryScanHashJob>();
        TestScanJob saveJob = CreateJob<IMediaLibraryScanResultsSaveJob>();
        TestScanJob enrichmentJob = CreateJob<IMediaLibraryScanMetadataEnrichmentJob>();
        BookLibraryTypeScanner sut = new(_mockJobFactory);

        // Act
        List<IMediaLibraryScanJob> rootJobs = [.. sut.CreateScanJobsForLibrary(libraryId, downloadMetadataFromWeb: true)];

        // Assert
        IMediaLibraryScanJob rootJob = Assert.Single(rootJobs);
        Assert.Same(discoveryJob, rootJob);
        Assert.Contains(saveJob, hashJob.Children);
        Assert.Contains(enrichmentJob, saveJob.Children);
        Assert.Contains(saveJob, enrichmentJob.Parents);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
    }

    [Fact]
    public void CreateScanJobsForLibrary_WhenCalled_ShouldCreateJobsForTheLibrary()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        CreateJob<IBooksFileSystemDiscoveryJob>();
        CreateJob<IMediaLibraryScanDiffJob>();
        CreateJob<IMediaLibraryScanHashJob>();
        CreateJob<IMediaLibraryScanResultsSaveJob>();
        BookLibraryTypeScanner sut = new(_mockJobFactory);

        // Act
        _ = sut.CreateScanJobsForLibrary(libraryId, downloadMetadataFromWeb: false).ToList();

        // Assert
        _mockJobFactory.Received(1).CreateJob<IBooksFileSystemDiscoveryJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanDiffJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanHashJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanResultsSaveJob>(libraryId);
    }

    private TestScanJob CreateJob<TJob>() where TJob : class, IMediaLibraryScanJob
    {
        TestScanJob job = new()
        {
            ScanId = _scanIdFixture.Create(),
            UserId = _userIdFixture.Create(),
            LibraryId = _libraryIdFixture.Create()
        };
        _mockJobFactory.CreateJob<TJob>(Arg.Any<LibraryId>()).Returns((TJob)(object)job);
        return job;
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="MediaLibraryScanJob"/> class that satisfies all job interfaces.
    /// </summary>
    private sealed class TestScanJob : MediaLibraryScanJob,
        IBooksFileSystemDiscoveryJob,
        IMediaLibraryScanDiffJob,
        IMediaLibraryScanHashJob,
        IMediaLibraryScanResultsSaveJob,
        IMediaLibraryScanMetadataEnrichmentJob
    {
        /// <summary>
        /// Executes the payload of the media library scan job.
        /// </summary>
        /// <typeparam name="TInput">The type of the input parameter.</typeparam>
        /// <param name="id">The unique identifier of the media library scan job.</param>
        /// <param name="input">The input data to be processed.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
