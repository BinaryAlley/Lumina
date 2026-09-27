#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Contains security tests for the <see cref="UpdateBookCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly LuminaApiFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateBookCoverEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateBookCover_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/books/{bookId}/cover", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Fact]
    public async Task UpdateBookCover_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, ownerId);
        await _apiFactory.SeedBookAsync(libraryId, "Other User Book");
        Guid bookId;
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            bookId = dbContext.Books.Single(book => book.LibraryId == libraryId).Id;
        }

        HttpClient requesterClient = _apiFactory.CreateClient();
        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(requesterClient);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await requesterClient.PutAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
        await _apiFactory.RemoveTestUserAsync(requesterUsername);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateBookCover_WithSQLInjectionInRouteId_ShouldNotCorruptOrLeakData(string maliciousBookId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{Uri.EscapeDataString(maliciousBookId)}/cover", form);

        // Assert
        // The unparseable route value becomes an empty book Id, which the command validator reports as a missing book Id.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Fact]
    public async Task UpdateBookCover_WithMaliciousFileName_ShouldNotCorruptOrLeakData()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        MultipartFormDataContent form = CreateCoverForm("..\\..\\evil.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{Guid.NewGuid()}/cover", form);

        // Assert
        // The book does not exist, so the handler returns a clean not found response before the uploaded file name is used.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    /// <summary>
    /// Creates a multipart form carrying a minimal cover payload with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>A configured <see cref="MultipartFormDataContent"/> instance.</returns>
    private static MultipartFormDataContent CreateCoverForm(string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return form;
    }
}
