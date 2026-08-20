#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to install a theme pack, sent as a multipart form whose single file field contains the theme archive.
/// </summary>
[DebuggerDisplay("InstallThemeRequest")]
public record InstallThemeRequest;
