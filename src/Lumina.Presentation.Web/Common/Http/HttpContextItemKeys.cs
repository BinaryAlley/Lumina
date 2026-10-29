namespace Lumina.Presentation.Web.Common.Http;

/// <summary>
/// Constants shared by HttpContext across a HTTP request (avoid magic strings).
/// </summary>
public static class HttpContextItemKeys
{
    public const string ERRORS = "errors";
    public const string PENDING_SUPER_ADMIN_SETUP = "PendingSuperAdminSetup";
    public const string LAST_DISPLAYED_VIEW = "LastViewUrl";

    /// <summary>
    /// The <c>Cache-Control</c> value chosen by the endpoint, restored when the response starts.
    /// </summary>
    public const string CACHE_CONTROL = "CacheControl";
}
