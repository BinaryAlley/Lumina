#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.DTO.Common;
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
