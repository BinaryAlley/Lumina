#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.MusicBrainz.Core;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzPlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzPluginTests
{
    private readonly MusicBrainzPlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(MusicBrainzPlugin.s_pluginId, result);
        Assert.Equal(new Guid("f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("MusicBrainz Metadata", result);
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
        Assert.Equal("Retrieves artist, album and track metadata from MusicBrainz.", result);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnTheSixSettingDescriptors()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        Assert.Equal(6, result.Count);
        Assert.Equal(
            [
                MusicBrainzSettingsKeys.BASE_URL,
                MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL,
                MusicBrainzSettingsKeys.CONTACT_EMAIL,
                MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT,
                MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT,
                MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS
            ],
            result.Select(descriptor => descriptor.Key));
    }

    [Theory]
    [InlineData(MusicBrainzSettingsKeys.BASE_URL, "Base URL", PluginSettingType.Text, "https://musicbrainz.org/ws/2/")]
    [InlineData(MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL, "Allow LAN/Private Base URL", PluginSettingType.Boolean, "false")]
    [InlineData(MusicBrainzSettingsKeys.CONTACT_EMAIL, "Contact Email", PluginSettingType.Text, null)]
    [InlineData(MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT, "Search Result Limit", PluginSettingType.Number, "10")]
    [InlineData(MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT, "Release Lookup Limit", PluginSettingType.Number, "25")]
    [InlineData(MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, "Minimum Request Interval (seconds)", PluginSettingType.Number, "1.0")]
    public void GetSettingsSchema_WhenCalled_ShouldDescribeEachSettingCorrectly(string key, string label, PluginSettingType type, string? defaultValue)
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        PluginSettingDescriptorDto descriptor = Assert.Single(result, candidate => candidate.Key == key);
        Assert.Equal(label, descriptor.Label);
        Assert.Equal(type, descriptor.Type);
        Assert.Equal(defaultValue, descriptor.DefaultValue);
        Assert.Null(descriptor.AllowedValues);
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
        Assert.Equal(3, serviceProvider.GetKeyedServices<IMetadataProvider>(MusicBrainzPlugin.s_pluginId).Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IMetadataProvider>(Guid.NewGuid()));
    }
}
