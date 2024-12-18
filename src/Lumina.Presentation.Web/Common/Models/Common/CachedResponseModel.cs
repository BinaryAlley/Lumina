#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a cached HTTP response, including its content and status code.
/// </summary>
[DebuggerDisplay("StatusCode: {StatusCode}")]
public class CachedResponseModel
{
    /// <summary>
    /// Gets or sets the content of the cached HTTP response.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code of the cached response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
}
