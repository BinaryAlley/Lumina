#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.UpdateLibrary;

/// <summary>
/// Contains security tests for the <see cref="UpdateLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UpdateLibraryRequestFixture _updateLibraryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateLibraryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateLibrary_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            id: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            title: "Updated Library",
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Libraries--")] // destructive injection
    public async Task UpdateLibrary_WithSQLInjectionInTitle_ShouldNotLeakOrError(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            id: libraryId,
            userId: userId,
            title: maliciousTitle,
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        // the malicious title passes the authenticated handler, reaches the parameterized data access, and is persisted verbatim
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Id == libraryId);
        Assert.NotNull(storedLibrary);
        Assert.Equal(maliciousTitle, storedLibrary!.Title);
    }

    [Theory]
    [InlineData(-1)] // just below the allowed range
    [InlineData(101)] // just above the allowed range
    [InlineData(int.MinValue)] // lowest possible value
    [InlineData(int.MaxValue)] // highest possible value
    public async Task UpdateLibrary_WhenThresholdIsOutsideAllowedRange_ShouldRejectTheRequestAndKeepTheStoredSettings(int completionThresholdPercentage)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 70);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            id: libraryId,
            userId: userId,
            title: "Test Library",
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: completionThresholdPercentage
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"/api/v1/libraries/{libraryId}", "CompletionThresholdPercentageMustBeBetweenZeroAndOneHundred");
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Id == libraryId);
        Assert.NotNull(storedLibrary);
        Assert.True(storedLibrary!.ShouldTrackConsumptionProgress);
        Assert.Equal(70, storedLibrary.CompletionThresholdPercentage);
    }

    [Fact]
    public async Task UpdateLibrary_WhenThresholdIsSuppliedWithProgressTrackingDisabled_ShouldRejectTheRequestAndKeepTheStoredSettings()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 70);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            id: libraryId,
            userId: userId,
            title: "Test Library",
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true,
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: 50
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        string content = await AssertValidationProblemAsync(response, $"/api/v1/libraries/{libraryId}", "CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff");
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Id == libraryId);
        Assert.NotNull(storedLibrary);
        Assert.True(storedLibrary!.ShouldTrackConsumptionProgress);
        Assert.Equal(70, storedLibrary.CompletionThresholdPercentage);
    }

    [Fact]
    public async Task UpdateLibrary_WhenNonOwnerTriesToChangeConsumptionSettings_ShouldNotApplyThem()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, ownerId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 70);
        // Authenticating a second user on the same client replaces the bearer token, so the update below is sent as the non-owner.
        (Guid _, string attackerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            id: libraryId,
            userId: ownerId,
            title: "Test Library",
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true,
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: null
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        // The update handler authorizes after loading the library and answers a non-owner with the NotAuthorized application error,
        // which the shared problem mapping exposes as 403 Forbidden (ownership is a permission problem, not a missing resource).
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Id == libraryId);
            Assert.NotNull(storedLibrary);
            Assert.True(storedLibrary!.ShouldTrackConsumptionProgress);
            Assert.Equal(70, storedLibrary.CompletionThresholdPercentage);
        }

        await _apiFactory.RemoveTestUserAsync(attackerUsername);
        await _apiFactory.RemoveTestUserAsync(ownerUsername);
    }

    /// <summary>
    /// Asserts that <paramref name="response"/> carries the standard validation problem of the update library route.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedInstance">The expected instance path of the problem.</param>
    /// <param name="expectedErrorDescription">The error description expected under the validation errors.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorDescription)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorDescription, errors["General.Validation"]);

        return content;
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the library.</param>
    /// <param name="shouldTrackConsumptionProgress">Whether the seeded library tracks the consumption progress of its items.</param>
    /// <param name="completionThresholdPercentage">The completion threshold percentage of the seeded library, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(
        Guid libraryId,
        Guid userId,
        string title = "Test Library",
        bool shouldTrackConsumptionProgress = false,
        int? completionThresholdPercentage = null)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId,
            userId: userId,
            title: title,
            libraryType: LibraryType.EBook,
            contentLocations: [],
            shouldTrackConsumptionProgress: shouldTrackConsumptionProgress,
            completionThresholdPercentage: completionThresholdPercentage));
        await dbContext.SaveChangesAsync();
    }
}
