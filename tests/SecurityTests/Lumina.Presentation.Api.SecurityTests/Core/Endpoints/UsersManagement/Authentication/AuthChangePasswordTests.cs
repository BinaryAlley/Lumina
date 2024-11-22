#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Authentication;

/// <summary>
/// Contains security tests for the <c>/auth/change-password</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthChangePasswordTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly HashService _hashService;
    private readonly string _testUsername;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthLoginTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AuthChangePasswordTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.12");
        _hashService = new HashService();
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        ChangePasswordRequest request = new(
            Username: _testUsername,
            CurrentPassword: "OldPass123!",
            NewPassword: "NewPass123!",
            NewPasswordConfirm: "NewPass123!"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithValidRequest_ShouldNotLeakTimingInformation()
    {
        // Arrange
        UserEntity user = await CreateAndAuthenticateUser();
        Stopwatch stopwatch = new();
        List<long> timings = [];

        // Act
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            stopwatch.Restart();
            await _client.PostAsJsonAsync("/api/v1/auth/change-password", new ChangePasswordRequest(
                Username: user.Username,
                CurrentPassword: "WrongPass123!",
                NewPassword: "NewPass123!",
                NewPasswordConfirm: "NewPass123!"
            ));
            stopwatch.Stop();
            timings.Add(stopwatch.ElapsedMilliseconds);
        }

        timings.Sort();
        timings = timings.Skip(1).SkipLast(1).ToList(); // remove outliers

        // Assert
        double stdDev = CalculateStandardDeviation(timings);
        stdDev.Should().BeLessThan(200);
    }

    private double CalculateStandardDeviation(List<long> values)
    {
        double average = values.Average();
        double sumOfSquaresOfDifferences = values.Select(value => (value - average) * (value - average)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
    }

    private async Task<UserEntity> CreateAndAuthenticateUser()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // create test user
        UserEntity user = new()
        {
            Username = _testUsername,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserRoles = [],
            UserPermissions = [],
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // authenticate user
        LoginRequest loginRequest = new(
            Username: user.Username,
            Password: "TestPass123!"
        );

        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? result = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        // set auth header for subsequent requests
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

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

        _client.DefaultRequestHeaders.Authorization = null;
    }
}
