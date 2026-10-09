#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
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

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryTypeScanner"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryTypeScannerTests
{
    private readonly IMediaLibraryScanJobFactory _mockJobFactory = Substitute.For<IMediaLibraryScanJobFactory>();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void SupportedLibraryType_ShouldBeMusic()
    {
        // Arrange
        MusicLibraryTypeScanner sut = new(_mockJobFactory);

        // Act & Assert
        Assert.Equal(LibraryType.Music, sut.SupportedLibraryType);
    }

    [Fact]
    public void CreateScanJobsForLibrary_WhenCalled_ShouldCreateJobChainWithEnrichmentJobs()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        TestScanJob discoveryJob = CreateJob<IMusicFileSystemDiscoveryJob>();
        TestScanJob diffJob = CreateJob<IMediaLibraryScanDiffJob>();
        TestScanJob hashJob = CreateJob<IMediaLibraryScanHashJob>();
        TestScanJob metadataExtractionJob = CreateJob<IMusicMetadataExtractionJob>();
        TestScanJob saveJob = CreateJob<IMediaLibraryScanResultsSaveJob>();
        TestScanJob invalidationJob = CreateJob<IMediaLibraryScanProviderConfigurationInvalidationJob>();
        TestScanJob enrichmentJob = CreateJob<IMediaLibraryScanMetadataEnrichmentJob>();
        TestScanJob artworkEnrichmentJob = CreateJob<IMediaLibraryScanArtworkEnrichmentJob>();
        MusicLibraryTypeScanner sut = new(_mockJobFactory);

        // Act
        List<IMediaLibraryScanJob> rootJobs = [.. sut.CreateScanJobsForLibrary(libraryId)];

        // Assert
        IMediaLibraryScanJob rootJob = Assert.Single(rootJobs);
        Assert.Same(discoveryJob, rootJob);
        Assert.Contains(diffJob, discoveryJob.Children);
        Assert.Contains(discoveryJob, diffJob.Parents);
        Assert.Contains(hashJob, diffJob.Children);
        Assert.Contains(diffJob, hashJob.Parents);
        Assert.Contains(metadataExtractionJob, hashJob.Children);
        Assert.Contains(hashJob, metadataExtractionJob.Parents);
        Assert.Contains(saveJob, metadataExtractionJob.Children);
        Assert.Contains(metadataExtractionJob, saveJob.Parents);
        Assert.Contains(invalidationJob, saveJob.Children);
        Assert.Contains(saveJob, invalidationJob.Parents);
        Assert.Contains(enrichmentJob, invalidationJob.Children);
        Assert.Contains(invalidationJob, enrichmentJob.Parents);
        Assert.Contains(artworkEnrichmentJob, enrichmentJob.Children);
        Assert.Contains(enrichmentJob, artworkEnrichmentJob.Parents);
        _mockJobFactory.Received(1).CreateJob<IMusicMetadataExtractionJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanArtworkEnrichmentJob>(libraryId);
    }

    [Fact]
    public void CreateScanJobsForLibrary_WhenCalled_ShouldCreateJobsForTheLibrary()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        CreateJob<IMusicFileSystemDiscoveryJob>();
        CreateJob<IMediaLibraryScanDiffJob>();
        CreateJob<IMediaLibraryScanHashJob>();
        CreateJob<IMusicMetadataExtractionJob>();
        CreateJob<IMediaLibraryScanResultsSaveJob>();
        CreateJob<IMediaLibraryScanProviderConfigurationInvalidationJob>();
        CreateJob<IMediaLibraryScanMetadataEnrichmentJob>();
        CreateJob<IMediaLibraryScanArtworkEnrichmentJob>();
        MusicLibraryTypeScanner sut = new(_mockJobFactory);

        // Act
        _ = sut.CreateScanJobsForLibrary(libraryId).ToList();

        // Assert
        _mockJobFactory.Received(1).CreateJob<IMusicFileSystemDiscoveryJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanDiffJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanHashJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMusicMetadataExtractionJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanResultsSaveJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanProviderConfigurationInvalidationJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanMetadataEnrichmentJob>(libraryId);
        _mockJobFactory.Received(1).CreateJob<IMediaLibraryScanArtworkEnrichmentJob>(libraryId);
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
        IMusicFileSystemDiscoveryJob,
        IMediaLibraryScanDiffJob,
        IMediaLibraryScanHashJob,
        IMusicMetadataExtractionJob,
        IMediaLibraryScanResultsSaveJob,
        IMediaLibraryScanProviderConfigurationInvalidationJob,
        IMediaLibraryScanMetadataEnrichmentJob,
        IMediaLibraryScanArtworkEnrichmentJob
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
