#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
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
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicMediaLibraryScanMetadataEnricher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaLibraryScanMetadataEnricherTests
{
    private const string PROVIDER_A_NAME = "ProviderA";
    private const string PROVIDER_B_NAME = "ProviderB";

    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockMetadataConfigurationRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IMetadataProvider _mockProviderA;
    private readonly IMetadataProvider _mockProviderB;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ServiceProvider _serviceProvider;
    private readonly MusicMediaLibraryScanMetadataEnricher _sut;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _metadataConfigurationEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly ArtistMetadataDtoFixture _artistMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryScanMetadataEnricherTests"/> class.
    /// </summary>
    public MusicMediaLibraryScanMetadataEnricherTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockMetadataConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockMetadataConfigurationRepository);
        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(Substitute.For<IMediaContributorRepository>());

        // Both plugins expose only an artist metadata provider, so that the tests exercise the artist lookup in isolation, without album or track lookups ever reaching a provider.
        _mockProviderA = Substitute.For<IMetadataProvider>();
        ConfigureProvider(_mockProviderA, PROVIDER_A_NAME);
        _mockProviderB = Substitute.For<IMetadataProvider>();
        ConfigureProvider(_mockProviderB, PROVIDER_B_NAME);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();

        // the configured providers are registered under the plugin Ids they are configured with, the way the host registers plugin provided services
        Guid pluginAId = Guid.NewGuid();
        Guid pluginBId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>.Success(
            [
                _metadataConfigurationEntityFixture.Create(_libraryId.Value, pluginAId, rank: 1),
                _metadataConfigurationEntityFixture.Create(_libraryId.Value, pluginBId, rank: 2)
            ]));

        ServiceCollection services = new();
        services.AddScoped<IUnitOfWork>(_ => _mockUnitOfWork);
        services.AddScoped<IDomainEventPublisher>(_ => _mockDomainEventPublisher);
        services.AddKeyedTransient<IMetadataProvider>(pluginAId, (serviceProvider, serviceKey) => _mockProviderA);
        services.AddKeyedTransient<IMetadataProvider>(pluginBId, (serviceProvider, serviceKey) => _mockProviderB);
        _serviceProvider = services.BuildServiceProvider();

        _sut = new MusicMediaLibraryScanMetadataEnricher(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), NullLogger<MusicMediaLibraryScanMetadataEnricher>.Instance);

        _mockArtistRepository.GetArtistsNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _mockAlbumRepository.GetAlbumsNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _mockTrackRepository.GetTracksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([]));
        _mockArtistRepository.UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
    }

    [Fact]
    public async Task EnrichAsync_WhenArtistsAlbumsAndTracksNeedEnrichment_ShouldPublishProgressMeasuredInItems()
    {
        // Arrange
        _mockArtistRepository.GetArtistsNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(2));
        _mockAlbumRepository.GetAlbumsNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(3));
        _mockTrackRepository.GetTracksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(5));
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        // the total is the sum of the artists, the albums and the tracks, and the final update reaches it
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanJobProgressChangedDomainEvent>(domainEvent =>
            domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.Progress.CompletedItems == 0
            && domainEvent.Progress.TotalItems == 10), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanJobProgressChangedDomainEvent>(domainEvent =>
            domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.Progress.CompletedItems == 10
            && domainEvent.Progress.TotalItems == 10), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheFirstProviderReturnsApplicableMetadata_ShouldNotQueryTheFollowingProviders()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([artist]), Result<IReadOnlyList<ArtistEntity>>.Success([]));
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Local Artist"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.DidNotReceive().GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).UpdateAsync(
            Arg.Is<ArtistEntity>(entity => entity.MetadataStatus == MetadataStatus.Enriched && entity.MetadataProvider == PROVIDER_A_NAME),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheFirstProviderReturnsNoUsableMetadata_ShouldQueryTheFollowingProviders()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([artist]), Result<IReadOnlyList<ArtistEntity>>.Success([]));
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)null);
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Remote Artist"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).UpdateAsync(
            Arg.Is<ArtistEntity>(entity => entity.MetadataStatus == MetadataStatus.Enriched && entity.MetadataProvider == PROVIDER_B_NAME),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheMetadataOfTheFirstProviderCannotBeApplied_ShouldQueryTheFollowingProviders()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([artist]), Result<IReadOnlyList<ArtistEntity>>.Success([]));
        // a genre without a name is usable as metadata, but makes the domain conversion of the whole metadata fail
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Local Artist", genres: [_genreDtoFixture.Create(includeName: false)]));
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Remote Artist"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).UpdateAsync(
            Arg.Is<ArtistEntity>(entity => entity.MetadataStatus == MetadataStatus.Enriched && entity.MetadataProvider == PROVIDER_B_NAME),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenSavingFailsWithAUniqueConstraintViolation_ShouldContinueWithTheFollowingArtists()
    {
        // Arrange
        ArtistEntity firstArtist = _artistEntityFixture.Create(libraryId: _libraryId.Value, name: "First Artist");
        ArtistEntity secondArtist = _artistEntityFixture.Create(libraryId: _libraryId.Value, name: "Second Artist");
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([firstArtist, secondArtist]), Result<IReadOnlyList<ArtistEntity>>.Success([]));
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Local Artist"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<Success>.Failure(Errors.Persistence.UniqueConstraintViolation), Result.From(Result.Success));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        // The first artist's save fails with a unique constraint conflict, which must not abort the scan, so the second artist is still processed.
        await _mockUnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(2).UpdateAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenAggregationIsEnabled_ShouldQueryEveryProviderAndMergeTheirMetadata()
    {
        // Arrange
        _mockUserSettingsRepository.GetByUserIdAsync(_userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<UserSettingsEntity?>.Success(_userSettingsEntityFixture.Create(userId: _userId.Value, shouldAggregateMetadataWhenMissing: true)));
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: _libraryId.Value);
        _mockArtistRepository.GetArtistsNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArtistEntity>>.Success([artist]), Result<IReadOnlyList<ArtistEntity>>.Success([]));
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Local Artist"));
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableArtistMetadata("Remote Artist"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).UpdateAsync(
            Arg.Is<ArtistEntity>(entity => entity.MetadataStatus == MetadataStatus.Enriched && entity.MetadataProvider == $"{PROVIDER_A_NAME}, {PROVIDER_B_NAME}"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Configures the provided substitute metadata provider so that it declares the artist lookup type for the music library type.
    /// </summary>
    /// <param name="metadataProvider">The metadata provider substitute to configure.</param>
    /// <param name="name">The display name the provider reports.</param>
    private static void ConfigureProvider(IMetadataProvider metadataProvider, string name)
    {
        metadataProvider.Name.Returns(name);
        metadataProvider.SupportedLibraryTypes.Returns(new List<LibraryType> { LibraryType.Music });
        metadataProvider.RequiresWebAccess.Returns(false);
        metadataProvider.LookupType.Returns(typeof(ArtistMetadataLookupDto));
    }

    /// <summary>
    /// Creates an artist metadata that is usable and carries only a name, and optionally a set of genres.
    /// </summary>
    /// <param name="name">The name of the artist.</param>
    /// <param name="genres">The optional genres of the artist.</param>
    /// <returns>The created artist metadata.</returns>
    private ArtistMetadataDto CreateApplicableArtistMetadata(string name, List<GenreDto>? genres = null)
    {
        return _artistMetadataDtoFixture.Create(
            name: name,
            genres: genres,
            includeSortName: false,
            includeDisambiguation: false,
            includeType: false,
            includeGender: false,
            includeCountry: false,
            includeArea: false,
            includeBeginArea: false,
            includeEndArea: false,
            includeLifeSpanBegin: false,
            includeLifeSpanEnd: false,
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            includeIpis: false,
            includeIsnis: false,
            includeGenres: genres is not null,
            includeTags: false,
            includeAliases: false,
            includeRatings: false,
            includeContributors: false);
    }
}
