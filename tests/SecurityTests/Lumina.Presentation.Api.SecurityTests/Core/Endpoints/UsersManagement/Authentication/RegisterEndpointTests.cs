#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <c>/auth/register</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly PasswordHashService _hashService;
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    public RegisterEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.5");
        _hashService = new PasswordHashService();
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task Register_WithValidData_ShouldNotLeakTimingInformation()
    {
        // Arrange
        Stopwatch stopwatch = new();
        List<long> timings = [];
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.6");
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!"
        );

        // Act
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            stopwatch.Restart();
            await _client.PostAsJsonAsync("/api/v1/auth/register", request);
            stopwatch.Stop();
            timings.Add(stopwatch.ElapsedMilliseconds);
        }
        timings.Sort();
        timings = timings.Skip(1).SkipLast(1).ToList();

        // Assert
        double stdDev = CalculateStandardDeviation(timings);
        Assert.True(stdDev < 200);
    }

    [Fact]
    public async Task Register_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.7");
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!"
        );

        // Act
        List<HttpResponseMessage> responses = [];
        for (int i = 0; i < 11; i++)
            responses.Add(await _client.PostAsJsonAsync("/api/v1/auth/register", request));

        // Assert
        HttpResponseMessage lastResponse = responses.Last();
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
        string content = await lastResponse.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.29", problemDetails["type"].GetString());
        Assert.Equal("TooManyRequests", problemDetails["title"].GetString());
        Assert.Equal("TooManyRequests", problemDetails["detail"].GetString());
        Assert.Equal("900", problemDetails["retryAfter"].GetString());

        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Limit"));
        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Reset"));
        Assert.True(lastResponse.Headers.Contains("X-RateLimit-Remaining"));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("' WAITFOR DELAY '0:0:10'--")] // time-based injection
    public async Task Register_WithSQLInjectionAttempt_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        RegistrationRequest request = new(
            Username: maliciousUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
        Assert.DoesNotContain("password", content);
        Assert.DoesNotContain("hash", content);
        Assert.DoesNotContain("salt", content);
    }

    [Theory]
    [InlineData("password123")] // common password
    [InlineData("123456789")] // numeric only
    [InlineData("abcdefgh")] // lowercase only
    [InlineData("short")] // too short
    public async Task Register_WithWeakPassword_ShouldBeDenied(string weakPassword)
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: weakPassword,
            PasswordConfirm: weakPassword
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.8");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.DoesNotContain(weakPassword, content); // shouldn't leak the actual password
    }

    [Theory]
    [InlineData("TestPass123!")] // valid password
    [InlineData("Ab1!defghijklmnopqrstuvwxyz")] // long password
    public async Task Register_WithValidPassword_ShouldAcceptPassword(string password)
    {
        // Arrange
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: password,
            PasswordConfirm: password
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingUsername_ShouldNotLeakUserExistence()
    {
        // Arrange
        await CreateTestUser();
        RegistrationRequest request = new(
            Username: _testUsername,
            Password: "TestPass123!",
            PasswordConfirm: "TestPass123!"
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.9");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain("password", content);
        Assert.DoesNotContain("hash", content);
        Assert.DoesNotContain("salt", content);
    }

    private double CalculateStandardDeviation(List<long> values)
    {
        double average = values.Average();
        double sumOfSquaresOfDifferences = values.Select(value => (value - average) * (value - average)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
    }

    private async Task<UserEntity> CreateTestUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = new()
        {
            Username = _testUsername,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
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
