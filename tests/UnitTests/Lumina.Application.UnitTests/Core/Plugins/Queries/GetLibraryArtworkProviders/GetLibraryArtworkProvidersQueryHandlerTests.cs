#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryArtworkProvidersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkProviderConfigurationRepository;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetLibraryArtworkProvidersQuery> _mockValidator;
    private readonly GetLibraryArtworkProvidersQueryHandler _sut;
    private readonly GetLibraryArtworkProvidersQueryFixture _getLibraryArtworkProvidersQueryFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryArtworkProvidersQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryArtworkProvidersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtworkProviderConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkProviderConfigurationRepository);
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetLibraryArtworkProvidersQuery>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetLibraryArtworkProvidersQuery>()).Returns([]);

        _sut = new GetLibraryArtworkProvidersQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIdIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetLibraryArtworkProvidersQuery>()).Returns([Errors.Plugins.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockArtworkProviderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationsExist_ShouldReturnProvidersOrderedByRankWithPluginNames()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        List<LibraryArtworkProviderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(query.LibraryId, secondPluginId, 2),
            _configurationEntityFixture.Create(query.LibraryId, firstPluginId, 1)
        ];
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>(configurations));
        List<PluginEntity> plugins =
        [
            _pluginEntityFixture.Create(firstPluginId),
            _pluginEntityFixture.Create(secondPluginId)
        ];
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(plugins);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstPluginId, result.Value[0].PluginId);
        Assert.Equal(plugins[0].Name, result.Value[0].Name);
        Assert.Equal(configurations[1].Rank, result.Value[0].Rank);
        Assert.Equal(secondPluginId, result.Value[1].PluginId);
        Assert.Equal(plugins[1].Name, result.Value[1].Name);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsMissingFromDetectedPlugins_ShouldUseEmptyName()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        Guid unknownPluginId = Guid.NewGuid();
        List<LibraryArtworkProviderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(query.LibraryId, unknownPluginId, 1)
        ];
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>(configurations));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([_pluginEntityFixture.Create()]));

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderResponse response = Assert.Single(result.Value);
        Assert.Equal(unknownPluginId, response.PluginId);
        Assert.Equal(string.Empty, response.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenNoConfigurationsExist_ShouldReturnEmptyList()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([]));

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenGetConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get configurations");
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockPluginRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetPluginsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        _mockArtworkProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get plugins"));

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenOwnershipPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == query.LibraryId), Arg.Any<CancellationToken>());
        await _mockArtworkProviderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<LibraryArtworkProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtworkProviderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
