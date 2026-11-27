#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemes;

/// <summary>
/// Query for retrieving all installed themes.
/// </summary>
[DebuggerDisplay("GetThemesQuery")]
public record GetThemesQuery : IQuery;
