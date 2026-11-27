#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetCurrentTheme;

/// <summary>
/// Query for retrieving the currently active theme.
/// </summary>
[DebuggerDisplay("GetCurrentThemeQuery")]
public record GetCurrentThemeQuery : IQuery;
