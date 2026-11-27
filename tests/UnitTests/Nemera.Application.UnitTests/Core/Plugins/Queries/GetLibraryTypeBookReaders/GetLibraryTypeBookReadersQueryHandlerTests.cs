#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeBookReaders;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryTypeBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeBookReadersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaLibraryProviderConfigurationStore _mockProviderConfigurationStore;
    private readonly IBookReaderRegistry _mockBookReaderRegistry;
    private readonly IValidator<GetLibraryTypeBookReadersQuery> _mockValidator;
    private readonly GetLibraryTypeBookReadersQueryHandler _sut;
    private readonly GetLibraryTypeBookReadersQueryFixture _getLibraryTypeBookReadersQueryFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly PaginatedResultDtoFixture<PluginEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryTypeBookReadersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockProviderConfigurationStore = Substitute.For<IMediaLibraryProviderConfigurationStore>();
        _mockBookReaderRegistry = Substitute.For<IBookReaderRegistry>();
        _mockValidator = Substitute.For<IValidator<GetLibraryTypeBookReadersQuery>>();

        // Default stubs: the query is valid, the current user is authenticated, no reader supports the library type, and no plugin is detected.
        _mockValidator.Validate(Arg.Any<GetLibraryTypeBookReadersQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(Guid.NewGuid());
        _mockProviderConfigurationStore.GetSupportedBookReaderPluginIds(Arg.Any<LibraryType>()).Returns([]);
        _mockBookReaderRegistry.GetSupportedExtensionsByPluginId().Returns(new Dictionary<Guid, IReadOnlyList<string>>());
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 0, count: 0, numberOfPages: 1)));

        _sut = new GetLibraryTypeBookReadersQueryHandler(_mockCurrentUserService, _mockProviderConfigurationStore, _mockBookReaderRegistry, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorWithoutReadingPlugins()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetLibraryTypeBookReadersQuery>()).Returns([Errors.Library.LibraryTypeCannotBeNull]);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryTypeCannotBeNull, result.FirstError);
        _mockProviderConfigurationStore.DidNotReceive().GetSupportedBookReaderPluginIds(Arg.Any<LibraryType>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        _mockProviderConfigurationStore.DidNotReceive().GetSupportedBookReaderPluginIds(Arg.Any<LibraryType>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenReadersSupportTheLibraryType_ShouldReturnThemDisabledWithTheirNamesAndExtensions()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create(libraryType: nameof(LibraryType.Book));
        Guid epubPluginId = Guid.NewGuid();
        Guid pdfPluginId = Guid.NewGuid();
        _mockProviderConfigurationStore.GetSupportedBookReaderPluginIds(LibraryType.Book).Returns([epubPluginId, pdfPluginId]);
        List<PluginEntity> plugins = [_pluginEntityFixture.Create(pdfPluginId, "PDF Reader"), _pluginEntityFixture.Create(epubPluginId, "EPUB Reader")];
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: plugins, currentPage: 1, perPage: plugins.Count, count: plugins.Count, numberOfPages: 1)));
        _mockBookReaderRegistry.GetSupportedExtensionsByPluginId().Returns(new Dictionary<Guid, IReadOnlyList<string>>
        {
            [epubPluginId] = [".epub"],
            [pdfPluginId] = [".pdf"]
        });

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(epubPluginId, result.Value[0].PluginId);
        Assert.Equal("EPUB Reader", result.Value[0].Name);
        Assert.Equal(".epub", Assert.Single(result.Value[0].SupportedExtensions));
        Assert.False(result.Value[0].IsEnabled);
        Assert.Equal(pdfPluginId, result.Value[1].PluginId);
        Assert.Equal("PDF Reader", result.Value[1].Name);
        Assert.Equal(".pdf", Assert.Single(result.Value[1].SupportedExtensions));
        Assert.False(result.Value[1].IsEnabled);
    }

    [Fact]
    public async Task HandleAsync_WhenReaderIsMissingFromDetectedPluginsAndRegistry_ShouldUseEmptyNameAndNoExtensions()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create(libraryType: nameof(LibraryType.Book));
        Guid unknownPluginId = Guid.NewGuid();
        _mockProviderConfigurationStore.GetSupportedBookReaderPluginIds(LibraryType.Book).Returns([unknownPluginId]);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderResponse response = Assert.Single(result.Value);
        Assert.Equal(unknownPluginId, response.PluginId);
        Assert.Equal(string.Empty, response.Name);
        Assert.Empty(response.SupportedExtensions);
    }

    [Fact]
    public async Task HandleAsync_WhenNoReaderSupportsTheLibraryType_ShouldReturnEmptyList()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create(libraryType: nameof(LibraryType.Music));

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        _mockProviderConfigurationStore.Received(1).GetSupportedBookReaderPluginIds(LibraryType.Music);
    }

    [Fact]
    public async Task HandleAsync_WhenGetPluginsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryTypeBookReadersQuery query = _getLibraryTypeBookReadersQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get plugins");
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
