#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// Contains security tests for the <see cref="GetBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly LuminaApiFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBookEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetBook_WhenCalledWithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/books/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetBook_WithSQLInjectionInBookId_ShouldNotCorruptOrLeakData(string maliciousBookId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/books/{Uri.EscapeDataString(maliciousBookId)}");

        // Assert
        // The unparseable route value becomes an empty book Id, which the query validator reports as a missing book Id.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());

        await _apiFactory.RemoveTestUserAsync(username);
    }
}
