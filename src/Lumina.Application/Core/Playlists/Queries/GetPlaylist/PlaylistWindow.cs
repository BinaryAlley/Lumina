namespace Lumina.Application.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// The bounds of one playlist entry read window.
/// </summary>
public static class PlaylistWindow
{
    /// <summary>
    /// The default number of entries a single read window returns.
    /// </summary>
    public const int DEFAULT_ENTRY_WINDOW_SIZE = 200;

    /// <summary>
    /// The maximum number of entries a single read window may return.
    /// </summary>
    public const int MAXIMUM_ENTRY_WINDOW_SIZE = 1000;
}
