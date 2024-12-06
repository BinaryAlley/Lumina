#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authorization;

/// <summary>
/// Contains integration tests for the <see cref="GetAuthorizationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private HttpClient _client;
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetAuthorizationEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.12");
        _testUsername = $"testuser_{Guid.NewGuid()}";
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        _client = _apiFactory.CreateClient(); // unauthenticated client
        GetAuthorizationRequest request = new(Guid.NewGuid());

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/auth/get-authorization?userId={request.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        problemDetails["title"].GetString().Should().Be("Unauthorized");
        problemDetails["instance"].GetProperty("value").GetString().Should().Be("/api/v1/auth/get-authorization");
        problemDetails["detail"].GetString().Should().Be("Authentication failed");
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
        }

        _client.DefaultRequestHeaders.Authorization = null;
    }
}
