#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="BookReadingService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookReadingServiceTests
{
    private readonly IPluginManager _mockPluginManager;
    private readonly IPlugin _mockPlugin;
    private readonly IBookReader _mockReader;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryBookReaderConfigurationRepository _mockConfigurationRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly BookReadingService _sut;
    private readonly BookReaderEnablementCache _enablementCache = new();
    private readonly Guid _pluginId = Guid.NewGuid();
    private readonly Guid _bookId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();
    private readonly string _bookPath;
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

        // The resolution and enablement guards under test stop before the book file is probed, so no real file is needed.
        _bookPath = $"lumina-test-book-{Guid.NewGuid():N}.epub";

        _sut = new BookReadingService(_serviceProvider, _mockPluginManager, _enablementCache, serviceScopeFactory, Substitute.For<ILogger<BookReadingService>>());
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
    public async Task GetSectionAsync_WhenNoReaderSupportsTheFormat_ShouldReturnNoReaderAvailableError()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);

        // Act
        Result<ReadingSectionDto> result = await _sut.GetSectionAsync(_bookId, _libraryId, _bookPath, LibraryType.EBook, "section-1", shouldRenderPdfAsImages: false, shouldPreserveStyles: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.NoReaderAvailable, result.FirstError);
        await _mockReader.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
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
}
