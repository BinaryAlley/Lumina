#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <c>/auth/recover-password</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthRecoverPasswordTests : IClassFixture<LuminaApiFactory>, IDisposable
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
    /// Initializes a new instance of the <see cref="AuthRecoverPasswordTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AuthRecoverPasswordTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // set a fake IP for this test instance
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.4");
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
        problemDetails["retryAfter"].GetString().Should().Be("900");

        lastResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Reset");
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Remaining");
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
