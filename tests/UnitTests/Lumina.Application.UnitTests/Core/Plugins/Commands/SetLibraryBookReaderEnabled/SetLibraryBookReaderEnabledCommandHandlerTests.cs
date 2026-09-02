#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryBookReaderEnabledCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryBookReaderConfigurationRepository _mockLibraryBookReaderConfigurationRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IBookReaderEnablementCache _mockEnablementCache;
    private readonly IValidator<SetLibraryBookReaderEnabledCommand> _mockValidator;
    private readonly SetLibraryBookReaderEnabledCommandHandler _sut;
    private readonly SetLibraryBookReaderEnabledCommandFixture _setLibraryBookReaderEnabledCommandFixture = new();
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledCommandHandlerTests"/> class.
    /// </summary>
    public SetLibraryBookReaderEnabledCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryBookReaderConfigurationRepository = Substitute.For<ILibraryBookReaderConfigurationRepository>();
        _mockUnitOfWork.LibraryBookReaderConfigurationRepository.Returns(_mockLibraryBookReaderConfigurationRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockEnablementCache = Substitute.For<IBookReaderEnablementCache>();
        _mockValidator = Substitute.For<IValidator<SetLibraryBookReaderEnabledCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<SetLibraryBookReaderEnabledCommand>()).Returns([]);
        _mockLibraryBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new SetLibraryBookReaderEnabledCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockEnablementCache, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationDoesNotExist_ShouldInsertNewConfigurationAndPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create(isEnabled: false);
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(null));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockLibraryBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration =>
                configuration.LibraryId == command.LibraryId &&
                configuration.PluginId == command.PluginId &&
                configuration.IsEnabled == command.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).Invalidate(command.LibraryId, command.PluginId);
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationExists_ShouldUpdateItsEnabledStateAndPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create(isEnabled: true);
        LibraryBookReaderConfigurationEntity existingConfiguration = _configurationEntityFixture.Create(
            libraryId: command.LibraryId,
            pluginId: command.PluginId,
            isEnabled: false);
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(existingConfiguration));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(existingConfiguration.IsEnabled);
        await _mockLibraryBookReaderConfigurationRepository.Received(1).UpsertAsync(existingConfiguration, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).Invalidate(command.LibraryId, command.PluginId);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<SetLibraryBookReaderEnabledCommand>()).Returns([Errors.Plugins.PluginIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdCannotBeEmpty, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().Invalidate(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenOwnershipPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorAndNotPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().Invalidate(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedErrorAndNotPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryAndPluginIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().Invalidate(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetConfigurationFails_ShouldReturnErrorAndNotPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();
        Error error = Error.Failure(description: "Failed to get configuration");
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().Invalidate(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpsertFails_ShouldReturnErrorAndNotPersist()
    {
        // Arrange
        SetLibraryBookReaderEnabledCommand command = _setLibraryBookReaderEnabledCommandFixture.Create();
        Error error = Error.Failure(description: "Failed to upsert configuration");
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryBookReaderConfigurationEntity?>(null));
        _mockLibraryBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().Invalidate(Arg.Any<Guid>(), Arg.Any<Guid>());
    }
}
