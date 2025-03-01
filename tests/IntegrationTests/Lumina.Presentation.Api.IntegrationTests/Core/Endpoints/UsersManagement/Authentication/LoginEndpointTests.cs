#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;
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
/// Contains integration tests for the <see cref="LoginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly PasswordHashService _hashService;
    private readonly ICryptographyService _cryptographyService;
    private readonly TotpTokenGenerator _totpTokenGenerator;
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
    public LoginEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
        _hashService = new PasswordHashService();
        _totpTokenGenerator = new TotpTokenGenerator();
        _testUsername = $"testuser_{Guid.NewGuid()}";

        using IServiceScope scope = apiFactory.Services.CreateScope();
        _cryptographyService = scope.ServiceProvider.GetRequiredService<ICryptographyService>();
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = _apiFactory.CreateClient();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidCredentials_ShouldLoginsuccessfuly()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TestPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(user.Username, result!.Username);
        Assert.False(result.UsesTotp);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidCredentialsAndTotp_ShouldLoginsuccessfuly()
    {
        // Arrange
        UserEntity user = await CreateTestUserWithTotp();
        byte[] totpSecret = Convert.FromBase64String(_cryptographyService.Decrypt(user.TotpSecret!));
        Totp totp = new(totpSecret);
        string validTotpCode = totp.ComputeTotp(DateTime.UtcNow);

        LoginRequest request = new(
            Username: user.Username,
            Password: "TestPass123!",
            TotpCode: validTotpCode
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(user.Username, result!.Username);
        Assert.True(result.UsesTotp);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithTempPassword_ShouldLoginsuccessfuly()
    {
        // Arrange
        UserEntity user = await CreateTestUserWithTempPassword();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TempPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(user.Username, result!.Username);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithExpiredTempPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        UserEntity user = await CreateTestUserWithExpiredTempPassword();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TempPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("TempPasswordExpired", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/login", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest request = new(
            Username: user.Username,
            Password: "WrongPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("InvalidUsernameOrPassword", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/login", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidTotpCode_ShouldReturnUnauthorized()
    {
        // Arrange
        UserEntity user = await CreateTestUserWithTotp();
        LoginRequest request = new(
            Username: user.Username,
            Password: "TestPass123!",
            TotpCode: "000000"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/auth/login", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("InvalidTotpCode", errors["General.Validation"]);
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
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
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
            UserPermissions = [],
            UserRole = null,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
