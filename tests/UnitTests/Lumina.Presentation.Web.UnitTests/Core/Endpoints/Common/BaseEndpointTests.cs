#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.Fixtures.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Common;

/// <summary>
/// Contains unit tests for the <see cref="Lumina.Presentation.Web.Core.Endpoints.Common.BaseEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseEndpointTests
{
    private readonly BaseEndpointFixture _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEndpointTests"/> class.
    /// </summary>
    public BaseEndpointTests()
    {
        _sut = Factory.Create<BaseEndpointFixture>();
    }

    [Fact]
    public async Task TestJsonSuccess_WhenCalledWithData_ShouldReturnSuccessJsonEnvelopeWithData()
    {
        // Act
        IResult result = _sut.TestJsonSuccess(new { itemId = "test-id" });
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("test-id", jsonDocument.RootElement.GetProperty("data").GetProperty("itemId").GetString());
    }

    [Fact]
    public async Task TestJsonSuccess_WhenCalledWithoutData_ShouldReturnSuccessJsonEnvelope()
    {
        // Act
        IResult result = _sut.TestJsonSuccess();
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public void TestView_WhenCalled_ShouldReturnRazorViewResult()
    {
        // Arrange
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/home";

        // Act
        IResult result = _sut.TestView("/Core/Views/Home/Index.cshtml");

        // Assert
        Assert.IsType<RazorViewResult>(result);
    }

    [Fact]
    public void TestView_WhenCalled_ShouldStoreLastDisplayedViewInSession()
    {
        // Arrange
        Microsoft.AspNetCore.Http.ISession session = TestHttpContextFactory.CreateSession();
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, session);
        _sut.HttpContext.Request.Path = "/en-us/libraries/manage";

        // Act
        _sut.TestView("/Core/Views/Library/Management/Index.cshtml");

        // Assert
        session.Received(1).Set(HttpContextItemKeys.LAST_DISPLAYED_VIEW, Arg.Any<byte[]>());
    }

    [Fact]
    public void TestCulture_WhenCultureRouteValuePresent_ShouldReturnCulture()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, "de-DE");

        // Act
        string culture = _sut.TestCulture;

        // Assert
        Assert.Equal("de-DE", culture);
    }

    [Fact]
    public void TestCulture_WhenCultureRouteValueMissing_ShouldReturnDefaultCulture()
    {
        // Arrange
        TestHttpContextFactory.ConfigureCulture(_sut.HttpContext, null);

        // Act
        string culture = _sut.TestCulture;

        // Assert
        Assert.Equal("en-US", culture);
    }
}
