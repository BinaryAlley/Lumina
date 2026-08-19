#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a statistic rendered by a theme.
/// </summary>
/// <param name="Value">The numeric or textual value of the statistic.</param>
/// <param name="Label">The label of the statistic.</param>
/// <param name="Detail">The additional detail of the statistic.</param>
[DebuggerDisplay("Value: {Value}, Label: {Label}")]
public sealed record StatItemDto(
    string Value,
    string Label,
    string Detail
);
