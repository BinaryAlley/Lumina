#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Decorator that wraps an <see cref="IQueryHandler{TQuery,TResult}"/> to emit traces, metrics, and structured logs for every query execution.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the query.</typeparam>
public class TelemetryQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery
{
    private const string HANDLER_TYPE = "query";

    private readonly IQueryHandler<TQuery, TResult> _innerHandler;
    private readonly ILogger<TelemetryQueryHandlerDecorator<TQuery, TResult>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryQueryHandlerDecorator{TQuery,TResult}"/> class.
    /// </summary>
    /// <param name="innerHandler">The wrapped query handler.</param>
    /// <param name="logger">Injected logger used for structured logging.</param>
    public TelemetryQueryHandlerDecorator(IQueryHandler<TQuery, TResult> innerHandler, ILogger<TelemetryQueryHandlerDecorator<TQuery, TResult>> logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified query, emitting telemetry around the execution of the wrapped handler.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the query execution.</returns>
    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        string handlerName = _innerHandler.GetType().Name;
        using (Activity? activity = ApplicationHandlerTelemetry.StartActivity(handlerName, HANDLER_TYPE))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                TResult result = await _innerHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                if (HandlerOutcomeDetector.IsSuccessful(result))
                    ApplicationHandlerTelemetry.RecordSuccess(_logger, activity, handlerName, HANDLER_TYPE, stopwatch.Elapsed);
                else
                    ApplicationHandlerTelemetry.RecordFailure(_logger, activity, handlerName, HANDLER_TYPE, stopwatch.Elapsed, HandlerOutcomeDetector.GetErrorDescription(result));
                return result;
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
