#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Diagnostics.Metrics;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Central registry of the <see cref="ActivitySource"/> and <see cref="Meter"/> used by the Application layer to emit traces and metrics.
/// </summary>
public static class ApplicationTelemetry
{
    /// <summary>
    /// The source name shared by the <see cref="ActivitySource"/> and <see cref="Meter"/> of the Application layer.
    /// </summary>
    public const string SOURCE_NAME = "Lumina.Application";

    /// <summary>
    /// The source of the traces emitted by the Application layer.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(SOURCE_NAME);

    /// <summary>
    /// The source of the metrics emitted by the Application layer.
    /// </summary>
    public static Meter Meter { get; } = new(SOURCE_NAME);
}
