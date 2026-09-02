#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;

/// <summary>
/// Contains integration tests for the <see cref="GetBooksLiteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBooksLiteEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithoutPaginationData_ShouldReturnAllBooks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Book A");
        await SeedBookAsync(libraryId, "Book B");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(2, paginatedBooks!.Count);
        Assert.Equal(2, paginatedBooks.Data.Count());
        Assert.Equal(1, paginatedBooks.CurrentPage);
        Assert.Equal(2, paginatedBooks.PerPage);
        Assert.Equal(1, paginatedBooks.NumberOfPages);
        Assert.Contains(paginatedBooks.Data, book => book.Title == "Book A");
        Assert.Contains(paginatedBooks.Data, book => book.Title == "Book B");
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithFilterAlphaKeyLetter_ShouldReturnOnlyBooksStartingWithThatLetter()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Quiet Place");
        await SeedBookAsync(libraryId, "The Quiet");
        await SeedBookAsync(libraryId, "Racing");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&filterAlphaKey=q");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        BookLiteResponse book = Assert.Single(paginatedBooks!.Data);
        Assert.Equal("Quiet Place", book.Title);
        Assert.Equal(1, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithFilterAlphaKeyLetterAndIgnoreThePrefix_ShouldIncludeThePrefixedTitles()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Quiet Place");
        await SeedBookAsync(libraryId, "The Quiet");
        await SeedBookAsync(libraryId, "Racing");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&filterAlphaKey=q&shouldIgnoreThePrefixForAlphaPicker=true");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(2, paginatedBooks!.Count);
        Assert.Contains(paginatedBooks.Data, book => book.Title == "Quiet Place");
        Assert.Contains(paginatedBooks.Data, book => book.Title == "The Quiet");
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithFilterAlphaKeyNumber_ShouldReturnOnlyBooksStartingWithADigit()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "1984");
        await SeedBookAsync(libraryId, "7 Habits of Highly Effective People");
        await SeedBookAsync(libraryId, "Brave New World");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&filterAlphaKey=%23");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(2, paginatedBooks!.Count);
        Assert.Contains(paginatedBooks.Data, book => book.Title == "1984");
        Assert.Contains(paginatedBooks.Data, book => book.Title == "7 Habits of Highly Effective People");
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithFilterAlphaKeySymbol_ShouldReturnOnlyBooksStartingWithASymbol()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "!Important");
        await SeedBookAsync(libraryId, "1984");
        await SeedBookAsync(libraryId, "Alpha");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&filterAlphaKey=%2A");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        BookLiteResponse book = Assert.Single(paginatedBooks!.Data);
        Assert.Equal("!Important", book.Title);
        Assert.Equal(1, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooksLite_WhenTitleIsEmptyAndFilterAlphaKeyIsProvided_ShouldUseOriginalTitle()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "", "Rendezvous");
        await SeedBookAsync(libraryId, "Racing");
        await SeedBookAsync(libraryId, "Quiet Place");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&filterAlphaKey=r");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(2, paginatedBooks!.Count);
        Assert.Contains(paginatedBooks.Data, book => book.Title == ""); // The empty title book matches the filter through its original title.
        Assert.Contains(paginatedBooks.Data, book => book.Title == "Racing");
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithSearchTerm_ShouldReturnOnlyMatchingBooks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "The Fellowship of the Ring");
        await SeedBookAsync(libraryId, "The Two Towers");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&searchTerm={Uri.EscapeDataString("Fellowship")}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        BookLiteResponse book = Assert.Single(paginatedBooks!.Data);
        Assert.Equal("The Fellowship of the Ring", book.Title);
        Assert.Equal(1, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithSearchTermAndFilterAlphaKey_ShouldReturnOnlyBooksSatisfyingBothFilters()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        // The search term matches only one book, while the alpha key matches only the other, so no book satisfies both filters at once.
        await SeedBookAsync(libraryId, "The Fellowship of the Ring");
        await SeedBookAsync(libraryId, "Zoo");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&searchTerm={Uri.EscapeDataString("Fellowship")}&filterAlphaKey=z");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Empty(paginatedBooks!.Data);
        Assert.Equal(0, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledWithPaginationData_ShouldReturnRequestedPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Book A");
        await SeedBookAsync(libraryId, "Book B");
        await SeedBookAsync(libraryId, "Book C");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&currentPage=2&perPage=2");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(3, paginatedBooks!.Count);
        Assert.Equal(2, paginatedBooks.NumberOfPages);
        Assert.Equal(2, paginatedBooks.CurrentPage);
        Assert.Equal(2, paginatedBooks.PerPage);
        BookLiteResponse book = Assert.Single(paginatedBooks.Data);
        Assert.Equal("Book C", book.Title);
    }

    [Fact]
    public async Task GetBooksLite_WhenNoBooksExist_ShouldReturnEmptyPaginatedResponse()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Empty(paginatedBooks!.Data);
        Assert.Equal(0, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooksLite_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedUserAsync();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, otherUserId);
        await SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksLite_WhenLibraryDoesNotExist_ShouldReturnForbidden()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksLite_WhenCalledByAdmin_ShouldReturnBooksOfAnyLibrary()
    {
        // Arrange
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        Guid otherUserId = await SeedUserAsync();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, otherUserId);
        await SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await adminClient.GetAsync($"/api/v1/books/lite?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookLiteResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookLiteResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(1, paginatedBooks!.Count);
    }

    /// <summary>
    /// Seeds a <see cref="BookEntity"/> belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="title">The title of the book.</param>
    /// <param name="originalTitle">Optional. The original title of the book.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedBookAsync(Guid libraryId, string title, string? originalTitle = null)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Books.Add(_bookEntityFixture.Create(libraryId: libraryId, title: title, originalTitle: originalTitle, path: $"/books/{Guid.NewGuid()}.epub", includeMetadata: false));
        await dbContext.SaveChangesAsync();
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
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a <see cref="UserEntity"/> and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return userId;
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
