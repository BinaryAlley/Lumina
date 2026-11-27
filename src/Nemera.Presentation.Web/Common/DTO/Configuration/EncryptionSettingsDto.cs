#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing encryption settings.
/// </summary>
[DebuggerDisplay("Section name: {SECTION_NAME}")]
public class EncryptionSettingsDto
{
    /// <summary>
    /// The name of the configuration section that holds the encryption settings.
    /// </summary>
    public const string SECTION_NAME = "EncryptionSettings";

    /// <summary>
    /// Gets or sets the Base64 encoded secret key used to encrypt and decrypt protected data.
    /// </summary>
    public required string SecretKey { get; init; }
}
