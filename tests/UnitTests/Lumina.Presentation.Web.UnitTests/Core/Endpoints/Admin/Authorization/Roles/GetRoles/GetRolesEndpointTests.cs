#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRoles;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Authorization.Roles.GetRoles;

/// <summary>
/// Contains unit tests for the <see cref="GetRolesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetRolesEndpoint _sut;
    private readonly RoleDtoFixture _roleDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpointTests"/> class.
    /// </summary>
    public GetRolesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetRolesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsRoles_ShouldReturnSuccessJsonWithRoles()
    {
        // Arrange
        RoleDto[] expectedRoles = [.. _roleDtoFixture.CreateMany(2)];
        _mockApiHttpClient.GetAsync<RoleDto[]>(ApiRoutes.Roles.GET_ROLES, Arg.Any<CancellationToken>())
            .Returns(expectedRoles);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        RoleDto[]? returnedRoles = jsonDocument.RootElement.GetProperty("data").Deserialize<RoleDto[]>(CreateCaseInsensitiveOptions());
        Assert.Equal(expectedRoles.Select(role => role.RoleName), returnedRoles!.Select(role => role.RoleName));
    }

    private static JsonSerializerOptions CreateCaseInsensitiveOptions()
    {
        return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestRolesFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<RoleDto[]>(ApiRoutes.Roles.GET_ROLES, Arg.Any<CancellationToken>())
            .Returns([.. _roleDtoFixture.CreateMany(2)]);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<RoleDto[]>(ApiRoutes.Roles.GET_ROLES, Arg.Any<CancellationToken>());
    }
}
