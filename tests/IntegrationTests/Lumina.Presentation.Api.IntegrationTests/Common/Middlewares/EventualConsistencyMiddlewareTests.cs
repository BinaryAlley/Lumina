#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Common.Middlewares;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Middlewares;

/// <summary>
/// Contains integration tests for the <see cref="EventualConsistencyMiddleware"/> class through the real HTTP pipeline.
/// </summary>
[ExcludeFromCodeCoverage]
public class EventualConsistencyMiddlewareTests : IClassFixture<EventualConsistencyApiFactory>, IAsyncLifetime
{
    private const string FAVORITES_PATH = "/api/v1/users/me/favorites";
    private const string LOGIN_PATH = "/api/v1/auth/login";

    private static readonly TimeSpan s_probeTimeout = TimeSpan.FromSeconds(10);

    private HttpClient _client;
    private readonly EventualConsistencyApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly List<Guid> _seededLibraryIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="EventualConsistencyMiddlewareTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory with observable transactions and domain events.</param>
    public EventualConsistencyMiddlewareTests(EventualConsistencyApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    /// <summary>
    /// Initializes the authenticated API client, and clears the probes after the login request completed.
    /// </summary>
    public async Task InitializeAsync()
    {
        _apiFactory.ResetProbes();
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        // The login request runs through the same middleware, so its dequeue of the response completion must finish before the probes observe the test request.
        await _apiFactory.QueueProbe.WaitForDequeueAsync(LOGIN_PATH, s_probeTimeout);
        _apiFactory.ResetProbes();
    }

    [Fact]
    public async Task AddFavorite_WhenTheResponseSucceeds_ShouldPublishTheQueuedEventAfterCommittingTheTransaction()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid bookId = await SeedBookAsync(userId);
        _apiFactory.ResetProbes();
        _apiFactory.QueueProbe.ShouldBlockOnEnqueue = true;
        Task<HttpResponseMessage> responseTask;

        try
        {
            // Act
            // The POST runs on a dedicated task, so that a blocking domain event queue can never block the test thread that must release it.
            responseTask = Task.Run(async () => await _client.PostAsJsonAsync(FAVORITES_PATH, new { mediaKind = "Book", mediaItemId = bookId }));
            await _apiFactory.QueueProbe.WaitForEnqueueAsync(s_probeTimeout);

            // Assert
            // The event is queued while the endpoint still runs, so it must not be published yet.
            Assert.Equal(0, _apiFactory.PublicationProbe.PublicationCount);
        }
        finally
        {
            _apiFactory.QueueProbe.ReleaseEnqueue();
            _apiFactory.QueueProbe.ShouldBlockOnEnqueue = false;
        }

        HttpResponseMessage response = await responseTask;
        await _apiFactory.PublicationProbe.WaitForPublicationAsync(s_probeTimeout);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(_apiFactory.PublicationProbe.WasFavoriteVisible);
        Assert.Equal(1, _apiFactory.QueueProbe.EnqueueCount);
        Assert.Equal(1, _apiFactory.QueueProbe.DequeueCount);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.True(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == bookId));
    }

    [Fact]
    public async Task AddFavorite_WhenTheRequestFailsAfterQueueingTheEvent_ShouldRollBackTheTransactionAndNotPublishTheEvent()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid bookId = await SeedBookAsync(userId);
        _apiFactory.ResetProbes();
        _apiFactory.QueueProbe.ShouldThrowOnEnqueue = true;

        try
        {
            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync(FAVORITES_PATH, new { mediaKind = "Book", mediaItemId = bookId });
            await _apiFactory.TransactionProbe.WaitForRollbackAsync(FAVORITES_PATH, s_probeTimeout);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(1, _apiFactory.QueueProbe.EnqueueCount);
            Assert.Equal(0, _apiFactory.QueueProbe.DequeueCount);
            Assert.Equal(0, _apiFactory.QueueProbe.GetDequeueAttemptCount(FAVORITES_PATH));
            Assert.Equal(0, _apiFactory.PublicationProbe.PublicationCount);

            using IServiceScope scope = _apiFactory.Services.CreateScope();
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            Assert.False(await dbContext.Favorites.AnyAsync(favorite => favorite.UserId == userId && favorite.MediaItemId == bookId));
        }
        finally
        {
            _apiFactory.QueueProbe.ShouldThrowOnEnqueue = false;
        }
    }

    [Fact]
    public async Task GetFavorites_WhenCalled_ShouldRunThroughTheEventualConsistencyMiddleware()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(FAVORITES_PATH);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // The middleware dequeues the events only after committing a successful request, so a dequeue attempt for this path proves the pipeline ran through it.
        await _apiFactory.QueueProbe.WaitForDequeueAsync(FAVORITES_PATH, s_probeTimeout);
        Assert.True(_apiFactory.QueueProbe.GetDequeueAttemptCount(FAVORITES_PATH) > 0);
    }

    /// <summary>
    /// Seeds a book of a book library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the seeded library.</param>
    /// <returns>The Id of the seeded book.</returns>
    private async Task<Guid> SeedBookAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.CreateVersion7();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(
            id: libraryId,
            userId: userId,
            title: $"Eventual Consistency Library-{Guid.NewGuid():N}",
            libraryType: LibraryType.EBook,
            contentLocations: []));
        Guid bookId = Guid.CreateVersion7();
        dbContext.Books.Add(_bookEntityFixture.Create(
            id: bookId,
            libraryId: libraryId,
            title: $"Eventual Consistency Book-{Guid.NewGuid():N}",
            includeMetadata: false));
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        return bookId;
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> libraryIds = [.. _seededLibraryIds];
            List<Guid> bookIds = await dbContext.Books.Where(book => libraryIds.Contains(book.LibraryId)).Select(book => book.Id).ToListAsync();
            Guid? userId = await dbContext.Users
                .Where(user => user.Username == _apiFactory.TestUsername)
                .Select(user => (Guid?)user.Id)
                .FirstOrDefaultAsync();
            if (userId is not null)
                dbContext.Favorites.RemoveRange(dbContext.Favorites.Where(favorite => favorite.UserId == userId.Value));
            dbContext.Books.RemoveRange(dbContext.Books.Where(book => bookIds.Contains(book.Id)));
            dbContext.Libraries.RemoveRange(dbContext.Libraries.Where(library => libraryIds.Contains(library.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
