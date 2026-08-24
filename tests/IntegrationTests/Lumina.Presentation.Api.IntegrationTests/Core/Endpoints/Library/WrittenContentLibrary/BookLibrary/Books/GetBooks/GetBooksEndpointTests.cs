#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Contains integration tests for the <see cref="GetBooksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookRatingEntityFixture _bookRatingEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly IsbnEntityFixture _isbnEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBooksEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetBooks_WhenCalledWithoutPaginationData_ShouldReturnAllBooks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Book A");
        await SeedBookAsync(libraryId, "Book B");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(2, paginatedBooks!.Count);
        Assert.Equal(2, paginatedBooks.Data.Count);
        Assert.Equal(1, paginatedBooks.CurrentPage);
        Assert.Equal(2, paginatedBooks.PerPage);
        Assert.Equal(1, paginatedBooks.NumberOfPages);
        Assert.Contains(paginatedBooks.Data, book => book.Metadata.Title == "Book A");
        Assert.Contains(paginatedBooks.Data, book => book.Metadata.Title == "Book B");
    }

    [Fact]
    public async Task GetBooks_WhenCalledWithSearchTerm_ShouldReturnOnlyMatchingBooks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "The Fellowship of the Ring");
        await SeedBookAsync(libraryId, "The Two Towers");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}&searchTerm={Uri.EscapeDataString("Fellowship")}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        BookResponse book = Assert.Single(paginatedBooks!.Data);
        Assert.Equal("The Fellowship of the Ring", book.Metadata.Title);
        Assert.Equal(1, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooks_WhenCalledWithPaginationData_ShouldReturnRequestedPage()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        await SeedBookAsync(libraryId, "Book A");
        await SeedBookAsync(libraryId, "Book B");
        await SeedBookAsync(libraryId, "Book C");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}&currentPage=2&perPage=2");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(3, paginatedBooks!.Count);
        Assert.Equal(2, paginatedBooks.NumberOfPages);
        Assert.Equal(2, paginatedBooks.CurrentPage);
        Assert.Equal(2, paginatedBooks.PerPage);
        BookResponse book = Assert.Single(paginatedBooks.Data);
        Assert.Equal("Book C", book.Metadata.Title);
    }

    [Fact]
    public async Task GetBooks_WhenNoBooksExist_ShouldReturnEmptyPaginatedResponse()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Empty(paginatedBooks!.Data);
        Assert.Equal(0, paginatedBooks.Count);
    }

    [Fact]
    public async Task GetBooks_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.GetAsync($"/api/v1/books?libraryId={libraryId}", cts.Token)
        );
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetBooks_WhenCancellationTokenIsCanceled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        using CancellationTokenSource cts = new();

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/books?libraryId={libraryId}", cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
    }

    [Fact]
    public async Task GetBooks_WhenLibraryBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedUserAsync();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, otherUserId);
        await SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WhenLibraryDoesNotExist_ShouldReturnForbidden()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WhenCalledByAdmin_ShouldReturnBooksOfAnyLibrary()
    {
        // Arrange
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        Guid otherUserId = await SeedUserAsync();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, otherUserId);
        await SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await adminClient.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        Assert.Equal(1, paginatedBooks!.Count);
    }

    [Fact]
    public async Task GetBooks_WhenBooksHaveTagsGenresIsbnsAndRatings_ShouldReturnThemInTheResponse()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        BookEntity seededBook = await SeedBookAsync(libraryId, "Tags And Ratings Book");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books?libraryId={libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        PaginatedResponse<BookResponse>? paginatedBooks = await response.Content.ReadFromJsonAsync<PaginatedResponse<BookResponse>>(_jsonOptions);
        Assert.NotNull(paginatedBooks);
        BookResponse book = Assert.Single(paginatedBooks!.Data);
        Assert.Equal(seededBook.Title, book.Metadata.Title);
        Assert.Equal(seededBook.Tags.Select(tag => tag.Name), book.Metadata.Tags.Select(tag => tag.Name));
        Assert.Equal(seededBook.Genres.Select(genre => genre.Name), book.Metadata.Genres.Select(genre => genre.Name));
        Assert.Equal(seededBook.ISBNs.Select(isbn => isbn.Value), book.ISBNs.Select(isbn => isbn.Value));
        Assert.Equal(seededBook.Ratings.Select(rating => rating.Value), book.Ratings.Select(rating => rating.Value));
        Assert.Equal(seededBook.Ratings.Single().Source, book.Ratings.Single().Source);
    }

    /// <summary>
    /// Seeds a <see cref="BookEntity"/> belonging to the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="title">The title of the book.</param>
    /// <returns>The seeded book entity.</returns>
    private async Task<BookEntity> SeedBookAsync(Guid libraryId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, title: title, path: $"/books/{Guid.NewGuid()}.epub", includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: $"Tag-{Guid.NewGuid()}")];
        book.Genres = [_genreEntityFixture.Create(name: $"Genre-{Guid.NewGuid()}")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9780395272237", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return book;
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
