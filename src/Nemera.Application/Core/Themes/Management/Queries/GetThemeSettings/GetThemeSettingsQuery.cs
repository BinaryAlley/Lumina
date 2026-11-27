#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemeSettings;

/// <summary>
/// Query for retrieving the theme engine settings.
/// </summary>
[DebuggerDisplay("GetThemeSettingsQuery")]
public record GetThemeSettingsQuery : IQuery;
