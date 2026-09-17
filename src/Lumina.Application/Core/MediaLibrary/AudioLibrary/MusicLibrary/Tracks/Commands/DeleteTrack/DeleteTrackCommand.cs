#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Command for deleting a track by its Id.
/// </summary>
/// <param name="TrackId">The unique identifier of the track to delete.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record DeleteTrackCommand(
    Guid TrackId
) : ICommand;
