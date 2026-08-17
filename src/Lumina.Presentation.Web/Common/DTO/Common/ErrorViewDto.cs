using System.Diagnostics;

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for the data shown on the error page.
/// </summary>
[DebuggerDisplay("RequestId: {RequestId}, ShowRequestId: {ShowRequestId}")]
public class ErrorViewDto
{
    /// <summary>
    /// Gets or sets the request identifier to display.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request identifier should be shown.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
