namespace Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

/// <summary>
/// Class for the collection of the music play history routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the music play history of the current user.
    /// </summary>
    public static class PlayHistory
    {
        public const string START_PLAY = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/plays";
        public const string GET_PLAY = "/users/me/music-play-history/plays/{playRecordId}";
        public const string REPORT_PLAY_PROGRESS = "/users/me/music-play-history/plays/{playRecordId}/progress";
        public const string COMPLETE_PLAY = "/users/me/music-play-history/plays/{playRecordId}/complete";
        public const string STOP_PLAY = "/users/me/music-play-history/plays/{playRecordId}/stop";
    }
}
