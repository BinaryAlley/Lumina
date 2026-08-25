#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AddRoleEndpoint _sut;
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointTests"/> class.
    /// </summary>
    public AddRoleEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<AddRoleEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldCreateRoleViaApiAndReturnSuccess()
    {
        // Arrange
        AddRoleRequest request = _addRoleRequestFixture.Create();
        RolePermissionsDto expectedResponse = _rolePermissionsDtoFixture.Create(role: _roleDtoFixture.Create(id: Guid.NewGuid(), roleName: request.RoleName!), permissions: [_permissionDtoFixture.Create(id: Guid.NewGuid(), permission: AuthorizationPermission.CanViewUsers)]);
        _mockApiHttpClient.PostAsync<RolePermissionsDto, AddRoleRequest>(Arg.Any<string>(), Arg.Any<AddRoleRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<RolePermissionsDto, AddRoleRequest>(
            ApiRoutes.Roles.CREATE_ROLE,
            Arg.Is<AddRoleRequest>(role => role.RoleName == request.RoleName && role.Permissions!.SequenceEqual(request.Permissions!)),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
