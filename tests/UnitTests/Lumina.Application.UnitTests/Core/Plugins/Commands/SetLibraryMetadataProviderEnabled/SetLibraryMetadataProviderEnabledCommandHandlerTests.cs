#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryMetadataProviderEnabledCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<SetLibraryMetadataProviderEnabledCommand> _mockValidator;
    private readonly SetLibraryMetadataProviderEnabledCommandHandler _sut;
    private readonly SetLibraryMetadataProviderEnabledCommandFixture _setLibraryMetadataProviderEnabledCommandFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Fixture for generating <see cref="LibraryMetadataProviderConfigurationEntity"/> test data.
    /// </summary>
    private readonly LibraryMetadataProviderConfigurationEntityFixture _libraryMetadataProviderConfigurationEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledCommandHandlerTests"/> class.
    /// </summary>
    public SetLibraryMetadataProviderEnabledCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<SetLibraryMetadataProviderEnabledCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<SetLibraryMetadataProviderEnabledCommand>())
            .Returns([]);
        _sut = new SetLibraryMetadataProviderEnabledCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationDoesNotExist_ShouldCreateEnabledConfigurationWithNextRank()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns((LibraryMetadataProviderConfigurationEntity?)null);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryMetadataProviderConfigurationEntity>());
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryMetadataProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(
            libraryId: Guid.NewGuid(),
            pluginId: Guid.NewGuid(),
            rank: 2,
            isEnabled: false);
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(existingConfiguration.LibraryId, existingConfiguration.PluginId, Arg.Any<CancellationToken>())
            .Returns(existingConfiguration);
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryMetadataProviderEnabledCommandFixture.Create(libraryId: existingConfiguration.LibraryId, pluginId: existingConfiguration.PluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockValidator.Validate(Arg.Any<SetLibraryMetadataProviderEnabledCommand>()).Returns([Errors.Plugins.PluginIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryMetadataProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdCannotBeEmpty, result.FirstError);
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
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
        Result<Success> result = await _sut.HandleAsync(_setLibraryMetadataProviderEnabledCommandFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(_setLibraryMetadataProviderEnabledCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
