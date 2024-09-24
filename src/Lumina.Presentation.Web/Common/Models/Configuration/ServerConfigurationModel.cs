#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Configuration;

/// <summary>
/// Model for deserializing server configuration settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class ServerConfigurationModel
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public const string SECTION_NAME = "ServerConfiguration";
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
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
    #endregion
}
