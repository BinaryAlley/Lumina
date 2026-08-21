#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemePageDataFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemePageDataFactoryTests
{
    private readonly IStringLocalizer _mockStringLocalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemePageDataFactoryTests"/> class.
    /// </summary>
    public ThemePageDataFactoryTests()
    {
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
    }

    [Fact]
    public void CreateLocalizedStrings_WhenLocalizerProvidesStrings_ShouldReturnDictionaryKeyedByName()
    {
        // Arrange
        LocalizedString[] localizedStrings =
        [
            new("Title", "The Title"),
            new("Search", "Search")
        ];
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns(localizedStrings);

        // Act
        Dictionary<string, object?> result = ThemePageDataFactory.CreateLocalizedStrings(_mockStringLocalizer);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("The Title", result["Title"]);
        Assert.Equal("Search", result["Search"]);
        _mockStringLocalizer.Received(1).GetAllStrings(includeParentCultures: true);
    }

    [Fact]
    public void CreateLocalizedStrings_WhenLocalizerProvidesNoStrings_ShouldReturnEmptyDictionary()
    {
        // Arrange
        _mockStringLocalizer.GetAllStrings(Arg.Any<bool>()).Returns([]);

        // Act
        Dictionary<string, object?> result = ThemePageDataFactory.CreateLocalizedStrings(_mockStringLocalizer);

        // Assert
        Assert.Empty(result);
    }
}
