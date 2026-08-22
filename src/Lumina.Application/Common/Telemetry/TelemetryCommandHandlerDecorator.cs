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
/// Decorator that wraps an <see cref="ICommandHandler{TCommand,TResult}"/> to emit traces, metrics, and structured logs for every command execution.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the command.</typeparam>
public class TelemetryCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    private const string HANDLER_TYPE = "command";

    private readonly ICommandHandler<TCommand, TResult> _innerHandler;
    private readonly ILogger<TelemetryCommandHandlerDecorator<TCommand, TResult>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryCommandHandlerDecorator{TCommand,TResult}"/> class.
    /// </summary>
    /// <param name="innerHandler">The wrapped command handler.</param>
    /// <param name="logger">Injected logger used for structured logging.</param>
    public TelemetryCommandHandlerDecorator(ICommandHandler<TCommand, TResult> innerHandler, ILogger<TelemetryCommandHandlerDecorator<TCommand, TResult>> logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified command, emitting telemetry around the execution of the wrapped handler.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the command execution.</returns>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        string handlerName = _innerHandler.GetType().Name;
        using (Activity? activity = ApplicationHandlerTelemetry.StartActivity(handlerName, HANDLER_TYPE))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                TResult result = await _innerHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
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
