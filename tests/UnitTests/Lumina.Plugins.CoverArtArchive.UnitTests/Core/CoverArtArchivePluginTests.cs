#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.CoverArtArchive.Core;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchivePlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchivePluginTests
{
    private readonly CoverArtArchivePlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(CoverArtArchivePlugin.s_pluginId, result);
        Assert.Equal(new Guid("b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Cover Art Archive", result);
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
        Assert.Equal("Retrieves the cover, back, booklet, medium and other artwork of music albums from the Cover Art Archive.", result);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnTheTwoSettingDescriptors()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(
            [
                CoverArtArchiveSettingsKeys.CONTACT_EMAIL,
                CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS
            ],
            result.Select(descriptor => descriptor.Key));
    }

    [Theory]
    [InlineData(CoverArtArchiveSettingsKeys.CONTACT_EMAIL, "Contact Email", PluginSettingType.Text, null)]
    [InlineData(CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, "Minimum Request Interval (seconds)", PluginSettingType.Number, "1.0")]
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
    public void RegisterServices_WhenCalled_ShouldRegisterTheKeyedArtworkProvider()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Single(serviceProvider.GetKeyedServices<IArtworkProvider>(CoverArtArchivePlugin.s_pluginId));
        Assert.Empty(serviceProvider.GetKeyedServices<IArtworkProvider>(Guid.NewGuid()));
    }
}
