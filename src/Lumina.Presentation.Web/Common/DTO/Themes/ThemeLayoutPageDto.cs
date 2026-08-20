#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the page content handed to the layout renderer, which wraps it in the themed shell.
/// </summary>
/// <param name="Title">The title of the page, displayed by the themed shell.</param>
/// <param name="Content">The rendered HTML content of the page section.</param>
/// <param name="Script">The rendered script element of the page section, when the template defines one.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record ThemeLayoutPageDto(
    string Title,
    string Content,
    string Script
);
