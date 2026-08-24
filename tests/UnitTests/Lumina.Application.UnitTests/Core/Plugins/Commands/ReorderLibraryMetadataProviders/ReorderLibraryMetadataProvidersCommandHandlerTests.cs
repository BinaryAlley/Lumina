#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<ReorderLibraryMetadataProvidersCommand> _mockValidator;
    private readonly ReorderLibraryMetadataProvidersCommandHandler _sut;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly ReorderLibraryMetadataProvidersCommandFixture _reorderLibraryMetadataProvidersCommandFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandHandlerTests"/> class.
    /// </summary>
    public ReorderLibraryMetadataProvidersCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<ReorderLibraryMetadataProvidersCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<ReorderLibraryMetadataProvidersCommand>())
            .Returns([]);
        _sut = new ReorderLibraryMetadataProvidersCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldAssignRanksInTheProvidedOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity firstProvider = _configurationEntityFixture.Create(libraryId, Guid.NewGuid(), 2);
        LibraryMetadataProviderConfigurationEntity secondProvider = _configurationEntityFixture.Create(libraryId, Guid.NewGuid(), 1);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryMetadataProviderConfigurationEntity> { firstProvider, secondProvider });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryMetadataProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [secondProvider.PluginId, firstProvider.PluginId]), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == secondProvider.PluginId && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == firstProvider.PluginId && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockValidator.Validate(Arg.Any<ReorderLibraryMetadataProvidersCommand>()).Returns([Errors.Plugins.PluginIdsListCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryMetadataProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdsListCannotBeEmpty, result.FirstError);
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
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
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryMetadataProvidersCommandFixture.Create(libraryId: libraryId, pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(_reorderLibraryMetadataProvidersCommandFixture.Create(pluginIds: [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
