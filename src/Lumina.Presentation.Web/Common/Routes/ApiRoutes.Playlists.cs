namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string GET_PLAYLISTS = "users/me/playlists";
        public const string CREATE_PLAYLIST = "users/me/playlists";
        public const string GET_PLAYLIST = "users/me/playlists/{playlistId}";
        public const string UPDATE_PLAYLIST = "users/me/playlists/{playlistId}";
        public const string DELETE_PLAYLIST = "users/me/playlists/{playlistId}";
        public const string ADD_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries";
        public const string ADD_PLAYLIST_ENTRIES = "users/me/playlists/{playlistId}/entries-batch";
        public const string ADD_PLAYLIST_SCOPE_ENTRIES = "users/me/playlists/{playlistId}/entries-scope";
        public const string REMOVE_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries/{playlistEntryId}";
        public const string MOVE_PLAYLIST_ENTRY = "users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
    }
}
