#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsCommandHandler"/> class.
/// </summary>
public class UpdatePluginSettingsCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly UpdatePluginSettingsCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsCommandHandlerTests"/> class.
    /// </summary>
    public UpdatePluginSettingsCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.GetRepository<IPluginRepository>().Returns(_mockPluginRepository);
        IValidator<UpdatePluginSettingsCommand> mockValidator = Substitute.For<IValidator<UpdatePluginSettingsCommand>>();
        mockValidator.Validate(Arg.Any<UpdatePluginSettingsCommand>())
            .Returns([]);
        _sut = new UpdatePluginSettingsCommandHandler(_mockUnitOfWork, mockValidator);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPersistTheSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        Dictionary<string, string> settings = new() { ["preferredLanguage"] = "fr" };
        _mockPluginRepository.UpdateSettingsAsync(pluginId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<Success> result = await _sut.HandleAsync(new UpdatePluginSettingsCommand(pluginId, settings), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        await _mockPluginRepository.Received(1).UpdateSettingsAsync(pluginId, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
