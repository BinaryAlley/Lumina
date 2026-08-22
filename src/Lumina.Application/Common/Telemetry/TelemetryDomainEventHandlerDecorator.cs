#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Decorator that wraps an <see cref="IDomainEventHandler{TDomainEvent}"/> to emit traces, metrics, and structured logs for every domain event handling.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle.</typeparam>
public class TelemetryDomainEventHandlerDecorator<TDomainEvent> : IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
    private const string HANDLER_TYPE = "domain-event";

    private readonly IDomainEventHandler<TDomainEvent> _innerHandler;
    private readonly ILogger<TelemetryDomainEventHandlerDecorator<TDomainEvent>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryDomainEventHandlerDecorator{TDomainEvent}"/> class.
    /// </summary>
    /// <param name="innerHandler">The wrapped domain event handler.</param>
    /// <param name="logger">Injected logger used for structured logging.</param>
    public TelemetryDomainEventHandlerDecorator(IDomainEventHandler<TDomainEvent> innerHandler, ILogger<TelemetryDomainEventHandlerDecorator<TDomainEvent>> logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified domain event, emitting telemetry around the execution of the wrapped handler.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string handlerName = _innerHandler.GetType().Name;
        using (Activity? activity = ApplicationHandlerTelemetry.StartActivity(handlerName, HANDLER_TYPE))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                await _innerHandler.HandleAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                ApplicationHandlerTelemetry.RecordSuccess(_logger, activity, handlerName, HANDLER_TYPE, stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                ApplicationHandlerTelemetry.RecordException(_logger, activity, handlerName, HANDLER_TYPE, exception, stopwatch.Elapsed);
                throw;
            }
        }
    }
}
