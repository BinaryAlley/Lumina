#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginInstaller"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginInstallerTests
{
    private readonly PluginInstaller _sut;
    private readonly PluginsSettingsDtoFixture _pluginsSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallerTests"/> class.
    /// </summary>
    public PluginInstallerTests()
    {
        ILogger<PluginInstaller> mockLogger = Substitute.For<ILogger<PluginInstaller>>();
        IOptions<PluginsSettingsDto> mockOptions = Substitute.For<IOptions<PluginsSettingsDto>>();
        mockOptions.Value.Returns(_pluginsSettingsDtoFixture.Create(directory: $".test-plugins-{Guid.NewGuid():N}"));
        _sut = new PluginInstaller(mockOptions, mockLogger);
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingUnsupportedFileType_ShouldReturnError()
    {
        // Arrange
        await using MemoryStream stream = new([.. new byte[] { 1, 2, 3, 4 }]);

        // Act
        Result<Success> result = await _sut.InstallAsync(stream, "plugin.txt", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.UnsupportedPluginFileType, result.FirstError);
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingCorruptZip_ShouldReturnNotReadableError()
    {
        // Arrange
        await using MemoryStream stream = new([.. new byte[] { 1, 2, 3, 4 }]);

        // Act
        Result<Success> result = await _sut.InstallAsync(stream, "plugin.zip", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginArchiveNotReadable, result.FirstError);
    }
}
