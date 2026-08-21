#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;

/// <summary>
/// Query for retrieving the currently active theme.
/// </summary>
[DebuggerDisplay("GetCurrentThemeQuery")]
public record GetCurrentThemeQuery : IQuery;
