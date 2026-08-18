#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly IValidator<UpdatePluginSettingsCommand> _mockValidator;
    private readonly UpdatePluginSettingsCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsCommandHandlerTests"/> class.
    /// </summary>
    public UpdatePluginSettingsCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockValidator = Substitute.For<IValidator<UpdatePluginSettingsCommand>>();
        _mockValidator.Validate(Arg.Any<UpdatePluginSettingsCommand>())
            .Returns([]);
        _sut = new UpdatePluginSettingsCommandHandler(_mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPersistTheSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        Dictionary<string, string> settings = new() { ["preferredLanguage"] = "fr" };
        _mockPluginRepository.UpdateSettingsAsync(pluginId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(new UpdatePluginSettingsCommand(pluginId, settings), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockPluginRepository.Received(1).UpdateSettingsAsync(pluginId, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        Dictionary<string, string> settings = new() { ["preferredLanguage"] = "fr" };
        _mockValidator.Validate(Arg.Any<UpdatePluginSettingsCommand>()).Returns([Errors.Plugins.PluginIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(new UpdatePluginSettingsCommand(pluginId, settings), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdCannotBeEmpty, result.FirstError);
        await _mockPluginRepository.DidNotReceive().UpdateSettingsAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
