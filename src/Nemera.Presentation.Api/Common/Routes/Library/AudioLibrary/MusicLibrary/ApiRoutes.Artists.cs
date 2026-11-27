namespace Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

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
        /// <summary>
        /// The route for getting an artist of a library.
        /// </summary>
        public const string GET_ARTIST_BY_ID = "/libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for getting the artists of a library.
        /// </summary>
        public const string GET_ARTISTS = "/libraries/{libraryId}/artists";

        /// <summary>
        /// The route for getting the artists of a library in their lightweight form.
        /// </summary>
        public const string GET_ARTISTS_LITE = "/libraries/{libraryId}/artists/lite";

        /// <summary>
        /// The route for adding an artist to a library.
        /// </summary>
        public const string ADD_ARTIST = "/libraries/{libraryId}/artists";

        /// <summary>
        /// The route for updating an artist of a library.
        /// </summary>
        public const string UPDATE_ARTIST = "/libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for updating the cover of an artist of a library.
        /// </summary>
        public const string UPDATE_ARTIST_COVER = "/libraries/{libraryId}/artists/{artistId}/cover";

        /// <summary>
        /// The route for deleting an artist of a library.
        /// </summary>
        public const string DELETE_ARTIST = "/libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for getting the deleted artists of a library.
        /// </summary>
        public const string GET_DELETED_ARTISTS = "/libraries/{libraryId}/artists/deleted-items";

        /// <summary>
        /// The route for restoring a deleted artist of a library.
        /// </summary>
        public const string RESTORE_ARTIST = "/libraries/{libraryId}/artists/{artistId}/restore";
    }
}
