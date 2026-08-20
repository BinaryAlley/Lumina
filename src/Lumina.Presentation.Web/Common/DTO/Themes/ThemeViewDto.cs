#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the rendered sections of a themed page, served through the shared themed view.
/// </summary>
/// <param name="Content">The rendered HTML content of the page section.</param>
/// <param name="Script">The rendered script element of the page section, when the template defines one.</param>
/// <param name="AssetBase">The base URL of the theme assets, used to load the theme stylesheet of the page.</param>
[DebuggerDisplay("Content length: {Content.Length}, Script length: {Script.Length}")]
public sealed record ThemeViewDto(
    string Content,
    string Script,
    string AssetBase
);
