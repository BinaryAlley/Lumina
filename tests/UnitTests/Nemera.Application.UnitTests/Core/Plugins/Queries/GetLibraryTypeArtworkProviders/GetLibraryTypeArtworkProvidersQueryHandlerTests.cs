#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeArtworkProvidersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaLibraryProviderConfigurationStore _mockProviderConfigurationStore;
    private readonly IValidator<GetLibraryTypeArtworkProvidersQuery> _mockValidator;
    private readonly GetLibraryTypeArtworkProvidersQueryHandler _sut;
    private readonly GetLibraryTypeArtworkProvidersQueryFixture _getLibraryTypeArtworkProvidersQueryFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly PaginatedResultDtoFixture<PluginEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryTypeArtworkProvidersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockProviderConfigurationStore = Substitute.For<IMediaLibraryProviderConfigurationStore>();
        _mockValidator = Substitute.For<IValidator<GetLibraryTypeArtworkProvidersQuery>>();

        // Default stubs: the query is valid, the current user is authenticated, no plugin supports the library type, and no plugin is detected.
        _mockValidator.Validate(Arg.Any<GetLibraryTypeArtworkProvidersQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(Guid.NewGuid());
        _mockProviderConfigurationStore.GetSupportedArtworkProviderPluginIds(Arg.Any<LibraryType>()).Returns([]);
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 0, count: 0, numberOfPages: 1)));

        _sut = new GetLibraryTypeArtworkProvidersQueryHandler(_mockCurrentUserService, _mockProviderConfigurationStore, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorWithoutReadingPlugins()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetLibraryTypeArtworkProvidersQuery>()).Returns([Errors.Library.UnknownLibraryType]);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.UnknownLibraryType, result.FirstError);
        _mockProviderConfigurationStore.DidNotReceive().GetSupportedArtworkProviderPluginIds(Arg.Any<LibraryType>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        _mockProviderConfigurationStore.DidNotReceive().GetSupportedArtworkProviderPluginIds(Arg.Any<LibraryType>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPluginsSupportTheLibraryType_ShouldReturnThemDisabledAndRankedInTheSupportedOrder()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create(libraryType: nameof(LibraryType.Book));
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        _mockProviderConfigurationStore.GetSupportedArtworkProviderPluginIds(LibraryType.Book).Returns([alphaPluginId, betaPluginId]);
        List<PluginEntity> plugins = [_pluginEntityFixture.Create(betaPluginId, "Beta"), _pluginEntityFixture.Create(alphaPluginId, "Alpha")];
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: plugins, currentPage: 1, perPage: plugins.Count, count: plugins.Count, numberOfPages: 1)));

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(new LibraryArtworkProviderResponse(alphaPluginId, "Alpha", false, 1), result.Value[0]);
        Assert.Equal(new LibraryArtworkProviderResponse(betaPluginId, "Beta", false, 2), result.Value[1]);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryTypeCasingDiffers_ShouldStillResolveTheLibraryType()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create(libraryType: "music");

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockProviderConfigurationStore.Received(1).GetSupportedArtworkProviderPluginIds(LibraryType.Music);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsMissingFromDetectedPlugins_ShouldUseEmptyName()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create(libraryType: nameof(LibraryType.Book));
        Guid unknownPluginId = Guid.NewGuid();
        _mockProviderConfigurationStore.GetSupportedArtworkProviderPluginIds(LibraryType.Book).Returns([unknownPluginId]);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderResponse response = Assert.Single(result.Value);
        Assert.Equal(unknownPluginId, response.PluginId);
        Assert.Equal(string.Empty, response.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPluginSupportsTheLibraryType_ShouldReturnEmptyList()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create();

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenGetPluginsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get plugins");
        _mockPluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
