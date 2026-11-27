#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Interface for the service that decides whether the audio of a track can be played as stored by a player, or has to be
/// repackaged into another container or re-encoded into another codec before the player can play it.
/// </summary>
public interface IPlaybackFormatService
{
    /// <summary>
    /// Decides how the audio of a track, described by <paramref name="sourceFormat"/>, is delivered to a player that advertises <paramref name="capabilities"/>.
    /// </summary>
    /// <param name="sourceFormat">The container and codec of the audio of the track, read from its stored probe result.</param>
    /// <param name="capabilities">The container and codec combinations the player advertises it can demux and decode directly.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the delivery decision, or an error when the server cannot produce any
    /// format the player advertises it supports.
    /// </returns>
    Result<PlaybackDeliveryDecisionDto> DecideDelivery(PlaybackSourceFormatDto sourceFormat, IReadOnlyList<PlaybackCapabilityDto> capabilities);
}
