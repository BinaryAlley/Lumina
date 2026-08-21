#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Query for retrieving the template of a theme selected by a page key.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme.</param>
/// <param name="PageKey">The page key that selects the template.</param>
[DebuggerDisplay("ThemeId: {ThemeId}, PageKey: {PageKey}")]
public record GetThemeTemplateQuery(
    string? ThemeId,
    string? PageKey
) : IQuery;
