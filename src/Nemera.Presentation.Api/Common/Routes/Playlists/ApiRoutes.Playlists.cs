namespace Lumina.Presentation.Api.Common.Routes.Playlists;

/// <summary>
/// Class for the collection of the playlist routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the playlists of the current user.
    /// </summary>
    public static class Playlists
    {
        /// <summary>
        /// The route for getting the playlists of the current user.
        /// </summary>
        public const string GET_PLAYLISTS = "/users/me/playlists";

        /// <summary>
        /// The route for creating a playlist of the current user.
        /// </summary>
        public const string CREATE_PLAYLIST = "/users/me/playlists";

        /// <summary>
        /// The route for getting a playlist of the current user.
        /// </summary>
        public const string GET_PLAYLIST = "/users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for updating a playlist of the current user.
        /// </summary>
        public const string UPDATE_PLAYLIST = "/users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for deleting a playlist of the current user.
        /// </summary>
        public const string DELETE_PLAYLIST = "/users/me/playlists/{playlistId}";

        /// <summary>
        /// The route for adding an entry to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRY = "/users/me/playlists/{playlistId}/entries";

        /// <summary>
        /// The route for adding several entries to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRIES = "/users/me/playlists/{playlistId}/entries-batch";

        /// <summary>
        /// The route for adding every track of a music library, an artist or an album to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_SCOPE_ENTRIES = "/users/me/playlists/{playlistId}/entries-scope";

        /// <summary>
        /// The route for removing an entry from a playlist of the current user.
        /// </summary>
        public const string REMOVE_PLAYLIST_ENTRY = "/users/me/playlists/{playlistId}/entries/{playlistEntryId}";

        /// <summary>
        /// The route for moving an entry within a playlist of the current user.
        /// </summary>
        public const string MOVE_PLAYLIST_ENTRY = "/users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
    }
}
