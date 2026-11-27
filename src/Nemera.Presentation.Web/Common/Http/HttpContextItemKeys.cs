namespace Nemera.Presentation.Web.Common.Http;

/// <summary>
/// Constants shared by HttpContext across a HTTP request (avoid magic strings).
/// </summary>
public static class HttpContextItemKeys
{
    /// <summary>
    /// The key under which the validation errors of the current request are stored.
    /// </summary>
    public const string ERRORS = "errors";

    /// <summary>
    /// The key under which the flag indicating that the super administrator account still has to be set up is stored.
    /// </summary>
    public const string PENDING_SUPER_ADMIN_SETUP = "PendingSuperAdminSetup";

    /// <summary>
    /// The key under which the URL of the last displayed view is stored.
    /// </summary>
    public const string LAST_DISPLAYED_VIEW = "LastViewUrl";

    /// <summary>
    /// Marks a rendered page whose authorization could not reach the API, so that the page reports the failure to the user.
    /// </summary>
    public const string SERVER_UNREACHABLE = "ServerUnreachable";

    /// <summary>
    /// The <c>Cache-Control</c> value chosen by the endpoint, restored when the response starts.
    /// </summary>
    public const string CACHE_CONTROL = "CacheControl";

    /// <summary>
    /// The key under which the flag indicating that the endpoint bound its request and started executing is stored.
    /// </summary>
    public const string ENDPOINT_STARTED = "EndpointStarted";
}
