namespace Nemera.Presentation.Api.Common.Routes.UsersManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Authentication route.
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// The route for registering a user account.
        /// </summary>
        public const string REGISTER_ACCOUNT = "/auth/register";

        /// <summary>
        /// The route for logging into a user account.
        /// </summary>
        public const string LOGIN_ACCOUNT = "/auth/login";

        /// <summary>
        /// The route for recovering the password of a user account.
        /// </summary>
        public const string RECOVER_PASSWORD = "/auth/recover-password";

        /// <summary>
        /// The route for changing the password of a user account.
        /// </summary>
        public const string CHANGE_PASSWORD = "/auth/change-password";

        /// <summary>
        /// The route for getting the user accounts.
        /// </summary>
        public const string USERS = "/auth/users";
    }
}
