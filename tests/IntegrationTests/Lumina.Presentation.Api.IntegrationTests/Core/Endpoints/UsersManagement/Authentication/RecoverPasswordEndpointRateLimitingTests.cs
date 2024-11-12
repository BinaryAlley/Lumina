#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains integration tests for the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
/// <remarks>
/// This test class needs to be separate from <see cref="RecoverPasswordEndpointTests"/> because rate limiting is set per application run, not HTTP client instantiation.
/// </remarks>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointRateLimitingTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointRateLimitingTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RecoverPasswordEndpointRateLimitingTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task ExecuteAsync_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
    {
        // Arrange
        RecoverPasswordRequest request = new(
            Username: "testuser",
            TotpCode: "123456"
        );

        // Act
        List<HttpResponseMessage> responses = [];
        for (int i = 0; i < 11; i++) // exceed the 10 request limit
            responses.Add(await _client.PostAsJsonAsync("/api/v1/auth/recover-password", request));

        // Assert
        HttpResponseMessage lastResponse = responses.Last();
        lastResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        string content = await lastResponse.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status429TooManyRequests);
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.29");
        problemDetails["title"].GetString().Should().Be("TooManyRequests");
        problemDetails["detail"].GetString().Should().Be("Too many attempts. Please try again later.");
        problemDetails["retryAfter"].GetString().Should().Be("15");
    }

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
    }
}
