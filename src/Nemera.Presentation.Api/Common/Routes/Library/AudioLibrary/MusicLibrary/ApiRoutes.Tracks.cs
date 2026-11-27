namespace Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Tracks route.
    /// </summary>
    public static class Tracks
    {
        /// <summary>
        /// The route for adding a track to an album of an artist in a library.
        /// </summary>
        public const string ADD_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks";

        /// <summary>
        /// The route for getting a track of an album of an artist in a library.
        /// </summary>
        public const string GET_TRACK_BY_ID = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for getting a shuffled selection of the tracks of a library.
        /// </summary>
        public const string SHUFFLE_TRACKS = "/libraries/{libraryId}/tracks/shuffle";

        /// <summary>
        /// The route for updating a track of an album of an artist in a library.
        /// </summary>
        public const string UPDATE_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for deleting a track of an album of an artist in a library.
        /// </summary>
        public const string DELETE_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for getting the deleted tracks of a library.
        /// </summary>
        public const string GET_DELETED_TRACKS = "/libraries/{libraryId}/tracks/deleted-items";

        /// <summary>
        /// The route for restoring a deleted track of an album of an artist in a library.
        /// </summary>
        public const string RESTORE_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore";

        /// <summary>
        /// The route for streaming a track of an album of an artist in a library.
        /// </summary>
        public const string STREAM_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream";

        /// <summary>
        /// The route for streaming a segment of a track delivery of a track of an album of an artist in a library.
        /// </summary>
        public const string STREAM_TRACK_SEGMENT = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}";

        /// <summary>
        /// The route for getting the playback manifest of a track of an album of an artist in a library.
        /// </summary>
        public const string GET_PLAYBACK_MANIFEST = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest";
    }
}
