/**
 * Web Audio output and scheduler of the audio player.
 *
 * The player never uses a native audio element, so the sound of a track is produced here. This module owns an
 * AudioContext, a master GainNode that carries the output volume and the mute state, and a per-track GainNode that
 * carries the ReplayGain normalization of the current track, and it schedules decoded audio for playback. It does not
 * read files, decode anything or fetch bytes: the download engine of the player brings the bytes in, the decode pipeline
 * turns them into decoded audio, and the scheduler of this module turns that decoded audio into sound.
 *
 * Signal chain
 * ------------
 * Every scheduled source is connected to the per-track GainNode, which feeds an AnalyserNode, which feeds the master GainNode, which feeds the destination, so:
 *   - the master gain is the output volume, and it is zero while the player is muted, which is how mute is honored;
 *   - the per-track gain is the ReplayGain linear gain of the current track, chosen by the user's ReplayGain mode among the values the playback manifest carries;
 *   - the analyzer sits after the per-track gain and before the master gain, so it observes the level of the track itself,
 *     after its ReplayGain normalization but before the output volume. The VU meter of the bar reads it, which is why
 *     changing the volume, or muting the player, never changes the height of the meter for the same moment of audio.
 * Keeping the master gain and the ReplayGain gain on two separate nodes lets either change without disturbing the other.
 * The engine of the player folds the mute state into the volume it passes, so it delivers a mute as setVolumeAsync(0)
 * and never calls setMutedAsync; that setter exists for a caller that owns both the volume and the mute state itself.
 * The analyzer is created only when the audio context exposes createAnalyser, so a reduced context, or a test double, still builds the gain graph and simply exposes no analyzer.
 *
 * Scheduling model
 * ----------------
 * A decoded AudioData is converted to an AudioBuffer, one channel at a time, and scheduled with start(when) on the
 * context timeline. The scheduler keeps a media cursor, the media position of the next buffer, and a wall clock cursor,
 * the context time at which the next buffer starts, and it lays every buffer directly after the one before it, so the
 * decoded audio of a track plays back to back with no gap. It maintains the playhead as an anchor, a pair of a context
 * time and the media position the audio had at that context time, so the current time is derived from the context clock
 * rather than guessed, and it stays accurate while the context runs and frozen while the context is suspended.
 *
 * The playhead is the media position of the audio, not the position of the download: the scheduler reports the position
 * of the samples that were actually handed to it, so a track that has only been partially decoded reports the position
 * it reached, never the position the buffer has merely downloaded.
 *
 * End of stream
 * -------------
 * A drained schedule is not the end of a track: a decode that is slower than real time, a garbage collection pause or a
 * cold start can leave the schedule empty while more audio is still on its way. The renderer reports the end only once
 * the schedule has drained and the decode pipeline has stated that it fed the last decoded frame, by calling
 * markStreamComplete. Completion is never inferred from the duration of the manifest, because that duration is a whole
 * number of seconds and a decode that is still on its way near the end would then have its last frames cut off. A drained
 * schedule that is not complete is a gap: the renderer keeps playing, reports the playhead pinned to the frontier of the
 * scheduled audio, and waits for more, which is what lets the pipeline treat a gap as an underrun and recover from
 * it instead of ending the track. The end is also detected from the end of the last source, so a tab that the browser
 * hides, and whose animation frame loop it suspends, still reaches the end while the audio keeps playing. The end is
 * reported through onEnded once per configured track: a buffer scheduled after an end resumes playback, but the callback
 * is not fired a second time, so a consumer that advances a queue on it is never advanced twice for one track.
 *
 * Backpressure
 * ------------
 * The scheduler accepts decoded audio as fast as it is handed to it and never drops a buffer, so a decoder that runs
 * faster than real time would queue a large part of a track and hold its decoded audio in memory. It therefore exposes
 * getScheduledAheadSeconds, the wall time between the audio clock and the end of the schedule, which a decode loop
 * polls to pace itself, exactly as it uses the decode queue size of its decoder. A caller that cannot be trusted to pace
 * itself can be contained with the maxScheduledAheadSeconds option, which refuses a buffer that would run the schedule
 * too far ahead instead of letting it, and the decoded audio it retains, grow without bound.
 *
 * Seeking
 * -------
 * The renderer owns the absolute media position of the track, not a position relative to whatever was decoded first, so
 * seekTo moves the schedule to a media offset: it releases the scheduled audio, places the playhead there, and treats
 * the next buffer that is scheduled as the audio at that position. It is only the scheduler half of a seek; the caller
 * resets its decoder, fetches the byte range of the target and re-demuxes from there, after which the renderer keeps the
 * reported time, the duration and the end check absolute across the seek.
 *
 * The caller must supply the first buffer that follows a seek starting exactly at the requested position. The renderer
 * stamps that buffer as the audio at the target and has no way to correct an offset it is not told about, so a demuxer
 * that lands on a packet boundary before the target must trim the leading samples to the target before it schedules
 * them. Because a null schedule reports no end by itself, the renderer also never declares the end until at least one
 * buffer has been scheduled since the last reset.
 *
 * Playing, pausing and the playback rate
 * --------------------------------------
 * playAsync resumes the AudioContext, which is the call that needs a user gesture, and is meant to be invoked from the
 * play affordance of the player. pauseAsync suspends the AudioContext, so the context clock freezes and the current
 * time freezes with it, while the buffers that are already scheduled stay scheduled; resuming continues at the very
 * position the pause froze. A playback rate other than one rescales how much media a unit of context time carries, so
 * changing it re-times the buffers that have not finished yet, keeping the audio continuous.
 *
 * Sample rate
 * -----------
 * A decoded AudioData may carry a sample rate that differs from the sample rate of the context. The buffer of such a
 * decode is created at the decoded rate whenever the browser accepts it, which makes the Web Audio graph resample it on
 * output; only a rate outside the range a buffer may carry falls back to a linear resample into the context rate, so the
 * common case never pays for a resample in JavaScript.
 *
 * Decoded audio contract
 * ----------------------
 * The scheduler copies each channel of an AudioData as planar 32 bit floats, so the decode pipeline must hand over planar
 * float audio. The module cannot enforce the layout of a decoder, so a source in any other layout is refused with a
 * typed error rather than reinterpreted. The scheduler takes ownership of every AudioData it receives: it closes it,
 * releasing its native memory, before schedule returns or throws, so a caller must not use the object again and must not
 * close it itself. A decode loop therefore hands each decoded frame to schedule exactly once.
 *
 * Reporting and failures
 * ----------------------
 * The renderer is a sub component, like the download engine. It records the failures it cannot recover from, so that they
 * are readable through getError: a context that refuses to suspend or to close, and a decode it refuses, all land there.
 * A condition it recovers from, such as a gain automation the browser rejects and that falls back to a direct assignment,
 * is only logged. The calls a pipeline drives, configure, playAsync, schedule and seekTo, throw the typed error, because
 * the caller that triggered the failure is the one that can react to it; setPlaybackRate can also throw when the re-time
 * of the schedule fails, while the other setters and the pause and teardown calls never throw. A caller observes the
 * playhead and the end of the audio through the optional onTimeUpdate and onEnded callbacks, or by polling getCurrentTime
 * and isPlaying: the renderer advances its own schedule for the whole duration of playback, so the end is reached and
 * isPlaying turns false either way. destroyAsync releases every resource, including the AudioContext when the renderer
 * owns it.
 *
 * Using it
 * --------
 *   const renderer = LuminaAudioPlayer.createTrackRenderer({
 *       replayGainMode: host.getPreferences().replayGainMode,
 *       events: host.events,
 *       onTimeUpdate: (time, trackDuration, timelineDuration) => host.reportTime(time, trackDuration),
 *       onEnded: () => host.reportEnded()
 *   });
 *   renderer.configure(manifest);
 *   await renderer.playAsync();          // from a user gesture
 *   renderer.schedule(audioData);        // from the decode output callback, once per decoded frame,
 *                                        // paced by renderer.getScheduledAheadSeconds()
 *   renderer.markStreamComplete();       // once the last decoded frame of the track was fed
 *   await renderer.pauseAsync();
 *   await renderer.destroyAsync();
 *
 * When the player event bus is provided, the renderer follows the ReplayGain mode of the player on its own. Volume and
 * mute are deliberately not read from the bus: they are driven by the caller through setVolumeAsync and setMutedAsync,
 * which is what the engine interface hands over, so the master gain has a single writer.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Clamps a number into an inclusive range, through the shared helper on the namespace. The reference is resolved on
     * every call rather than captured at load time, so this module does not depend on the load order of the util.
     * @param {number} value - The value to clamp.
     * @param {number} minimum - The inclusive lower bound.
     * @param {number} maximum - The inclusive upper bound.
     * @returns {number} The clamped value.
     */
    function clamp(value, minimum, maximum) {
        return LuminaAudioPlayer.clamp(value, minimum, maximum);
    }

    /**
     * The user facing ways ReplayGain normalization is applied to a track.
     *
     * This list mirrors the ReplayGainMode enumeration of the server (Lumina.Domain.SharedKernel.Common.Enums.Playback).
     * A mode added to the server must be added here too: a value the server sends that is not listed is reported by
     * normalizeReplayGainMode and treated as no normalization, so the two lists cannot drift apart silently.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerReplayGainMode = Object.freeze({
        // ReplayGain is not applied, and the track plays at the loudness at which it was stored.
        OFF: 'Off',
        // Each track is normalized with its own ReplayGain track gain.
        TRACK: 'Track',
        // Each track is normalized with the ReplayGain album gain of its album.
        ALBUM: 'Album'
    });

    /**
     * The lowest playback rate the renderer accepts. A lower rate stretches the audio so far that it stops being useful,
     * and a rate of zero or below would freeze or reverse the timeline of the scheduler. It is a module local, not a
     * namespace value, so no other script can change the bound the renderer enforces.
     * @readonly
     * @type {number}
     */
    const MIN_PLAYBACK_RATE = 0.25;

    /**
     * The highest playback rate the renderer accepts.
     * @readonly
     * @type {number}
     */
    const MAX_PLAYBACK_RATE = 4;

    /**
     * The lowest sample rate an AudioBuffer may carry, per the Web Audio specification. A buffer created below it, or
     * above MAX_AUDIO_BUFFER_SAMPLE_RATE, is rejected by the browser.
     * @readonly
     * @type {number}
     */
    const MIN_AUDIO_BUFFER_SAMPLE_RATE = 3000;

    /**
     * The highest sample rate an AudioBuffer may carry, per the Web Audio specification.
     * @readonly
     * @type {number}
     */
    const MAX_AUDIO_BUFFER_SAMPLE_RATE = 768000;

    /**
     * The largest number of frames a single decoded chunk may carry. It is a hard allocation ceiling, far above the size of
     * a real decoded packet, which is a fraction of a second: it exists only to refuse a value so large that allocating a
     * buffer for it would be the wrong thing to do.
     * @readonly
     * @type {number}
     */
    const MAX_DECODED_FRAMES_PER_CHUNK = 1000000;

    /**
     * The largest number of channels a single decoded chunk may carry. Web Audio refuses to create a buffer with more than
     * 32 channels, so the bound matches that limit and a chunk beyond it is rejected here instead of failing later in createBuffer.
     * @readonly
     * @type {number}
     */
    const MAX_DECODED_CHANNELS = 32;

    /**
     * The largest number of samples, that is frames times channels, a single decoded chunk, or the resampled output of
     * one, may carry. It is the bound that actually matters, because a buffer allocates its samples in one block, so the
     * frame and channel bounds are checked together as a product through this one. At four bytes a sample it caps a
     * single allocation at sixteen megabytes.
     * @readonly
     * @type {number}
     */
    const MAX_DECODED_SAMPLES_PER_CHUNK = 4194304;

    /**
     * The lead, in seconds, the very first buffer of a chain is scheduled ahead of the context clock. Without a lead, the
     * scheduling could land in the past of the audio thread and drop the start, while a large lead would delay the first
     * sound; this one is small enough to be inaudible and large enough to absorb the audio thread quantum.
     * @readonly
     * @type {number}
     */
    const MIN_SCHEDULE_LEAD_SECONDS = 0.02;

    /**
     * The time constant, in seconds, of a master gain change. It de-zippers a volume or mute change, so dragging the
     * volume control never produces a click.
     * @readonly
     * @type {number}
     */
    const MASTER_GAIN_RAMP_SECONDS = 0.02;

    /**
     * The time constant, in seconds, of a per-track gain change, applied when a track starts or the ReplayGain mode
     * changes.
     * @readonly
     * @type {number}
     */
    const TRACK_GAIN_RAMP_SECONDS = 0.05;

    /**
     * The ramp, in seconds, of the ReplayGain change at a gapless boundary. It is scheduled at the context time the next
     * track starts, so it is short enough to be inaudible as a blend and long enough not to click.
     * @readonly
     * @type {number}
     */
    const TRACK_GAIN_HANDOFF_RAMP_SECONDS = 0.01;

    /**
     * The interval, in milliseconds, the reporter falls back to when the page has no requestAnimationFrame.
     * @readonly
     * @type {number}
     */
    const REPORTER_FALLBACK_INTERVAL_MS = 16;

    /**
     * The transform size of the analyzer that taps the output for the VU meter. It is the smallest size that resolves the low bands the meter starts at.
     * @readonly
     * @type {number}
     */
    const OUTPUT_ANALYSER_FFT_SIZE = 2048;

    /**
     * Coerces a value to a finite number, or null when it is absent or not finite.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The finite number, or null.
     */
    function toFiniteNumber(value) {
        if (value === null || value === undefined || value === '')
            return null;
        const number = Number(value);
        return Number.isFinite(number) ? number : null;
    }

    /**
     * Normalizes a ReplayGain mode into one of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values, defaulting an absent or unrecognized value to no normalization.
     * @param {string} value - The mode to normalize, as the user settings or the manifest spell it.
     * @returns {string} One of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values.
     */
    function normalizeReplayGainMode(value) {
        // An absent mode, or an empty string, is simply no normalization and is not reported.
        if (value === null || value === undefined || value === '')
            return LuminaAudioPlayer.PlayerReplayGainMode.OFF;
        if (typeof value === 'string') {
            const text = value.trim().toLowerCase();
            if (text === '' || text === 'off')
                return LuminaAudioPlayer.PlayerReplayGainMode.OFF;
            if (text === 'track')
                return LuminaAudioPlayer.PlayerReplayGainMode.TRACK;
            if (text === 'album')
                return LuminaAudioPlayer.PlayerReplayGainMode.ALBUM;
        }
        // A value the renderer does not know is very likely a mode the server added and this list does not carry yet, so it
        // is reported, whatever its type, instead of being dropped silently; the sync of the two lists is documented on the enum.
        console.warn(`Audio player: the ReplayGain mode '${String(value)}' is not known to the renderer, so no normalization is applied.`);
        return LuminaAudioPlayer.PlayerReplayGainMode.OFF;
    }

    /**
     * Converts a ReplayGain value in decibels into a linear gain, capped so it cannot clip the signal.
     *
     * ReplayGain carries a track, or album, gain to bring the loudness of the audio to the reference level, and a peak,
     * the highest sample amplitude of the audio. A gain that would push the peak above the full scale would clip, so the
     * gain is limited to the largest value that keeps the normalized peak at or below the full scale, exactly as the
     * ReplayGain specification prescribes. A missing gain means no normalization, and a missing or non positive peak leaves the gain uncapped.
     * @param {number|null} gainDb - The ReplayGain value, in decibels.
     * @param {number|null} peak - The peak amplitude the gain is applied to.
     * @returns {number} The linear gain, one when there is nothing to apply.
     */
    function computeReplayGainLinearGain(gainDb, peak) {
        const decibels = toFiniteNumber(gainDb);
        if (decibels === null)
            return 1;
        let linear = Math.pow(10, decibels / 20);
        const peakValue = toFiniteNumber(peak);
        if (peakValue !== null && peakValue > 0) {
            const ceiling = 1 / peakValue;
            if (linear > ceiling)
                linear = ceiling;
        }
        return Number.isFinite(linear) && linear >= 0 ? linear : 1;
    }

    /**
     * Resolves the ReplayGain values a mode selects out of a playback manifest.
     *
     * A gain and its peak belong together: the peak is the amplitude the gain is applied to, so a pair is always taken as
     * a unit. The track mode uses the track pair and falls back to the album pair, the album mode does the opposite, and
     * the off mode selects nothing. The fallback matters because a track whose album was not analyzed can carry only one
     * of the two pairs, and normalizing with the pair that exists is better than not normalizing at all. A peak is never
     * taken from one pair while the gain comes from the other, because the anti-clip ceiling would then be computed from a peak the applied gain does not belong to.
     * @param {Object|null} manifest - The playback manifest of the track.
     * @param {string} mode - One of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values.
     * @returns {{ gainDb: number|null, peak: number|null }} The selected pair, null when nothing is selected.
     */
    function resolveReplayGain(manifest, mode) {
        if (!manifest || mode === LuminaAudioPlayer.PlayerReplayGainMode.OFF)
            return { gainDb: null, peak: null };
        const trackGain = toFiniteNumber(manifest.replayGainTrackGain);
        const trackPeak = toFiniteNumber(manifest.replayGainTrackPeak);
        const albumGain = toFiniteNumber(manifest.replayGainAlbumGain);
        const albumPeak = toFiniteNumber(manifest.replayGainAlbumPeak);
        // The gain decides which pair is used, and the peak of that same pair goes with it.
        if (mode === LuminaAudioPlayer.PlayerReplayGainMode.ALBUM) {
            if (albumGain !== null)
                return { gainDb: albumGain, peak: albumPeak };
            return { gainDb: trackGain, peak: trackPeak };
        }
        if (trackGain !== null)
            return { gainDb: trackGain, peak: trackPeak };
        return { gainDb: albumGain, peak: albumPeak };
    }

    /**
     * Creates an AudioContext through the browser constructors.
     * @returns {AudioContext|null} The created context, or null when the browser exposes no constructor.
     */
    function createDefaultAudioContext() {
        const targetWindow = typeof window !== 'undefined' ? window : null;
        if (!targetWindow)
            return null;
        const ContextConstructor = targetWindow.AudioContext || targetWindow.webkitAudioContext;
        return typeof ContextConstructor === 'function' ? new ContextConstructor() : null;
    }

    /**
     * Produces sound through the Web Audio API and schedules the decoded audio of a track for playback.
     */
    class TrackRenderer {
        /**
         * Creates an instance of TrackRenderer.
         * @param {Object} [options] - The renderer options.
         * @param {AudioContext} [options.audioContext] - An existing audio context to use. One is created through the context factory when omitted.
         * @param {Function} [options.contextFactory] - A factory that returns an AudioContext. Defaults to the browser constructors.
         * @param {boolean} [options.ownsContext] - True when the renderer owns the audio context and may close it on destroy. Defaults to true; pass false when the context is shared or owned by the caller and must outlive this renderer.
         * @param {number} [options.volume] - The output volume, between 0 and 1. Defaults to 1.
         * @param {boolean} [options.isMuted] - True to mute the output. Defaults to false.
         * @param {string} [options.replayGainMode] - The user's ReplayGain mode, one of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values. Defaults to no normalization.
         * @param {number} [options.playbackRate] - The playback rate, within the accepted playback rate range. Defaults to 1.
         * @param {number} [options.maxScheduledAheadSeconds] - An optional ceiling, in seconds, on how far ahead of the audio clock the schedule may run. Scheduling beyond it is refused with a typed error, so a caller that never paces itself cannot grow the schedule without bound. Leave the option out to disable the ceiling; a value of zero is a real ceiling that refuses every buffer, not a way to disable it.
         * @param {Object} [options.events] - The player event bus. When provided, the renderer follows the player's ReplayGain mode changes.
         * @param {Function} [options.onTimeUpdate] - Called while the audio plays with the absolute timeline position, the
         * duration of the current track, and the duration of the whole shared timeline, in seconds. The position is always
         * on the timeline, so the timeline duration is the value that bounds a seek; the track duration is the one the bar shows for the current item.
         * @param {Function} [options.onEnded] - Called once per configured track when its scheduled audio has been fully played. Audio scheduled after the end, without a configure or a seek, continues the same logical track and does not report the end again.
         */
        constructor(options = {}) {
            this.context = options.audioContext || null;
            this.contextFactory = typeof options.contextFactory === 'function' ? options.contextFactory : createDefaultAudioContext;
            // A context is a long lived resource that a browser counts, so the renderer closes it on destroy only when it
            // owns it; a caller that shares or injects one passes ownsContext false.
            this.ownsContext = options.ownsContext !== false;
            // A ceiling of zero is meaningful, so a negative value is clamped to it rather than disabling the ceiling.
            this.maxScheduledAheadSeconds = toFiniteNumber(options.maxScheduledAheadSeconds);
            if (this.maxScheduledAheadSeconds !== null)
                this.maxScheduledAheadSeconds = Math.max(0, this.maxScheduledAheadSeconds);
            this.volume = clamp(toFiniteNumber(options.volume) === null ? 1 : Number(options.volume), 0, 1);
            // Boolean state keeps its conventional is/has prefix, except where a public is*() accessor already owns that
            // name: isMuted() and isDestroyed() are those accessors here, so the backing fields keep the participles.
            this.muted = options.isMuted === true;
            this.replayGainMode = normalizeReplayGainMode(options.replayGainMode);
            // The default is assigned to the field before it is normalized, so an absent or invalid option falls back to
            // it instead of to the still unset field, which would make every duration computation a NaN.
            this.playbackRate = 1;
            this.playbackRate = this.normalizePlaybackRate(options.playbackRate);
            this.events = options.events || null;
            this.onTimeUpdate = typeof options.onTimeUpdate === 'function' ? options.onTimeUpdate : null;
            this.onEnded = typeof options.onEnded === 'function' ? options.onEnded : null;

            this.masterGain = null;
            this.trackGain = null;
            // The analyzer that taps the track level for the VU meter of the bar, after the ReplayGain gain and before the
            // master gain that carries the volume and the mute state. It is null when the audio context of the environment
            // cannot create one, in which case the meter renders empty instead of failing.
            this.analyser = null;
            this.manifest = null;
            this.duration = 0;
            // The media duration of the whole shared timeline. It equals the duration of the current track until a gapless
            // handoff appends another track, which is why it is kept apart from the duration of the current track.
            this.timelineDuration = 0;
            this.trackGainValue = 1;
            // The ReplayGain change of a gapless handoff, scheduled at the context time the next track starts, or null.
            this.pendingGainChange = null;
            this.error = null;

            // The scheduled sources, in media order, each one holding the audio buffer it plays so a playback rate change
            // can re-time the part of the chain that has not finished yet.
            this.scheduledSources = [];
            // The index of the first entry of scheduledSources that has not finished yet. Cleanup advances it instead of
            // shifting the array, so releasing a finished source never costs the size of the schedule behind it.
            this.firstLiveIndex = 0;
            // The media position of the next buffer to schedule, in seconds.
            this.mediaCursor = 0;
            // The context time at which the next buffer starts, in seconds.
            this.nextStartTime = 0;
            // The playhead anchor: the context time and the media position of the audio at that context time.
            this.anchorContextTime = 0;
            this.anchorMediaTime = 0;
            // The media position the audio was frozen at by a pause, restored when playback resumes.
            this.pausedMediaTime = 0;

            this.playing = false;
            this.paused = false;
            this.destroyed = false;
            // True once a buffer has been scheduled since the last reset (configure, seekTo or destroy). It gates the end
            // detection, so a schedule that was just reset to a media position does not report the end of the track
            // before a single buffer has been scheduled at that position.
            this.hasScheduledSinceReset = false;
            // True once the decode pipeline has fed the last decoded frame of the track, which is what tells a drained schedule apart from the end of the track.
            this.streamComplete = false;
            // True once the current track has reached its end, that is once the schedule drained while the track was known
            // to be complete. It drives whether a newly scheduled buffer resumes playback; a configure, a seek or a newly scheduled buffer clears it.
            this.reachedEnd = false;
            // True once the end of the current configured track has been reported through onEnded. It is a latch of its
            // own, so the callback fires once per configured track even when audio is scheduled after the end; only a
            // configure or a seek, which start a new logical track, clear it.
            this.endReported = false;
            this.reporterHandle = null;
            this.unsubscribers = [];
            this.subscribeToEvents();
        }

        //+======================================================================================+
        //|                                      Lifecycle                                       |
        //+======================================================================================+

        /**
         * Prepares the renderer for a track: it makes sure the context and its gain nodes exist, resets the schedule and
         * the playhead, and applies the ReplayGain normalization of the track.
         * @param {Object} manifest - The playback manifest of the track, carrying its duration and ReplayGain values.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed.
         */
        configure(manifest) {
            if (this.destroyed)
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
            this.ensureContext();
            // A reconfigure starts a fresh track, so a failure of the previous one is not reported as a failure of this.
            this.error = null;
            this.stopAllSources();
            this.manifest = manifest || null;
            this.duration = this.manifest && toFiniteNumber(this.manifest.durationInSeconds) !== null
                ? Math.max(0, Number(this.manifest.durationInSeconds))
                : 0;
            this.timelineDuration = this.duration;
            this.mediaCursor = 0;
            this.nextStartTime = 0;
            this.pausedMediaTime = 0;
            this.playing = false;
            this.paused = false;
            this.streamComplete = false;
            this.anchorContextTime = this.context.currentTime;
            this.anchorMediaTime = 0;
            // A new track replaces any scheduled handoff gain of the previous one.
            this.pendingGainChange = null;
            this.applyTrackGain(0);
            this.applyMasterGain(0);
            return this;
        }

        /**
         * Adopts the manifest of a track that continues the current schedule without resetting it, which is how a gapless
         * transition applies the ReplayGain values of the next track. The schedule, the playhead and the completion state
         * are left untouched, because the audio of the next track is already appended after the current one.
         * @param {Object} manifest - The playback manifest of the continuing track.
         * @param {boolean} [isGainAlreadyScheduled=false] - True when scheduleManifestGain already scheduled the ReplayGain
         * change at the boundary, so applying it again with a late ramp is skipped.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed.
         */
        adoptManifest(manifest, isGainAlreadyScheduled = false) {
            if (this.destroyed)
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
            this.manifest = manifest || null;
            this.duration = this.manifest && toFiniteNumber(this.manifest.durationInSeconds) !== null
                ? Math.max(0, Number(this.manifest.durationInSeconds))
                : this.duration;
            this.pendingGainChange = null;
            // The gapless handoff schedules the gain automation at the exact boundary context time, so the manifest is
            // adopted without applying a second, late ramp when that automation was accepted.
            if (isGainAlreadyScheduled) {
                this.trackGainValue = this.computeTrackGain(this.manifest);
            } else {
                this.applyTrackGain(TRACK_GAIN_RAMP_SECONDS);
            }
            return this;
        }

        /**
         * Schedules the ReplayGain change of the next track at the context time that track starts, instead of applying it
         * when the boundary is detected up to a reporter tick later. The schedule is stored, so a ReplayGain mode change
         * before the boundary can reschedule it, and it is cleared when the handoff is abandoned.
         * @param {Object} manifest - The playback manifest of the next track.
         * @param {number} whenContextTime - The context time, in seconds, at which the next track starts.
         * @returns {boolean} True when the automation was scheduled, false when the parameter cannot automate.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed.
         */
        scheduleManifestGain(manifest, whenContextTime) {
            if (this.destroyed)
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
            this.ensureContext();
            // A call that cannot schedule anything must not leave a previous handoff change pending.
            this.pendingGainChange = null;
            const when = toFiniteNumber(whenContextTime);
            if (when === null || !this.trackGain || !this.trackGain.gain)
                return false;
            const param = this.trackGain.gain;
            if (typeof param.setValueAtTime !== 'function' || typeof param.linearRampToValueAtTime !== 'function')
                return false;
            const start = Math.max(this.context.currentTime, when);
            const gain = this.computeTrackGain(manifest);
            try {
                if (typeof param.cancelAndHoldAtTime === 'function') {
                    // Holding at the boundary samples the value the automation actually has at that time, including a ramp
                    // that is still in progress because the ReplayGain mode changed after the handoff was scheduled.
                    param.cancelAndHoldAtTime(start);
                } else {
                    const current = Number.isFinite(param.value) ? param.value : 1;
                    param.setValueAtTime(current, start);
                }
                param.linearRampToValueAtTime(gain, start + TRACK_GAIN_HANDOFF_RAMP_SECONDS);
                this.pendingGainChange = { manifest: manifest || null, when: start };
                return true;
            } catch (error) {
                this.pendingGainChange = null;
                console.debug('Audio player: the ReplayGain change of the next track could not be scheduled.', error);
                return false;
            }
        }

        /**
         * Cancels a scheduled gapless ReplayGain change and restores the gain of the current manifest. It is used when the handoff is abandoned before the next track starts.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         */
        cancelTrackGainChange() {
            this.pendingGainChange = null;
            if (this.trackGain && this.trackGain.gain && this.context
                && typeof this.trackGain.gain.cancelScheduledValues === 'function') {
                try {
                    this.trackGain.gain.cancelScheduledValues(this.context.currentTime);
                } catch (error) {
                    console.debug('Audio player: the scheduled ReplayGain change could not be cancelled.', error);
                }
            }
            if (!this.destroyed)
                this.applyTrackGain(0);
            return this;
        }

        /**
         * Extends the media duration of the shared timeline, so that a seek into a gapless chain is bounded by the audio
         * that is actually scheduled rather than by the duration of the first track of the chain. The duration is only
         * used to bound a seek and to report the timeline length; the end of the audio is detected from the schedule.
         * @param {number} duration - The media duration of the whole timeline, in seconds.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         */
        setTimelineDuration(duration) {
            const value = toFiniteNumber(duration);
            if (value !== null && value >= 0)
                this.timelineDuration = value;
            return this;
        }

        /**
         * Releases every resource of the renderer: the schedule, the gain nodes, the event subscriptions and, when the renderer owns it, the audio context.
         * @returns {Promise<void>} A promise that completes when the context is closed.
         */
        async destroyAsync() {
            this.destroyed = true;
            this.playing = false;
            this.paused = false;
            this.pendingGainChange = null;
            this.stopReporter();
            for (const unsubscribe of this.unsubscribers) {
                try {
                    unsubscribe();
                } catch (error) {
                    console.debug('Audio player: a renderer event subscription could not be released.', error);
                }
            }
            this.unsubscribers = [];
            this.stopAllSources();
            if (this.trackGain) {
                try {
                    this.trackGain.disconnect();
                } catch (error) {
                    console.debug('Audio player: the track gain node could not be disconnected.', error);
                }
                this.trackGain = null;
            }
            if (this.analyser) {
                try {
                    this.analyser.disconnect();
                } catch (error) {
                    console.debug('Audio player: the output analyzer could not be disconnected.', error);
                }
                this.analyser = null;
            }
            if (this.masterGain) {
                try {
                    this.masterGain.disconnect();
                } catch (error) {
                    console.debug('Audio player: the master gain node could not be disconnected.', error);
                }
                this.masterGain = null;
            }
            if (!this.context)
                return;
            // A context the caller owns is left open, because it outlives this renderer.
            if (!this.ownsContext) {
                this.context = null;
                return;
            }
            if (typeof this.context.close === 'function') {
                try {
                    await this.context.close();
                } catch (error) {
                    // The context is still live. Its reference is kept rather than dropped, and the failure is recorded, so a
                    // caller can still reach the context through getContext() and close it itself; the renderer itself is
                    // destroyed and never uses it again. Dropping the only reference would lose a context that the browser counts until it is closed.
                    this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                    console.error('Audio player: the audio context could not be closed.', error);
                    return;
                }
            }
            this.context = null;
        }

        //+======================================================================================+
        //|                                     Playback control                                 |
        //+======================================================================================+

        /**
         * Resumes the audio context, which is the call that needs a user gesture, and starts the playhead.
         * @returns {Promise<void>} A promise that completes when the context is running.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed, or the context could not be resumed.
         */
        async playAsync() {
            if (this.destroyed)
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
            this.ensureContext();
            try {
                if (this.context.state === 'suspended' && typeof this.context.resume === 'function')
                    await this.context.resume();
            } catch (error) {
                throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
            }
            if (this.playing)
                return;
            this.reanchor(this.context.currentTime, this.pausedMediaTime);
            this.paused = false;
            this.playing = true;
            this.startReporter();
        }

        /**
         * Suspends the audio context, freezing the context clock and the current time, while the schedule is kept.
         * @returns {Promise<void>} A promise that completes when the context is suspended.
         */
        async pauseAsync() {
            if (!this.playing || this.destroyed)
                return;
            if (this.context && this.context.state === 'running' && typeof this.context.suspend === 'function') {
                try {
                    await this.context.suspend();
                } catch (error) {
                    // A context that refuses to suspend keeps playing, so the player is not paused: nothing is changed,
                    // and the failure is recorded, so isPaused never claims a pause that did not happen.
                    this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                    return;
                }
            }
            // The context is suspended now, so the clock is frozen and the media position it reports is the position the
            // pause holds; marking the state after the suspend is what keeps the two consistent.
            this.pausedMediaTime = this.getCurrentTime();
            this.playing = false;
            this.paused = true;
            this.stopReporter();
        }

        /**
         * Sets the playback rate, re-timing the part of the schedule that has not finished yet, so the audio stays
         * continuous. Like the other setters, it does not throw on a destroyed renderer; it merely has nothing to apply.
         * It can still throw when the re-time of the schedule itself fails, which is a failure of the audio pipeline,
         * rather than of the state of the renderer.
         * @param {number} rate - The playback rate, within the accepted playback rate range.
         * @returns {number} The applied playback rate.
         * @throws {LuminaAudioPlayer.PlayerError} When the schedule could not be re-timed.
         */
        setPlaybackRate(rate) {
            if (this.destroyed)
                return this.playbackRate;
            const next = this.normalizePlaybackRate(rate);
            if (next === this.playbackRate)
                return this.playbackRate;
            if (this.context) {
                const now = this.context.currentTime;
                // The media position is captured under the old rate before the rate changes, so the anchor can be moved to
                // the same position under the new rate, at the context time the rescheduled audio actually resumes. The
                // re-time is applied only once it fully succeeded, so a failure leaves the rate and the anchor unchanged.
                const mediaAtNow = this.getCurrentTime();
                const restartTime = this.retimeScheduledSources(now, mediaAtNow, next);
                this.anchorContextTime = restartTime;
                this.anchorMediaTime = mediaAtNow;
            }
            this.playbackRate = next;
            return this.playbackRate;
        }

        /**
         * Sets the output volume, which is independent of the mute state. The engine of the player passes the effective
         * volume here, which is zero while the player is muted, so driving the renderer through the engine honors mute
         * without ever calling setMutedAsync.
         * @param {number} volume - The output volume, between 0 and 1.
         * @returns {Promise<number>} The applied volume.
         */
        async setVolumeAsync(volume) {
            const value = clamp(toFiniteNumber(volume) === null ? this.volume : Number(volume), 0, 1);
            this.volume = value;
            this.applyMasterGain(MASTER_GAIN_RAMP_SECONDS);
            return this.volume;
        }

        /**
         * Sets the mute state. Mute is a flag of its own: the volume is kept, and only the gain sent to the output is
         * zeroed, so unmuting restores the volume the user had set.
         *
         * A caller that drives the renderer through the player engine does not need this: the engine delivers the mute as
         * setVolumeAsync(0), because it folds the mute state into the effective volume. Use this setter only when a caller
         * owns both the volume and the mute state itself and wants the volume kept across a mute.
         * @param {boolean} muted - True to mute the output, false to unmute it.
         * @returns {Promise<boolean>} The applied mute state.
         */
        async setMutedAsync(muted) {
            this.muted = muted === true;
            this.applyMasterGain(MASTER_GAIN_RAMP_SECONDS);
            return this.muted;
        }

        /**
         * Sets the ReplayGain mode and applies the gain it selects out of the manifest of the current track.
         * @param {string} mode - One of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values.
         * @returns {string} The applied mode.
         */
        setReplayGainMode(mode) {
            const next = normalizeReplayGainMode(mode);
            if (next === this.replayGainMode)
                return this.replayGainMode;
            this.replayGainMode = next;
            // The current gain is applied first, which cancels the scheduled handoff change, so a scheduled change is put back with the values of the new mode.
            const pending = this.pendingGainChange;
            this.applyTrackGain(TRACK_GAIN_RAMP_SECONDS);
            if (pending)
                this.scheduleManifestGain(pending.manifest, pending.when);
            return this.replayGainMode;
        }

        //+======================================================================================+
        //|                                     Scheduling                                       |
        //+======================================================================================+

        /**
         * Converts a decoded AudioData into an AudioBuffer, schedules it to play directly after the audio that is already scheduled, and advances the playhead.
         *
         * The buffer is adopted as the audio at the current media position of the timeline, so a buffer that follows a
         * seekTo is held to begin exactly at the position that was requested. The renderer takes ownership of the
         * AudioData: it closes it, releasing its native memory, before this call returns or throws, so a caller must not
         * use the object again and must not close it itself. Scheduling audio while the renderer had reached the end of
         * the current track resumes the playback state, so the playhead and the callbacks continue to run.
         * @param {AudioData} audioData - The decoded audio to schedule. Ownership transfers to the renderer, which closes it.
         * @returns {number} The media position, in seconds, at the end of the scheduled audio.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed, or the audio could not be converted or scheduled. A refusal caused by the renderer's own <c>maxScheduledAheadSeconds</c> ceiling carries <c>details.reason === {@link LuminaAudioPlayer.PlayerErrorReason}.SCHEDULED_AHEAD_LIMIT</c> and is backpressure rather than corruption: it is thrown without being recorded as the renderer's last error, so a caller can treat it as recoverable. The frame is closed before this call returns or throws, whichever happens, so a caller must never close it and a refusal never leaks. Both properties are load-bearing for a caller, such as the decode pipeline, that classifies the refusal from the thrown value.
         */
        schedule(audioData) {
            // The decoded audio is consumed by this call whatever happens, so it is released even when the renderer refuses
            // it, which is what keeps a decode loop from leaking native memory when a frame is rejected.
            try {
                if (this.destroyed)
                    throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
                this.ensureContext();
                const buffer = this.createBufferFromAudioData(audioData);
                // A decode with no frame carries no media, so it contributes nothing to the timeline and is dropped.
                if (!buffer || buffer.duration <= 0)
                    return this.mediaCursor;
                // Scheduling after the current track already reached its end means the audio is playing on, so the playback
                // state is resumed, which is what a gapless handoff or a recovered underrun relies on.
                const wasEnded = this.reachedEnd;
                const now = this.context.currentTime;
                const rate = this.playbackRate;
                const mediaDuration = buffer.duration;
                const wallDuration = mediaDuration / rate;
                const hasScheduled = this.hasLiveEntries();
                // The ceiling is checked against the frontier the schedule would reach once this buffer is added, not against
                // the frontier it already has, so the schedule never settles a buffer beyond the ceiling. The start this
                // buffer would take is the one the chain gives it, which is the existing frontier when it is still ahead of
                // the clock and one lead ahead otherwise.
                const prospectiveStart = hasScheduled && this.nextStartTime > now ? this.nextStartTime : now + MIN_SCHEDULE_LEAD_SECONDS;
                if (this.maxScheduledAheadSeconds !== null && prospectiveStart + wallDuration - now > this.maxScheduledAheadSeconds)
                    // The refusal is backpressure, not a failure: a caller that paces itself retries once the schedule has
                    // drained, so it is thrown without being recorded as the renderer's last error, which would mark a
                    // renderer that is still healthy as failed, contradicting the reason the shared enum publishes.
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR, 'The schedule would run further ahead than the renderer allows.', { reason: LuminaAudioPlayer.PlayerErrorReason.SCHEDULED_AHEAD_LIMIT });
                let when = this.nextStartTime;
                // The chain is anchored on its first buffer, and re-anchored whenever it fell behind the context clock, which
                // is an underrun: the buffer starts as soon as it can and the playhead follows the audio that actually plays.
                // The first buffer of a chain, and any buffer whose start time has already passed (the chain fell behind the
                // clock), starts one lead ahead and re-anchors the playhead. A buffer that still starts in the future is laid
                // exactly on its start time, so a chain that is merely scheduled close to the clock is not turned into a gap.
                if (!hasScheduled || when < now) {
                    when = now + MIN_SCHEDULE_LEAD_SECONDS;
                    this.reanchor(when, this.mediaCursor);
                }
                let source = null;
                try {
                    source = this.context.createBufferSource();
                    source.buffer = buffer;
                    if (source.playbackRate)
                        source.playbackRate.value = rate;
                    // The end of a source is an end-of-audio signal that is not tied to the animation frame loop, which a
                    // hidden tab suspends, so the end of the track is still detected while the tab is in the background.
                    source.onended = () => this.handleSourceEnded();
                    source.connect(this.trackGain);
                    source.start(when);
                } catch (error) {
                    if (source) {
                        try {
                            source.disconnect();
                        } catch (disconnectError) {
                            console.debug('Audio player: the failed source could not be disconnected.', disconnectError);
                        }
                    }
                    throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                }
                this.scheduledSources.push({
                    source,
                    buffer,
                    // The media origin of the buffer, i.e. the media position of its first frame. It is kept so a later
                    // playback rate change can measure the resume offset against the buffer itself, never against a position
                    // the entry happened to start at.
                    bufferMediaStart: this.mediaCursor,
                    mediaDuration,
                    when,
                    rate
                });
                this.hasScheduledSinceReset = true;
                // Scheduling audio means the track is playing on, so a previous end of the current track no longer holds.
                this.reachedEnd = false;
                this.mediaCursor += mediaDuration;
                this.nextStartTime = when + wallDuration;
                this.cleanupFinishedSources();
                if (wasEnded && !this.playing && !this.paused) {
                    this.playing = true;
                    this.startReporter();
                }
                return this.mediaCursor;
            } finally {
                this.closeAudioData(audioData);
            }
        }

        /**
         * Releases the native memory of a decoded AudioData, which the renderer owns once schedule received it.
         * @param {AudioData} audioData - The decoded audio to close.
         */
        closeAudioData(audioData) {
            if (!audioData || typeof audioData.close !== 'function')
                return;
            try {
                audioData.close();
            } catch (error) {
                console.debug('Audio player: the decoded audio could not be closed.', error);
            }
        }

        /**
         * Moves the schedule to a media position: it stops and releases the scheduled audio, moves the playhead to that
         * position, and treats the next buffer that is scheduled as the audio at that position. It is the scheduler half
         * of a seek; the caller still resets its decoder, fetches the byte range of the position and re-demuxes from
         * there before it feeds decoded audio again. Because the renderer owns the absolute position of the track, a seek
         * keeps getCurrentTime, the reported time and the end check absolute instead of forcing every caller to translate
         * them through an offset of its own.
         *
         * The first buffer scheduled after this call must begin exactly at that position. A caller whose demuxer instead
         * lands on a packet boundary before the target must trim the leading samples to the target before scheduling
         * them, because the renderer adopts the requested position as the media origin of that buffer and cannot correct
         * an offset it is not told about.
         * @param {number} seconds - The media position to move to, in seconds.
         * @returns {number} The applied media position, in seconds.
         * @throws {LuminaAudioPlayer.PlayerError} When the renderer was destroyed.
         */
        seekTo(seconds) {
            if (this.destroyed)
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The renderer was destroyed.'));
            const request = toFiniteNumber(seconds);
            // A position that is not a number is not a seek: nothing is reset and the current media frontier is returned,
            // so an accidental NaN never tears down the schedule and drops the audio that is already playing.
            if (request === null)
                return this.mediaCursor;
            this.ensureContext();
            const position = Math.max(0, request);
            // A track whose duration is not known has no timeline bound to clamp against; clamping to the end of the
            // previous track would silently restart it at its base position.
            const limit = this.duration > 0 ? (this.timelineDuration > 0 ? this.timelineDuration : this.duration) : 0;
            const bounded = limit > 0 ? Math.min(position, limit) : position;
            this.stopAllSources();
            this.mediaCursor = bounded;
            this.nextStartTime = 0;
            this.streamComplete = false;
            this.pausedMediaTime = bounded;
            this.anchorContextTime = this.context.currentTime;
            this.anchorMediaTime = bounded;
            return bounded;
        }

        /**
         * States that the decode pipeline has fed the last decoded frame of the track. A drained schedule is treated as
         * the end of the track only after this call, so a gap caused by a decode that is slower than real time is not
         * mistaken for the end.
         * @returns {void}
         */
        markStreamComplete() {
            this.streamComplete = true;
        }

        /**
         * Clears the stream-complete state, so a drained schedule is treated as an underrun gap again instead of the end
         * of the track. It is used when a gapless handoff appends the next track after the end of the current decode was
         * already recorded: the schedule then carries more audio, and a slow download of that audio must pause and resume rather than end the second track early.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         */
        clearStreamComplete() {
            this.streamComplete = false;
            return this;
        }

        //+======================================================================================+
        //|                                      Query surface                                   |
        //+======================================================================================+

        /**
         * Gets the current media position of the audio, in seconds. It is derived from the context clock through the
         * playhead anchor, so it is precise while the context runs, and frozen while the context is suspended by a pause.
         * @returns {number} The current time, in seconds.
         */
        getCurrentTime() {
            // The playhead can never run past the media that has actually been scheduled: before the first buffer is
            // scheduled there is nothing to play, so the position is zero rather than the wall time that has elapsed
            // since playAsync, and a schedule that drained holds the position at its frontier until more audio arrives.
            const upperBound = this.mediaCursor;
            if (this.paused)
                return clamp(this.pausedMediaTime, 0, upperBound);
            if (!this.playing || !this.context)
                return clamp(this.anchorMediaTime, 0, upperBound);
            const media = this.anchorMediaTime + (this.context.currentTime - this.anchorContextTime) * this.playbackRate;
            return clamp(media, 0, upperBound);
        }

        /**
         * Converts an absolute media position of the shared timeline into the context time at which it will play. The
         * renderer owns the anchor, the playback rate and the lead time of the schedule, so a caller that needs to schedule
         * something at a media position must use this conversion instead of repeating the anchor math.
         * @param {number} mediaSeconds - The absolute media position, in seconds.
         * @returns {number} The context time, in seconds, or zero when the renderer has no context.
         */
        getContextTimeForMediaPosition(mediaSeconds) {
            const target = toFiniteNumber(mediaSeconds);
            if (target === null || !this.context)
                return 0;
            const rate = this.playbackRate > 0 ? this.playbackRate : 1;
            return this.context.currentTime + Math.max(0, (target - this.getCurrentTime()) / rate);
        }

        /**
         * Gets the duration of the current track, from its playback manifest.
         * @returns {number} The duration, in seconds, or zero when it is unknown.
         */
        getDuration() {
            return this.duration;
        }

        /**
         * Gets the duration of the whole shared timeline, which is longer than the current track when a gapless handoff
         * has appended another track. It is the value a seek is bounded by.
         * @returns {number} The timeline duration, in seconds, or zero when it is unknown.
         */
        getTimelineDuration() {
            return this.timelineDuration;
        }

        /**
         * Gets the output volume, which is independent of the mute state.
         * @returns {number} The volume, between 0 and 1.
         */
        getVolume() {
            return this.volume;
        }

        /**
         * Gets the mute state.
         * @returns {boolean} True when the output is muted.
         */
        isMuted() {
            return this.muted;
        }

        /**
         * Gets the playback rate.
         * @returns {number} The playback rate.
         */
        getPlaybackRate() {
            return this.playbackRate;
        }

        /**
         * Gets the ReplayGain mode in effect.
         * @returns {string} One of the {@link LuminaAudioPlayer.PlayerReplayGainMode} values.
         */
        getReplayGainMode() {
            return this.replayGainMode;
        }

        /**
         * Gets the linear gain currently applied to the current track for ReplayGain.
         * @returns {number} The per-track gain.
         */
        getTrackGainValue() {
            return this.trackGainValue;
        }

        /**
         * Gets the gain currently applied to the output, which is the volume, or zero when the output is muted.
         * @returns {number} The master gain.
         */
        getMasterGainValue() {
            return this.muted ? 0 : this.volume;
        }

        /**
         * Checks whether the renderer is playing.
         * @returns {boolean} True when the audio context is running and the schedule produces sound.
         */
        isPlaying() {
            return this.playing;
        }

        /**
         * Checks whether playback is suspended by a pause.
         * @returns {boolean} True when the renderer was paused.
         */
        isPaused() {
            return this.paused;
        }

        /**
         * Checks whether the renderer was destroyed.
         * @returns {boolean} True when the renderer was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Checks whether the schedule holds an entry that has not finished yet.
         * @returns {boolean} True when a live entry is scheduled.
         */
        hasLiveEntries() {
            return this.firstLiveIndex < this.scheduledSources.length;
        }

        /**
         * Checks whether any audio is scheduled and has not finished playing yet.
         * @returns {boolean} True when the schedule still holds audio.
         */
        hasScheduledAudio() {
            if (!this.hasLiveEntries())
                return false;
            const now = this.context ? this.context.currentTime : 0;
            // The entries are held in media order, so their finish times ascend and only the last one can still be ahead
            // of the clock: testing that one is enough, and it costs nothing per call.
            const last = this.scheduledSources[this.scheduledSources.length - 1];
            return last.when + last.mediaDuration / last.rate > now;
        }

        /**
         * Gets the media position, in seconds, of the next buffer to schedule, which is the frontier of the schedule. It
         * is the total media scheduled from the start of the track, and after a seek it is the absolute position of the
         * seek target plus the media scheduled since, because the renderer owns the absolute position of the track.
         * @returns {number} The media position of the schedule frontier, in seconds.
         */
        getScheduledMediaDuration() {
            return this.mediaCursor;
        }

        /**
         * Gets how far ahead of the audio clock, in seconds, the scheduled audio currently extends.
         *
         * The scheduler accepts decoded audio as fast as it is handed to it and never drops a buffer, so a decoder that
         * runs faster than real time would otherwise queue a large part of a track and hold its decoded audio in memory.
         * A decode loop polls this value to pace itself, exactly as it uses the decode queue size of its decoder, and
         * stops feeding while the schedule is already far enough ahead. It is zero when the schedule has drained.
         * @returns {number} The scheduled ahead duration, in seconds.
         */
        getScheduledAheadSeconds() {
            if (!this.context || this.nextStartTime <= 0)
                return 0;
            return Math.max(0, this.nextStartTime - this.context.currentTime);
        }

        /**
         * Gets the scheduled ahead ceiling the renderer enforces, in seconds, or null when it enforces none.
         *
         * It is published so that a caller that paces itself, such as the decode pipeline, can keep its own ceiling
         * strictly below it and never feed a frame the renderer would refuse. Keeping a decode ceiling above the renderer's
         * is what makes the renderer refuse, and a refused frame is closed and lost.
         * @returns {number|null} The ceiling, in seconds, or null when the renderer enforces none.
         */
        getMaxScheduledAheadSeconds() {
            return this.maxScheduledAheadSeconds;
        }

        /**
         * Checks whether the decode pipeline has stated that it fed the last decoded frame of the track.
         * @returns {boolean} True when the track is known to be complete.
         */
        isStreamComplete() {
            return this.streamComplete;
        }

        /**
         * Gets the last failure, or null when there was none.
         * @returns {LuminaAudioPlayer.PlayerError|null} The last failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Gets the audio context, or null when it has not been created yet, when the renderer does not own it, or once it
         * was closed. When destroyAsync ran but the context refused to close, it returns the still open context even though
         * isDestroyed() is true, so a caller can close it itself; the renderer never uses it again.
         * @returns {AudioContext|null} The audio context, which may outlive a destroyed renderer only when closing it failed.
         */
        getContext() {
            return this.context;
        }

        /**
         * Gets the analyzer that taps the output of the track gain, which is what the VU meter of the bar reads. The
         * analyzer sits after the ReplayGain normalization of the track and before the master gain that carries the
         * volume and the mute state, so it observes the level of the track itself and never the level of the speaker.
         * @returns {AnalyserNode|null} The output analyzer, or null when the context has none, when it has not been created yet, or once the renderer was destroyed.
         */
        getOutputAnalyser() {
            return this.analyser;
        }

        //+======================================================================================+
        //|                                     Internal helpers                                 |
        //+======================================================================================+

        /**
         * Makes sure the audio context and its gain nodes exist, creating them on first use.
         * @returns {AudioContext} The audio context.
         * @throws {LuminaAudioPlayer.PlayerError} When the environment exposes no audio context, or it could not be created.
         */
        ensureContext() {
            if (this.context) {
                if (!this.masterGain || !this.trackGain)
                    this.buildGainGraph();
                return this.context;
            }
            let context = null;
            try {
                context = this.contextFactory();
            } catch (error) {
                throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED);
            }
            if (!context || typeof context.createGain !== 'function')
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'This environment provides no audio context for the player output.'));
            this.context = context;
            this.buildGainGraph();
            return context;
        }

        /**
         * Builds the gain nodes of the output and connects them: the sources feed the per-track gain, which feeds the
         * output analyzer, which feeds the master gain, which feeds the destination. The analyzer is optional and is
         * skipped when the context cannot create one, in which case the per-track gain feeds the master gain directly.
         * Putting the analyzer before the master gain keeps the meter independent of the output volume and of the mute
         * state, so it measures the level of the track instead of the level of the speaker.
         */
        buildGainGraph() {
            this.masterGain = this.context.createGain();
            this.trackGain = this.context.createGain();
            this.analyser = this.createOutputAnalyser();
            if (this.analyser) {
                this.trackGain.connect(this.analyser);
                this.analyser.connect(this.masterGain);
            } else 
                this.trackGain.connect(this.masterGain);
            this.masterGain.connect(this.context.destination);
            this.applyMasterGain(0);
            this.applyTrackGain(0);
        }

        /**
         * Creates the analyzer that taps the output for the VU meter. Only its time domain buffer is used: the meter
         * applies its own window and transform to those samples, so the analyzer is configured with the transform size that buffer must have and nothing else.
         * @returns {AnalyserNode|null} The analyzer, or null when the context cannot create one.
         */
        createOutputAnalyser() {
            if (typeof this.context.createAnalyser !== 'function')
                return null;
            try {
                const analyser = this.context.createAnalyser();
                analyser.fftSize = OUTPUT_ANALYSER_FFT_SIZE;
                return analyser;
            } catch (error) {
                console.debug('Audio player: the output analyzer of the VU meter could not be created.', error);
                return null;
            }
        }

        /**
         * Applies the output gain, which is the volume, or zero while muted, to the master gain node.
         * @param {number} rampSeconds - The duration of the ramp to the new value, in seconds. Zero applies it at once.
         */
        applyMasterGain(rampSeconds) {
            if (!this.masterGain || !this.masterGain.gain)
                return;
            this.setAudioParamValue(this.masterGain.gain, this.muted ? 0 : this.volume, rampSeconds);
        }

        /**
         * Applies the ReplayGain normalization selected by the mode and the manifest to the per-track gain node.
         * @param {number} rampSeconds - The duration of the ramp to the new value, in seconds. Zero applies it at once.
         */
        applyTrackGain(rampSeconds) {
            this.trackGainValue = this.computeTrackGain(this.manifest);
            if (!this.trackGain || !this.trackGain.gain)
                return;
            this.setAudioParamValue(this.trackGain.gain, this.trackGainValue, rampSeconds);
        }

        /**
         * Computes the linear ReplayGain value a manifest and the current mode select.
         * @param {Object|null} manifest - The playback manifest.
         * @returns {number} The linear gain, one when nothing is selected.
         */
        computeTrackGain(manifest) {
            const values = resolveReplayGain(manifest, this.replayGainMode);
            return computeReplayGainLinearGain(values.gainDb, values.peak);
        }

        /**
         * Applies a value to an audio parameter, ramping to it so a change never clicks.
         * @param {AudioParam} param - The audio parameter to change.
         * @param {number} value - The value to apply.
         * @param {number} rampSeconds - The duration of the ramp, in seconds. Zero, or a parameter that cannot be ramped, assigns the value directly.
         */
        setAudioParamValue(param, value, rampSeconds) {
            if (!param)
                return;
            const target = Number.isFinite(value) ? value : 0;
            if (rampSeconds > 0
                && typeof param.cancelScheduledValues === 'function'
                && typeof param.setValueAtTime === 'function'
                && typeof param.linearRampToValueAtTime === 'function'
                && this.context) {
                try {
                    const now = this.context.currentTime;
                    const current = Number.isFinite(param.value) ? param.value : target;
                    param.cancelScheduledValues(now);
                    param.setValueAtTime(current, now);
                    param.linearRampToValueAtTime(target, now + rampSeconds);
                    return;
                } catch (error) {
                    // A parameter that rejects automation falls through to the direct assignment below.
                    console.debug('Audio player: a gain ramp could not be scheduled.', error);
                }
            }
            param.value = target;
        }

        /**
         * Converts a decoded AudioData into an AudioBuffer, one channel at a time, handling a sample rate that differs from the context rate.
         * @param {AudioData} audioData - The decoded audio to convert.
         * @returns {AudioBuffer|null} The audio buffer, or null when the decode carries no frame.
         * @throws {LuminaAudioPlayer.PlayerError} When the decode carries nothing copyable.
         */
        createBufferFromAudioData(audioData) {
            if (!audioData || typeof audioData.copyTo !== 'function')
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR, 'The decoded audio could not be read.'));
            const frames = Math.floor(toFiniteNumber(audioData.numberOfFrames) || 0);
            const channels = Math.floor(toFiniteNumber(audioData.numberOfChannels) || 0);
            const sampleRate = toFiniteNumber(audioData.sampleRate);
            if (frames <= 0 || channels <= 0 || sampleRate === null || sampleRate <= 0)
                return null;
            if (!this.isWithinDecodeBudget(frames, channels))
                throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR, 'The decoded audio carries an implausible frame count or channel count.'));
            // A buffer created at the decoded rate is resampled to the context rate by the Web Audio graph itself, which is
            // both cheaper and higher quality than a resample in JavaScript. Only a rate a buffer may not carry is resampled here.
            const canUseDecodedRate = sampleRate >= MIN_AUDIO_BUFFER_SAMPLE_RATE && sampleRate <= MAX_AUDIO_BUFFER_SAMPLE_RATE;
            try {
                if (canUseDecodedRate) {
                    const buffer = this.context.createBuffer(channels, frames, sampleRate);
                    // The decoded samples are copied straight into the channel data of the buffer, so the pass-through path allocates no scratch array and copies the audio only once.
                    for (let channel = 0; channel < channels; channel++)
                        this.copyChannelData(audioData, channel, frames, buffer.getChannelData(channel));
                    return buffer;
                }
                // A rate a buffer may not carry, outside the range createBuffer accepts, is the only case that reaches here,
                // because any accepted rate is used directly above. A conforming decoder never reports such a rate, so this
                // path, and the JavaScript resampler it uses, exist only as a safety net for a non-conforming decoder.
                const targetRate = this.context.sampleRate;
                const resampledFrames = Math.max(1, Math.round(frames / sampleRate * targetRate));
                // The resampled output is a fresh allocation whose size is derived from the decoded rate, so it is held to
                // the same budget as the decoded chunk. The length is computed once here and handed to the resampler, so the
                // budget check and the allocation can never disagree.
                if (!this.isWithinDecodeBudget(resampledFrames, channels))
                    throw this.raise(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR, 'The resampled audio would be implausibly large.'));
                const buffer = this.context.createBuffer(channels, resampledFrames, targetRate);
                for (let channel = 0; channel < channels; channel++) {
                    const source = this.copyChannelData(audioData, channel, frames);
                    buffer.copyToChannel(this.resampleChannel(source, resampledFrames), channel);
                }
                return buffer;
            } catch (error) {
                throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
            }
        }

        /**
         * Checks that a chunk size is within the decode budget, as a product: a buffer allocates all of its samples in
         * one block, so the frame count and the channel count are only meaningful together.
         * @param {number} frames - The number of frames of the chunk.
         * @param {number} channels - The number of channels of the chunk.
         * @returns {boolean} True when the chunk fits the budget.
         */
        isWithinDecodeBudget(frames, channels) {
            return frames <= MAX_DECODED_FRAMES_PER_CHUNK && channels <= MAX_DECODED_CHANNELS && frames * channels <= MAX_DECODED_SAMPLES_PER_CHUNK;
        }

        /**
         * Copies one channel of a decoded AudioData into a Float32Array, as planar 32 bit floats.
         *
         * The copy targets planar float samples, which is the layout the scheduler reads and writes into an AudioBuffer
         * channel. The decode pipeline must therefore supply planar float audio; the module cannot enforce the layout of
         * a decoder, so a source in any other layout is refused rather than reinterpreted, because copying an interleaved
         * source into a single plane would silently write the wrong layout instead of failing.
         * @param {AudioData} audioData - The decoded audio to copy from.
         * @param {number} channel - The zero based channel to copy.
         * @param {number} frames - The number of frames of the decode.
         * @param {Float32Array} [destination] - The array to copy into, which must hold at least the frames of the channel. A new array is allocated when it is omitted, which is what the resample path passes; the pass-through path passes the channel data of the AudioBuffer, so the samples are copied once and no scratch array is allocated.
         * @returns {Float32Array} The destination array, holding the channel samples.
         * @throws {LuminaAudioPlayer.PlayerError} When the channel cannot be copied as planar float samples.
         */
        copyChannelData(audioData, channel, frames, destination) {
            const target = destination || new Float32Array(frames);
            try {
                audioData.copyTo(target, { planeIndex: channel, format: 'f32-planar' });
                return target;
            } catch (error) {
                // A browser that rejects the explicit planar float format is retried without one, which is safe only when
                // the decode already carries planar float samples, so the native format is checked first.
                if (audioData.format !== 'f32-planar')
                    throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                try {
                    audioData.copyTo(target, { planeIndex: channel, frameCount: frames });
                    return target;
                } catch (fallbackError) {
                    throw this.raise(fallbackError, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                }
            }
        }

        /**
         * Resamples one channel of samples with linear interpolation, to an exact output length. It is a safety net for a
         * decoder that reports a sample rate the Web Audio graph cannot itself resample, which a conforming decoder never
         * does, so only a non-conforming one, and a test, reach it.
         * @param {Float32Array} channelData - The samples to resample.
         * @param {number} outputLength - The exact number of frames to produce, which the caller computed and budgeted.
         * @returns {Float32Array} The resampled samples.
         */
        resampleChannel(channelData, outputLength) {
            if (outputLength <= 0 || channelData.length === 0)
                return channelData;
            const ratio = outputLength / channelData.length;
            const output = new Float32Array(outputLength);
            for (let index = 0; index < outputLength; index++) {
                // Map each output sample back to its fractional position in the input. The neighbours are clamped at the
                // end of the source, so the last sample repeats instead of reading past the array or turning into zero.
                const position = index / ratio;
                const lower = Math.floor(position);
                const fraction = position - lower;
                const first = lower < channelData.length ? channelData[lower] : 0;
                const second = lower + 1 < channelData.length ? channelData[lower + 1] : first;
                output[index] = first + (second - first) * fraction;
            }
            return output;
        }

        /**
         * Re-times the schedule for a playback rate change, so the audio stays continuous.
         *
         * The source that is already playing is not recreated: stopping and restarting it would put a gap into the audio
         * that is being heard and churn a node for a change its rate can carry, so its rate is changed in place and its
         * entry is re-based at the current instant. Only the buffers that have not started yet are recreated, laid
         * contiguously after the playing source from their first frame, because the playhead cannot be past a buffer that
         * starts in the future. A rate change therefore re-times the whole pending chain and closes any gap the original
         * start times carried; the scheduler never lays a gap itself, so this is the only place one could appear. The whole
         * mutation is applied only after every new source was created, so a failure leaves the schedule that is already
         * playing exactly as it was and the caller keeps reporting the old rate.
         * @param {number} now - The current context time.
         * @param {number} mediaAtNow - The current media position, in seconds.
         * @param {number} newRate - The playback rate to apply.
         * @returns {number} The context time the audio resumes at, which the caller anchors the playhead to.
         */
        retimeScheduledSources(now, mediaAtNow, newRate) {
            // One linear pass over the live entries splits the schedule into the ones that have finished, the one that is
            // playing, and the ones that have not started yet. The entries are in media order, so there is at most one
            // playing entry, and the entries before the live cursor have already been released.
            const finished = [];
            const notStarted = [];
            let running = null;
            for (let index = this.firstLiveIndex; index < this.scheduledSources.length; index++) {
                const entry = this.scheduledSources[index];
                const end = entry.when + entry.mediaDuration / entry.rate;
                if (end <= now)
                    finished.push(entry);
                else if (entry.when <= now)
                    running = entry;
                else
                    notStarted.push(entry);
            }
            // The playing source is kept, so the chain continues from the media position it has already reached, at the new
            // rate. A chain with nothing playing restarts one scheduling lead ahead, like a fresh chain.
            let wall = now;
            let restartTime = now;
            let runningRemaining = 0;
            if (running) {
                const consumedMedia = clamp(mediaAtNow - running.bufferMediaStart, 0, running.buffer.duration);
                runningRemaining = running.buffer.duration - consumedMedia;
                wall = now + runningRemaining / newRate;
            } else {
                wall = now + MIN_SCHEDULE_LEAD_SECONDS;
                restartTime = wall;
            }
            // The buffers that have not started are rebuilt first, without touching the live schedule, so a failure to
            // create a source leaves everything that is already playing untouched. Each one is laid contiguously after the
            // playing source from its first frame, because the playhead cannot be past a buffer that starts in the future.
            const rebuilt = [];
            for (const entry of notStarted) {
                let source = null;
                try {
                    source = this.context.createBufferSource();
                    source.buffer = entry.buffer;
                    if (source.playbackRate)
                        source.playbackRate.value = newRate;
                    source.onended = () => this.handleSourceEnded();
                    source.connect(this.trackGain);
                    source.start(wall);
                } catch (error) {
                    this.releaseSources(rebuilt);
                    if (source) {
                        try {
                            source.disconnect();
                        } catch (disconnectError) {
                            console.debug('Audio player: the re-timed source could not be disconnected.', disconnectError);
                        }
                    }
                    throw this.raise(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                }
                rebuilt.push({
                    source,
                    buffer: entry.buffer,
                    bufferMediaStart: entry.bufferMediaStart,
                    mediaDuration: entry.buffer.duration,
                    when: wall,
                    rate: newRate
                });
                wall += entry.buffer.duration / newRate;
            }
            // Every new source exists, so the mutation is applied: the playing source takes the new rate in place, the
            // finished and replaced sources are released, and the rebuilt entries replace the schedule.
            if (running) {
                if (running.source.playbackRate)
                    running.source.playbackRate.value = newRate;
                running.when = now;
                running.mediaDuration = runningRemaining;
                running.rate = newRate;
            }
            for (const entry of finished)
                this.releaseSource(entry.source);
            for (const entry of notStarted)
                this.releaseSource(entry.source);
            this.scheduledSources = running ? [running, ...rebuilt] : rebuilt;
            // The rebuilt array holds only live entries, so the cursor starts over at its beginning.
            this.firstLiveIndex = 0;
            // A schedule with nothing left leaves the schedule drained, which getScheduledAheadSeconds reports as zero.
            this.nextStartTime = this.scheduledSources.length > 0 ? wall : 0;
            return restartTime;
        }

        /**
         * Stops and disconnects one scheduled source.
         * @param {AudioBufferSourceNode} source - The source to release.
         * @param {number} [when] - The context time to stop at, or nothing to stop immediately.
         */
        releaseSource(source, when) {
            if (!source)
                return;
            // An intentional stop must not be read as the end of the track, so the end handler is detached before it.
            try {
                source.onended = null;
            } catch (error) {
                console.debug('Audio player: the source end handler could not be detached.', error);
            }
            try {
                if (when === undefined)
                    source.stop();
                else
                    source.stop(when);
            } catch (error) {
                console.debug('Audio player: a scheduled source could not be stopped.', error);
            }
            try {
                source.disconnect();
            } catch (error) {
                console.debug('Audio player: a scheduled source could not be disconnected.', error);
            }
        }

        /**
         * Releases every source of a list.
         * @param {Array<{ source: AudioBufferSourceNode }>} entries - The entries whose sources are released.
         */
        releaseSources(entries) {
            for (const entry of entries)
                this.releaseSource(entry.source);
        }

        /**
         * Stops and disconnects every scheduled source and resets the schedule cursors.
         */
        stopAllSources() {
            // Every source is released through the one helper that stops, detaches its end handler and disconnects it, so
            // the teardown path here and the swap path of a rate change can never drift apart.
            for (let index = this.firstLiveIndex; index < this.scheduledSources.length; index++)
                this.releaseSource(this.scheduledSources[index].source);
            this.scheduledSources = [];
            this.firstLiveIndex = 0;
            this.mediaCursor = 0;
            this.nextStartTime = 0;
            // Nothing is scheduled any more, so the end detection is suspended until a buffer is scheduled again, and the
            // track is no longer considered to have reached its end. A reset starts a new logical track, so the reported
            // end of the previous one is cleared as well.
            this.hasScheduledSinceReset = false;
            this.reachedEnd = false;
            this.endReported = false;
        }

        /**
         * Removes every scheduled source whose audio begins at or after a media position, keeping the audio before it. It is
         * used when a gapless handoff is abandoned after the next track was already appended: the appended audio is stopped
         * and released, the media frontier is moved back to the boundary, and the current track can end normally there
         * instead of being followed by a transition the user disabled.
         *
         * Call it while the boundary is still ahead of getCurrentTime(), after the gain change of the next track was
         * cancelled and before markStreamComplete(). Truncating behind the playhead would move the media cursor backwards
         * and replay audio that already played, so the caller checks getCurrentTime() first and leaves a handoff that has already begun alone.
         * @param {number} mediaPosition - The absolute media position from which scheduled audio is removed.
         * @returns {LuminaAudioPlayer.TrackRenderer} This renderer, for chaining.
         */
        truncateScheduleAt(mediaPosition) {
            const target = toFiniteNumber(mediaPosition);
            if (this.destroyed || target === null)
                return this;
            const kept = [];
            for (let index = 0; index < this.scheduledSources.length; index++) {
                const entry = this.scheduledSources[index];
                if (index >= this.firstLiveIndex && entry.bufferMediaStart >= target)
                    this.releaseSource(entry.source);
                else
                    kept.push(entry);
            }
            this.scheduledSources = kept;
            if (this.firstLiveIndex > kept.length)
                this.firstLiveIndex = kept.length;
            this.mediaCursor = Math.max(0, target);
            const last = kept[kept.length - 1];
            this.nextStartTime = last ? last.when + last.mediaDuration / last.rate : 0;
            if (this.pausedMediaTime > this.mediaCursor)
                this.pausedMediaTime = this.mediaCursor;
            return this;
        }

        /**
         * Releases the scheduled sources whose audio has finished playing. The entries are held in media order, so the
         * finished ones are always a prefix; the cursor of the first live entry is advanced over that prefix instead of the
         * array being shifted, so releasing a finished source never costs the size of the schedule behind it.
         */
        cleanupFinishedSources() {
            const now = this.context ? this.context.currentTime : 0;
            while (this.firstLiveIndex < this.scheduledSources.length) {
                const entry = this.scheduledSources[this.firstLiveIndex];
                if (entry.when + entry.mediaDuration / entry.rate > now)
                    break;
                this.releaseSource(entry.source);
                this.firstLiveIndex++;
            }
            // The dead prefix is dropped entirely once nothing is live, so a track that ended holds no buffer, and otherwise
            // it is reclaimed in bulk only once it is at least as large as what remains, which keeps the array bounded and
            // the reclaim amortized against the entries it discards.
            if (!this.hasLiveEntries() || (this.firstLiveIndex > 64 && this.firstLiveIndex * 2 >= this.scheduledSources.length)) {
                this.scheduledSources = this.scheduledSources.slice(this.firstLiveIndex);
                this.firstLiveIndex = 0;
            }
        }

        /**
         * Moves the playhead anchor to a context time and the media position the audio has at that context time.
         * @param {number} contextTime - The context time of the anchor.
         * @param {number} mediaTime - The media position at the anchor.
         */
        reanchor(contextTime, mediaTime) {
            this.anchorContextTime = contextTime;
            this.anchorMediaTime = mediaTime;
        }

        /**
         * Normalizes a playback rate into the accepted range, keeping the current rate when the value is not a number.
         * @param {number} rate - The playback rate to normalize.
         * @returns {number} The normalized playback rate.
         */
        normalizePlaybackRate(rate) {
            const value = toFiniteNumber(rate);
            if (value === null)
                return this.playbackRate;
            return clamp(value, MIN_PLAYBACK_RATE, MAX_PLAYBACK_RATE);
        }

        /**
         * Starts the reporter that reports the playhead and the end of the scheduled audio while it plays.
         */
        startReporter() {
            if (this.reporterHandle !== null || this.destroyed)
                return;
            // The reporter runs for the whole duration of playback, even without a consumer callback, so the end of the
            // audio is detected and isPlaying turns false whether the caller observes it through a callback or by polling.
            this.scheduleReporterFrame();
        }

        /**
         * Stops the reporter.
         */
        stopReporter() {
            if (this.reporterHandle === null)
                return;
            const handle = this.reporterHandle;
            this.reporterHandle = null;
            try {
                if (typeof cancelAnimationFrame === 'function' && handle.isFrame === true)
                    cancelAnimationFrame(handle.id);
                else
                    clearTimeout(handle.id);
            } catch (error) {
                console.debug('Audio player: the renderer reporter could not be stopped.', error);
            }
        }

        /**
         * Schedules the next reporter tick, on the next frame when the page has an animation frame and on a short timeout
         * otherwise.
         */
        scheduleReporterFrame() {
            const tick = () => {
                this.reporterHandle = null;
                this.notifyTick();
            };
            // An animation frame is the smooth choice while the page is visible, but the browser suspends it while the tab
            // is hidden, even though the audio graph keeps producing sound. A timer, which a hidden tab throttles but does
            // not suspend, drives the report then, so the playhead and the end are still observed in the background.
            const isVisible = typeof document === 'undefined' || document.hidden !== true;
            if (isVisible && typeof requestAnimationFrame === 'function')
                this.reporterHandle = { id: requestAnimationFrame(tick), isFrame: true };
            else
                this.reporterHandle = { id: setTimeout(tick, REPORTER_FALLBACK_INTERVAL_MS), isFrame: false };
        }

        /**
         * Checks whether the track has ended: the scheduled audio has drained and the track is known to be complete. A
         * drained schedule that is not complete is a gap, not the end, however the gap came about, and an empty schedule
         * right after a reset is never an end because nothing has been scheduled at that position yet. Completion is only
         * ever stated by markStreamComplete: the duration of the manifest is not used as a completion signal, because the
         * last frames of a decode that is still on its way would then be cut off.
         * @returns {boolean} True when the track has ended.
         */
        isEndOfTrack() {
            if (!this.streamComplete || !this.hasScheduledSinceReset || this.mediaCursor <= 0)
                return false;
            const now = this.context ? this.context.currentTime : 0;
            return this.nextStartTime <= now;
        }

        /**
         * Reports the playhead, and the end of the scheduled audio, to the consumer callbacks.
         */
        notifyTick() {
            if (!this.playing || this.destroyed)
                return;
            if (this.onTimeUpdate) {
                try {
                    this.onTimeUpdate(this.getCurrentTime(), this.getDuration(), this.getTimelineDuration());
                } catch (error) {
                    console.error('Audio player: the renderer time callback threw.', error);
                }
            }
            const now = this.context ? this.context.currentTime : 0;
            // The finished sources are released as soon as the schedule drains, because no further buffer will arrive to trigger the sweep of schedule().
            if (this.hasScheduledSinceReset && this.mediaCursor > 0 && this.nextStartTime <= now)
                this.cleanupFinishedSources();
            // The track ends only when the drained schedule is also known to be complete; a decode that fell behind keeps playing and waits for the audio that is still on its way.
            if (this.isEndOfTrack()) {
                this.completeEnd();
                return;
            }
            this.scheduleReporterFrame();
        }

        /**
         * Handles the end of a scheduled source. It is the end-of-audio signal that survives a hidden tab, because it does
         * not depend on the animation frame loop, which the browser suspends while the tab is in the background, while the
         * audio graph keeps producing sound.
         */
        handleSourceEnded() {
            if (!this.playing || this.destroyed)
                return;
            if (this.isEndOfTrack())
                this.completeEnd();
        }

        /**
         * Completes the current track: the playhead is pinned to the end of the media, playback stops, and the reporter
         * stops. The end is reported through the onEnded callback once per configured track: scheduling audio after an end
         * resumes the playback state, so completeEnd runs again for that audio, but the callback is not fired a second
         * time, and only a configure or a seek, which start a new logical track, allow it to fire again.
         */
        completeEnd() {
            const now = this.context ? this.context.currentTime : 0;
            // The playhead is pinned to the end of the media, so a report after the track ends shows its end instead of the anchor of the first buffer.
            this.reanchor(now, this.mediaCursor);
            this.pausedMediaTime = this.mediaCursor;
            this.playing = false;
            this.stopReporter();
            // The schedule has drained, so the finished sources are released here too, because the end can also be reached
            // from the end of a source, which does not pass through the reporter tick that would otherwise sweep them.
            this.cleanupFinishedSources();
            this.reachedEnd = true;
            if (this.endReported)
                return;
            this.endReported = true;
            if (!this.onEnded)
                return;
            try {
                this.onEnded();
            } catch (error) {
                console.error('Audio player: the renderer end callback threw.', error);
            }
        }

        /**
         * Subscribes the renderer to the player event bus, so a ReplayGain mode change made on the player is applied to
         * the per-track gain without the caller pushing it again. The volume and the mute state are deliberately not read
         * here: they are driven by the caller through setVolumeAsync and setMutedAsync, which is exactly what the engine
         * interface hands over, so the master gain has a single writer.
         */
        subscribeToEvents() {
            if (!this.events || !LuminaAudioPlayer.PlayerEvent || typeof this.events.on !== 'function')
                return;
            if (LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED) {
                this.unsubscribers.push(this.events.on(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, (preferences) => {
                    if (preferences && typeof preferences.replayGainMode === 'string')
                        this.setReplayGainMode(preferences.replayGainMode);
                }));
            }
        }

        /**
         * Records and types a failure, so a caller or a poll of getError sees a consistent error.
         * @param {*} error - The failure to record.
         * @param {string} [code] - The code to use when the failure is not already a player error.
         * @returns {LuminaAudioPlayer.PlayerError} The recorded typed error.
         */
        raise(error, code = LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR) {
            const typed = LuminaAudioPlayer.PlayerError.from(error, code);
            this.error = typed;
            return typed;
        }
    }

    LuminaAudioPlayer.TrackRenderer = TrackRenderer;

    /**
     * Creates a track renderer. See {@link LuminaAudioPlayer.TrackRenderer} for the options.
     * @param {Object} options - The renderer options.
     * @returns {LuminaAudioPlayer.TrackRenderer} The created renderer.
     */
    LuminaAudioPlayer.createTrackRenderer = function (options) {
        return new TrackRenderer(options);
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
