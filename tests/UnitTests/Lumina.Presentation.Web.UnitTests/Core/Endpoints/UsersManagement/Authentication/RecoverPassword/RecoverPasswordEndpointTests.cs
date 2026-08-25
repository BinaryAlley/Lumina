#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RecoverPasswordEndpoint _sut;
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();
    private readonly RecoverPasswordResponseFixture _recoverPasswordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    public RecoverPasswordEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RecoverPasswordEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRecoverPasswordViaApiAndReturnSuccess()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        RecoverPasswordResponse expectedResponse = _recoverPasswordResponseFixture.Create(isPasswordReset: true);
        _mockApiHttpClient.PostAsync<RecoverPasswordResponse, RecoverPasswordRequest>(Arg.Any<string>(), Arg.Any<RecoverPasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<RecoverPasswordResponse, RecoverPasswordRequest>(
            ApiRoutes.Authentication.RECOVER_PASSWORD,
            Arg.Is<RecoverPasswordRequest>(recovery => recovery.Username == request.Username),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isPasswordReset").GetBoolean());
    }
}
