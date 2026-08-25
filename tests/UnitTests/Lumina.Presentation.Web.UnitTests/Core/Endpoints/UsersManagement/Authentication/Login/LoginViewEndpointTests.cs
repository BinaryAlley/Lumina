#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;
using Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement.Authentication;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Contains unit tests for the <see cref="LoginViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginViewEndpointTests
{
    private readonly IUrlService _mockUrlService;
    private readonly LoginViewEndpoint _sut;
    private readonly LoginViewRequestFixture _loginViewRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewEndpointTests"/> class.
    /// </summary>
    public LoginViewEndpointTests()
    {
        _mockUrlService = Substitute.For<IUrlService>();
        _sut = Factory.Create<LoginViewEndpoint>(_mockUrlService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyAuthenticated_ShouldRedirectToHome()
    {
        // Arrange
        TestHttpContextFactory.ConfigureUser(_sut.HttpContext, TestHttpContextFactory.CreateAuthenticatedUser());
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us");

        // Act
        IResult result = await _sut.ExecuteAsync(_loginViewRequestFixture.Create(), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/en-us", redirectResult.Url);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuperAdminSetupPending_ShouldRedirectToRegisterView()
    {
        // Arrange
        ISession session = Substitute.For<ISession>();
        byte[] pendingBytes = Encoding.UTF8.GetBytes("true");
#pragma warning disable CS8601 // possible null reference assignment in the NSubstitute out-argument stub
        session.TryGetValue(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP, out Arg.Any<byte[]>()).Returns(callInfo =>
        {
            callInfo[1] = pendingBytes;
            return true;
        });
#pragma warning restore CS8601
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, session);
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW).Returns("http://localhost/en-us/auth/register");

        // Act
        IResult result = await _sut.ExecuteAsync(_loginViewRequestFixture.Create(), CancellationToken.None);

        // Assert
        RedirectHttpResult redirectResult = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("http://localhost/en-us/auth/register", redirectResult.Url);
        _mockUrlService.Received(1).GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthenticatedAndInitialized_ShouldReturnRazorViewResult()
    {
        // Arrange
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());

        // Act
        IResult result = await _sut.ExecuteAsync(_loginViewRequestFixture.Create(), CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
    }
}
