#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;
using Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Contains unit tests for the <see cref="RegisterEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RegisterEndpoint _sut;
    private readonly RegisterRequestFixture _registerRequestFixture = new();
    private readonly RegisterResponseFixture _registerResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointTests"/> class.
    /// </summary>
    public RegisterEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RegisterEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRegistrationTypeIsAdmin_ShouldSetupApplicationViaApi()
    {
        // Arrange
        RegisterRequest request = _registerRequestFixture.Create(registrationType: "Admin");
        _mockApiHttpClient.PostAsync<RegisterResponse, RegisterRequest>(Arg.Any<string>(), Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
            .Returns(_registerResponseFixture.Create(username: request.Username));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<RegisterResponse, RegisterRequest>(
            ApiRoutes.Initialization.SETUP_APPLICATION,
            Arg.Is<RegisterRequest>(registration => registration.Username == request.Username),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRegistrationTypeIsUser_ShouldRegisterAccountViaApi()
    {
        // Arrange
        RegisterRequest request = _registerRequestFixture.Create(registrationType: "User");
        _mockApiHttpClient.PostAsync<RegisterResponse, RegisterRequest>(Arg.Any<string>(), Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
            .Returns(_registerResponseFixture.Create(username: request.Username));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<RegisterResponse, RegisterRequest>(
            ApiRoutes.Authentication.REGISTER_ACCOUNT,
            Arg.Is<RegisterRequest>(registration => registration.Username == request.Username),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
