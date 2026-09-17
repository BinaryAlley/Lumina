#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="GetTrackRequest"/>.
/// </summary>
public static class GetTrackRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetTrackQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetTrackQuery ToQuery(this GetTrackRequest request)
    {
        return new GetTrackQuery(
            request.TrackId);
    }
}
