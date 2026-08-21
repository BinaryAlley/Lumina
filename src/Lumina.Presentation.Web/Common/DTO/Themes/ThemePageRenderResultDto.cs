#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the rendered content and script of a themed page section.
/// </summary>
/// <param name="Content">The rendered HTML content of the page section.</param>
/// <param name="Script">The rendered script element of the page section, when the template defines one.</param>
[DebuggerDisplay("Content length: {Content.Length}, Script length: {Script.Length}")]
public sealed record ThemePageRenderResultDto(
    string Content,
    string Script
);
