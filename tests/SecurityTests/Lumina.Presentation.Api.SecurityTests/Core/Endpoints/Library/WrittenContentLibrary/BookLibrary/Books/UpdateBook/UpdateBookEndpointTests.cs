#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;

/// <summary>
/// Contains security tests for the <see cref="UpdateBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateBookEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        UpdateBookRequest request = _updateBookRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/books/{request.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateBook_WithSQLInjectionInBody_ShouldNotCorruptOrLeakData(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        UpdateBookRequest request = _updateBookRequestFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: maliciousTitle));

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/books/{request.Id}", request);

        // Assert
        // The injected title is validated and handled by the parameterized update flow: the route book does not exist,
        // so the request fails with a clean not found response and the injected payload is never executed as SQL.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousTitle, content, StringComparison.Ordinal);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateBook_WithSQLInjectionInRouteId_ShouldNeverCorruptOrLeakData(string maliciousBookId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        UpdateBookRequest request = _updateBookRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/books/{Uri.EscapeDataString(maliciousBookId)}", request);

        // Assert
        // The route is authoritative and its raw value is kept as a string, so an unparseable route Id reaches the command
        // mapping and becomes Guid.Empty, which the command validator reports as a clean validation error. The response must
        // not leak stack traces, database details, or the injected payload, and the body Id never redirects the update.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousBookId, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("BookIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Fact]
    public async Task UpdateBook_WhenRouteBookDoesNotExistEvenIfBodyIdDiffers_ShouldReturnNotFound()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/books/{routeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("BookNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        await _apiFactory.RemoveTestUserAsync(username);
    }
}
