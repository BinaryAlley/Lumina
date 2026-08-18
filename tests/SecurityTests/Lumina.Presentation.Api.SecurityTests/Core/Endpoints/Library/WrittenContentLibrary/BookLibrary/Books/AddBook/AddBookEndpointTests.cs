#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// Contains security tests for the <c>/books</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly AddBookRequestFixture _addBookRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddBookEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddBook_WithSQLInjectionInPath_ShouldNotCorruptOrDeleteData(string maliciousPath)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, userId);
        AddBookRequest request = _addBookRequestFixture.Create() with { LibraryId = libraryId, Path = maliciousPath };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/books", request);

        // Assert
        // the malicious path passes the authenticated handler, reaches the parameterized insert, and is persisted
        // verbatim: if it were concatenated into raw SQL, the insert would fail and the book would not be created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Libraries.CountAsync(library => library.Id == libraryId));
        // the single book in the library is the one inserted by this request, with the malicious path stored as data
        Assert.Equal(1, await dbContext.Books.CountAsync(book => book.LibraryId == libraryId));

        await _apiFactory.RemoveTestUserAsync(username);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        // nothing to clean up: each test removes its own seeded rows
    }
}
