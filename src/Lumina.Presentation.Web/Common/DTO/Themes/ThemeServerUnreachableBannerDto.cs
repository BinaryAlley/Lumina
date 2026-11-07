#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to the themed server unreachable banner template.
/// </summary>
/// <param name="Message">The localized message that tells the user that the server is unreachable.</param>
[DebuggerDisplay("Message: {Message}")]
public sealed record ThemeServerUnreachableBannerDto(
    string Message
);
