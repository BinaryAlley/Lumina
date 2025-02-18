#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DomainEvents;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Common.Middlewares;

/// <summary>
/// Middleware that ensures eventual consistency by handling domain events and publishing them after the request has completed successfuly.
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
    /// <param name="publisher">The domain event publisher used to publish events.</param>
    /// <param name="dbContext">The database context used to manage the transaction.</param>
    /// <param name="domainEventQueue">In memory queue containing the queued domain events.</param>
    public async Task InvokeAsync(HttpContext context, IPublisher publisher, LuminaDbContext dbContext, IDomainEventsQueue domainEventQueue)
    {
        // begin a new database transaction - this will wrap any database operations performed during this HTTP request in an atomic operation
        IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(context.RequestAborted).ConfigureAwait(false);
        // register an action to be executed once the response is completed (the user already received a response for the request)
        context.Response.OnCompleted(async () =>
        {
            try
            {
                // only proceed if the response status code indicates success
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    // commit the transaction: because some domain events trigger jobs that end up being processed in background hosted services, and those are
                    // singletons that require their own DI scope to be created, any changes made on the DbContext in this HTTP scope would not be seen on the
                    // DbContext created in the background service scope, because the transaction is not yet commited here; therefor, the transaction needs to be
                    // commited first, before any domain events are fired
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0002.md for details:
                    await transaction.CommitAsync().ConfigureAwait(false);

                    // check if domain events exist in the channel queue and publish them
                    while (domainEventQueue.TryDequeue(out IDomainEvent? nextEvent))
                        await publisher.Publish(nextEvent);
                }
            }
            catch (EventualConsistencyException ex)
            {
                _logger.LogCritical(ex, "Failed to perform eventual consistency");
            }
            finally
            {
                // DisposeAsync automatically roles back any changes, if a call to CommitAsync was not performed prior
                await transaction.DisposeAsync().ConfigureAwait(false);
            }
        });
        await _next(context).ConfigureAwait(false);
    }
}
