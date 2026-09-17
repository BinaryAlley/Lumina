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
        public const string GET_TRACK_BY_ID = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
        public const string UPDATE_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
        public const string DELETE_TRACK = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
    }
}
