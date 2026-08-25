#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryArtworkProvidersCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtworkProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<ReorderLibraryArtworkProvidersCommand> _mockValidator;
    private readonly ReorderLibraryArtworkProvidersCommandHandler _sut;
    private readonly LibraryArtworkProviderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly ReorderLibraryArtworkProvidersCommandFixture _reorderLibraryArtworkProvidersCommandFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersCommandHandlerTests"/> class.
    /// </summary>
    public ReorderLibraryArtworkProvidersCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<ReorderLibraryArtworkProvidersCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<ReorderLibraryArtworkProvidersCommand>())
            .Returns([]);
        _sut = new ReorderLibraryArtworkProvidersCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldAssignRanksInTheProvidedOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity firstProvider = _configurationEntityFixture.Create(libraryId, Guid.NewGuid(), 2);
        LibraryArtworkProviderConfigurationEntity secondProvider = _configurationEntityFixture.Create(libraryId, Guid.NewGuid(), 1);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryArtworkProviderConfigurationEntity> { firstProvider, secondProvider });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [secondProvider.PluginId, firstProvider.PluginId]), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == secondProvider.PluginId && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == firstProvider.PluginId && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAProvidedPluginIsNotConfigured_ShouldSkipIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity configuration = _configurationEntityFixture.Create(libraryId, Guid.NewGuid(), 1);
        Guid unknownPluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryArtworkProviderConfigurationEntity> { configuration });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [unknownPluginId, configuration.PluginId]), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(candidate => candidate.PluginId == configuration.PluginId && candidate.Rank == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockValidator.Validate(Arg.Any<ReorderLibraryArtworkProvidersCommand>()).Returns([Errors.Plugins.PluginIdsListCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdsListCannotBeEmpty, result.FirstError);
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOwnershipPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get configurations"));

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryArtworkProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
