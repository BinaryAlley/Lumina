#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing plugins settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class PluginsSettingsDto
{
    public const string SECTION_NAME = "PluginsSettings";

    /// <summary>
    /// Gets or sets the directory where the plugin assemblies are located.
    /// </summary>
    public required string Directory { get; init; }
}
