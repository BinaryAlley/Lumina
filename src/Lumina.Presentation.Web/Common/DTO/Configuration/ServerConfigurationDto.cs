#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing server configuration settings.
/// </summary>
[DebuggerDisplay("Section name: {SECTION_NAME}")]
public class ServerConfigurationDto
{
    public const string SECTION_NAME = "ServerConfiguration";

    /// <summary>
    /// Gets or sets the remote API server path.
    /// </summary>
    public required char ApiVersion { get; init; }

    /// <summary>
    /// Gets or sets the remote API server path.
    /// </summary>
    public required string BaseAddress { get; init; }

    /// <summary>
    /// Gets or sets the remote API server port.
    /// </summary>
    public required ushort Port { get; init; }
}
