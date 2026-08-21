#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Represents a cached HTTP response, including its content and status code.
/// </summary>
[DebuggerDisplay("StatusCode: {StatusCode}")]
public class CachedResponse
{
    /// <summary>
    /// Gets or sets the textual content of the cached HTTP response.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw bytes of the cached HTTP response.
    /// </summary>
    public byte[] Bytes { get; set; } = [];

    /// <summary>
    /// Gets or sets the content type of the cached HTTP response.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the cached response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
}
