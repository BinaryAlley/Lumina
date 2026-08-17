#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DeleteRoleEndpoint _sut;
    private readonly DeleteRoleRequestFixture _deleteRoleRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpointTests"/> class.
    /// </summary>
    public DeleteRoleEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<DeleteRoleEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldDeleteRoleViaApiAndReturnSuccess()
    {
        // Arrange
        DeleteRoleRequest request = _deleteRoleRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.Roles.DELETE_ROLE.Replace("{roleId}", request.RoleId.ToString());
        await _mockApiHttpClient.Received(1).DeleteAsync(expectedEndpoint, Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
