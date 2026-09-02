#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Pdf.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Pdf.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="PdfPlugin"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PdfPluginTests
{
    private readonly PdfPlugin _sut = new();

    [Fact]
    public void Id_WhenCalled_ShouldReturnStablePluginIdentifier()
    {
        // Act
        Guid result = _sut.Id;

        // Assert
        Assert.Equal(PdfPlugin.s_pluginId, result);
        Assert.Equal(new Guid("c2f9b8f0-5a4b-4e3d-8b2f-7e4a9c1d3e5f"), result);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnPluginDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("PDF Reader", result);
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
        Assert.Equal("Allows reading of PDF format books.", result);
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
        Assert.NotNull(serviceProvider.GetKeyedService<IBookReader>(PdfPlugin.s_pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IBookReader>(Guid.NewGuid()));
    }
}
