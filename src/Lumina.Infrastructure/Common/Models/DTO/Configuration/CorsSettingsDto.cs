#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing cross-origin resource sharing settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class CorsSettingsDto
{
    /// <summary>
    /// The name of the configuration section that holds the cross-origin resource sharing settings.
    /// </summary>
    public const string SECTION_NAME = "CorsSettings";

    /// <summary>
    /// The allowed hosts in regards to cross-origin resource sharing rules.
    /// </summary>
    public required string[] AllowedOrigins { get; init; }
}
