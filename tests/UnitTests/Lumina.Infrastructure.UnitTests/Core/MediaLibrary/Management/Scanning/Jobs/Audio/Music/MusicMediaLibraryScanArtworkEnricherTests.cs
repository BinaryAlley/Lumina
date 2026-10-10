#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicMediaLibraryScanArtworkEnricher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaLibraryScanArtworkEnricherTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkProviderConfigurationRepository;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IMusicArtworkService _mockMusicArtworkService;
    private readonly IFileHashService _mockFileHashService;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly TestArtworkProvider<ArtistMetadataLookupDto> _artistProvider;
    private readonly TestArtworkProvider<AlbumMetadataLookupDto> _albumProvider;
    private readonly MusicMediaLibraryScanArtworkEnricher _sut;
    private readonly ServiceProvider _serviceProvider;
    private readonly Guid _artistPluginId = Guid.NewGuid();
    private readonly Guid _albumPluginId = Guid.NewGuid();
    private readonly LibraryId _libraryId;
    private readonly ScanId _scanId;
    private readonly UserId _userId;

    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryScanArtworkEnricherTests"/> class.
    /// </summary>
    public MusicMediaLibraryScanArtworkEnricherTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockArtworkProviderConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkProviderConfigurationRepository);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);

        _artistProvider = new TestArtworkProvider<ArtistMetadataLookupDto> { Name = "Artist Provider", RequiresWebAccess = false };
        _albumProvider = new TestArtworkProvider<AlbumMetadataLookupDto> { Name = "Album Provider", RequiresWebAccess = false };

        _mockMusicArtworkService = Substitute.For<IMusicArtworkService>();
        _mockFileHashService = Substitute.For<IFileHashService>();
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);

        // the enricher resolves the keyed artwork providers from the scope of the service provider, and specializes them by the lookup they accept,
        // so a real container is used to honor the keyed registration of the typed providers.
        ServiceCollection services = new();
        services.AddScoped<IUnitOfWork>(_ => _mockUnitOfWork);
        services.AddScoped<IDomainEventPublisher>(_ => _mockDomainEventPublisher);
        services.AddSingleton(_mockMusicArtworkService);
        services.AddSingleton(_mockFileHashService);
        services.AddKeyedSingleton<IArtworkProvider>(_artistPluginId, _artistProvider);
        services.AddKeyedSingleton<IArtworkProvider>(_albumPluginId, _albumProvider);
        _serviceProvider = services.BuildServiceProvider();

        _sut = new MusicMediaLibraryScanArtworkEnricher(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), NullLogger<MusicMediaLibraryScanArtworkEnricher>.Instance);

        _libraryId = _libraryIdFixture.Create();
        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();

        // Default stubs that keep the happy path working, overridden per test where needed.
        SetupLibrary();
        SetupConfiguration();
        _mockUserSettingsRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserSettingsEntity?>.Success(null));
        _mockArtistRepository.GetArtistsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(0));
        _mockArtistRepository.GetArtistsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ArtistEntity>>([]));
        _mockAlbumRepository.GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(0));
        _mockAlbumRepository.GetAlbumsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([]));
        _mockAlbumRepository.GetFirstTrackPathsByAlbumIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string>>(new Dictionary<Guid, string>()));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        SetupSuccessArtworkStorage();
    }

    [Fact]
    public void SupportedLibraryType_WhenCalled_ShouldReturnMusic()
    {
        // Act
        LibraryType result = _sut.SupportedLibraryType;

        // Assert
        Assert.Equal(LibraryType.Music, result);
    }

    [Fact]
    public async Task EnrichAsync_WhenNoArtworkProviderIsConfigured_ShouldSkipTheEnrichment()
    {
        // Arrange
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockArtistRepository.DidNotReceive().GetArtistsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheArtworkConfigurationsCannotBeRead_ShouldSkipTheEnrichment()
    {
        // Arrange
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Provider.Error", "Failed to read the configurations"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockArtistRepository.DidNotReceive().GetArtistsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheLibraryDisallowsWebDownloads_ShouldSkipProvidersThatRequireWebAccess()
    {
        // Arrange
        SetupLibrary(canDownloadMetadataFromWeb: false);
        _artistProvider.RequiresWebAccess = true;
        _albumProvider.RequiresWebAccess = true;

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockArtistRepository.DidNotReceive().GetArtistsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheLibraryDisallowsWebDownloadsButTheProviderIsLocal_ShouldStillResolve()
    {
        // Arrange
        SetupLibrary(canDownloadMetadataFromWeb: false);
        _artistProvider.RequiresWebAccess = true;
        AlbumEntity album = _albumEntityFixture.Create(libraryId: _libraryId.Value);
        SetupAlbums(album);
        _albumProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null, type: ArtworkType.Cover, ordinal: 0)]);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        Assert.Equal(1, _albumProvider.CallCount);
        Assert.Equal(0, _artistProvider.CallCount);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheArtistProviderReturnsArtwork_ShouldStoreAndPersistEnrichedArtistArtwork()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null, type: ArtworkType.Cover, ordinal: 0)]);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkService.Received(1).SaveArtistArtworkAsync(
            _libraryId.Value,
            Arg.Any<string>(),
            artist.Name,
            Arg.Is<ArtworkDto>(artwork => artwork.Type == ArtworkType.Cover && artwork.Ordinal == 0),
            Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artist.Id, Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicArtworkEntity>>(entities => entities.Count == 1
                && entities.First().OwnerType == MusicArtworkOwnerType.Artist
                && entities.First().OwnerId == artist.Id
                && entities.First().ArtworkType == ArtworkType.Cover
                && entities.First().Ordinal == 0
                && entities.First().Status == ArtworkStatus.Enriched
                && entities.First().Provider == "Artist Provider"
                && entities.First().FileName == "/media/music/cover.jpg"
                && entities.First().ContentHash == 123UL),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheAlbumProviderReturnsArtwork_ShouldStoreAndPersistEnrichedAlbumArtwork()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(libraryId: _libraryId.Value);
        SetupAlbums(album);
        _albumProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null, type: ArtworkType.Cover, ordinal: 0)]);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            _libraryId.Value,
            album.Id,
            Arg.Any<string>(),
            Arg.Any<string>(),
            album.Title,
            Arg.Is<ArtworkDto>(artwork => artwork.Type == ArtworkType.Cover && artwork.Ordinal == 0),
            Arg.Any<CancellationToken>(),
            Arg.Any<string?>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, album.Id, Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicArtworkEntity>>(entities => entities.Count == 1
                && entities.First().OwnerType == MusicArtworkOwnerType.Album
                && entities.First().OwnerId == album.Id
                && entities.First().Status == ArtworkStatus.Enriched
                && entities.First().Provider == "Album Provider"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheProviderAnswersWithNoArtwork_ShouldMarkTheArtistNotAvailable()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([]);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicArtworkEntity>>(entities => entities.Count == 1
                && entities.First().Status == ArtworkStatus.NotAvailable
                && entities.First().FileName == null
                && entities.First().Provider == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenEveryProviderFails_ShouldMarkTheArtistFailed()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromException<IReadOnlyList<ArtworkDto>>(new HttpRequestException("Transient failure."));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicArtworkEntity>>(entities => entities.Count == 1 && entities.First().Status == ArtworkStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenStoringTheArtworkFails_ShouldMarkTheArtistFailed()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null)]);
        _mockMusicArtworkService.SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Artwork.Error", "Failed to store the artwork"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicArtworkEntity>>(entities => entities.Count == 1 && entities.First().Status == ArtworkStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenClearingTheStoredArtworkFilesFails_ShouldLeaveTheArtistUnresolved()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null)]);
        _mockMusicArtworkService.DeleteArtistArtwork(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Error.Failure("Artwork.Error", "Failed to clear the artwork files"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicArtworkEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenClearingTheStoredArtworkRecordsFails_ShouldLeaveTheArtistUnresolved()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null)]);
        _mockMusicArtworkRepository.DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Artwork.Error", "Failed to clear the artwork records"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicArtworkEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheAlbumHasNoMusicBrainzIdentifierAndNoLocalProvider_ShouldLeaveItUnresolved()
    {
        // Arrange
        _albumProvider.RequiresWebAccess = true;
        AlbumEntity album = _albumEntityFixture.Create(
            libraryId: _libraryId.Value,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false);
        SetupAlbums(album);
        _albumProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null)]);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        Assert.Equal(0, _albumProvider.CallCount);
        await _mockMusicArtworkRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicArtworkEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheAlbumCountCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockAlbumRepository.GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to count the albums"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheAlbumsPageCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockAlbumRepository.GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(1));
        _mockAlbumRepository.GetAlbumsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to read the albums"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheTrackPathsCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(libraryId: _libraryId.Value);
        _mockAlbumRepository.GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(1));
        _mockAlbumRepository.GetAlbumsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockAlbumRepository.GetFirstTrackPathsByAlbumIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to read the track paths"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenSavingTheChangesFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        SetupArtists(artist);
        _artistProvider.OnGetArtworkAsync = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/tmp/cover.jpg", remoteUrl: null)]);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to save the changes"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    /// <summary>
    /// Stubs the media library reader with a library owned by the test user.
    /// </summary>
    /// <param name="canDownloadMetadataFromWeb">Whether the library allows downloading data from the web.</param>
    private void SetupLibrary(bool canDownloadMetadataFromWeb = true)
    {
        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            canDownloadMetadataFromWeb: canDownloadMetadataFromWeb);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the artwork provider configuration reader with one enabled configuration per test plugin.
    /// </summary>
    private void SetupConfiguration()
    {
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>(
            [
                _libraryArtworkProviderConfigurationEntityFixture.Create(_libraryId.Value, _artistPluginId, 0),
                _libraryArtworkProviderConfigurationEntityFixture.Create(_libraryId.Value, _albumPluginId, 1)
            ]));
    }

    /// <summary>
    /// Stubs the artist reader with a single page holding the provided artists.
    /// </summary>
    /// <param name="artists">The artists of the page.</param>
    private void SetupArtists(params ArtistEntity[] artists)
    {
        _mockArtistRepository.GetArtistsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(artists.Length));
        _mockArtistRepository.GetArtistsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<ArtistEntity>>(artists),
                Result.From<IReadOnlyList<ArtistEntity>>([]));
    }

    /// <summary>
    /// Stubs the album reader with a single page holding the provided albums.
    /// </summary>
    /// <param name="albums">The albums of the page.</param>
    private void SetupAlbums(params AlbumEntity[] albums)
    {
        _mockAlbumRepository.GetAlbumsNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(albums.Length));
        _mockAlbumRepository.GetAlbumsNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<AlbumEntity>>(albums),
                Result.From<IReadOnlyList<AlbumEntity>>([]));
        Dictionary<Guid, string> trackPaths = albums.ToDictionary(album => album.Id, album => "/music/artist/album/track.flac");
        _mockAlbumRepository.GetFirstTrackPathsByAlbumIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string>>(trackPaths));
    }

    /// <summary>
    /// Stubs the artwork storage and persistence so that an item is stored successfully.
    /// </summary>
    private void SetupSuccessArtworkStorage()
    {
        _mockMusicArtworkService.SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/music/cover.jpg"));
        _mockMusicArtworkService.SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>(), Arg.Any<string?>())
            .Returns(Result.From("/media/music/cover.jpg"));
        _mockMusicArtworkService.DeleteArtistArtwork(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Result.Deleted);
        _mockMusicArtworkService.DeleteAlbumArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>()).Returns(Result.Deleted);
        _mockMusicArtworkRepository.DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockMusicArtworkRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicArtworkEntity>>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockFileHashService.ComputeFileHash(Arg.Any<string>()).Returns(123UL);
    }

    /// <summary>
    /// Deterministic artwork provider for a specific lookup type, used instead of a substitute because the default implementation of
    /// <see cref="IArtworkProvider.GetArtworkAsync(MetadataLookupDto, CancellationToken)"/> cannot be intercepted reliably.
    /// </summary>
    /// <typeparam name="TLookup">The type of the lookup the provider accepts.</typeparam>
    private sealed class TestArtworkProvider<TLookup> : IArtworkProvider<TLookup> where TLookup : MetadataLookupDto
    {
        /// <summary>
        /// Gets or sets the display name of the artwork provider.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the media library types this artwork provider supports.
        /// </summary>
        public IReadOnlyList<LibraryType> SupportedLibraryTypes { get; set; } = [LibraryType.Music];

        /// <summary>
        /// Gets or sets a value indicating whether this artwork provider requires access to the web to retrieve artwork.
        /// </summary>
        public bool RequiresWebAccess { get; set; }

        /// <summary>
        /// Gets or sets the callback that supplies the artwork of a media item.
        /// </summary>
        public Func<TLookup, CancellationToken, Task<IReadOnlyList<ArtworkDto>>> OnGetArtworkAsync { get; set; } = (_, _) => Task.FromResult<IReadOnlyList<ArtworkDto>>([]);

        /// <summary>
        /// Gets the number of times the provider was asked for artwork.
        /// </summary>
        public int CallCount { get; private set; }

        /// <summary>
        /// Gets the artworks of the media item described by <paramref name="lookup"/>.
        /// </summary>
        /// <param name="lookup">The lookup describing the media item to get the artwork for.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The artworks of the media item, or an empty collection when no artwork was found.</returns>
        public Task<IReadOnlyList<ArtworkDto>> GetArtworkAsync(TLookup lookup, CancellationToken cancellationToken)
        {
            CallCount++;
            return OnGetArtworkAsync(lookup, cancellationToken);
        }
    }
}
