namespace Lumina.Presentation.Web.Common.Http;

/// <summary>
/// Constants shared by HttpContext across a HTTP request (avoid magic strings).
/// </summary>
public static class HttpContextItemKeys
{
    public const string ERRORS = "errors";
    public const string PENDING_SUPER_ADMIN_SETUP = "PendingSuperAdminSetup";
}
