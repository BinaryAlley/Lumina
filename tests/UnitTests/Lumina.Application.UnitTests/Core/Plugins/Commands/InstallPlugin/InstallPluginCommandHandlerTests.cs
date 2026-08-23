#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.InstallPlugin;
using Lumina.Application.Fixtures.Core.Plugins.Commands.InstallPlugin;
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

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Contains unit tests for the <see cref="InstallPluginCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginCommandHandlerTests
{
    private readonly IPluginInstaller _mockPluginInstaller;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<InstallPluginCommand> _mockValidator;
    private readonly InstallPluginCommandHandler _sut;
    private readonly InstallPluginCommandFixture _installPluginCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginCommandHandlerTests"/> class.
    /// </summary>
    public InstallPluginCommandHandlerTests()
    {
        _mockPluginInstaller = Substitute.For<IPluginInstaller>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockValidator = Substitute.For<IValidator<InstallPluginCommand>>();
        _mockValidator.Validate(Arg.Any<InstallPluginCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(Guid.NewGuid());
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), "Admin", Arg.Any<CancellationToken>()).Returns(true);
        _sut = new InstallPluginCommandHandler(_mockPluginInstaller, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
        _installPluginCommandFixture = new InstallPluginCommandFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldInstallThePlugin()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();
        _mockPluginInstaller.InstallAsync(Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockPluginInstaller.Received(1).InstallAsync(command.Archive!, command.FileName!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutInstalling()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<InstallPluginCommand>()).Returns([Errors.Plugins.PluginArchiveCannotBeNull]);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginArchiveCannotBeNull, result.FirstError);
        await _mockPluginInstaller.DidNotReceiveWithAnyArgs().InstallAsync(default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsMissing_ShouldReturnNotAuthorizedWithoutInstalling()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPluginInstaller.DidNotReceiveWithAnyArgs().InstallAsync(default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedWithoutInstalling()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPluginInstaller.DidNotReceiveWithAnyArgs().InstallAsync(default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_WhenInstallationFails_ShouldReturnTheInstallationError()
    {
        // Arrange
        InstallPluginCommand command = _installPluginCommandFixture.Create();
        _mockPluginInstaller.InstallAsync(Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Plugins.PluginArchiveNotReadable);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginArchiveNotReadable, result.FirstError);
    }
}
