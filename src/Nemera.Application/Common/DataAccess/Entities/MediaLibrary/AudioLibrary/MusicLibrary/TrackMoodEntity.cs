#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a mood of a track.
/// </summary>
/// <param name="Name">The name of the mood.</param>
[DebuggerDisplay("Name: {Name}")]
public record TrackMoodEntity(
    string Name
);
