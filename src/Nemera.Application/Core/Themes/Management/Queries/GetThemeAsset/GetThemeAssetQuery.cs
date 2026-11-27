#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Query for retrieving an asset file of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme.</param>
/// <param name="AssetPath">The asset path relative to the theme pack root.</param>
[DebuggerDisplay("ThemeId: {ThemeId}; AssetPath: {AssetPath}")]
public record GetThemeAssetQuery(
    string? ThemeId,
    string? AssetPath
) : IQuery;
