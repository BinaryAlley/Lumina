#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Admin.Authorization.Roles;

/// <summary>
/// Contains security tests for the <c>/auth/roles</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetRolesEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetRoles_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/auth/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string responseContent = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/auth/roles", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }
}
