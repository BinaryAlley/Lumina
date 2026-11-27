#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Query for retrieving the downloadable archive of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record GetThemeArchiveQuery(
    string? ThemeId
) : IQuery;
