#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryArtworkProviderEnabledCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtworkProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<SetLibraryArtworkProviderEnabledCommand> _mockValidator;
    private readonly SetLibraryArtworkProviderEnabledCommandHandler _sut;
    private readonly SetLibraryArtworkProviderEnabledCommandFixture _setLibraryArtworkProviderEnabledCommandFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Fixture for generating <see cref="LibraryArtworkProviderConfigurationEntity"/> test data.
    /// </summary>
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledCommandHandlerTests"/> class.
    /// </summary>
    public SetLibraryArtworkProviderEnabledCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<SetLibraryArtworkProviderEnabledCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<SetLibraryArtworkProviderEnabledCommand>())
            .Returns([]);
        _sut = new SetLibraryArtworkProviderEnabledCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationDoesNotExist_ShouldCreateEnabledConfigurationWithNextRank()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns((LibraryArtworkProviderConfigurationEntity?)null);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryArtworkProviderConfigurationEntity>());
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationDoesNotExistAndOthersExist_ShouldAppendAfterTheHighestRank()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns((LibraryArtworkProviderConfigurationEntity?)null);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryArtworkProviderConfigurationEntity>
            {
                _libraryArtworkProviderConfigurationEntityFixture.Create(
                    libraryId: libraryId,
                    pluginId: Guid.NewGuid(),
                    rank: 3,
                    isEnabled: true)
            });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: false), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => !configuration.IsEnabled && configuration.Rank == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        LibraryArtworkProviderConfigurationEntity existingConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(
            libraryId: Guid.NewGuid(),
            pluginId: Guid.NewGuid(),
            rank: 2,
            isEnabled: false);
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(existingConfiguration.LibraryId, existingConfiguration.PluginId, Arg.Any<CancellationToken>())
            .Returns(existingConfiguration);
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: existingConfiguration.LibraryId, pluginId: existingConfiguration.PluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockValidator.Validate(Arg.Any<SetLibraryArtworkProviderEnabledCommand>()).Returns([Errors.Plugins.PluginIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdCannotBeEmpty, result.FirstError);
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOwnershipPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByLibraryAndPluginIdFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get configuration"));

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpsertFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns((LibraryArtworkProviderConfigurationEntity?)null);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryArtworkProviderConfigurationEntity>());
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to upsert configuration"));

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryArtworkProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
