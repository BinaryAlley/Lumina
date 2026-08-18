#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Core.Settings;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibrarySettingsLoader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibrarySettingsLoaderTests
{
    private readonly OpenLibrarySettingsDtoFixture _settingsFixture = new();

    [Fact]
    public void Apply_WhenStoredSettingsContainValidValues_ShouldOverlayThemOntoTheRuntimeSettings()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.CONTACT_EMAIL] = "  contact@example.com  ",
            [OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT] = "25",
            [OpenLibrarySettingsKeys.WORK_EDITION_LIMIT] = "75",
            [OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = "2.5"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal("contact@example.com", settings.ContactEmail);
        Assert.Equal(25, settings.SearchResultLimit);
        Assert.Equal(75, settings.WorkEditionLimit);
        Assert.Equal(TimeSpan.FromSeconds(2.5), settings.MinimumRequestInterval);
    }

    [Fact]
    public void Apply_WhenStoredSettingsAreEmpty_ShouldKeepTheRuntimeSettingsDefaults()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create(
            userAgent: "Lumina-OpenLibrary/1.0",
            searchResultLimit: 10,
            workEditionLimit: 50,
            minimumRequestInterval: TimeSpan.FromSeconds(1.1));
        Dictionary<string, string> storedSettings = [];

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal("Lumina-OpenLibrary/1.0", settings.UserAgent);
        Assert.Null(settings.ContactEmail);
        Assert.Equal(10, settings.SearchResultLimit);
        Assert.Equal(50, settings.WorkEditionLimit);
        Assert.Equal(TimeSpan.FromSeconds(1.1), settings.MinimumRequestInterval);
    }

    [Fact]
    public void Apply_WhenStoredSettingsContainUnknownKeys_ShouldIgnoreThem()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create(searchResultLimit: 10, workEditionLimit: 50);
        Dictionary<string, string> storedSettings = new()
        {
            ["UnknownSetting"] = "value"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(10, settings.SearchResultLimit);
        Assert.Equal(50, settings.WorkEditionLimit);
    }

    [Fact]
    public void Apply_WhenContactEmailIsWhiteSpace_ShouldSetItToNull()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.CONTACT_EMAIL] = "   "
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Null(settings.ContactEmail);
    }

    [Fact]
    public void Apply_WhenSearchResultLimitIsNotParsable_ShouldKeepTheCurrentValue()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create(searchResultLimit: 10);
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT] = "not-a-number"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(10, settings.SearchResultLimit);
    }

    [Fact]
    public void Apply_WhenSearchResultLimitIsNotParsableAndCurrentValueWasChanged_ShouldKeepTheChangedValue()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create(searchResultLimit: 7);
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT] = "not-a-number"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(7, settings.SearchResultLimit);
    }

    [Theory]
    [InlineData("-5", 1)]
    [InlineData("0", 1)]
    [InlineData("1", 1)]
    [InlineData(" 12 ", 12)]
    [InlineData("300", 300)]
    public void Apply_WhenSearchResultLimitIsParsable_ShouldClampItToAtLeastOne(string storedValue, int expected)
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT] = storedValue
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(expected, settings.SearchResultLimit);
    }

    [Theory]
    [InlineData("-5", 1)]
    [InlineData("0", 1)]
    [InlineData("80", 80)]
    public void Apply_WhenWorkEditionLimitIsParsable_ShouldClampItToAtLeastOne(string storedValue, int expected)
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.WORK_EDITION_LIMIT] = storedValue
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(expected, settings.WorkEditionLimit);
    }

    [Fact]
    public void Apply_WhenMinimumRequestIntervalIsNegative_ShouldClampItToZero()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = "-3"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(TimeSpan.Zero, settings.MinimumRequestInterval);
    }

    [Theory]
    [InlineData("1.5", 1500)]
    [InlineData("0.25", 250)]
    [InlineData("0", 0)]
    public void Apply_WhenMinimumRequestIntervalUsesInvariantCultureDecimalSeparator_ShouldParseIt(string storedValue, int expectedMilliseconds)
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create();
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = storedValue
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), settings.MinimumRequestInterval);
    }

    [Fact]
    public void Apply_WhenMinimumRequestIntervalIsNotParsable_ShouldKeepTheCurrentValue()
    {
        // Arrange
        OpenLibrarySettingsDto settings = _settingsFixture.Create(minimumRequestInterval: TimeSpan.FromSeconds(1.1));
        Dictionary<string, string> storedSettings = new()
        {
            [OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = "fast"
        };

        // Act
        OpenLibrarySettingsLoader.Apply(settings, storedSettings);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1.1), settings.MinimumRequestInterval);
    }
}
