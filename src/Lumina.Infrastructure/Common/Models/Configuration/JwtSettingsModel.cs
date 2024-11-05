#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing media settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class JwtSettingsModel
{
    public const string SECTION_NAME = "JwtSettings";

    /// <summary>
    /// The secret key used to sign the JWT. This should be a strong, securely stored key.
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// The duration (in minutes) for which the JWT is valid.
    /// </summary>
    public required int ExpiryMinutes { get; init; }

    /// <summary>
    /// The issuer of the JWT, typically the application or service generating the token.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// The intended audience for the JWT, usually representing the clients that can consume the token.
    /// </summary>
    public required string Audience { get; init; }
}
