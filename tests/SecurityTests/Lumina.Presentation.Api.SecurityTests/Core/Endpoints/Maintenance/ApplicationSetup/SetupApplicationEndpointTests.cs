#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains security tests for the <c>/initialization</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly RegistrationRequestFixture _registrationRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetupApplicationEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task SetupApplication_WhenCalledWithoutAuthentication_ShouldBeAccessible()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create(
            username: $"testuser_{Guid.NewGuid()}",
            password: "TestPass123!",
            passwordConfirm: "TestPass123!",
            use2fa: false
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // the setup endpoint is anonymous, so it must never demand authentication; the admin account is created once, so
        // the request either creates it or reports that it already exists, depending on the shared database state of the class
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Conflict);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    public async Task SetupApplication_WithSQLInjectionAttempt_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        // note: the setup validator only checks that the username is not empty, so the malicious value reaches the handler,
        // where it is used as a literal username in parameterized queries; the security guarantee is that it never
        // executes as SQL or leaks the query internals
        RegistrationRequest request = _registrationRequestFixture.Create(
            username: maliciousUsername,
            password: "TestPass123!",
            passwordConfirm: "TestPass123!",
            use2fa: false
        );
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/initialization", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // the malicious username is treated as literal data (the setup either creates the admin or reports that one already exists),
        // so the request must never fail with an unhandled exception or leak the query internals
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Conflict);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
