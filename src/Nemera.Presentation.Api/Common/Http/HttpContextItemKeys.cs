namespace Nemera.Presentation.Api.Common.Http;

/// <summary>
/// Constants shared by HttpContext across a HTTP request (avoid magic strings).
/// </summary>
public static class HttpContextItemKeys
{
    /// <summary>
    /// The key under which the errors of the current HTTP request are stored in the items collection of the HTTP context.
    /// </summary>
    public const string ERRORS = "errors";
}
