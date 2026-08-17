namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the authentication pages.
    /// </summary>
    public static class Authentication
    {
        public const string REGISTER_VIEW = "{culture}/auth/register";
        public const string LOGIN_VIEW = "{culture}/auth/login/{returnUrl?}";
        public const string RECOVER_PASSWORD_VIEW = "{culture}/auth/recover-password";
        public const string CHANGE_PASSWORD_VIEW = "{culture}/auth/change-password";
        public const string PROFILE_VIEW = "{culture}/auth/profile";
        public const string ACCESS_DENIED_VIEW = "{culture}/auth/access-denied";
        public const string LOGOUT = "{culture}/auth/logout";
        public const string REGISTER = "{culture}/auth/api-register";
        public const string LOGIN = "{culture}/auth/api-login";
        public const string RECOVER_PASSWORD = "{culture}/auth/api-recover-password";
        public const string CHANGE_PASSWORD = "{culture}/auth/api-change-password";
    }
}
