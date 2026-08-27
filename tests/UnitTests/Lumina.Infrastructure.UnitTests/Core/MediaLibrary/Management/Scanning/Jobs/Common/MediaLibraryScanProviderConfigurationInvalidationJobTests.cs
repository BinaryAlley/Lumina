#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanProviderConfigurationInvalidationJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProviderConfigurationInvalidationJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockMetadataConfigurationRepository;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkConfigurationRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<MediaLibraryScanProviderConfigurationInvalidationJob> _mockLogger;
    private readonly MediaLibraryScanProviderConfigurationInvalidationJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryMetadataProviderConfigurationEntityFixture _metadataConfigurationEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _artworkConfigurationEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanProviderConfigurationInvalidationJobTests"/> class.
    /// </summary>
    public MediaLibraryScanProviderConfigurationInvalidationJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMetadataConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockArtworkConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockMetadataConfigurationRepository);
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkConfigurationRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _mockLogger = Substitute.For<ILogger<MediaLibraryScanProviderConfigurationInvalidationJob>>();

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new MediaLibraryScanProviderConfigurationInvalidationJob(_mockServiceScopeFactory, _mockLogger)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFingerprintIsStored_ShouldSeedTheFingerprintsWithoutResettingAnything()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", metadataProvidersConfigurationFingerprint: null, artworkProvidersConfigurationFingerprint: null);
        SetupLibraryAndConfigurations(library);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a missing stored fingerprint means the configuration was never recorded yet, so the current state of the books is trusted
        await _mockBookRepository.DidNotReceive().ResetMetadataStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().ResetArtworkStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMetadataFingerprintDiffers_ShouldResetTheMetadataStatus()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", metadataProvidersConfigurationFingerprint: "STALE", artworkProvidersConfigurationFingerprint: null);
        SetupLibraryAndConfigurations(library);
        _mockBookRepository.ResetMetadataStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the metadata provider configuration changed, so the books must be re-enriched by the metadata enrichment job
        await _mockBookRepository.Received(1).ResetMetadataStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().ResetArtworkStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtworkFingerprintDiffers_ShouldResetTheArtworkStatus()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", metadataProvidersConfigurationFingerprint: null, artworkProvidersConfigurationFingerprint: "STALE");
        SetupLibraryAndConfigurations(library);
        _mockBookRepository.ResetArtworkStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the artwork provider configuration changed, so the artwork of the books must be re-resolved by the artwork enrichment job
        await _mockBookRepository.Received(1).ResetArtworkStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().ResetMetadataStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFingerprintsMatch_ShouldNotResetAnythingButStillPersistTheFingerprints()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library");
        List<LibraryMetadataProviderConfigurationEntity> metadataConfigurations = _metadataConfigurationEntityFixture.CreateMany(2, _libraryId.Value, Guid.NewGuid(), 1);
        List<LibraryArtworkProviderConfigurationEntity> artworkConfigurations = _artworkConfigurationEntityFixture.CreateMany(2, _libraryId.Value, Guid.NewGuid(), 1);
        SetupLibraryAndConfigurations(library, metadataConfigurations, artworkConfigurations);

        // the stored fingerprints match the ones computed from the current provider configuration
        string metadataFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(metadataConfigurations, false, library.CanDownloadMetadataFromWeb);
        string artworkFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint(artworkConfigurations, library.CanDownloadMetadataFromWeb);
        library.MetadataProvidersConfigurationFingerprint = metadataFingerprint;
        library.ArtworkProvidersConfigurationFingerprint = artworkFingerprint;

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the configuration did not change, so the enrichment state of the books is trusted, and only the fingerprints are persisted
        await _mockBookRepository.DidNotReceive().ResetMetadataStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().ResetArtworkStatusForLibraryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBothFingerprintsDiffer_ShouldPersistTheUpdatedFingerprintsViaSaveChangesOnly()
    {
        // Arrange
        // both fingerprints are stale, so both enrichment channels must be reset and both fingerprints must be rewritten on the tracked entity
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", metadataProvidersConfigurationFingerprint: "STALE_METADATA", artworkProvidersConfigurationFingerprint: "STALE_ARTWORK");
        SetupLibraryAndConfigurations(library);
        _mockBookRepository.ResetMetadataStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockBookRepository.ResetArtworkStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // regression guard for the content-location data-loss bug: the fingerprints are persisted on the already-tracked entity and
        // flushed through SaveChanges, the repository update action (which clears and re-adds the owned content locations) must never run
        await _mockBookRepository.Received(1).ResetMetadataStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).ResetArtworkStatusForLibraryAsync(_libraryId.Value, Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheLibraryFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the media library"));
        SetupConfigurations();

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldMarkJobAsCanceledAndThrow()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task operation = _sut.ExecuteAsync(Guid.NewGuid(), new { }, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(LibraryScanJobStatus.Canceled, _sut.Status);
    }

    /// <summary>
    /// Stubs the repositories so that the job reads the provided library and its provider configurations.
    /// </summary>
    /// <param name="library">The media library the job must read.</param>
    /// <param name="metadataConfigurations">The metadata provider configurations of the media library.</param>
    /// <param name="artworkConfigurations">The artwork provider configurations of the media library.</param>
    private void SetupLibraryAndConfigurations(LibraryEntity library, List<LibraryMetadataProviderConfigurationEntity>? metadataConfigurations = null, List<LibraryArtworkProviderConfigurationEntity>? artworkConfigurations = null)
    {
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        SetupConfigurations(metadataConfigurations, artworkConfigurations);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Stubs the provider configuration repositories and the user settings repository used by the job.
    /// </summary>
    /// <param name="metadataConfigurations">The metadata provider configurations of the media library, if already created by the caller.</param>
    /// <param name="artworkConfigurations">The artwork provider configurations of the media library, if already created by the caller.</param>
    private void SetupConfigurations(List<LibraryMetadataProviderConfigurationEntity>? metadataConfigurations = null, List<LibraryArtworkProviderConfigurationEntity>? artworkConfigurations = null)
    {
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>(
                metadataConfigurations ?? [
                    _metadataConfigurationEntityFixture.Create(_libraryId.Value, Guid.NewGuid(), 1),
                    _metadataConfigurationEntityFixture.Create(_libraryId.Value, Guid.NewGuid(), 2)
                ]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>(
                artworkConfigurations ?? [
                    _artworkConfigurationEntityFixture.Create(_libraryId.Value, Guid.NewGuid(), 1),
                    _artworkConfigurationEntityFixture.Create(_libraryId.Value, Guid.NewGuid(), 2)
                ]));

        IUserSettingsRepository mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        mockUserSettingsRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(_userSettingsEntityFixture.Create(shouldAggregateMetadataWhenMissing: false)));
        _mockUnitOfWork.UserSettingsRepository.Returns(mockUserSettingsRepository);
    }
}
