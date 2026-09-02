#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Tools;

/// <summary>
/// Represents the request for setting the culture used by the application.
/// </summary>
/// <param name="NewCulture">The new culture to set. Required.</param>
/// <param name="ReturnUrl">The URL to return to, after setting the new culture. Optional.</param>
[DebuggerDisplay("NewCulture: {NewCulture}, ReturnUrl: {ReturnUrl}")]
public record SetLanguageRequest(
    string? NewCulture,
    string? ReturnUrl
);
