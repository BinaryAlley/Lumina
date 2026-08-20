#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Themes;

/// <summary>
/// Represents a theme asset response.
/// </summary>
/// <param name="Bytes">The bytes of the asset file.</param>
/// <param name="ContentType">The MIME content type of the asset file.</param>
[DebuggerDisplay("ContentType: {ContentType}")]
public record ThemeAssetResponse(
    byte[] Bytes,
    string ContentType
);
