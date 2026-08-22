#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Telemetry;

/// <summary>
/// Central registry of the <see cref="ActivitySource"/> used by the Presentation Web layer to emit traces.
/// </summary>
public static class WebTelemetry
{
    /// <summary>
    /// The source name shared by the <see cref="ActivitySource"/> of the Presentation Web layer.
    /// </summary>
    public const string SOURCE_NAME = "Lumina.Web";

    /// <summary>
    /// The source of the traces emitted by the Presentation Web layer.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(SOURCE_NAME);
}
