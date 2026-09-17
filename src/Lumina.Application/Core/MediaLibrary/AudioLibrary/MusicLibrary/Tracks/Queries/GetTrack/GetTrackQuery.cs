#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Query for getting a track by its Id.
/// </summary>
/// <param name="TrackId">The unique identifier of the track to get.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record GetTrackQuery(
    Guid TrackId
) : IQuery;
