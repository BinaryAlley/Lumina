#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing media settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class CorsSettingsDto
{
    public const string SECTION_NAME = "CorsSettings";

    /// <summary>
    /// The allowed hosts in regards to cross-origin resource sharing rules.
    /// </summary>
    public required string[] AllowedOrigins { get; init; }
}
