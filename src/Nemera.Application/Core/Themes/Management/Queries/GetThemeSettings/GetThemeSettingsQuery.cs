#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeSettings;

/// <summary>
/// Query for retrieving the theme engine settings.
/// </summary>
[DebuggerDisplay("GetThemeSettingsQuery")]
public record GetThemeSettingsQuery : IQuery;
