#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Core.Plugins;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingsStore"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingsStoreTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly ILogger<PluginSettingsStore> _mockLogger;
    private readonly PluginSettingsStore _sut;
    private readonly PluginEntityFixture _pluginEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginSettingsStoreTests"/> class.
    /// </summary>
    public PluginSettingsStoreTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockLogger = Substitute.For<ILogger<PluginSettingsStore>>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);

        _sut = new PluginSettingsStore(_mockUnitOfWork, _mockLogger);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenPluginHasPersistedSettings_ShouldReturnDeserializedSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        Dictionary<string, string> expectedSettings = new() { ["ApiKey"] = "test-key", ["Enabled"] = "true" };
        PluginEntity pluginEntity = _pluginEntityFixture.Create(id: pluginId, settingsJson: JsonSerializer.Serialize(expectedSettings));
        _mockPluginRepository.GetByIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<PluginEntity?>(pluginEntity));

        // Act
        IReadOnlyDictionary<string, string>? result = await _sut.GetSettingsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSettings, result);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenPluginHasNoPersistedSettings_ShouldReturnNull()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        PluginEntity pluginEntity = _pluginEntityFixture.Create(id: pluginId, includeSettingsJson: false);
        _mockPluginRepository.GetByIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<PluginEntity?>(pluginEntity));

        // Act
        IReadOnlyDictionary<string, string>? result = await _sut.GetSettingsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenPluginDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockPluginRepository.GetByIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<PluginEntity?>(null));

        // Act
        IReadOnlyDictionary<string, string>? result = await _sut.GetSettingsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenRepositoryReturnsFailure_ShouldReturnNullAndLogWarning()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockPluginRepository.GetByIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve the plugin"));

        // Act
        IReadOnlyDictionary<string, string>? result = await _sut.GetSettingsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
