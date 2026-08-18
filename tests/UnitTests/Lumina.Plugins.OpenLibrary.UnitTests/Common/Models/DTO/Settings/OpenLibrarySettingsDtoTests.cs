#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Common.Models.DTO.Settings;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibrarySettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibrarySettingsDtoTests
{
    [Fact]
    public void Defaults_WhenCreated_ShouldUseTheDocumentedDefaultValues()
    {
        // Arrange
        OpenLibrarySettingsDto sut = new();

        // Act

        // Assert
        Assert.Equal("Lumina-OpenLibrary/1.0", sut.UserAgent);
        Assert.Null(sut.ContactEmail);
        Assert.Equal(10, sut.SearchResultLimit);
        Assert.Equal(50, sut.WorkEditionLimit);
        Assert.Equal(TimeSpan.FromSeconds(1.1), sut.MinimumRequestInterval);
    }

    [Fact]
    public void Setters_WhenAssigned_ShouldExposeTheAssignedValues()
    {
        // Arrange
        OpenLibrarySettingsDto sut = new();
        TimeSpan expectedInterval = TimeSpan.FromSeconds(3);

        // Act
        sut.UserAgent = "CustomAgent/2.0";
        sut.ContactEmail = "contact@example.com";
        sut.SearchResultLimit = 15;
        sut.WorkEditionLimit = 60;
        sut.MinimumRequestInterval = expectedInterval;

        // Assert
        Assert.Equal("CustomAgent/2.0", sut.UserAgent);
        Assert.Equal("contact@example.com", sut.ContactEmail);
        Assert.Equal(15, sut.SearchResultLimit);
        Assert.Equal(60, sut.WorkEditionLimit);
        Assert.Equal(expectedInterval, sut.MinimumRequestInterval);
    }
}
