#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <c>/auth/login</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthLoginTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly HashService _hashService;
    private readonly ICryptographyService _cryptographyService;
    private readonly TotpTokenGenerator _totpTokenGenerator;
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthLoginTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AuthLoginTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // set a fake IP for this test instance
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.1");
        _hashService = new HashService();
        _totpTokenGenerator = new TotpTokenGenerator();
        _testUsername = $"testuser_{Guid.NewGuid()}";

        using IServiceScope scope = apiFactory.Services.CreateScope();
        _cryptographyService = scope.ServiceProvider.GetRequiredService<ICryptographyService>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldAuthenticateSuccessfully()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TestPass123!"
        );
        DateTimeOffset beforeRequest = DateTimeOffset.UtcNow;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.10");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        string content = await response.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result!.Token);
       
        string expValue = token.Claims.First(c => c.Type == "exp").Value; // get token claims
        DateTimeOffset tokenExpiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expValue));
        DateTimeOffset expectedExpiration = beforeRequest.AddMinutes(15);

        // verify token duration (should be 15 minutes from request time - expiration value is set in tests setup class)
        TimeSpan allowedVariance = TimeSpan.FromSeconds(30); // allow for processing time
        TimeSpan actualDifference = tokenExpiration - beforeRequest;
        TimeSpan expectedDifference = TimeSpan.FromMinutes(15);

        actualDifference.Should().BeCloseTo(expectedDifference, precision: allowedVariance);

        token.Header.Alg.Should().Be("HS256");
        token.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Username);
        token.Claims.Should().Contain(c => c.Type == "jti");
        token.Claims.Should().Contain(c => c.Type == "iss" && c.Value == "Lumina");
        token.Claims.Should().Contain(c => c.Type == "aud" && c.Value == "Lumina");

        content.Should().NotContain("password");
        content.Should().NotContain("hash");
        content.Should().NotContain("salt");

        // TODO: implement more strict security checks:
        //    // check security headers
        //    response.Headers.Should().ContainKey("X-Content-Type-Options")
        //        .WhoseValue.Should().Contain("nosniff");
        //    response.Headers.Should().ContainKey("X-Frame-Options")
        //        .WhoseValue.Should().Contain("DENY");
        //    response.Headers.Should().ContainKey("X-XSS-Protection")
        //        .WhoseValue.Should().Contain("1; mode=block");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldNotLeakTimingInformation()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        Stopwatch stopwatch = new();
        List<long> timings = [];
        // since this test is rigged to trigger the rate limiting, assign a different IP to it
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.2");

        // Act
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100); // add small delay between requests
            stopwatch.Restart();
            await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(
                Username: user.Username,
                Password: "TestPass123!"
            ));
            stopwatch.Stop();
            timings.Add(stopwatch.ElapsedMilliseconds);
        }
        // remove the highest and lowest values to account for outliers
        timings.Sort();
        timings = timings.Skip(1).SkipLast(1).ToList();

        // Assert
        // check for timing consistency to prevent timing attacks
        double stdDev = CalculateStandardDeviation(timings);
        stdDev.Should().BeLessThan(200); 
    }

    [Fact]
    public async Task ExecuteAsync_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
    {
        // Arrange
        // set a fake IP for this test instance
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.3");
        LoginRequest request = new(
            Username: "testuser",
            Password: "TestPass123!"
        );

        // Act
        List<HttpResponseMessage> responses = [];
        for (int i = 0; i < 11; i++) // exceed the 10 request limit
            responses.Add(await _client.PostAsJsonAsync("/api/v1/auth/login", request));

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

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("' WAITFOR DELAY '0:0:10'--")] // time-based injection
    public async Task Login_WithSQLInjectionAttempt_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        UserEntity legitimateUser = await CreateTestUser();
        LoginRequest request = new(
            Username: maliciousUsername,
            Password: "Abcd123$"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        content.Should().NotContain("SQL");
        content.Should().NotContain("Exception");
        content.Should().NotContain(legitimateUser.Username); // shouldn't expose other usernames
        content.Should().NotContain(legitimateUser.Password); // shouldn't expose password hashes
    }

    [Fact]
    public async Task Login_WithSQLInjectionInValidUser_ShouldRemainSecure()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest request = new(
            Username: user.Username,
            Password: "' OR '1'='1"
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.11");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        content.Should().NotContain("SQL");
        content.Should().NotContain("Exception");
    }

    [Fact]
    public async Task Login_WithInjectionInTotpCode_ShouldRemainSecure()
    {
        // Arrange
        UserEntity user = await CreateTestUserWithTotp();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TestPass123!",
            TotpCode: "' OR '1'='1"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        content.Should().NotContain("SQL");
        content.Should().NotContain("Exception");
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
            Created = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private async Task<UserEntity> CreateTestUserWithTotp()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        byte[] totpSecret = _totpTokenGenerator.GenerateSecret();
        UserEntity user = new()
        {
            Username = _testUsername,
            Password = _hashService.HashString("TestPass123!"),
            TotpSecret = _cryptographyService.Encrypt(Convert.ToBase64String(totpSecret)),
            Libraries = [],
            Created = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private async Task<UserEntity> CreateTestUserWithTempPassword()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = new()
        {
            Username = _testUsername,
            Password = _hashService.HashString("TestPass123!"),
            TempPassword = Uri.EscapeDataString(_hashService.HashString("TempPass123!")),
            TempPasswordCreated = DateTime.UtcNow,
            Libraries = [],
            Created = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private async Task<UserEntity> CreateTestUserWithExpiredTempPassword()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = new()
        {
            Username = _testUsername,
            Password = _hashService.HashString("TestPass123!"),
            TempPassword = Uri.EscapeDataString(_hashService.HashString("TempPass123!")),
            TempPasswordCreated = DateTime.UtcNow.AddMinutes(-16), // expired (> 15 minutes old)
            Libraries = [],
            Created = DateTime.UtcNow
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
