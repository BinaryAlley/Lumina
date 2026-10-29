#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetUsers;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.GetUsers;

/// <summary>
/// Contains unit tests for the <see cref="GetUsersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetUsersEndpoint _sut;
    private readonly UserDtoFixture _userDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpointTests"/> class.
    /// </summary>
    public GetUsersEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetUsersEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUsers_ShouldReturnSuccessJsonWithUsers()
    {
        // Arrange
        UserDto[] expectedUsers = [.. _userDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<UserDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedUsers);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        UserDto[]? returnedUsers = jsonDocument.RootElement.GetProperty("data").Deserialize<UserDto[]>(_jsonOptions);
        Assert.NotNull(returnedUsers);
        Assert.Equal(expectedUsers.Select(user => user.Id), returnedUsers.Select(user => user.Id));
        Assert.Equal(expectedUsers.Select(user => user.Username), returnedUsers.Select(user => user.Username));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestUsersFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<UserDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _userDtoFixture.CreateMany(2)]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<UserDto[]>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Authentication.USERS),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<UserDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([.. _userDtoFixture.CreateMany(1)]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public void Configure_WhenCalled_ShouldRestrictTheRouteToAdministrators()
    {
        // Act
        _sut.Configure();

        // Assert
        Assert.NotNull(_sut.Definition.PreBuiltUserPolicies);
        Assert.Contains(AuthorizationPolicies.REQUIRE_ADMIN_ROLE, _sut.Definition.PreBuiltUserPolicies);
        Assert.Contains("GET", _sut.Definition.Verbs);
        Assert.Contains(WebRoutes.LibraryStatistics.GET_USERS, _sut.Definition.Routes);
    }
}
