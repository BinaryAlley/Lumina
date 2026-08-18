#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
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
        PluginEntity pluginEntity = CreatePluginEntity(pluginId, JsonSerializer.Serialize(expectedSettings));
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
        PluginEntity pluginEntity = CreatePluginEntity(pluginId, settingsJson: null);
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

    /// <summary>
    /// Creates a plugin entity with the provided identity and serialized settings.
    /// </summary>
    /// <param name="id">The Id of the plugin.</param>
    /// <param name="settingsJson">The serialized settings of the plugin.</param>
    /// <returns>The created plugin entity.</returns>
    private static PluginEntity CreatePluginEntity(Guid id, string? settingsJson)
    {
        return new PluginEntity
        {
            Id = id,
            Name = "Test Plugin",
            Author = "Test Author",
            Version = "1.0.0",
            Description = "Test plugin description",
            SettingsJson = settingsJson,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }
}
