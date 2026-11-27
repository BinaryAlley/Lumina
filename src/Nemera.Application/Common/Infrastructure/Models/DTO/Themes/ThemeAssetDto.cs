#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.DTO.Themes;

/// <summary>
/// Data transfer object for a theme asset file served by the theme service.
/// </summary>
/// <param name="Bytes">The bytes of the asset file.</param>
/// <param name="ContentType">The MIME content type of the asset file.</param>
[DebuggerDisplay("ContentType: {ContentType}")]
public sealed record ThemeAssetDto(
    byte[] Bytes,
    string ContentType
);
