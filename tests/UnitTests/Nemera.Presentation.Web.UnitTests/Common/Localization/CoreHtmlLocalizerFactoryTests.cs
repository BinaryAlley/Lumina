#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Localization;

/// <summary>
/// Contains unit tests for the <see cref="CoreHtmlLocalizerFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoreHtmlLocalizerFactoryTests
{
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly CoreHtmlLocalizerFactory _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreHtmlLocalizerFactoryTests"/> class.
    /// </summary>
    public CoreHtmlLocalizerFactoryTests()
    {
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(Substitute.For<IStringLocalizer>());
        _sut = new CoreHtmlLocalizerFactory(_mockStringLocalizerFactory);
    }

    [Fact]
    public void Create_WhenBaseNameStartsWithLocationCorePrefix_ShouldStripCoreSegment()
    {
        // Act
        IHtmlLocalizer localizer = _sut.Create("Lumina.Presentation.Web.Core.Views.Auth.Login", "Lumina.Presentation.Web");

        // Assert
        Assert.NotNull(localizer);
        _mockStringLocalizerFactory.Received(1).Create("Lumina.Presentation.Web.Views.Auth.Login", "Lumina.Presentation.Web");
    }

    [Fact]
    public void Create_WhenBaseNameDoesNotStartWithLocationCorePrefix_ShouldKeepBaseNameUnchanged()
    {
        // Act
        IHtmlLocalizer localizer = _sut.Create("Lumina.Presentation.Web.Views.Shared.Layout", "Lumina.Presentation.Web");

        // Assert
        Assert.NotNull(localizer);
        _mockStringLocalizerFactory.Received(1).Create("Lumina.Presentation.Web.Views.Shared.Layout", "Lumina.Presentation.Web");
    }

    [Fact]
    public void Create_WhenResourceSourceTypeProvided_ShouldCreateLocalizerForType()
    {
        // Arrange
        _mockStringLocalizerFactory.Create(Arg.Any<Type>()).Returns(Substitute.For<IStringLocalizer>());

        // Act
        IHtmlLocalizer localizer = _sut.Create(typeof(TestLocalizationResource));

        // Assert
        Assert.NotNull(localizer);
        _mockStringLocalizerFactory.Received(1).Create(typeof(TestLocalizationResource));
    }

    /// <summary>
    /// Marker type used for the resource source based localizer creation test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestLocalizationResource
    {
    }
}
