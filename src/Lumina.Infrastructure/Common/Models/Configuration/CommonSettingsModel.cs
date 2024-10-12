#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing shared application configuration settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class CommonSettingsModel
{
    public const string SECTION_NAME = "CommonSettings";

    /// <summary>
    /// Gets or sets the theme of the application.
    /// </summary>
    public required string Theme { get; init; }
}
