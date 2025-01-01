#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authorization;

/// <summary>
/// Contains security tests for the <c>/auth/users/{userId}/role-and-permissions</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateUserRoleAndPermissionsEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateUserRoleAndPermissions_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string url = $"/api/v1/auth/users/{userId}/role-and-permissions";

        // Act
        HttpResponseMessage response = await _client.PutAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        string responseContent = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        problemDetails["title"].GetString().Should().Be("Unauthorized");
        problemDetails["instance"].GetProperty("value").GetString().Should().Be(url);
        problemDetails["detail"].GetString().Should().Be("Authentication failed");
    }
}
