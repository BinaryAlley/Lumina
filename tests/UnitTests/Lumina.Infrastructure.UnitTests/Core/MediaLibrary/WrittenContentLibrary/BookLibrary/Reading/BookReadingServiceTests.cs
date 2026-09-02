#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="BookReadingService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookReadingServiceTests : IDisposable
{
    private readonly IPluginManager _mockPluginManager;
    private readonly IPlugin _mockPlugin;
    private readonly IBookReader _mockReader;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryBookReaderConfigurationRepository _mockConfigurationRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly BookReadingService _sut;
    private readonly TestBookReaderEnablementCache _enablementCache = new();
    private readonly Guid _pluginId = Guid.NewGuid();
    private readonly Guid _bookId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();
    private readonly string _bookPath;
    private readonly ReadingDocumentDtoFixture _readingDocumentDtoFixture = new();
    private readonly ReadingSpineItemDtoFixture _readingSpineItemDtoFixture = new();
    private readonly ReadingResourceInfoDtoFixture _readingResourceInfoDtoFixture = new();
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookReadingServiceTests"/> class.
    /// </summary>
    public BookReadingServiceTests()
    {
        _mockReader = Substitute.For<IBookReader>();
        _mockReader.SupportedExtensions.Returns([".epub"]);
        _mockReader.SupportedLibraryTypes.Returns([LibraryType.EBook]);
        _mockPlugin = Substitute.For<IPlugin>();
        _mockPlugin.Id.Returns(_pluginId);
        _mockPlugin.Name.Returns("Test Reader");
        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockPluginManager.GetPlugins().Returns([_mockPlugin]);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryBookReaderConfigurationRepository>();
        _mockUnitOfWork.LibraryBookReaderConfigurationRepository.Returns(_mockConfigurationRepository);
        LibraryBookReaderConfigurationEntity enabledConfiguration = _configurationEntityFixture.Create(libraryId: _libraryId, pluginId: _pluginId, isEnabled: true);
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(_libraryId, _pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(enabledConfiguration));

        ServiceCollection services = new();
        services.AddKeyedSingleton(_pluginId, _mockReader);
        services.AddSingleton(_mockUnitOfWork);
        _serviceProvider = services.BuildServiceProvider();
        IServiceScopeFactory serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        _bookPath = Path.Combine(Path.GetTempPath(), $"lumina-test-book-{Guid.NewGuid():N}.epub");
        File.WriteAllText(_bookPath, "test epub content");

        _sut = new BookReadingService(_serviceProvider, _mockPluginManager, _enablementCache, serviceScopeFactory, Substitute.For<ILogger<BookReadingService>>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenBookIsNotCached_ShouldExtractAndReturnTheManifest()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(document.Title, result.Value.Title);
        Assert.Equal(document.Author, result.Value.Author);
        await _mockReader.Received(1).OpenAsync(_bookPath, Arg.Any<string>(), Arg.Any<bool>(), cancellationToken);
    }

    [Fact]
    public async Task GetManifestAsync_WhenFileIsUnchanged_ShouldReuseTheCachedDocument()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);
        Result<ReadingManifestResponse> secondResult = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);

        // Assert
        Assert.False(secondResult.IsFailure);
        await _mockReader.Received(1).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenFileChanged_ShouldReExtractTheBook()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);
        File.WriteAllText(_bookPath, "modified epub content, longer than before");
        Result<ReadingManifestResponse> secondResult = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);

        // Assert
        Assert.False(secondResult.IsFailure);
        await _mockReader.Received(2).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenRenderingPreferenceDiffers_ShouldExtractTheBookTwice()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);
        Result<ReadingManifestResponse> secondResult = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: true, cancellationToken);

        // Assert
        Assert.False(secondResult.IsFailure);
        await _mockReader.Received(2).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockReader.Received(1).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), false, Arg.Any<CancellationToken>());
        await _mockReader.Received(1).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenBothRenderingPreferencesAreExtracted_ShouldKeepTheirWorkingDirectoriesSeparate()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: true, cancellationToken);
        // The text extraction must survive the image extraction of the same book, and still be served from the cache.
        Result<ReadingManifestResponse> thirdResult = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, cancellationToken);

        // Assert
        Assert.False(thirdResult.IsFailure);
        string textWorkingDirectory = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "text");
        string imageWorkingDirectory = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "images");
        Assert.True(File.Exists(Path.Combine(textWorkingDirectory, "sections", "0.html")));
        Assert.True(File.Exists(Path.Combine(imageWorkingDirectory, "sections", "0.html")));
        await _mockReader.Received(2).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenConcurrentRequests_ShouldShareASingleExtraction()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        TaskCompletionSource<bool> extractionStarted = new();
        TaskCompletionSource<bool> allowExtractionToFinish = new();
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                extractionStarted.SetResult(true);
                await allowExtractionToFinish.Task;
                return document;
            }));

        // Act
        Task<Result<ReadingManifestResponse>> firstTask = _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);
        await extractionStarted.Task;
        Task<Result<ReadingManifestResponse>> secondTask = _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);
        allowExtractionToFinish.SetResult(true);
        Result<ReadingManifestResponse> firstResult = await firstTask;
        Result<ReadingManifestResponse> secondResult = await secondTask;

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        await _mockReader.Received(1).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenNoReaderSupportsTheFormat_ShouldReturnNoReaderAvailableError()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.NoReaderAvailable, result.FirstError);
        await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderIsDisabledForTheLibrary_ShouldReturnReaderDisabledError()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(_libraryId, _pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(null));

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.ReaderDisabled, result.FirstError);
        await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionExists_ShouldReturnSanitizedContent()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("section-1", result.Value.LocationRef);
        Assert.Contains("Chapter 1", result.Value.ContentHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionDoesNotExist_ShouldReturnSectionNotFoundError()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "missing-section", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.SectionNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionFileDoesNotExist_ShouldReturnSectionNotFoundError()
    {
        // Arrange
        ReadingDocumentDto document = _readingDocumentDtoFixture.Create(
            title: "Test Book",
            tableOfContents: [],
            spine:
            [
                _readingSpineItemDtoFixture.Create(locationRef: "section-1", title: "Chapter 1", relativeSectionFilePath: "sections/missing.html")
            ],
            resources: new Dictionary<string, ReadingResourceInfoDto>()
        );
        StubOpenAsyncToExtract(document);

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.SectionNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionPathEscapesTheWorkingDirectory_ShouldReturnSectionNotFoundError()
    {
        // Arrange
        ReadingDocumentDto document = _readingDocumentDtoFixture.Create(
            title: "Test Book",
            tableOfContents: [],
            spine:
            [
                _readingSpineItemDtoFixture.Create(locationRef: "section-1", title: "Chapter 1", relativeSectionFilePath: "../../escape.html")
            ],
            resources: new Dictionary<string, ReadingResourceInfoDto>()
        );
        StubOpenAsyncToExtract(document);

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.SectionNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetResourceAsync_WhenResourceDoesNotExistInTheDocument_ShouldReturnResourceNotFoundError()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "missing-key", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.ResourceNotFound, result.FirstError);
        await _mockReader.DidNotReceive().GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenResourcePathEscapesTheWorkingDirectory_ShouldReturnResourceNotFoundError()
    {
        // Arrange
        ReadingDocumentDto document = _readingDocumentDtoFixture.Create(
            title: "Test Book",
            tableOfContents: [],
            spine: [],
            resources: new Dictionary<string, ReadingResourceInfoDto>
            {
                ["escaping"] = _readingResourceInfoDtoFixture.Create(relativeFilePath: "../../escape.png", mimeType: "image/png")
            }
        );
        StubOpenAsyncToExtract(document);

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "escaping", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.ResourceNotFound, result.FirstError);
        await _mockReader.DidNotReceive().GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenResourceFileAlreadyExists_ShouldServeItFromDiskWithoutCallingTheReader()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);
        string workingDirectory = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "text");
        string resourceFilePath = Path.Combine(workingDirectory, "resources", "cover.png");
        Directory.CreateDirectory(Path.GetDirectoryName(resourceFilePath)!);
        File.WriteAllBytes(resourceFilePath, [1, 2, 3]);

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Value.Data);
        Assert.Equal("image/png", result.Value.MimeType);
        await _mockReader.DidNotReceive().GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenResourceFileDoesNotExist_ShouldProduceItAndCacheItToDisk()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        byte[] producedResource = [4, 5, 6];
        _mockReader.GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), "cover", Arg.Any<CancellationToken>()).Returns(producedResource);

        // Act
        Result<ReadingResourceDataDto> firstResult = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);
        Result<ReadingResourceDataDto> secondResult = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.Equal(new byte[] { 4, 5, 6 }, firstResult.Value.Data);
        Assert.Equal("image/png", firstResult.Value.MimeType);
        Assert.False(secondResult.IsFailure);
        Assert.Equal(new byte[] { 4, 5, 6 }, secondResult.Value.Data);
        Assert.Equal("image/png", secondResult.Value.MimeType);
        await _mockReader.Received(1).GetResourceAsync(_bookPath, Arg.Any<string>(), "cover", Arg.Any<CancellationToken>());
        string cachedFilePath = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "text", "resources", "cover.png");
        Assert.True(File.Exists(cachedFilePath));
        Assert.Equal(new byte[] { 4, 5, 6 }, File.ReadAllBytes(cachedFilePath));
    }

    [Fact]
    public async Task GetResourceAsync_WhenReaderIsDisabledForTheLibrary_ShouldReturnReaderDisabledError()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(_libraryId, _pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(null));
        // The resource already exists on disk, from an earlier extraction, but the enablement is read on every request, so it must not be served.
        string workingDirectory = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "text");
        string resourceFilePath = Path.Combine(workingDirectory, "resources", "cover.png");
        Directory.CreateDirectory(Path.GetDirectoryName(resourceFilePath)!);
        File.WriteAllBytes(resourceFilePath, [1, 2, 3]);

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.ReaderDisabled, result.FirstError);
        await _mockReader.DidNotReceive().GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenEnablementCacheSaysDisabledAndResourceFileAlreadyExists_ShouldNotServeTheResource()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        // The reader was disabled after the book was extracted, so the enablement cache already knows it; the request must be cut
        // off before the already extracted resource on disk is considered, and without consulting the database again.
        _enablementCache.Set(_libraryId, _pluginId, isEnabled: false);
        string workingDirectory = Path.Combine(ReadingCachePaths.GetRootDirectory(), _bookId.ToString("N"), "text");
        string resourceFilePath = Path.Combine(workingDirectory, "resources", "cover.png");
        Directory.CreateDirectory(Path.GetDirectoryName(resourceFilePath)!);
        File.WriteAllBytes(resourceFilePath, [1, 2, 3]);

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.ReaderDisabled, result.FirstError);
        await _mockReader.DidNotReceive().GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenReaderThrowsWhileProducingTheResource_ShouldReturnResourceProductionFailedError()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        _mockReader.GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), "cover", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<byte[]>(new InvalidOperationException("The page renderer failed")));

        // Act
        Result<ReadingResourceDataDto> result = await _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reading.ResourceProductionFailed", result.FirstError.Code);
        await _mockReader.Received(1).GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), "cover", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResourceAsync_WhenReaderProductionIsCancelled_ShouldPropagateTheCancellation()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        _mockReader.GetResourceAsync(Arg.Any<string>(), Arg.Any<string>(), "cover", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<byte[]>(new OperationCanceledException()));

        // Act
        Task<Result<ReadingResourceDataDto>> operationTask = _sut.GetResourceAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "cover", shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task GetSectionAsync_WhenStylesArePreserved_ShouldKeepTheStyleAttributeInTheContent()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtractStyledContent(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: true, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Contains("style", result.Value.ContentHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSectionAsync_WhenStylesAreNotPreserved_ShouldStripTheStyleAttributeFromTheContent()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtractStyledContent(document);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.DoesNotContain("style", result.Value.ContentHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("color:red", result.Value.ContentHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter 1", result.Value.ContentHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenReaderIsAvailableAndEnabled_ShouldReportAvailable()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<ReadingAvailabilityResponse> result = await _sut.GetAvailabilityAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.IsAvailable);
        Assert.Null(result.Value.ErrorCode);
        await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenNoReaderSupportsTheFormat_ShouldReportUnavailableWithNoReaderAvailable()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);

        // Act
        Result<ReadingAvailabilityResponse> result = await _sut.GetAvailabilityAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.IsAvailable);
        Assert.Equal(nameof(Errors.Reading.NoReaderAvailable), result.Value.ErrorCode);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenReaderIsDisabled_ShouldReportUnavailableWithReaderDisabled()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(_libraryId, _pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(null));

        // Act
        Result<ReadingAvailabilityResponse> result = await _sut.GetAvailabilityAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.IsAvailable);
        Assert.Equal(nameof(Errors.Reading.ReaderDisabled), result.Value.ErrorCode);
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderOpenThrowsFileNotFoundException_ShouldReturnBookFileNotFoundError()
    {
        // Arrange
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ReadingDocumentDto>(new FileNotFoundException("book missing")));

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.BookFileNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderOpenThrowsDirectoryNotFoundException_ShouldReturnBookFileNotFoundError()
    {
        // Arrange
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ReadingDocumentDto>(new DirectoryNotFoundException("directory missing")));

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.BookFileNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderOpenThrowsOtherException_ShouldReturnExtractionFailedError()
    {
        // Arrange
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ReadingDocumentDto>(new InvalidDataException("broken book")));

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reading.ExtractionFailed", result.FirstError.Code);
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderOpenIsCancelled_ShouldPropagateTheCancellation()
    {
        // Arrange
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ReadingDocumentDto>(new OperationCanceledException()));

        // Act
        Task<Result<ReadingManifestResponse>> operationTask = _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task GetManifestAsync_WhenBookFileIsDeletedAfterExtraction_ShouldReExtractAndFail()
    {
        // Arrange
        ReadingDocumentDto document = CreateExtractableDocument();
        StubOpenAsyncToExtract(document);
        await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ReadingDocumentDto>(new FileNotFoundException("book missing")));
        File.Delete(_bookPath);

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.BookFileNotFound, result.FirstError);
        await _mockReader.Received(2).OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenConfigurationsReadFails_ShouldReturnError()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(_libraryId, _pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Reading.Error", "Failed to read the configuration"));

        // Act
        Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Reading.Error", result.FirstError.Code);
        await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderMatchesExtensionCaseInsensitively_ShouldResolveIt()
    {
        // Arrange
        string uppercaseExtensionPath = Path.Combine(Path.GetTempPath(), $"lumina-test-book-{Guid.NewGuid():N}.EPUB");
        File.WriteAllText(uppercaseExtensionPath, "test epub content");
        try
        {
            ReadingDocumentDto document = CreateExtractableDocument();
            StubOpenAsyncToExtract(document);

            // Act
            Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, uppercaseExtensionPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
        }
        finally
        {
            File.Delete(uppercaseExtensionPath);
        }
    }

    [Fact]
    public async Task GetManifestAsync_WhenReaderSupportsTheLibraryTypeButNotTheExtension_ShouldReturnNoReaderAvailableError()
    {
        // Arrange
        string unsupportedExtensionPath = Path.Combine(Path.GetTempPath(), $"lumina-test-book-{Guid.NewGuid():N}.txt");
        File.WriteAllText(unsupportedExtensionPath, "test content");
        try
        {
            // Act
            Result<ReadingManifestResponse> result = await _sut.GetManifestAsync(_bookId, _libraryId, unsupportedExtensionPath, LibraryType.EBook, shouldRenderPdfAsImages: false, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.Reading.NoReaderAvailable, result.FirstError);
            await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
        finally
        {
            File.Delete(unsupportedExtensionPath);
        }
    }

    /// <summary>
    /// Creates a reading document whose spine and resources are extractable into the working directory.
    /// </summary>
    /// <returns>The created reading document.</returns>
    private ReadingDocumentDto CreateExtractableDocument()
    {
        return _readingDocumentDtoFixture.Create(
            title: "Test Book",
            author: "Test Author",
            tableOfContents: [],
            spine:
            [
                _readingSpineItemDtoFixture.Create(locationRef: "section-1", title: "Chapter 1", relativeSectionFilePath: "sections/0.html")
            ],
            resources: new Dictionary<string, ReadingResourceInfoDto>
            {
                ["cover"] = _readingResourceInfoDtoFixture.Create(relativeFilePath: "resources/cover.png", mimeType: "image/png")
            },
            hasTextContent: true
        );
    }

    /// <summary>
    /// Stubs the reader's OpenAsync to create the extractable files of the document into the working directory.
    /// </summary>
    /// <param name="document">The document returned by the extraction.</param>
    private void StubOpenAsyncToExtract(ReadingDocumentDto document)
    {
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                string workingDirectory = callInfo.ArgAt<string>(1);
                Directory.CreateDirectory(Path.Combine(workingDirectory, "sections"));
                File.WriteAllText(Path.Combine(workingDirectory, "sections", "0.html"), "<section><h1>Chapter 1</h1><p>Some text.</p></section>");
                return Task.FromResult(document);
            });
    }

    /// <summary>
    /// Stubs the reader's OpenAsync to create a section whose content carries a style attribute, so that the sanitization
    /// preference of the service is observable.
    /// </summary>
    /// <param name="document">The document returned by the extraction.</param>
    private void StubOpenAsyncToExtractStyledContent(ReadingDocumentDto document)
    {
        _mockReader.OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                string workingDirectory = callInfo.ArgAt<string>(1);
                Directory.CreateDirectory(Path.Combine(workingDirectory, "sections"));
                File.WriteAllText(Path.Combine(workingDirectory, "sections", "0.html"), "<section><h1>Chapter 1</h1><p style=\"color:red\">Some text.</p></section>");
                return Task.FromResult(document);
            });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(ReadingCachePaths.GetRootDirectory()))
            Directory.Delete(ReadingCachePaths.GetRootDirectory(), recursive: true);
        if (File.Exists(_bookPath))
            File.Delete(_bookPath);
        (_serviceProvider as IDisposable)?.Dispose();
    }

    /// <summary>
    /// In-memory test double for the <see cref="IBookReaderEnablementCache"/> used by the service under test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestBookReaderEnablementCache : IBookReaderEnablementCache
    {
        private readonly Dictionary<(Guid LibraryId, Guid PluginId), bool> _enablements = [];

        /// <inheritdoc/>
        public bool? Get(Guid libraryId, Guid pluginId)
        {
            return _enablements.TryGetValue((libraryId, pluginId), out bool isEnabled) ? isEnabled : null;
        }

        /// <inheritdoc/>
        public void Set(Guid libraryId, Guid pluginId, bool isEnabled)
        {
            _enablements[(libraryId, pluginId)] = isEnabled;
        }

        /// <inheritdoc/>
        public void Invalidate(Guid libraryId, Guid pluginId)
        {
            _enablements.Remove((libraryId, pluginId));
        }

        /// <inheritdoc/>
        public void InvalidateLibrary(Guid libraryId)
        {
            foreach (Guid cachedPluginId in _enablements.Keys.Where(cachedPair => cachedPair.LibraryId == libraryId).Select(cachedPair => cachedPair.PluginId).ToList())
                _enablements.Remove((libraryId, cachedPluginId));
        }

        /// <inheritdoc/>
        public void InvalidatePlugin(Guid pluginId)
        {
            foreach (Guid cachedLibraryId in _enablements.Keys.Where(cachedPair => cachedPair.PluginId == pluginId).Select(cachedPair => cachedPair.LibraryId).ToList())
                _enablements.Remove((cachedLibraryId, pluginId));
        }
    }
}
