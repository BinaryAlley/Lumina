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
        /// <summary>
        /// The route for the account registration API endpoint.
        /// </summary>
        public const string REGISTER_ACCOUNT = "auth/register";

        /// <summary>
        /// The route for the account login API endpoint.
        /// </summary>
        public const string LOGIN_ACCOUNT = "auth/login";

        /// <summary>
        /// The route for the password recovery API endpoint.
        /// </summary>
        public const string RECOVER_PASSWORD = "auth/recover-password";

        /// <summary>
        /// The route for the password change API endpoint.
        /// </summary>
        public const string CHANGE_PASSWORD = "auth/change-password";

        /// <summary>
        /// The route for the user accounts API endpoint.
        /// </summary>
        public const string USERS = "auth/users";
    }
}
