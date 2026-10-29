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
        public const string INDEX = "{culture}/playlists";
        public const string GET_PLAYLISTS = "{culture}/playlists/api-get-playlists";
        public const string CREATE_PLAYLIST = "{culture}/playlists/api-create-playlist";
        public const string ADD = "{culture}/playlists/item";
        public const string ITEM = "{culture}/playlists/item/{playlistId}";
        public const string GET_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-get-playlist";
        public const string UPDATE_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-update-playlist";
        public const string DELETE_PLAYLIST = "{culture}/playlists/item/{playlistId}/api-delete-playlist";
        public const string ADD_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/api-add-entry";
        public const string ADD_PLAYLIST_ENTRIES = "{culture}/playlists/item/{playlistId}/api-add-entries";
        public const string ADD_PLAYLIST_SCOPE_ENTRIES = "{culture}/playlists/item/{playlistId}/api-add-scope-entries";
        public const string REMOVE_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-remove-entry";
        public const string MOVE_PLAYLIST_ENTRY = "{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-move-entry";
    }
}
