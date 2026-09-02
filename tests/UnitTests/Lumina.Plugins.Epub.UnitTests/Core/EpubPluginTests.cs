#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Epub.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Epub.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="EpubPlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpubPluginTests
{
    private readonly EpubPlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(EpubPlugin.s_pluginId, result);
        Assert.Equal(new Guid("b1e8a8e0-4f3a-4d2c-9a1e-6d3f8a9b2c3d"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("EPUB Reader", result);
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
        Assert.Equal("Allows reading of EPUB format books.", result);
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
    public void RegisterServices_WhenCalled_ShouldRegisterTheKeyedBookReader()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IBookReader>(EpubPlugin.s_pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IBookReader>(Guid.NewGuid()));
    }
}
