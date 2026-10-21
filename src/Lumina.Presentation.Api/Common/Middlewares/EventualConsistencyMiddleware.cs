#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Presentation.Api.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Common.Middlewares;

/// <summary>
/// Middleware that owns the request transaction, committing it and publishing the queued domain events after a successful request, and rolling it back otherwise.
/// </summary>
public class EventualConsistencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EventualConsistencyMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventualConsistencyMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the HTTP request pipeline.</param>
    /// <param name="logger">Injected service for logging.</param>
    public EventualConsistencyMiddleware(RequestDelegate next, ILogger<EventualConsistencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming HTTP requests and ensures that domain events are published and the database transaction is committed upon successful request completion.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="domainEventPublisher">The domain event publisher used to publish events.</param>
    /// <param name="unitOfWork">Injected unit of work that owns the request transaction.</param>
    /// <param name="domainEventQueue">In memory queue containing the queued domain events.</param>
    public async Task InvokeAsync(HttpContext context, IDomainEventPublisher domainEventPublisher, IUnitOfWork unitOfWork, IDomainEventsQueue domainEventQueue)
    {
        // Endpoints that serve long lived responses (for example a track stream) opt out through endpoint metadata, so that their response never
        // holds the request's database transaction open; the opt out is metadata based, so it follows the endpoint even when its route changes.
        if (context.GetEndpoint()?.Metadata.GetMetadata<SkipEventualConsistencyMetadata>() is not null)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Begin a new database transaction - this will wrap any database operations performed during this HTTP request in an atomic operation.
        await unitOfWork.BeginTransactionAsync(context.RequestAborted).ConfigureAwait(false);
        // Register an action to be executed once the response is completed (the user already received a response for the request).
        context.Response.OnCompleted(async () =>
        {
            try
            {
                // Only proceed if the response status code indicates success. The request abort token is deliberately not consulted here:
                // it can already be cancelled when the callback runs, even though the operation completed successfully, and refusing to
                // commit then would discard a write whose success response was already sent. An aborted request is instead reported as a
                // non-success status by the exception handling middleware.
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    // Commit the transaction: because some domain events trigger jobs that end up being processed in background hosted services, and those are
                    // singletons that require their own DI scope to be created, any changes made on the DbContext in this HTTP scope would not be seen on the
                    // DbContext created in the background service scope, because the transaction is not yet committed here; therefor, the transaction needs to be
                    // committed first, before any domain events are fired.
                    // See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0002.md for details:
                    await unitOfWork.CommitTransactionAsync(CancellationToken.None).ConfigureAwait(false);

                    // Check if domain events exist in the channel queue and publish them.
                    while (domainEventQueue.TryDequeue(out IDomainEvent? nextEvent))
                        await domainEventPublisher.PublishAsync(nextEvent);
                }
            }
            catch (EventualConsistencyException ex)
            {
                _logger.LogCritical(ex, "Failed to perform eventual consistency");
            }
            catch (Exception ex) // The response is already complete, so this failure cannot reach the client. It is logged, and the transaction is still closed by the rollback below.
            {                
                _logger.LogError(ex, "Failed to complete the request transaction for {Path}", context.Request.Path);
            }
            finally
            {
                // A commit or a rollback clears the transaction, so this only closes one that the failure above left open. It runs for
                // every response, including the non-success one that never attempted a commit. Rollback must never throw out of the
                // completion callback, and must never mask the failure that is already being handled.
                try
                {
                    await unitOfWork.RollbackTransactionAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to roll back the request transaction for {Path}", context.Request.Path);
                }
            }
        });
        await _next(context).ConfigureAwait(false);
    }
}
