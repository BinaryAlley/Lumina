namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the playlists endpoints of the remote API.
    /// </summary>
    public static class Playlists
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the playlists of the current user.
        /// </summary>
        public const string GET_PLAYLISTS = "users/me/playlists";

        /// <summary>
        /// The route for the API endpoint that creates a playlist for the current user.
        /// </summary>
        public const string CREATE_PLAYLIST = "users/me/playlists";

        /// <summary>
        /// The route for the API endpoint that retrieves a playlist of the current user.
        /// </summary>
        public const string GET_PLAYLIST = "users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for the API endpoint that updates a playlist of the current user.
        /// </summary>
        public const string UPDATE_PLAYLIST = "users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for the API endpoint that deletes a playlist of the current user.
        /// </summary>
        public const string DELETE_PLAYLIST = "users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for the API endpoint that adds an entry to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries";

        /// <summary>
        /// The route for the API endpoint that adds multiple entries to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRIES = "users/me/playlists/{playlistId}/entries-batch";

        /// <summary>
        /// The route for the API endpoint that adds every track of a library, an artist, or an album to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_SCOPE_ENTRIES = "users/me/playlists/{playlistId}/entries-scope";

        /// <summary>
        /// The route for the API endpoint that removes an entry from a playlist of the current user.
        /// </summary>
        public const string REMOVE_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries/{playlistEntryId}";

        /// <summary>
        /// The route for the API endpoint that moves an entry within a playlist of the current user.
        /// </summary>
        public const string MOVE_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
    }
}
