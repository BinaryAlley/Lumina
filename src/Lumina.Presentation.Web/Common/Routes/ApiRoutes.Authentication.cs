namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the authentication endpoints of the remote API.
    /// </summary>
    public static class Authentication
    {
        public const string REGISTER_ACCOUNT = "auth/register";
        public const string LOGIN_ACCOUNT = "auth/login";
        public const string RECOVER_PASSWORD = "auth/recover-password";
        public const string CHANGE_PASSWORD = "auth/change-password";
        public const string USERS = "auth/users";
    }
}
