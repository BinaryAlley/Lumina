#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Models;

/// <summary>
/// Small model class used to verify section rendering against a plain object scope.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SectionModel
{
    /// <summary>
    /// Gets the text rendered by the section.
    /// </summary>
    public string Text { get; init; } = string.Empty;
}
