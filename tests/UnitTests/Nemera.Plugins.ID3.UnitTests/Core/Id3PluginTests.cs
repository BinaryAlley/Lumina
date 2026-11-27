#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.ID3.Core;
using Lumina.Plugins.ID3.Core.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="Id3Plugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3PluginTests
{
    private readonly Id3Plugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(Id3Plugin.s_pluginId, result);
        Assert.Equal(new Guid("a3f1c8d2-6b47-4e9a-8c15-2d7f9e4b6a03"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("ID3 Metadata", result);
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
        Assert.Equal("Populates artist, album and track metadata from the embedded tags of the audio files, without accessing the web.", result);
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
    public void RegisterServices_WhenCalled_ShouldRegisterTheThreeKeyedMetadataProviders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Equal(3, serviceProvider.GetKeyedServices<IMetadataProvider>(Id3Plugin.s_pluginId).Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IMetadataProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void RegisterServices_WhenCalled_ShouldRegisterTheTagReader()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<Id3TagReader>());
    }
}
