#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
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
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Contains unit tests for the <see cref="BooksMediaLibraryScanArtworkEnricher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksMediaLibraryScanArtworkEnricherTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkProviderConfigurationRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IArtworkProvider _mockArtworkProvider;
    private readonly IBookArtworkService _mockBookArtworkService;
    private readonly IFileHashService _mockFileHashService;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<BooksMediaLibraryScanArtworkEnricher> _mockLogger;
    private readonly BooksMediaLibraryScanArtworkEnricher _sut;
    private readonly ServiceProvider _serviceProvider;
    private readonly Guid _pluginId = Guid.NewGuid();
    private readonly LibraryId _libraryId;
    private readonly ScanId _scanId;
    private readonly UserId _userId;

    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _artworkConfigurationFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryScanArtworkEnricherTests"/> class.
    /// </summary>
    public BooksMediaLibraryScanArtworkEnricherTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockArtworkProviderConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkProviderConfigurationRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        _mockArtworkProvider = Substitute.For<IArtworkProvider>();
        _mockArtworkProvider.Name.Returns("Test Provider");
        _mockArtworkProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        _mockArtworkProvider.RequiresWebAccess.Returns(true);

        _mockBookArtworkService = Substitute.For<IBookArtworkService>();
        _mockFileHashService = Substitute.For<IFileHashService>();
        _mockFileHashService.ComputeFileHash(Arg.Any<string>()).Returns(123UL);
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockLogger = Substitute.For<ILogger<BooksMediaLibraryScanArtworkEnricher>>();

        // The enricher resolves the keyed artwork providers from the scope of the service provider, so a real container is used to honor the keyed registration.
        ServiceCollection services = new();
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        services.AddSingleton(_mockBookArtworkService);
        services.AddSingleton(_mockFileHashService);
        services.AddKeyedSingleton<IArtworkProvider>(_pluginId, _mockArtworkProvider);
        _serviceProvider = services.BuildServiceProvider();
        _sut = new BooksMediaLibraryScanArtworkEnricher(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), _mockLogger);

        _libraryId = _libraryIdFixture.Create();
        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
    }

    [Fact]
    public void SupportedLibraryType_WhenCalled_ShouldReturnBook()
    {
        // Act
        LibraryType result = _sut.SupportedLibraryType;

        // Assert
        Assert.Equal(LibraryType.Book, result);
    }

    [Fact]
    public async Task EnrichAsync_WhenNoArtworkProviderIsConfigured_ShouldSkipTheEnrichment()
    {
        // Arrange
        SetupLibrary();
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheArtworkConfigurationsCannotBeRead_ShouldSkipTheEnrichment()
    {
        // Arrange
        SetupLibrary();
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Provider.Error", "Failed to read the configurations"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheLibraryDisallowsWebDownloads_ShouldSkipProvidersThatRequireWebAccess()
    {
        // Arrange
        SetupLibrary(canDownloadMetadataFromWeb: false);
        SetupConfiguration();
        _mockArtworkProvider.RequiresWebAccess.Returns(true);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookRepository.DidNotReceive().GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheLibraryDisallowsWebDownloadsButTheProviderIsLocal_ShouldStillResolve()
    {
        // Arrange
        SetupLibrary(canDownloadMetadataFromWeb: false);
        SetupConfiguration();
        _mockArtworkProvider.RequiresWebAccess.Returns(false);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockArtworkProvider.Received(1).GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheBookHasArtwork_ShouldStoreAndPersistEnrichedArtwork()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://covers.openlibrary.org/b/id/1-L.jpg", type: ArtworkType.Cover, ordinal: 0)]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            book.LibraryId,
            book.Id,
            Arg.Any<string>(),
            Arg.Any<string>(),
            book.Title,
            Arg.Is<ArtworkDto>(artwork => artwork.Type == ArtworkType.Cover && artwork.Ordinal == 0),
            Arg.Any<CancellationToken>());
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkType.Cover, artwork.ArtworkType);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(ArtworkStatus.Enriched, artwork.Status);
        Assert.Equal("Test Provider", artwork.Provider);
        Assert.Equal("/media/books/cover.jpg", artwork.FileName);
        Assert.Equal(123UL, artwork.ContentHash);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenAProviderAnswersWithNoArtwork_ShouldMarkTheBookFailed()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkStatus.Failed, artwork.Status);
        Assert.Null(artwork.FileName);
        Assert.Null(artwork.Provider);
    }

    [Fact]
    public async Task EnrichAsync_WhenEveryProviderFails_ShouldMarkTheBookFailed()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<ArtworkDto>>(new HttpRequestException("Transient failure.")));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkStatus.Failed, artwork.Status);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheProviderReturnsRemoteArtworkButDoesNotRequireWebAccess_ShouldMarkTheBookFailed()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        _mockArtworkProvider.RequiresWebAccess.Returns(false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://covers.openlibrary.org/b/id/1-L.jpg")]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkStatus.Failed, artwork.Status);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheStoredArtworkIsIdentical_ShouldKeepItAndMarkItEnriched()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        _mockArtworkProvider.RequiresWebAccess.Returns(false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        BookArtworkEntity existingArtwork = _bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, contentHash: 123UL, status: ArtworkStatus.Enriched);
        book.Artwork = [existingArtwork];
        SetupBooks(book);
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: "/media/books/local.jpg", remoteUrl: null, type: ArtworkType.Cover, ordinal: 0)]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>());
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkStatus.Enriched, artwork.Status);
        Assert.Equal("Test Provider", artwork.Provider);
    }

    [Fact]
    public async Task EnrichAsync_WhenStoringTheArtworkFails_ShouldMarkTheBookFailed()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Artwork.Error", "Failed to store the artwork"));
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://covers.openlibrary.org/b/id/1-L.jpg")]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        BookArtworkEntity artwork = Assert.Single(book.Artwork);
        Assert.Equal(ArtworkStatus.Failed, artwork.Status);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheBookCountCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to count the books"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheBooksPageCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to read the books"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheAuthorsCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to read the authors"));

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
        SetupLibrary();
        SetupConfiguration();
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        SetupSuccessStore();
        _mockArtworkProvider.GetArtworkAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ArtworkDto>>([_artworkDtoFixture.Create(localPath: null, remoteUrl: "https://covers.openlibrary.org/b/id/1-L.jpg")]));
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
        SetupLibrary();
        SetupConfiguration();
        SetupBooks();

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
            libraryType: LibraryType.Book,
            canDownloadMetadataFromWeb: canDownloadMetadataFromWeb);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the artwork provider configuration reader with a single enabled configuration for the test plugin.
    /// </summary>
    private void SetupConfiguration()
    {
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([_artworkConfigurationFixture.Create(_libraryId.Value, _pluginId, 0)]));
    }

    /// <summary>
    /// Stubs the book reader with a single page holding the provided books.
    /// </summary>
    /// <param name="books">The books of the page.</param>
    private void SetupBooks(params BookEntity[] books)
    {
        _mockBookRepository.GetBooksNeedingArtworkCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(books.Length));
        _mockBookRepository.GetBooksNeedingArtworkAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<BookEntity>>(books),
                Result.From<IReadOnlyList<BookEntity>>([]));
        Dictionary<Guid, string?> authors = books.ToDictionary(book => book.Id, book => (string?)"Frank Herbert");
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(authors));
    }

    /// <summary>
    /// Stubs the artwork storage so that a book stores its artwork successfully.
    /// </summary>
    private void SetupSuccessStore()
    {
        _mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ArtworkDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/books/cover.jpg"));
    }
}
