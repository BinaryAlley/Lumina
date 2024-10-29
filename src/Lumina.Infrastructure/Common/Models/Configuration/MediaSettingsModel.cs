#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing media settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class MediaSettingsModel
{
    public const string SECTION_NAME = "MediaSettings";

    /// <summary>
    /// Gets or sets the root directory where media files are stored.
    /// </summary>
    public required string RootDirectory { get; init; }
}
