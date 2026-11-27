#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.OpenLibrary.Core;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibraryPlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibraryPluginTests
{
    private readonly OpenLibraryPlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(OpenLibraryPlugin.s_pluginId, result);
        Assert.Equal(new Guid("08b17802-7f9c-4c4b-9d7f-b507bbed3e58"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Open Library Metadata", result);
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
        Assert.Equal("Retrieves book and edition metadata from Open Library.", result);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnAllDeclaredSettingsWithTheirDefaults()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> result = _sut.GetSettingsSchema();

        // Assert
        Assert.Equal(4, result.Count);

        PluginSettingDescriptorDto contactEmail = result[0];
        Assert.Equal(OpenLibrarySettingsKeys.CONTACT_EMAIL, contactEmail.Key);
        Assert.Equal("Contact Email", contactEmail.Label);
        Assert.Equal(PluginSettingType.Text, contactEmail.Type);
        Assert.Null(contactEmail.DefaultValue);

        PluginSettingDescriptorDto searchResultLimit = result[1];
        Assert.Equal(OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT, searchResultLimit.Key);
        Assert.Equal("Search Result Limit", searchResultLimit.Label);
        Assert.Equal(PluginSettingType.Number, searchResultLimit.Type);
        Assert.Equal("10", searchResultLimit.DefaultValue);

        PluginSettingDescriptorDto workEditionLimit = result[2];
        Assert.Equal(OpenLibrarySettingsKeys.WORK_EDITION_LIMIT, workEditionLimit.Key);
        Assert.Equal("Work Edition Limit", workEditionLimit.Label);
        Assert.Equal(PluginSettingType.Number, workEditionLimit.Type);
        Assert.Equal("50", workEditionLimit.DefaultValue);

        PluginSettingDescriptorDto minimumRequestInterval = result[3];
        Assert.Equal(OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, minimumRequestInterval.Key);
        Assert.Equal("Minimum Request Interval (seconds)", minimumRequestInterval.Label);
        Assert.Equal(PluginSettingType.Number, minimumRequestInterval.Type);
        Assert.Equal("1.1", minimumRequestInterval.DefaultValue);
    }

    [Fact]
    public void GetSettingsSchema_WhenCalledRepeatedly_ShouldReturnAnIndependentSchemaEachTime()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptorDto> first = _sut.GetSettingsSchema();
        IReadOnlyList<PluginSettingDescriptorDto> second = _sut.GetSettingsSchema();

        // Assert
        Assert.NotSame(first, second);
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void RegisterServices_WhenCalled_ShouldRegisterTheKeyedMetadataProviderAndHttpClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _sut.RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IMetadataProvider>(OpenLibraryPlugin.s_pluginId));
        Assert.NotNull(serviceProvider.GetService<OpenLibraryHttpClient>());
    }
}
