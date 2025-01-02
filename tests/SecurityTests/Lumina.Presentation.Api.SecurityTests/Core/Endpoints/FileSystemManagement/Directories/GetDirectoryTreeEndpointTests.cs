#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.FileSystemManagement.Directories;

/// <summary>
/// Contains security tests for the <c>/directories/get-directory-tree</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDirectoryTreeEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetDirectoryTree_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/directories/get-directory-tree");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        problemDetails["title"].GetString().Should().Be("Unauthorized");
        problemDetails["instance"].GetProperty("value").GetString().Should().Be("/api/v1/directories/get-directory-tree");
        problemDetails["detail"].GetString().Should().Be("Authentication failed");
    }
}
