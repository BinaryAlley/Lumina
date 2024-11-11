namespace Lumina.Presentation.Api.Common.Routes.UsersManagement;

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
        public const string REGISTER_ACCOUNT = "/auth/register";
        public const string LOGIN_ACCOUNT = "/auth/login";
    }
}
