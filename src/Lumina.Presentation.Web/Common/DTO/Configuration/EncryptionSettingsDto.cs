#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing media settings.
/// </summary>
[DebuggerDisplay("Section name: {SECTION_NAME}")]
public class EncryptionSettingsDto
{
    public const string SECTION_NAME = "EncryptionSettings";

    /// <summary>
    /// Gets or sets the root directory where media files are stored.
    /// </summary>
    public required string SecretKey { get; init; }
}
