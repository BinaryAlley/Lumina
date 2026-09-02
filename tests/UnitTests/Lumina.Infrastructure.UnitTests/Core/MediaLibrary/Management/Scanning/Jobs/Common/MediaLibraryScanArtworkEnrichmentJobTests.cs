#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanArtworkEnrichmentJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanArtworkEnrichmentJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkConfigurationRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<MediaLibraryScanArtworkEnrichmentJob> _mockLogger;
    private readonly MediaLibraryScanArtworkEnrichmentJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _artworkConfigurationEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanArtworkEnrichmentJobTests"/> class.
    /// </summary>
    public MediaLibraryScanArtworkEnrichmentJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtworkConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkConfigurationRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _mockLogger = Substitute.For<ILogger<MediaLibraryScanArtworkEnrichmentJob>>();

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new MediaLibraryScanArtworkEnrichmentJob(_mockServiceScopeFactory, _mockLogger)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnArtworkProviderReturnsArtwork_ShouldStoreItAndTrackTheCoverArtworkAsEnriched()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));

        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/cover.jpg").Returns(123ul);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub", title: "Test Title");
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            _libraryId.Value,
            book.Id,
            "My Library",
            "Frank Herbert",
            "Test Title",
            Arg.Any<ArtworkDto>(),
            Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkType.Cover, coverArtwork.ArtworkType);
        Assert.Equal(0, coverArtwork.Ordinal);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("\\media\\books\\Library\\Author\\Title\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal(123ul, coverArtwork.ContentHash);
        Assert.Equal("Artwork Provider", coverArtwork.Provider);
        Assert.NotNull(coverArtwork.LastUpdateUtc);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResolvedArtworkIsIdenticalToTheStoredOne_ShouldKeepTheStoredArtworkAndMarkItAsEnriched()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/cover.jpg").Returns(123ul);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        // the book already has a stored cover with the same content hash as the resolved artwork
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "existing\\cover.jpeg", contentHash: 123ul, status: ArtworkStatus.Pending, provider: "Old Provider", lastUpdateUtc: DateTime.UtcNow.AddDays(-1))];
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the identical artwork is not copied again, so the artwork storage service is not called and the stored file name is kept
        await mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("existing\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal("Artwork Provider", coverArtwork.Provider);
        Assert.NotNull(coverArtwork.LastUpdateUtc);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProviderReturnsArtwork_ShouldMarkTheCoverArtworkAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((ArtworkDto?)null);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkType.Cover, coverArtwork.ArtworkType);
        Assert.Equal(0, coverArtwork.Ordinal);
        Assert.Equal(ArtworkStatus.Failed, coverArtwork.Status);
        Assert.Null(coverArtwork.FileName);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoArtworkProviderIsConfigured_ShouldSkipTheEnrichmentWithoutMarkingBooksAsFailed()
    {
        // Arrange
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a library without artwork providers must not mark its books as failed to resolve, so the books are not even queried
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
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

    [Fact]
    public async Task ExecuteAsync_WhenReadingArtworkProviderConfigurationsFails_ShouldLogWarningAndSkipEnrichment()
    {
        // Arrange
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to read the artwork provider configurations"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a failure to read the artwork configurations is best-effort, so the enrichment is skipped without touching the books
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheConfiguredArtworkProviderIsNotRegistered_ShouldLogWarningAndSkipEnrichment()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);

        // the configuration references a plugin that is not registered in the dependency injection container
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([_artworkConfigurationEntityFixture.Create(_libraryId.Value, Guid.NewGuid(), 1)]));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheLibraryForbidsWebDownloads_ShouldSkipArtworkProvidersThatRequireWebAccess()
    {
        // Arrange
        Guid webPluginId = Guid.NewGuid();
        Guid localPluginId = Guid.NewGuid();
        IArtworkProvider webProvider = Substitute.For<IArtworkProvider>();
        webProvider.Name.Returns("Web Provider");
        webProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        webProvider.RequiresWebAccess.Returns(true);
        IArtworkProvider localProvider = Substitute.For<IArtworkProvider>();
        localProvider.Name.Returns("Local Provider");
        localProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        localProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/local-cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/local-cover.jpg").Returns(111UL);

        ServiceCollection services = new();
        services.AddKeyedSingleton(webPluginId, webProvider);
        services.AddKeyedSingleton(localPluginId, localProvider);
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        services.AddSingleton(mockBookArtworkService);
        services.AddSingleton(mockFileHashService);
        // the provider is intentionally not disposed here, so that the async service scope used by the job stays alive for the whole test
        ServiceProvider realServiceProvider = services.BuildServiceProvider();
        AsyncServiceScope asyncServiceScope = realServiceProvider.CreateAsyncScope();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);

        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", canDownloadMetadataFromWeb: false)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, webPluginId, 1),
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, localPluginId, 2)
            ]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the provider that requires access to the web is skipped when the library does not permit web downloads
        await webProvider.DidNotReceive().GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("Local Provider", coverArtwork.Provider);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCountingTheBooksNeedingArtworkFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);

        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([_artworkConfigurationEntityFixture.Create(_libraryId.Value, artworkPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to count the books that need artwork"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheBooksNeedingArtworkFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);

        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([_artworkConfigurationEntityFixture.Create(_libraryId.Value, artworkPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the books that need artwork"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheAuthorDisplayNamesFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", canDownloadMetadataFromWeb: true)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([_artworkConfigurationEntityFixture.Create(_libraryId.Value, artworkPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the author display names"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProviderReturnsArtworkAndTheBookHasAnExistingCover_ShouldMarkTheExistingCoverAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((ArtworkDto?)null);

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "existing\\cover.jpeg", contentHash: 123ul, status: ArtworkStatus.Pending, provider: "Old Provider", lastUpdateUtc: DateTime.UtcNow.AddDays(-1))];
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Failed, coverArtwork.Status);
        Assert.Equal("existing\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoringTheArtworkFailsAndTheBookHasNoExistingCover_ShouldTrackTheCoverAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Storage.Error", "Failed to store the artwork"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/cover.jpg").Returns(111UL);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Failed, coverArtwork.Status);
        Assert.Null(coverArtwork.FileName);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoringTheArtworkFailsAndTheBookHasAnExistingCover_ShouldMarkTheExistingCoverAsFailed()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Storage.Error", "Failed to store the artwork"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/cover.jpg").Returns(222UL);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "existing\\cover.jpeg", contentHash: 111ul, status: ArtworkStatus.Pending, provider: "Old Provider", lastUpdateUtc: DateTime.UtcNow.AddDays(-1))];
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Failed, coverArtwork.Status);
        Assert.Equal("existing\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheResolvedArtworkIsRemote_ShouldComputeTheContentHashOfTheStoredArtwork()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.RequiresWebAccess.Returns(true);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://example.com/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash(Arg.Any<string>()).Returns(999UL);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, artworkPluginId);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // for remote artwork, the content hash is computed on the stored copy, so it is only known after the artwork is stored
        await mockBookArtworkService.Received(1).SaveBookArtworkAsync(_libraryId.Value, book.Id, "My Library", "Frank Herbert", Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("\\media\\books\\Library\\Author\\Title\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal(999ul, coverArtwork.ContentHash);
        Assert.Equal("Artwork Provider", coverArtwork.Provider);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheBookHasAnExistingCoverAndTheArtworkDiffersFromIt_ShouldStoreItAndUpdateTheExistingCover()
    {
        // Arrange
        Guid artworkPluginId = Guid.NewGuid();
        IArtworkProvider artworkProvider = Substitute.For<IArtworkProvider>();
        artworkProvider.Name.Returns("Artwork Provider");
        artworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        artworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/cover.jpg").Returns(222UL);

        SetupRealServiceProvider(artworkPluginId, artworkProvider, mockBookArtworkService, mockFileHashService);
        // the book has no known ISBNs of its own, so the artwork lookup is built without an ISBN
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub", title: "Test Title", includeMetadata: false);
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "existing\\cover.jpeg", contentHash: 111ul, status: ArtworkStatus.Pending, provider: "Old Provider", lastUpdateUtc: DateTime.UtcNow.AddDays(-1))];
        SetupSingleBookPage(book, artworkPluginId);
        // the author of the book is not known, so its artwork directory falls back to an empty author name
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?>()));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            _libraryId.Value,
            book.Id,
            "My Library",
            string.Empty,
            "Test Title",
            Arg.Any<ArtworkDto>(),
            Arg.Any<CancellationToken>());
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("\\media\\books\\Library\\Author\\Title\\cover.jpeg", coverArtwork.FileName);
        Assert.Equal(222ul, coverArtwork.ContentHash);
        Assert.Equal("Artwork Provider", coverArtwork.Provider);
        Assert.NotNull(coverArtwork.LastUpdateUtc);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnArtworkProviderThrows_ShouldTryTheNextProvider()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IArtworkProvider firstProvider = Substitute.For<IArtworkProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ArtworkDto?>(new InvalidOperationException("The artwork provider failed")));
        IArtworkProvider secondProvider = Substitute.For<IArtworkProvider>();
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/second-cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/second-cover.jpg").Returns(333UL);

        ServiceCollection services = new();
        services.AddKeyedSingleton(firstPluginId, firstProvider);
        services.AddKeyedSingleton(secondPluginId, secondProvider);
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        services.AddSingleton(mockBookArtworkService);
        services.AddSingleton(mockFileHashService);
        // the provider is intentionally not disposed here, so that the async service scope used by the job stays alive for the whole test
        ServiceProvider realServiceProvider = services.BuildServiceProvider();
        AsyncServiceScope asyncServiceScope = realServiceProvider.CreateAsyncScope();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", canDownloadMetadataFromWeb: true)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, firstPluginId, 1),
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, secondPluginId, 2)
            ]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a failing artwork provider must not prevent the artwork of the other provider from being used
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("Second Provider", coverArtwork.Provider);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAProviderReturnsRemoteArtworkWithoutRequiringWebAccess_ShouldSkipItAndUseTheNextProvider()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IArtworkProvider firstProvider = Substitute.For<IArtworkProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.RequiresWebAccess.Returns(false);
        firstProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://example.com/cover.jpg"));
        IArtworkProvider secondProvider = Substitute.For<IArtworkProvider>();
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_artworkDtoFixture.Create(localPath: "/some/second-cover.jpg"));

        IBookArtworkService mockBookArtworkService = Substitute.For<IBookArtworkService>();
        mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("\\media\\books\\Library\\Author\\Title\\cover.jpeg"));
        IFileHashService mockFileHashService = Substitute.For<IFileHashService>();
        mockFileHashService.ComputeFileHash("/some/second-cover.jpg").Returns(444UL);

        ServiceCollection services = new();
        services.AddKeyedSingleton(firstPluginId, firstProvider);
        services.AddKeyedSingleton(secondPluginId, secondProvider);
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        services.AddSingleton(mockBookArtworkService);
        services.AddSingleton(mockFileHashService);
        // the provider is intentionally not disposed here, so that the async service scope used by the job stays alive for the whole test
        ServiceProvider realServiceProvider = services.BuildServiceProvider();
        AsyncServiceScope asyncServiceScope = realServiceProvider.CreateAsyncScope();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", canDownloadMetadataFromWeb: true)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, firstPluginId, 1),
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, secondPluginId, 2)
            ]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a provider that returns remote artwork must require web access, so its artwork is skipped and the next provider is used
        BookArtworkEntity coverArtwork = Assert.Single(book.BookArtwork);
        Assert.Equal(ArtworkStatus.Enriched, coverArtwork.Status);
        Assert.Equal("Second Provider", coverArtwork.Provider);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheJobHasChildJobs_ShouldExecuteThemAfterCompletingItsPayload()
    {
        // Arrange
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        IMediaLibraryScanJob mockChildJob = Substitute.For<IMediaLibraryScanJob>();
        mockChildJob.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _sut.AddChild(mockChildJob);
        Guid id = Guid.NewGuid();
        object input = new();

        // Act
        await _sut.ExecuteAsync(id, input, CancellationToken.None);

        // Assert
        await mockChildJob.Received(1).ExecuteAsync(id, input, Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    /// <summary>
    /// Wires a real service provider with the given artwork provider and services, so that the job resolves them from the dependency injection container.
    /// </summary>
    /// <param name="artworkPluginId">The Id of the plugin providing the artwork.</param>
    /// <param name="artworkProvider">The artwork provider to resolve.</param>
    /// <param name="bookArtworkService">The service used to store the artwork of the books.</param>
    /// <param name="fileHashService">The service used to hash the artwork.</param>
    private void SetupRealServiceProvider(Guid artworkPluginId, IArtworkProvider artworkProvider, IBookArtworkService bookArtworkService, IFileHashService fileHashService)
    {
        ServiceCollection services = new();
        services.AddKeyedSingleton(artworkPluginId, artworkProvider);
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        services.AddSingleton(bookArtworkService);
        services.AddSingleton(fileHashService);
        // the provider is intentionally not disposed here, so that the async service scope used by the job stays alive for the whole test
        ServiceProvider realServiceProvider = services.BuildServiceProvider();
        AsyncServiceScope asyncServiceScope = realServiceProvider.CreateAsyncScope();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);
    }

    /// <summary>
    /// Stubs the repositories so that the job processes a single page containing the given book, configured with the given artwork provider.
    /// </summary>
    /// <param name="book">The book whose artwork the job must resolve.</param>
    /// <param name="artworkPluginId">The Id of the plugin configured as artwork provider for the library.</param>
    private void SetupSingleBookPage(BookEntity book, Guid artworkPluginId)
    {
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, title: "My Library", canDownloadMetadataFromWeb: true)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([
                _artworkConfigurationEntityFixture.Create(_libraryId.Value, artworkPluginId, 1)
            ]));
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }
}
