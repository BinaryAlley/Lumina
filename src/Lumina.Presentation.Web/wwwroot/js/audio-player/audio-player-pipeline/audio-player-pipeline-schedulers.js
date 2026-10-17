/**
 * Schedulers of the audio player pipeline.
 *
 * The seekable scheduler trims the first decoded frame after a seek to the requested position before it reaches the
 * renderer. The buffering scheduler holds the decoded audio of a gapless prefetch until the handoff appends it to the
 * shared schedule. They are the only pipeline stages that know how decoded AudioData is shaped and laid on the timeline.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The duration, in seconds, of the next track that is pre-decoded for a gapless transition.
     * @readonly
     * @type {number}
     */
    const GAPLESS_PRELOAD_SECONDS = 2;

    /**
     * The scheduled ahead ceiling, in seconds, of the scheduler that receives the pre-decoded audio of the next track. It
     * bounds how much decoded audio is retained before the handoff, and it is lifted when the pre-decoded audio is appended to the renderer.
     * @readonly
     * @type {number}
     */
    const GAPLESS_BUFFER_CEILING_SECONDS = GAPLESS_PRELOAD_SECONDS + 1;

    /**
     * The tolerance, in seconds, within which the first frame after a seek counts as beginning at the seek target, so that a frame is not trimmed for a rounding error.
     * @readonly
     * @type {number}
     */
    const SEEK_TRIM_EPSILON_SECONDS = 0.001;

    /**
     * Coerces a value to a finite number, or null when it is absent or not finite.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The finite number, or null.
     */
    function toFiniteNumber(value) {
        return LuminaAudioPlayer.toFiniteNumber(value);
    }

    /**
     * Reads the media duration of a decoded frame, in seconds.
     * @param {AudioData} audioData - The decoded frame.
     * @returns {number} The duration, in seconds, or zero when it cannot be read.
     */
    function getAudioDataDurationSeconds(audioData) {
        const frames = toFiniteNumber(audioData && audioData.numberOfFrames);
        const sampleRate = toFiniteNumber(audioData && audioData.sampleRate);
        if (frames === null || sampleRate === null || frames <= 0 || sampleRate <= 0)
            return 0;
        return frames / sampleRate;
    }

    /**
     * Releases the native memory of a decoded frame. It is safe to call on a frame that was already closed.
     * @param {AudioData} audioData - The frame to close.
     */
    function closeAudioData(audioData) {
        if (!audioData || typeof audioData.close !== 'function')
            return;
        try {
            audioData.close();
        } catch (error) {
            console.debug('Audio player: a decoded frame could not be released.', error);
        }
    }

    /**
     * Trims the leading frames of a decoded frame, so that the audio it carries starts exactly at a target position.
     *
     * The decoded frame is a planar 32 bit float frame, because that is what the decode pipeline copies and what the
     * renderer consumes. The trimmed frame is a new AudioData whose timestamp is the target position, so the renderer
     * adopts the requested seek position as the origin of the audio. The original frame is closed by this function when a trimmed frame is produced.
     * @param {AudioData} audioData - The decoded frame to trim.
     * @param {number} offsetSeconds - The number of seconds to remove from the start of the frame.
     * @returns {AudioData|null} The trimmed frame, or null when the frame cannot be rebuilt.
     */
    function trimAudioData(audioData, offsetSeconds) {
        // Read the WebCodecs constructor at call time, so a host without AudioData (or a test) can fall back to scheduling
        // the untrimmed frame instead of failing at module load.
        const AudioDataConstructor = typeof window !== 'undefined' && typeof window.AudioData === 'function'
            ? window.AudioData
            : null;
        if (!AudioDataConstructor || !audioData || typeof audioData.copyTo !== 'function')
            return null;
        // AudioData exposes readonly typed properties. Read them through the shared helper so a missing or non numeric
        // value rejects the trim instead of producing a frame with NaN dimensions.
        const frames = toFiniteNumber(audioData.numberOfFrames);
        const channels = toFiniteNumber(audioData.numberOfChannels);
        const sampleRate = toFiniteNumber(audioData.sampleRate);
        const timestamp = toFiniteNumber(audioData.timestamp);
        if (frames === null || channels === null || sampleRate === null || timestamp === null)
            return null;
        if (frames <= 0 || channels <= 0 || sampleRate <= 0)
            return null;
        // Round to the nearest sample. The offset is a time difference, so flooring it could leave a fraction of a sample
        // of audio from before the seek target audible.
        const framesToTrim = Math.round(offsetSeconds * sampleRate);
        // AudioData can only hold whole frames. Drop any partial trailing sample from the source count.
        const keptFrames = Math.floor(frames) - framesToTrim;
        // Nothing to remove, or nothing left to play. The caller keeps the original frame, which is the safer fallback for
        // a frame that starts at the target or ends exactly at it.
        if (framesToTrim <= 0 || keptFrames <= 0)
            return null;
        // One planar block holds every channel back to back. The subarray per channel is exactly the layout AudioData
        // expects for f32-planar, and it avoids one allocation per channel.
        const samples = new Float32Array(keptFrames * Math.floor(channels));
        try {
            for (let channel = 0; channel < channels; channel++) {
                const destination = samples.subarray(channel * keptFrames, (channel + 1) * keptFrames);
                // Skip the pre-target frames and copy only the kept tail. copyTo is the only way to read the native
                // AudioData memory without holding on to the original frame.
                audioData.copyTo(destination, {
                    planeIndex: channel,
                    frameOffset: framesToTrim,
                    frameCount: keptFrames,
                    format: 'f32-planar'
                });
            }
            // The trimmed frame starts at the seek target, and WebCodecs timestamps are integer microseconds. Keeping the
            // original timestamp plus the removed audio keeps the frame on the same absolute timeline.
            const trimmed = new AudioDataConstructor({
                format: 'f32-planar',
                sampleRate: sampleRate,
                numberOfFrames: keptFrames,
                numberOfChannels: Math.floor(channels),
                timestamp: Math.round(timestamp + offsetSeconds * 1000000),
                data: samples
            });
            // The original is released only after the replacement exists. If the constructor above throws, ownership stays
            // with the caller, which then schedules the original frame.
            closeAudioData(audioData);
            return trimmed;
        } catch (error) {
            // Returning null rather than throwing lets the scheduler play the original frame. A seek may then be a packet
            // early, but a browser that refuses to construct an AudioData never turns the seek into silence.
            console.debug('Audio player: the first decoded frame after a seek could not be trimmed.', error);
            return null;
        }
    }

    /**
     * A scheduler proxy over the renderer that trims the first decoded frame after a seek to the requested position.
     *
     * A demuxer repositions to a packet boundary at or before the seek target, so the first decoded frame may carry audio
     * from before the target. The renderer adopts the requested position as the origin of the first buffer it receives and
     * cannot correct an offset it is not told about, so the offset is removed here, before the frame reaches the renderer.
     */
    class SeekableScheduler {
        /**
         * Creates an instance of SeekableScheduler.
         * @param {Object} target - The scheduler the frames are handed to, normally the renderer.
         * @param {Function} [onScheduled] - Called once, when the first frame is handed to the target after a seek or a load.
         */
        constructor(target, onScheduled) {
            this.target = target;
            this.onScheduled = typeof onScheduled === 'function' ? onScheduled : null;
            this.hasScheduled = false;
            // The absolute media position the first frame after a seek must start at, or null when no trim is pending.
            this.trimTargetSeconds = null;
            // A scheduler of a released item drops every frame that is still emitted, so a decoder that was destroyed with
            // frames in flight can never schedule audio for a track that is no longer playing.
            this.stopped = false;
            // While a seek owns the timeline, frames that the old decode loop still emits are dropped instead of being laid
            // at the new position before the decoder reset takes effect.
            this.suppressed = false;
        }

        /**
         * Sets whether frames are dropped instead of scheduled, which a seek uses until every stage is repositioned.
         * @param {boolean} value - True to drop frames, false to schedule them again.
         */
        setSuppressed(value) {
            this.suppressed = value === true;
        }

        /**
         * Marks the position the next decoded frame must begin at, removing any audio that a demuxer landed before it.
         * @param {number} seconds - The absolute media position of the seek target, in seconds.
         */
        setSeekTarget(seconds) {
            const value = toFiniteNumber(seconds);
            this.trimTargetSeconds = value === null ? null : Math.max(0, value);
        }

        /**
         * Re-arms the first frame notification, so the state of a fresh seek or a fresh track is observed again.
         */
        rearm() {
            this.hasScheduled = false;
        }

        /**
         * Stops the scheduler, so every later frame is dropped instead of being handed to the target. It is used when the item that owns the scheduler is released or fails.
         */
        stop() {
            this.stopped = true;
        }

        /**
         * Hands a decoded frame to the target, trimming it when it carries audio from before the pending seek target.
         * @param {AudioData} audioData - The decoded frame. Ownership transfers to this scheduler, which always closes it.
         * @returns {number} The media position at the end of the frame, or the target position when the frame was dropped.
         */
        schedule(audioData) {
            if (this.stopped || this.suppressed) {
                closeAudioData(audioData);
                return 0;
            }
            let frame = audioData;
            const target = this.trimTargetSeconds;
            if (target !== null) {
                const frameStart = toFiniteNumber(frame.timestamp);
                const frameDuration = getAudioDataDurationSeconds(frame);
                if (frameStart !== null && frameDuration > 0) {
                    // WebCodecs reports the frame timestamp in microseconds, while the scheduler and the renderer work in
                    // seconds.
                    const secondsStart = frameStart / 1000000;
                    const secondsEnd = secondsStart + frameDuration;
                    if (secondsEnd <= target + SEEK_TRIM_EPSILON_SECONDS) {
                        // The whole frame is before the target, so it is dropped instead of played early.
                        closeAudioData(frame);
                        return target;
                    }
                    if (secondsStart < target - SEEK_TRIM_EPSILON_SECONDS) {
                        const trimmed = trimAudioData(frame, target - secondsStart);
                        if (trimmed) {
                            frame = trimmed;
                        } else {
                            // A frame that cannot be rebuilt is scheduled from its own start rather than dropped, so a
                            // browser that refuses to construct an AudioData never turns a seek into a gap. The offset is at most one packet long.
                            this.trimTargetSeconds = null;
                        }
                    }
                }
                // Only the first frame after a seek can straddle the target. Later frames already start after it, so a
                // pending trim must not leak into the next seek.
                this.trimTargetSeconds = null;
            }
            if (!this.hasScheduled) {
                this.hasScheduled = true;
                if (this.onScheduled)
                    this.onScheduled();
            }
            return this.target.schedule(frame);
        }

        /**
         * Attaches the scheduler to the target it was buffering for, which is how a prefetch becomes the audio that plays.
         * @param {Object} target - The scheduler the frames are handed to from now on.
         */
        attach(target) {
            const previous = this.target;
            this.target = target;
            // A buffering target flushes the audio it pre-decoded into the new target, which is the renderer. A renderer target has no attach method and is simply replaced.
            if (previous && typeof previous.attach === 'function')
                previous.attach(target);
        }

        /**
         * Gets the scheduled ahead duration the current target reports.
         * @returns {number} The scheduled ahead duration, in seconds.
         */
        getScheduledAheadSeconds() {
            return typeof this.target.getScheduledAheadSeconds === 'function' ? this.target.getScheduledAheadSeconds() : 0;
        }

        /**
         * Gets the scheduled ahead ceiling the current target publishes, or null when it publishes none.
         * @returns {number|null} The ceiling, in seconds.
         */
        getMaxScheduledAheadSeconds() {
            return typeof this.target.getMaxScheduledAheadSeconds === 'function' ? this.target.getMaxScheduledAheadSeconds() : null;
        }

        /**
         * Gets the playback rate of the current target, which is one when the target does not report it.
         * @returns {number} The playback rate.
         */
        getPlaybackRate() {
            return typeof this.target.getPlaybackRate === 'function' ? this.target.getPlaybackRate() : 1;
        }

        /**
         * Releases the audio a buffering target still holds. A renderer target is left untouched, because it is shared.
         */
        dispose() {
            if (this.target && typeof this.target.close === 'function')
                this.target.close();
        }
    }

    /**
     * A scheduler that holds the decoded audio of a prefetched track until the current track hands over to it.
     *
     * While it is detached it reports the duration it holds as its scheduled ahead duration, so the decoder paces itself
     * against the buffer instead of decoding the whole next track, and it publishes a ceiling that bounds that buffer.
     * Once attached, it flushes the buffered frames into the target in order and delegates every later frame to it.
     */
    class BufferingScheduler {
        /**
         * Creates an instance of BufferingScheduler.
         */
        constructor() {
            this.target = null;
            this.buffer = [];
            this.bufferedSeconds = 0;
        }

        /**
         * Stores a decoded frame until the scheduler is attached, or hands it to the target once it is.
         * @param {AudioData} audioData - The decoded frame. Ownership transfers to this scheduler.
         * @returns {number} The buffered duration in seconds while detached, or the target position after attaching.
         */
        schedule(audioData) {
            if (this.target)
                return this.target.schedule(audioData);
            const duration = getAudioDataDurationSeconds(audioData);
            this.buffer.push(audioData);
            this.bufferedSeconds += duration;
            return this.bufferedSeconds;
        }

        /**
         * Attaches the scheduler to the target it buffered for: the buffered frames are appended to the target in order,
         * so the next track starts exactly at the end of the audio of the current one.
         * @param {Object} target - The scheduler to hand the buffered and later frames to.
         */
        attach(target) {
            this.target = target;
            const pending = this.buffer;
            let index = 0;
            try {
                for (; index < pending.length; index++)
                    target.schedule(pending[index]);
            } catch (error) {
                // The target owns and closes the frame it refused; every frame after it is still buffered and must be
                // released here, or up to the whole pre-decode window is retained as native audio until garbage collection.
                console.debug('Audio player: a pre-decoded frame could not be scheduled.', error);
                for (let remaining = index + 1; remaining < pending.length; remaining++)
                    closeAudioData(pending[remaining]);
            } finally {
                this.buffer = [];
                this.bufferedSeconds = 0;
            }
        }

        /**
         * Gets the buffered duration while detached, or the scheduled ahead duration of the target once attached.
         * @returns {number} The scheduled ahead duration, in seconds.
         */
        getScheduledAheadSeconds() {
            return this.target ? this.target.getScheduledAheadSeconds() : this.bufferedSeconds;
        }

        /**
         * Gets the ceiling the decoder paces itself against. While detached it bounds the prefetch buffer, and once attached it is the ceiling of the target.
         * @returns {number|null} The ceiling, in seconds.
         */
        getMaxScheduledAheadSeconds() {
            if (this.target)
                return typeof this.target.getMaxScheduledAheadSeconds === 'function' ? this.target.getMaxScheduledAheadSeconds() : null;
            return GAPLESS_BUFFER_CEILING_SECONDS;
        }

        /**
         * Gets the playback rate of the target, or one while detached.
         * @returns {number} The playback rate.
         */
        getPlaybackRate() {
            return this.target && typeof this.target.getPlaybackRate === 'function' ? this.target.getPlaybackRate() : 1;
        }

        /**
         * Releases every frame that was buffered and never scheduled.
         */
        close() {
            for (const audioData of this.buffer)
                closeAudioData(audioData);
            this.buffer = [];
            this.bufferedSeconds = 0;
        }
    }

    LuminaAudioPlayer.GAPLESS_PRELOAD_SECONDS = GAPLESS_PRELOAD_SECONDS;
    LuminaAudioPlayer.SEEK_TRIM_EPSILON_SECONDS = SEEK_TRIM_EPSILON_SECONDS;
    LuminaAudioPlayer.SeekableScheduler = SeekableScheduler;
    LuminaAudioPlayer.BufferingScheduler = BufferingScheduler;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
