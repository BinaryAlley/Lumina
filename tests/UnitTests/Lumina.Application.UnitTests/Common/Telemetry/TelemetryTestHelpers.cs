#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
#endregion

namespace Lumina.Application.UnitTests.Common.Telemetry;

/// <summary>
/// Test utilities for capturing the traces and metrics emitted by the application telemetry.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class TelemetryTestHelpers
{
    /// <summary>
    /// Creates an <see cref="ActivityListener"/> that captures every activity emitted by the <see cref="ApplicationTelemetry"/> activity source.
    /// </summary>
    /// <param name="capturedActivities">The collection that stopped activities are added to.</param>
    /// <returns>The started <see cref="ActivityListener"/>.</returns>
    public static ActivityListener CreateActivityListener(ICollection<Activity> capturedActivities)
    {
        ActivityListener listener = new()
        {
            ShouldListenTo = source => source.Name == ApplicationTelemetry.SOURCE_NAME,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => capturedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    /// <summary>
    /// Creates a <see cref="MeterListener"/> that captures every measurement emitted by the <see cref="ApplicationTelemetry"/> meter.
    /// </summary>
    /// <param name="measurements">The collection that measurements are added to.</param>
    /// <returns>The started <see cref="MeterListener"/>.</returns>
    public static MeterListener CreateMeterListener(ICollection<(string InstrumentName, double Value, string Outcome)> measurements)
    {
        MeterListener listener = new()
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == ApplicationTelemetry.SOURCE_NAME)
                    meterListener.EnableMeasurementEvents(instrument);
            }
        };
        // InstrumentPublished fires for instruments created after Start(), and Start() also replays every instrument
        // that already exists on the meter; therefore the lazily-initialized static handler instruments are captured by
        // every listener, whether they were created before or during the test
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, state) => measurements.Add((instrument.Name, value, GetOutcomeTag(tags))));
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, state) => measurements.Add((instrument.Name, value, GetOutcomeTag(tags))));
        listener.Start();
        return listener;
    }

    /// <summary>
    /// Reads the value of the <c>lumina.outcome</c> tag from the specified tag list.
    /// </summary>
    /// <param name="tags">The tag list of a measurement.</param>
    /// <returns>The outcome tag value, or an empty string when the tag is not present.</returns>
    private static string GetOutcomeTag(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        foreach (KeyValuePair<string, object?> tag in tags)
            if (tag.Key == "lumina.outcome")
                return tag.Value?.ToString() ?? string.Empty;
        return string.Empty;
    }
}
