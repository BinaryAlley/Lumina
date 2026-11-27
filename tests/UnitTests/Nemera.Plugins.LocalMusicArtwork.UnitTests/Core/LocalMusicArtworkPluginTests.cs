#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="LocalMusicArtworkPlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtworkPluginTests
{
    private readonly LocalMusicArtworkPlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(LocalMusicArtworkPlugin.s_pluginId, result);
        Assert.Equal(new Guid("c9f1a7d3-2b64-4e18-8f05-7a3d9c1b6e42"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Local Music Artwork", result);
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
        Assert.Equal("Reads the artwork of the artists and the albums from the images stored in the folders of a local music library.", result);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnTheSingleSettingDescriptor()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        PluginSettingDescriptorDto descriptor = Assert.Single(result);
        Assert.Equal(LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER, descriptor.Key);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldDescribeTheEmbeddedCoverSettingCorrectly()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        PluginSettingDescriptorDto descriptor = Assert.Single(result);
        Assert.Equal("Extract embedded album cover image from audio files when it is not already present on disk", descriptor.Label);
        Assert.Equal(PluginSettingType.Boolean, descriptor.Type);
        Assert.Equal("true", descriptor.DefaultValue);
        Assert.Null(descriptor.AllowedValues);
    }

    [Fact]
    public void RegisterServices_WhenCalled_ShouldRegisterTheAlbumAndArtistArtworkProviders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IReadOnlyList<IArtworkProvider> providers = [.. serviceProvider.GetKeyedServices<IArtworkProvider>(LocalMusicArtworkPlugin.s_pluginId)];
        Assert.Equal(2, providers.Count);
        Assert.Contains(providers, provider => provider is LocalMusicAlbumArtworkProvider);
        Assert.Contains(providers, provider => provider is LocalMusicArtistArtworkProvider);
        Assert.Equal(2, providers.Select(provider => provider.GetType()).Distinct().Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IArtworkProvider>(Guid.NewGuid()));
    }
}
