#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using Nemera.DataAccess.Core.UoW;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Contains security tests for the <see cref="AddLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointTests : IClassFixture<NemeraApiFactory>
{
    private readonly HttpClient _client;
    private readonly NemeraApiFactory _apiFactory;
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddLibraryEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task AddLibrary_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "New Library",
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/libraries", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // Basic SQL injection.
    [InlineData("'; DROP TABLE Libraries--")] // Destructive injection.
    public async Task AddLibrary_WithSQLInjectionInTitle_ShouldNotLeakOrError(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        // The malicious title passes the authenticated handler, reaches the parameterized data access, and is persisted verbatim.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Title == maliciousTitle);
        Assert.NotNull(storedLibrary);
        Assert.Equal(maliciousTitle, storedLibrary!.Title);
    }

    [Theory]
    [InlineData(-1)] // Just below the allowed range.
    [InlineData(101)] // Just above the allowed range.
    [InlineData(int.MinValue)] // Lowest possible value.
    [InlineData(int.MaxValue)] // Highest possible value.
    public async Task AddLibrary_WhenThresholdIsOutsideAllowedRange_ShouldRejectTheRequestWithoutPersisting(int completionThresholdPercentage)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        string title = $"Out Of Range Library {completionThresholdPercentage}";
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: title,
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        string content = await AssertValidationProblemAsync(response, "CompletionThresholdPercentageMustBeBetweenZeroAndOneHundred");
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Assert.False(await dbContext.Libraries.AnyAsync(library => library.Title == title));
    }

    [Fact]
    public async Task AddLibrary_WhenThresholdIsSuppliedWithProgressTrackingDisabled_ShouldRejectTheRequestWithoutPersisting()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        string title = "Disabled Tracking Library";
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: title,
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        string content = await AssertValidationProblemAsync(response, "CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff");
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Assert.False(await dbContext.Libraries.AnyAsync(library => library.Title == title));
    }

    /// <summary>
    /// Asserts that <paramref name="response"/> carries the standard validation problem of the add library route.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedErrorDescription">The error description expected under the validation errors.</param>
    /// <returns>The raw response body, so that leak assertions can be applied to it.</returns>
    private async Task<string> AssertValidationProblemAsync(HttpResponseMessage response, string expectedErrorDescription)
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
        Assert.Equal("/api/v1/libraries", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors!.Keys);
        Assert.Contains(expectedErrorDescription, errors["General.Validation"]);

        return content;
    }
}
