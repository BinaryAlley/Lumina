#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Common.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="EventualConsistencyMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EventualConsistencyMiddlewareTests
{
    private readonly EventualConsistencyMiddleware _sut;
    private bool _nextInvoked;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventualConsistencyMiddlewareTests"/> class.
    /// </summary>
    public EventualConsistencyMiddlewareTests()
    {
        ILogger<EventualConsistencyMiddleware> mockLogger = Substitute.For<ILogger<EventualConsistencyMiddleware>>();
        _sut = new EventualConsistencyMiddleware(
            context =>
            {
                _nextInvoked = true;
                return Task.CompletedTask;
            },
            mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_WhenEndpointOptsOutThroughMetadata_ShouldInvokeNextWithoutTouchingTheDatabase()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.SetEndpoint(new Endpoint(
            requestDelegate: null,
            new EndpointMetadataCollection(new SkipEventualConsistencyMetadata()),
            displayName: "GetTrackStream"));

        // Act
        // The database, the domain event publisher and the domain events queue are deliberately not supplied: an endpoint that opted out through
        // metadata must invoke the next delegate without touching any of them, which is exactly what a long lived stream needs.
        await _sut.InvokeAsync(context, null!, null!, null!);

        // Assert
        Assert.True(_nextInvoked);
    }

    [Fact]
    public async Task InvokeAsync_WhenTheClientAbortsAfterASuccessfulOperation_ShouldStillCommitTheTransaction()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-eventual-consistency-abort-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext dbContext = CreateContext(anchorConnection);
        using UnitOfWork unitOfWork = new(dbContext);
        await dbContext.Database.ExecuteSqlRawAsync("CREATE TABLE CommitProbe (Id INTEGER NOT NULL PRIMARY KEY);");
        CapturingHttpResponseFeature responseFeature = new();
        DefaultHttpContext httpContext = new();
        httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
        httpContext.Request.Path = "/api/v1/probe";
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        using CancellationTokenSource cancellationTokenSource = new();
        httpContext.RequestAborted = cancellationTokenSource.Token;
        ILogger<EventualConsistencyMiddleware> mockLogger = Substitute.For<ILogger<EventualConsistencyMiddleware>>();
        EventualConsistencyMiddleware sut = new(
            async requestContext =>
            {
                await dbContext.Database.ExecuteSqlRawAsync("INSERT INTO CommitProbe (Id) VALUES (1);");
                // The abort happens after the operation has completed, which is exactly when the response token can already be cancelled as the completion callback runs.
                cancellationTokenSource.Cancel();
            },
            mockLogger);
        IDomainEventPublisher mockPublisher = Substitute.For<IDomainEventPublisher>();
        IDomainEventsQueue mockQueue = Substitute.For<IDomainEventsQueue>();

        // Act
        await sut.InvokeAsync(httpContext, mockPublisher, unitOfWork, mockQueue);
        await responseFeature.InvokeCompletedCallbacksAsync();

        // Assert
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        int committedCount = await verificationContext.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM CommitProbe")
            .SingleAsync();
        Assert.Equal(1, committedCount);
    }

    [Fact]
    public async Task InvokeAsync_WhenTheResponseIsNotSuccessful_ShouldRollBackTheTransactionAndTheEvents()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-eventual-consistency-rollback-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext dbContext = CreateContext(anchorConnection);
        using UnitOfWork unitOfWork = new(dbContext);
        await dbContext.Database.ExecuteSqlRawAsync("CREATE TABLE CommitProbe (Id INTEGER NOT NULL PRIMARY KEY);");
        CapturingHttpResponseFeature responseFeature = new();
        DefaultHttpContext httpContext = new();
        httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
        httpContext.Request.Path = "/api/v1/probe";
        ILogger<EventualConsistencyMiddleware> mockLogger = Substitute.For<ILogger<EventualConsistencyMiddleware>>();
        EventualConsistencyMiddleware sut = new(
            async requestContext =>
            {
                await dbContext.Database.ExecuteSqlRawAsync("INSERT INTO CommitProbe (Id) VALUES (1);");
                // A client abort is reported by the exception handling middleware as a non-success status, even though the request reached the database.
                requestContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            },
            mockLogger);
        IDomainEventPublisher mockPublisher = Substitute.For<IDomainEventPublisher>();
        IDomainEventsQueue mockQueue = Substitute.For<IDomainEventsQueue>();

        // Act
        await sut.InvokeAsync(httpContext, mockPublisher, unitOfWork, mockQueue);
        await responseFeature.InvokeCompletedCallbacksAsync();

        // Assert
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        int committedCount = await verificationContext.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM CommitProbe")
            .SingleAsync();
        Assert.Equal(0, committedCount);
        await mockPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenTheCommitFails_ShouldRollBackTheTransactionAndLogTheFailure()
    {
        // Arrange
        CapturingHttpResponseFeature responseFeature = new();
        DefaultHttpContext httpContext = new();
        httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
        httpContext.Request.Path = "/api/v1/probe";
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        ILogger<EventualConsistencyMiddleware> mockLogger = Substitute.For<ILogger<EventualConsistencyMiddleware>>();
        EventualConsistencyMiddleware sut = new(_ => Task.CompletedTask, mockLogger);
        IUnitOfWork mockUnitOfWork = Substitute.For<IUnitOfWork>();
        mockUnitOfWork
            .CommitTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("commit failed")));
        IDomainEventPublisher mockPublisher = Substitute.For<IDomainEventPublisher>();
        IDomainEventsQueue mockQueue = Substitute.For<IDomainEventsQueue>();

        // Act
        await sut.InvokeAsync(httpContext, mockPublisher, mockUnitOfWork, mockQueue);
        await responseFeature.InvokeCompletedCallbacksAsync();

        // Assert
        // The completion callback must never leave a transaction open after a failed commit, and it must never publish events that belong to a transaction that did not commit.
        await mockUnitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await mockPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Creates a context backed by the provided in-memory SQLite connection.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateContext(SqliteConnection anchorConnection)
    {
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Response feature that captures the completed callbacks so a test can run them when the response ends.
    /// </summary>
    private sealed class CapturingHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<Func<Task>> _completedCallbacks = [];

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        /// <summary>
        /// Gets or sets the reason phrase of the response.
        /// </summary>
        public string? ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        /// <summary>
        /// Gets or sets the response body stream.
        /// </summary>
        public Stream Body { get; set; } = Stream.Null;

        /// <summary>
        /// Gets a value indicating whether the response has already started.
        /// </summary>
        public bool HasStarted => false;

        /// <summary>
        /// Registers a callback to be invoked when the response is about to start.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        /// <summary>
        /// Registers a callback to be invoked when the response has completed.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnCompleted(Func<object, Task> callback, object state)
        {
            _completedCallbacks.Add(() => callback(state));
        }

        /// <summary>
        /// Invokes every callback that was registered through <see cref="OnCompleted"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeCompletedCallbacksAsync()
        {
            foreach (Func<Task> callback in _completedCallbacks)
                await callback();
        }
    }
}
