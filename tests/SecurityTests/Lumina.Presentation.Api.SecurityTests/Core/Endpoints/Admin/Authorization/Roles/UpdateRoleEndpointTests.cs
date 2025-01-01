#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Admin.Authorization.Roles;

/// <summary>
/// Contains security tests for the <c>/roles</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateRoleEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateRole_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        UpdateRoleRequest requestBody = new(
            RoleId: Guid.NewGuid(),
            RoleName: "Editor",
            Permissions: []
        );

        StringContent content = new(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/v1/roles", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        string responseContent = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        problemDetails["title"].GetString().Should().Be("Unauthorized");
        problemDetails["instance"].GetProperty("value").GetString().Should().Be("/api/v1/roles");
        problemDetails["detail"].GetString().Should().Be("Authentication failed");
    }
}
