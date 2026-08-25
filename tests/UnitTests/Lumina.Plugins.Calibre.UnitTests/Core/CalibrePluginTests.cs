#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Core;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Calibre.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CalibrePlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibrePluginTests
{
    private readonly CalibrePlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(CalibrePlugin.s_pluginId, result);
        Assert.Equal(new Guid("a1b2c3d4-5e6f-4a7b-8c9d-0e1f2a3b4c5d"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Calibre Metadata", result);
    }

    [Fact]
    public void Author_WhenCalled_ShouldReturnPluginAuthor()
    {
        // Act
        string result = _sut.Author;

        // Assert
        Assert.Equal("Lumina", result);
    }

    [Fact]
    public void Version_WhenCalled_ShouldReturnPluginVersion()
    {
        // Act
        Version result = _sut.Version;

        // Assert
        Assert.Equal(new Version(1, 0, 0), result);
    }

    [Fact]
    public void Description_WhenCalled_ShouldReturnPluginDescription()
    {
        // Act
        string result = _sut.Description;

        // Assert
        Assert.Equal("Reads book metadata and covers from the OPF files of a Calibre library.", result);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnEmptySettingsSchema()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void RegisterServices_WhenCalled_ShouldRegisterTheKeyedMetadataAndArtworkProviders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IMetadataProvider>(CalibrePlugin.s_pluginId));
        Assert.NotNull(serviceProvider.GetKeyedService<IArtworkProvider>(CalibrePlugin.s_pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IMetadataProvider>(Guid.NewGuid()));
        Assert.Null(serviceProvider.GetKeyedService<IArtworkProvider>(Guid.NewGuid()));
    }
}
