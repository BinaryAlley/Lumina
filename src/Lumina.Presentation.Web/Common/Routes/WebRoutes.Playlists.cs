namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the playlist pages.
    /// </summary>
    public static class Playlists
    {
        /// <summary>
        /// The route for the playlists page.
        /// </summary>
        public const string INDEX = "{culture}/playlists";

        /// <summary>
        /// The route for the action that retrieves the playlists of the current user.
        /// </summary>
        public const string GET_PLAYLISTS = "{culture}/playlists/api-get-playlists";

        /// <summary>
        /// The route for the action that creates a playlist for the current user.
        /// </summary>
        public const string CREATE_PLAYLIST = "{culture}/playlists/api-create-playlist";

        /// <summary>
        /// The route for the playlist creation page.
        /// </summary>
        public const string ADD = "{culture}/playlists/item";

        /// <summary>
        /// The route for the playlist details page.
        /// </summary>
        public const string ITEM = "{culture}/playlists/item/{playlistId}";

        /// <summary>
        /// The route for the action that retrieves a playlist of the current user.
        /// </summary>
        public const string GET_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-get-playlist";

        /// <summary>
        /// The route for the action that updates a playlist of the current user.
        /// </summary>
        public const string UPDATE_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-update-playlist";

        /// <summary>
        /// The route for the action that deletes a playlist of the current user.
        /// </summary>
        public const string DELETE_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-delete-playlist";

        /// <summary>
        /// The route for the action that adds an entry to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/api-add-entry";

        /// <summary>
        /// The route for the action that adds multiple entries to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_ENTRIES = "{culture}/playlists/item/{playlistId}/api-add-entries";

        /// <summary>
        /// The route for the action that adds every track of a library, an artist, or an album to a playlist of the current user.
        /// </summary>
        public const string ADD_PLAYLIST_SCOPE_ENTRIES = "{culture}/playlists/item/{playlistId}/api-add-scope-entries";

        /// <summary>
        /// The route for the action that removes an entry from a playlist of the current user.
        /// </summary>
        public const string REMOVE_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-remove-entry";

        /// <summary>
        /// The route for the action that moves an entry within a playlist of the current user.
        /// </summary>
        public const string MOVE_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-move-entry";
    }
}
