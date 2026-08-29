#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeFileSystemBrowserBuilder"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeFileSystemBrowserBuilderTests
{
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ThemeFileSystemBrowserBuilder _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeFileSystemBrowserBuilderTests"/> class.
    /// </summary>
    public ThemeFileSystemBrowserBuilderTests()
    {
        _mockStringLocalizer = CreateLocalizer();
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new ThemeFileSystemBrowserBuilder(_mockStringLocalizerFactory, _mockHttpContextAccessor);
    }

    [Fact]
    public void Build_WhenCalled_ShouldResolveThemeAssetUrlsFromAssetBase()
    {
        // Arrange
        // Act
        ThemeFileSystemBrowserDto model = _sut.Build("/theme-assets/editorial/assets", "<div>{{name}}</div>", "<div>{{name}}</div>", "<li>{{path}}</li>");

        // Assert
        Assert.Equal("/theme-assets/editorial/assets", model.AssetBase);
        Assert.Equal("/theme-assets/editorial/assets/images/icons", model.IconBaseUrl);
        Assert.Equal("/theme-assets/editorial/assets/file-icons.json", model.FileIconsUrl);
    }

    [Fact]
    public void Build_WhenCalled_ShouldKeepTheSubTemplatesOfTheDynamicContent()
    {
        // Arrange
        // Act
        ThemeFileSystemBrowserDto model = _sut.Build("/theme-assets/editorial/assets", "<div class=\"tree-node\">{{name}}</div>", "<div class=\"e\">{{name}}</div>", "<li>{{path}}</li>");

        // Assert
        Assert.Equal("<div class=\"tree-node\">{{name}}</div>", model.TreeNodeTemplate);
        Assert.Equal("<div class=\"e\">{{name}}</div>", model.ExplorerItemTemplate);
        Assert.Equal("<li>{{path}}</li>", model.PathSegmentTemplate);
    }

    [Fact]
    public void Build_WhenCalled_ShouldLocalizeAllFileSystemBrowserStrings()
    {
        // Arrange
        // Act
        ThemeFileSystemBrowserDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty, string.Empty);

        // Assert
        string[] expectedKeys =
        [
            "listView", "detailsView", "smallIconsView", "mediumIconsView", "largeIconsView", "extraLargeIconsView",
            "back", "forward", "upOneLevel", "toggleTreeView", "toggleThumbnails", "toggleHiddenItems",
            "toggleSelectionMode", "editPath", "navigate", "newDirectory", "favoriteDirectory",
            "name", "directory", "cancel", "open"
        ];
        foreach (string key in expectedKeys)
        {
            Assert.True(model.Strings.TryGetValue(key, out object? value), $"The '{key}' string is missing.");
            Assert.StartsWith("Localized-", Assert.IsType<string>(value));
        }
    }

    [Fact]
    public void Build_WhenRequestIsAvailable_ShouldResolveAppBaseFromCurrentRequest()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);
        // Act
        ThemeFileSystemBrowserDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.Equal("https://localhost:5001/", model.AppBase);
    }

    [Fact]
    public void Build_WhenRequestIsUnavailable_ShouldReturnEmptyAppBase()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);
        // Act
        ThemeFileSystemBrowserDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.Equal(string.Empty, model.AppBase);
    }

    /// <summary>
    /// Creates a substitute string localizer that returns a string prefixed with "Localized-".
    /// </summary>
    /// <returns>The created substitute string localizer.</returns>
    private static IStringLocalizer CreateLocalizer()
    {
        IStringLocalizer localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"Localized-{callInfo.Arg<string>()}"));
        return localizer;
    }
}
