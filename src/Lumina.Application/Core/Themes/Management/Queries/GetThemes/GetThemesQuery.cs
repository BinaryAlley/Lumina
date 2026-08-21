#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemes;

/// <summary>
/// Query for retrieving all installed themes.
/// </summary>
[DebuggerDisplay("GetThemesQuery")]
public record GetThemesQuery : IQuery;
