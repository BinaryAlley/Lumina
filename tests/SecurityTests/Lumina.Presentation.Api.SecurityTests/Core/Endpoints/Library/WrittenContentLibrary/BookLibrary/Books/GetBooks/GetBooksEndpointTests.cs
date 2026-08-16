#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Contains security tests for the <c>/books</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly PasswordHashService _hashService = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBooksEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/books");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/books", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Books--")] // destructive injection
    public async Task GetBooks_WhenSearchTermContainsSqlInjection_ShouldNotLeakOrError(string maliciousSearchTerm)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/books?libraryId={libraryId}&searchTerm={Uri.EscapeDataString(maliciousSearchTerm)}");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a test user, authenticates it on <paramref name="client"/>, and returns its Id.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>The Id of the created test user.</returns>
    private async Task<Guid> CreateAndAuthenticateUserAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.{Random.Shared.Next(1, 255)}");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"testuser_{Guid.NewGuid()}";
        dbContext.Users.Add(new UserEntity
        {
            Id = userId,
            Username = username,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(username, "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        return userId;
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(new LibraryEntity
        {
            Id = libraryId,
            UserId = userId,
            Title = "Test Library",
            LibraryType = LibraryType.EBook,
            ContentLocations = [],
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a <see cref="BookEntity"/> belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="title">The title of the book.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedBookAsync(Guid libraryId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Books.Add(new BookEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            Path = $"/books/{Guid.NewGuid()}.epub",
            Title = title,
            CreatedBy = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }
}
