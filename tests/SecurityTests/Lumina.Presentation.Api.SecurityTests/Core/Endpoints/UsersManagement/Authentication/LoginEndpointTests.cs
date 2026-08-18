#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
public class LoginEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly PasswordHashService _hashService = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly ICryptographyService _cryptographyService;
    private readonly TotpTokenGenerator _totpTokenGenerator = new();
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public LoginEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // set a fake IP for this test instance
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.1");
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

        Assert.True(Math.Abs((actualDifference - expectedDifference).TotalSeconds) < allowedVariance.TotalSeconds);

        Assert.Equal("HS256", token.Header.Alg);
        Assert.Contains(token.Claims, c => c.Type == "sub" && c.Value == user.Id.ToString());
        Assert.Contains(token.Claims, c => c.Type == "unique_name" && c.Value == user.Username);
        Assert.Contains(token.Claims, c => c.Type == "jti");
        Assert.Contains(token.Claims, c => c.Type == "iss" && c.Value == "Lumina");
        Assert.Contains(token.Claims, c => c.Type == "aud" && c.Value == "Lumina");

        Assert.DoesNotContain("password", content);
        Assert.DoesNotContain("hash", content);
        Assert.DoesNotContain("salt", content);

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
    public async Task Login_WithUnknownUsernameAndWrongPassword_ShouldReturnIndistinguishableResponses()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest unknownUserRequest = new(
            Username: "nonexistent_user",
            Password: "TestPass123!"
        );
        LoginRequest wrongPasswordRequest = new(
            Username: user.Username,
            Password: "WrongPass123!"
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.2");

        // Act
        HttpResponseMessage unknownUserResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", unknownUserRequest);
        string unknownUserContent = await unknownUserResponse.Content.ReadAsStringAsync();
        HttpResponseMessage wrongPasswordResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPasswordRequest);
        string wrongPasswordContent = await wrongPasswordResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, unknownUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, wrongPasswordResponse.StatusCode);
        AssertProblemDetail(unknownUserContent, "General.Failure", "InvalidUsernameOrPassword");
        AssertProblemDetail(wrongPasswordContent, "General.Failure", "InvalidUsernameOrPassword");
    }

    [Fact]
    public async Task Login_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
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
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
        Assert.DoesNotContain(legitimateUser.Username, content); // shouldn't expose other usernames
        Assert.DoesNotContain(legitimateUser.Password, content); // shouldn't expose password hashes

        // the injected statement must never be executed: the Users table and the seeded user must still be there
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(user => user.Id == legitimateUser.Id));
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
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
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
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
    }

    private void AssertProblemDetail(string content, string expectedTitle, string expectedDetail)
    {
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(expectedTitle, problemDetails!["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
    }

    private async Task<UserEntity> CreateTestUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity user = _userEntityFixture.Create(username: _testUsername, password: _hashService.HashString("TestPass123!"));

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
