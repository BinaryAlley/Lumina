#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.Contracts.Fixtures.Core.Requests.Authentication;
using Nemera.Contracts.Requests.Authentication;
using Nemera.DataAccess.Core.UoW;
using Nemera.Infrastructure.Core.Security;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly PasswordHashService _hashService = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RecoverPasswordEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // Set a fake IP for this test instance.
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.4");
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task RecoverPassword_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(
            username: "testuser",
            totpCode: "123456"
        );

        // Act
        List<HttpResponseMessage> responses = [];
        for (int i = 0; i < 11; i++) // Exceed the 10 request limit.
            responses.Add(await _client.PostAsJsonAsync("/api/v1/auth/recover-password", request));

        // Assert
        HttpResponseMessage lastResponse = responses.Last();
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
        string content = await lastResponse.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc6585#section-4", problemDetails["type"].GetString());
        Assert.Equal("TooManyRequests", problemDetails["title"].GetString());
        Assert.Equal("TooManyRequests", problemDetails["detail"].GetString());
        Assert.Equal("900", problemDetails["retryAfter"].GetString());

        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Limit"));
        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Reset"));
        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Remaining"));
    }

    [Theory]
    [InlineData("'; DROP TABLE Users--")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    public async Task RecoverPassword_WithSQLInjectionInUsername_ShouldNotCorruptOrDeleteData(string maliciousUsername)
    {
        // Arrange
        // Use a dedicated unique IP, so the rate-limit test (which exhausts the 10-permit window on its own IP)
        // cannot run first and 429 this test; the NotEqual(TooManyRequests) assertion below guards against any collision.
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", NemeraApiFactory.GetUniqueTestIp());
        UserEntity legitimateUser = await CreateTestUser();
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(
            username: maliciousUsername,
            totpCode: "123456"
        );

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/auth/recover-password", request);

        // Assert
        // The malicious username queries for a user, so it must never be executed against the database.
        Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(user => user.Id == legitimateUser.Id));
    }

    /// <summary>
    /// Creates a test user in the database.
    /// </summary>
    /// <returns>The created user entity.</returns>
    private async Task<UserEntity> CreateTestUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

        UserEntity user = _userEntityFixture.Create(username: _testUsername, password: _hashService.HashString("TestPass123!"));

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
        }
    }
}
