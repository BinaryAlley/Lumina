#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Helper that emits the traces, metrics, and structured logs for the execution of application handlers.
/// </summary>
internal static class ApplicationHandlerTelemetry
{
    private const string HANDLER_DURATION_METRIC_NAME = "lumina.application.handler.duration";
    private const string HANDLER_INVOCATIONS_METRIC_NAME = "lumina.application.handler.invocations";

    private static readonly Histogram<double> s_handlerDuration = ApplicationTelemetry.Meter.CreateHistogram<double>(
        name: HANDLER_DURATION_METRIC_NAME,
        description: "Duration in milliseconds of the execution of an application handler.");

    private static readonly Counter<long> s_handlerInvocations = ApplicationTelemetry.Meter.CreateCounter<long>(
        name: HANDLER_INVOCATIONS_METRIC_NAME,
        description: "Number of invocations of an application handler.");

    /// <summary>
    /// Starts a new <see cref="Activity"/> for the execution of the handler with the specified name.
    /// </summary>
    /// <param name="handlerName">The name of the handler being executed.</param>
    /// <param name="handlerType">The type of the handler being executed.</param>
    /// <returns>The started activity, or <see langword="null"/> when no trace listener is registered.</returns>
    public static Activity? StartActivity(string handlerName, string handlerType)
    {
        Activity? activity = ApplicationTelemetry.ActivitySource.StartActivity($"Handle {handlerName}", ActivityKind.Internal);
        activity?.SetTag("lumina.handler.type", handlerType);
        activity?.SetTag("lumina.handler.name", handlerName);
        return activity;
    }

    /// <summary>
    /// Records the successful completion of a handler execution, including its trace status, metrics, and a structured log entry.
    /// </summary>
    /// <param name="logger">The logger to write the structured log entry to.</param>
    /// <param name="activity">The activity to mark as successful.</param>
    /// <param name="handlerName">The name of the handled handler.</param>
    /// <param name="handlerType">The type of the handled handler.</param>
    /// <param name="duration">The duration of the handler execution.</param>
    public static void RecordSuccess(ILogger logger, Activity? activity, string handlerName, string handlerType, TimeSpan duration)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        RecordOutcome(activity, handlerName, handlerType, duration, isSuccess: true);
        logger.LogDebug("Completed handling {HandlerType} {HandlerName} in {DurationMs}ms", handlerType, handlerName, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Records the failed completion of a handler execution, including its trace status, metrics, and a structured log entry.
    /// </summary>
    /// <param name="logger">The logger to write the structured log entry to.</param>
    /// <param name="activity">The activity to mark as failed.</param>
    /// <param name="handlerName">The name of the handled handler.</param>
    /// <param name="handlerType">The type of the handled handler.</param>
    /// <param name="duration">The duration of the handler execution.</param>
    /// <param name="error">An optional description of the failure.</param>
    public static void RecordFailure(ILogger logger, Activity? activity, string handlerName, string handlerType, TimeSpan duration, string? error)
    {
        activity?.SetStatus(ActivityStatusCode.Error, error);
        RecordOutcome(activity, handlerName, handlerType, duration, isSuccess: false);
        // business-rule failures are routine outcomes (e.g. failed logins), so log at Debug to avoid noise from high-frequency handlers
        logger.LogDebug("Failed handling {HandlerType} {HandlerName} in {DurationMs}ms: {Error}", handlerType, handlerName, duration.TotalMilliseconds, error);
    }

    /// <summary>
    /// Records the exceptional completion of a handler execution, including its trace status, metrics, and a structured log entry.
    /// </summary>
    /// <param name="logger">The logger to write the structured log entry to.</param>
    /// <param name="activity">The activity to mark as failed.</param>
    /// <param name="handlerName">The name of the handled handler.</param>
    /// <param name="handlerType">The type of the handled handler.</param>
    /// <param name="exception">The exception that was thrown during the handler execution.</param>
    /// <param name="duration">The duration of the handler execution.</param>
    public static void RecordException(ILogger logger, Activity? activity, string handlerName, string handlerType, Exception exception, TimeSpan duration)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        RecordOutcome(activity, handlerName, handlerType, duration, isSuccess: false);
        logger.LogError(exception, "Failed handling {HandlerType} {HandlerName} in {DurationMs}ms", handlerType, handlerName, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Records the invocation count and duration of a handler execution into the corresponding metrics, and tags the activity with the outcome.
    /// </summary>
    /// <param name="activity">The activity to tag with the outcome.</param>
    /// <param name="handlerName">The name of the handled handler.</param>
    /// <param name="handlerType">The type of the handled handler.</param>
    /// <param name="duration">The duration of the handler execution.</param>
    /// <param name="isSuccess">Whether the handler execution completed successfully.</param>
    private static void RecordOutcome(Activity? activity, string handlerName, string handlerType, TimeSpan duration, bool isSuccess)
    {
        TagList tags = new()
        {
            { "lumina.handler.type", handlerType },
            { "lumina.handler.name", handlerName },
            { "lumina.outcome", isSuccess ? "success" : "failure" }
        };
        s_handlerInvocations.Add(1, tags);
        s_handlerDuration.Record(duration.TotalMilliseconds, tags);
        activity?.SetTag("lumina.outcome", isSuccess ? "success" : "failure");
    }
}
