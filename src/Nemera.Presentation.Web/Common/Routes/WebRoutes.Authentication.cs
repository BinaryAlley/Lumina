namespace Nemera.Presentation.Web.Common.Routes;

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
        /// <summary>
        /// The route for the account registration page.
        /// </summary>
        public const string REGISTER_VIEW = "{culture}/auth/register";

        /// <summary>
        /// The route for the account login page.
        /// </summary>
        public const string LOGIN_VIEW = "{culture}/auth/login/{returnUrl?}";

        /// <summary>
        /// The route for the password recovery page.
        /// </summary>
        public const string RECOVER_PASSWORD_VIEW = "{culture}/auth/recover-password";

        /// <summary>
        /// The route for the password change page.
        /// </summary>
        public const string CHANGE_PASSWORD_VIEW = "{culture}/auth/change-password";

        /// <summary>
        /// The route for the user profile page.
        /// </summary>
        public const string PROFILE_VIEW = "{culture}/auth/profile";

        /// <summary>
        /// The route for the access denied page.
        /// </summary>
        public const string ACCESS_DENIED_VIEW = "{culture}/auth/access-denied";

        /// <summary>
        /// The route for the action that logs the current user out.
        /// </summary>
        public const string LOGOUT = "{culture}/auth/logout";

        /// <summary>
        /// The route for the action that retrieves an API access token.
        /// </summary>
        public const string GET_API_ACCESS_TOKEN = "{culture}/auth/api-access-token";

        /// <summary>
        /// The route for the action that registers an account through the Web application.
        /// </summary>
        public const string REGISTER = "{culture}/auth/api-register";

        /// <summary>
        /// The route for the action that logs an account in through the Web application.
        /// </summary>
        public const string LOGIN = "{culture}/auth/api-login";

        /// <summary>
        /// The route for the action that recovers the password of an account through the Web application.
        /// </summary>
        public const string RECOVER_PASSWORD = "{culture}/auth/api-recover-password";

        /// <summary>
        /// The route for the action that changes the password of an account through the Web application.
        /// </summary>
        public const string CHANGE_PASSWORD = "{culture}/auth/api-change-password";
    }
}
