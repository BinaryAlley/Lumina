#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing media settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class CorsSettingsModel
{
    public const string SECTION_NAME = "CorsSettings";

    /// <summary>
    /// The allowed hosts in regards to cross-origin resource sharing rules.
    /// </summary>
    public required string[] AllowedOrigins { get; init; }
}
