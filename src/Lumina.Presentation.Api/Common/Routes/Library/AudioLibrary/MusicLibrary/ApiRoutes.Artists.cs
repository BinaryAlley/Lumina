namespace Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Artists route.
    /// </summary>
    public static class Artists
    {
        public const string GET_ARTIST_BY_ID = "/libraries/{libraryId}/artists/{artistId}";
        public const string GET_ARTISTS = "/libraries/{libraryId}/artists";
        public const string GET_ARTISTS_LITE = "/libraries/{libraryId}/artists/lite";
        public const string ADD_ARTIST = "/libraries/{libraryId}/artists";
        public const string UPDATE_ARTIST = "/libraries/{libraryId}/artists/{artistId}";
        public const string DELETE_ARTIST = "/libraries/{libraryId}/artists/{artistId}";
    }
}
