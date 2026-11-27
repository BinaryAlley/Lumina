#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Plugins;
using Nemera.Application.Common.DataAccess.Repositories.Plugins;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Nemera.Domain.Common.Primitives;
using Nemera.Infrastructure.Core.Plugins;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.UnitTests.Core.Plugins;

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
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
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
        _mockPluginRepository.GetByIdAsync(pluginId, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockPluginRepository.GetByIdAsync(pluginId, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockPluginRepository.GetByIdAsync(pluginId, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        Guid pluginId = Guid.NewGuid();
        _mockPluginRepository.GetByIdAsync(pluginId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve the plugin"));

        // Act
        IReadOnlyDictionary<string, string>? result = await _sut.GetSettingsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("Failed to read the settings of the plugin")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
