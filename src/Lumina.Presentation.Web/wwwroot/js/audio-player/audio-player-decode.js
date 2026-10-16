/**
 * WebCodecs decode pipeline of the audio player.
 *
 * The player never uses a native audio element, so the encoded audio of a track is turned into sound in three steps: the
 * download engine brings the bytes in, the demuxers parse them into encoded packets, and this module decodes those packets
 * with WebCodecs into decoded audio, which the scheduler of the render module turns into sound. This module is the middle
 * step: it owns one WebCodecs.AudioDecoder, feeds it the packets a demuxer produces, and hands every decoded frame to the
 * renderer's scheduler.
 *
 * WebCodecs, and why a decoder is not a file reader
 * -------------------------------------------------
 * AudioDecoder does not read a file and does not know about containers. It consumes two things: a configuration, which
 * names the codec, the sample rate and the channel count, plus the codec private data the container carries, and a stream
 * of EncodedAudioChunk objects, each one a single encoded packet with its presentation timestamp and its duration, both
 * in microseconds. Its output is a stream of AudioData objects. Nothing about a container, a byte offset or a file lives
 * here: the demuxer (audio-player-demux/) has already done that work, and this module deliberately knows nothing about it,
 * so one decoder serves every supported container.
 *
 * The demuxer contract
 * --------------------
 * The demuxers of the player (audio-player-demux/) expose, per format, the configuration and the packets this module consumes:
 *   - a configuration, the shape of options.config below: { codec, sampleRate, numberOfChannels, description? }, where
 *     the codec is a WebCodecs codec string ('mp3', 'flac', 'opus', 'mp4a.40.2', 'vorbis', 'pcm-s16', ...) and the
 *     description is the codec private data the container carries (the AudioSpecificConfig of an ADTS stream, the
 *     identification header of an Ogg Opus or Ogg Vorbis stream, the EXTRADATA of an MP4, and so on), when the codec needs one;
 *   - a stream of encoded packets, each one { data, timestamp, duration, type? }, where data is the encoded bytes, the
 *     timestamp and the duration are integers (or numbers) of microseconds, and the type is 'key' or 'delta', defaulting
 *     to 'key', which is the type of every audio packet of the formats the player supports directly.
 * A demuxer that already builds WebCodecs objects may hand over an EncodedAudioChunk directly instead of a plain packet,
 * in which case it is fed as it is. Keeping the conversion here, rather than in each demuxer, is what makes the
 * microsecond timestamps and the packet type uniform across the formats.
 *
 * The scheduled audio contract
 * ----------------------------
 * The decoded AudioData does not go to a queue of this module: the output callback hands each frame to a scheduler, by
 * default the TrackRenderer of audio-player-render.js, through scheduler.schedule(audioData). The scheduler takes
 * ownership of every frame and closes it, releasing its native memory, so this module hands each frame over exactly once
 * and never touches it again. The scheduler must accept decoded audio as fast as it is produced and lay it back to back,
 * and it must expose getScheduledAheadSeconds, which is what keeps a decoder faster than real time from decoding, and
 * retaining, a whole track: a scheduler that cannot report it is refused at construction rather than decoded against
 * without a bound. When the scheduler also publishes its own scheduled ahead ceiling through getMaxScheduledAheadSeconds,
 * the decoder stays a margin below it automatically, so the scheduler does not refuse frames and no frame is lost.
 *
 * Backpressure
 * ------------
 * A decoder that is fed as fast as the demuxer produces would queue a large part of an encoded track and, through its
 * output callback, a large part of a decoded one. Two ceilings bound that:
 *   - the decode queue size, decoder.decodeQueueSize, capped by maxQueueSize, which bounds how many encoded packets are
 *     in flight inside the decoder at once;
 *   - the scheduled ahead duration of the scheduler, capped by the decoder's own maxDecodeAheadSeconds and by the
 *     scheduler's published scheduled ahead ceiling, whichever is smaller, which bounds how much decoded audio is
 *     retained by the scheduler ahead of the playhead. It matters most while playback is paused or has not started, when
 *     the audio clock is frozen and the schedule would otherwise grow without bound.
 * A caller paces itself with canAccept, or simply awaits decodeAsync, which waits for room itself. The wait is released
 * when the decoder dequeues a chunk (the dequeue event) or, when only the scheduled ahead duration is holding it back,
 * when a poll notices that the playhead has consumed enough of the schedule.
 *
 * The decoder stays a margin below the scheduler's published scheduled ahead ceiling, so the scheduler does not normally
 * refuse a frame. It still can, when the scheduler does not publish its ceiling, or when a frame is longer than that
 * margin. Such a refusal is backpressure, not corruption, so while the ceiling can still be satisfied it is not recorded
 * as a failure: the decoder tightens the schedule ahead it feeds to, down to the point of the refusal minus the wall time
 * of the refused frame, and resumes once the playhead has drained below it, which settles at the scheduler's ceiling
 * instead of refusing one frame per decoded frame. A sustained mismatch is a configuration slip, so it is logged once per
 * stream. A refusal is not free, however: the scheduler closes the refused frame before it throws, so that frame's audio is
 * lost and the backoff can only keep the following frames from being refused too. The one refusal that is recorded as a
 * failure is a refusal of an empty schedule, which can only come from a ceiling smaller than one frame plus the scheduling
 * lead and so can never be satisfied.
 *
 * Seeking
 * -------
 * A seek is two halves. The scheduler half is renderer.seekTo(seconds), which moves the playhead. The decoder half is
 * resetAsync: it resets the decoder, dropping the chunks that are queued and the frames that have not been emitted, and
 * reconfigures it with the configuration it was last given, because reset returns a decoder to the unconfigured state. A
 * decode that was waiting for room when the seek arrived belongs to the stream before the seek, so it is released as a
 * clean stop rather than resumed to submit a packet the new playhead must not play. The caller then fetches and demuxes the
 * byte range of the target position and feeds the packets from there. Two separate things must line up, and they are not
 * the same thing: the first decoded samples must begin exactly at the target, which the caller gets by trimming the
 * demuxer's leading samples to that position, because the scheduler places the first buffer it receives after a seekTo at
 * the exact target it was given and ignores that buffer's timestamp, so it cannot correct a leading offset it is not told
 * about; and the chunk timestamps the decoder itself is fed must stay monotonic across the seek, which is the decoder's own
 * ordering concern and does not re-base the scheduler's timeline.
 *
 * Failures
 * --------
 * The decoder is a sub component, like the download engine: it does not throw strings and does not move the player state
 * machine. A failure it cannot recover from, a decoder error, a chunk the decoder refuses, a configuration it rejects, or
 * a scheduler failure other than a scheduled ahead refusal it can recover from, is recorded on the decoder for polling
 * through getError, is reported to the onError callback as a {@link LuminaAudioPlayer.PlayerError}, and stops the decoder
 * cleanly: no chunk is accepted, the queue is dropped and the native decoder is closed. The recorded error carries
 * {@link LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED} when the failure is the decoder's own, and the code of the
 * error when another component already typed it with {@link LuminaAudioPlayer.PlayerError} (for example the renderer's
 * ENGINE_ERROR), because that code is more specific than the decoder's. The recorded error is always owned by this
 * decoder: a PlayerError raised by another component is copied rather than adopted, so its details are never mutated and
 * getError never returns an object the raising component still holds. An engine must forward the callback to
 * <c>host.reportError(error)</c>, exactly as the download engine's callback is forwarded, so the failure reaches the
 * player and the event bus.
 *
 * The pipeline calls a caller drives, configure, decode, decodeAsync, flushAsync and resetAsync, also throw the typed
 * error, because the caller that triggered the failure is the one that can react to it; the other calls never throw. A
 * teardown is never a failure: an abort, a reset or a destroy that interrupts a pending flush is observed as a clean stop,
 * not as a decode failure.
 *
 * Using it
 * --------
 *   const decoder = LuminaAudioPlayer.createTrackDecoder({
 *       scheduler: renderer,
 *       maxQueueSize: 8,
 *       maxDecodeAheadSeconds: 5,
 *       onError: (error) => host.reportError(error)
 *   });
 *   decoder.configure(demuxer.getConfiguration());   // codec, sampleRate, numberOfChannels, description
 *   renderer.configure(manifest);
 *   await renderer.playAsync();                      // from a user gesture
 *   while (true) {
 *       const packet = await demuxer.readPacketAsync();
 *       if (!packet)
 *           break;
 *       if (!await decoder.decodeAsync(packet))      // waits for room, false when the decoder stopped
 *           break;
 *   }
 *   await decoder.flushAsync();                      // all frames emitted
 *   renderer.markStreamComplete();                   // the scheduler may now detect the end of the track
 *   await decoder.resetAsync();                      // the decoder half of a seek
 *   await decoder.destroyAsync();
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The number of encoded chunks the decoder may hold before a caller is asked to wait. It bounds the encoded audio in
     * flight inside the decoder and, indirectly, how far ahead of the playhead the decode runs.
     * @readonly
     * @type {number}
     */
    const DEFAULT_MAX_QUEUE_SIZE = 8;

    /**
     * The default ceiling, in seconds, on how far ahead of the audio clock the scheduler may run before the decoder is
     * asked to wait. It bounds the decoded audio retained by the scheduler, which matters while playback is paused or has
     * not started; it is deliberately larger than the queue bound so that a short decode hiccup never starves playback.
     * @readonly
     * @type {number}
     */
    const DEFAULT_MAX_SCHEDULED_AHEAD_SECONDS = 5;

    /**
     * The interval, in milliseconds, at which a wait for room is re-checked when only the scheduled ahead duration is
     * holding it back. The scheduler reports no event when the playhead consumes its schedule, so a poll is the only way
     * to notice it; the interval is short enough to be inaudible against the audio already scheduled ahead.
     * @readonly
     * @type {number}
     */
    const CAPACITY_POLL_INTERVAL_MS = 200;

    /**
     * The lowest value the scheduled ahead gate may take, in seconds. A refusal proves only that the scheduler's ceiling is
     * smaller than the schedule ahead plus the refused frame, not that it is smaller than a single frame, so a gate of
     * zero or below would be a false failure and would also block every future chunk. The gate is therefore floored here:
     * any ceiling that can accept one frame is above this floor, so a satisfiable ceiling recovers while a genuinely
     * unsatisfiable one is detected when it refuses an empty schedule.
     * @readonly
     * @type {number}
     */
    const MINIMUM_SCHEDULE_BACKOFF_SECONDS = 0.01;

    /**
     * The margin, in seconds, the decoder keeps between its own scheduled ahead ceiling and the scheduler's published one,
     * so that a frame it feeds fits under the scheduler's ceiling instead of being refused and lost. It is approximately
     * one frame's wall time: a frame is refused once the schedule ahead plus its wall length would exceed the scheduler's
     * ceiling, so feeding up to the scheduler's ceiling minus this margin leaves room for a frame of that length.
     * @readonly
     * @type {number}
     */
    const SCHEDULER_CEILING_MARGIN_SECONDS = 0.1;

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
     * Coerces a value to a positive integer, or null when it is absent, not finite or not positive.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The positive integer, or null.
     */
    function toPositiveInteger(value) {
        const number = toFiniteNumber(value);
        if (number === null || number <= 0)
            return null;
        const integer = Math.round(number);
        return integer > 0 ? integer : null;
    }

    /**
     * Accepts the buffer source collections WebCodecs reads a codec description or an encoded packet from, and rejects
     * everything else.
     * @param {*} value - The value to check.
     * @returns {ArrayBuffer|ArrayBufferView|null} The value when it is a buffer source, null otherwise.
     */
    function toBufferSource(value) {
        if (value === null || value === undefined)
            return null;
        if (typeof ArrayBuffer === 'undefined')
            return null;
        if (value instanceof ArrayBuffer)
            return value;
        if (ArrayBuffer.isView(value))
            return value;
        return null;
    }

    /**
     * Checks whether a value is the AbortError a reset or a close rejects a pending flush with. An AbortError is tested
     * by name rather than by type, because in a browser it is a DOMException, which is not an Error.
     * @param {*} error - The value to check.
     * @returns {boolean} True when the value is an AbortError.
     */
    function isAbortError(error) {
        return !!error && error.name === 'AbortError';
    }

    /**
     * Checks whether a scheduler refused a frame because its own scheduled ahead ceiling is tighter than the decoder's.
     * That is backpressure, not corruption: the scheduler refused the frame before scheduling it, so the pipeline can keep
     * running, it must only wait for the schedule to drain. The classification rests on two properties the scheduler's
     * schedule contract must keep, both documented on the renderer's schedule: the refusal is thrown without being recorded
     * as the scheduler's own last error, and the refused frame is closed before the throw.
     * @param {*} error - The value the scheduler threw.
     * @returns {boolean} True when the refusal is a scheduled ahead limit.
     */
    function isScheduledAheadRefusal(error) {
        return !!error && !!error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SCHEDULED_AHEAD_LIMIT;
    }

    /**
     * Reads the media duration of a decoded frame, in seconds. It must be read before the frame is handed to the
     * scheduler, because the scheduler closes the frame before it returns or throws, and a closed frame no longer reports
     * its duration. It is the media the frame would add to the schedule ahead, so it is the margin a scheduled ahead
     * refusal is honored with.
     * @param {AudioData} audioData - The decoded frame.
     * @returns {number} The duration, in seconds, or zero when it cannot be read.
     */
    function getAudioDataDurationSeconds(audioData) {
        if (!audioData)
            return 0;
        const duration = toFiniteNumber(audioData.duration);
        // WebCodecs reports the duration of a decoded frame in microseconds.
        return duration === null ? 0 : Math.max(0, duration) / 1000000;
    }

    /**
     * Merges two error detail objects into a new one, so that a details object a component owns is never mutated in place.
     * @param {Object|null} base - The existing details.
     * @param {Object|null} extra - The details to add.
     * @returns {Object|null} The merged details, or null when both are absent.
     */
    function mergeErrorDetails(base, extra) {
        const hasBase = base !== null && base !== undefined;
        const hasExtra = extra !== null && extra !== undefined;
        if (!hasBase && !hasExtra)
            return null;
        if (!hasExtra)
            return Object.assign({}, base);
        if (!hasBase)
            return Object.assign({}, extra);
        return Object.assign({}, base, extra);
    }

    /**
     * Builds an error this decoder owns. A PlayerError raised by another component is copied rather than adopted, so its
     * details are never mutated and the decoder failure never aliases the error the raising component keeps for itself.
     * @param {*} error - The failure to own.
     * @param {string} fallbackCode - The code to use when the failure is not already a player error.
     * @param {Object|null} details - Optional details to merge in.
     * @returns {LuminaAudioPlayer.PlayerError} The owned typed error.
     */
    function toOwnedError(error, fallbackCode, details) {
        if (error instanceof LuminaAudioPlayer.PlayerError)
            return new LuminaAudioPlayer.PlayerError(error.code, error.message, mergeErrorDetails(error.details, details));
        const typed = LuminaAudioPlayer.PlayerError.from(error, fallbackCode);
        if (!details)
            return typed;
        return new LuminaAudioPlayer.PlayerError(typed.code, typed.message, mergeErrorDetails(typed.details, details));
    }

    /**
     * Creates the WebCodecs audio decoder of the player through the browser constructor.
     * @param {{ output: Function, error: Function }} handlers - The output and error callbacks the decoder is created with.
     * @returns {AudioDecoder|null} The created decoder, or null when the browser exposes no constructor.
     */
    function createDefaultAudioDecoder(handlers) {
        const targetWindow = typeof window !== 'undefined' ? window : null;
        if (!targetWindow || typeof targetWindow.AudioDecoder !== 'function')
            return null;
        return new targetWindow.AudioDecoder({ output: handlers.output, error: handlers.error });
    }

    /**
     * Gets the WebCodecs EncodedAudioChunk constructor, or null when the browser exposes none.
     * @returns {Function|null} The constructor, or null.
     */
    function getEncodedAudioChunkConstructor() {
        const targetWindow = typeof window !== 'undefined' ? window : null;
        return targetWindow && typeof targetWindow.EncodedAudioChunk === 'function' ? targetWindow.EncodedAudioChunk : null;
    }

    /**
     * Decodes the encoded audio of one track with WebCodecs, feeding the frames it produces to a scheduler.
     */
    class TrackDecoder {
        /**
         * Creates an instance of TrackDecoder.
         * @param {Object} options - The decoder options.
         * @param {Object} options.scheduler - The decoded audio sink, by default the TrackRenderer of audio-player-render.js. It must expose schedule(audioData), which takes ownership of the frame and closes it, and getScheduledAheadSeconds(), which the decoder uses to bound the decode-ahead. When it also publishes its own scheduled ahead ceiling through getMaxScheduledAheadSeconds(), the decoder stays a margin below it automatically, so the scheduler never refuses a frame.
         * @param {Function} [options.decoderFactory] - A factory that receives { output, error } and returns an AudioDecoder. Defaults to the browser AudioDecoder constructor, and exists for injection in a test.
         * @param {Object} [options.config] - The initial configuration, of the shape configure accepts; it may also be given to configure later.
         * @param {number} [options.maxQueueSize] - The number of encoded chunks the decoder may hold before a caller is asked to wait. Defaults to eight.
         * @param {number|null} [options.maxDecodeAheadSeconds] - The decoder's own ceiling, in seconds, on how far ahead of the audio clock it decodes and feeds, so that a fast decoder does not retain a whole decoded track. It is distinct from the scheduler's own ceiling, whose option has a different name and the opposite convention, absent meaning disabled there and a default here. Defaults to five seconds; pass null to disable the decoder's own ceiling, which leaves only the scheduler's, if any. An absent key, an explicit undefined, and a value of zero or below all keep the default, the last because a ceiling of zero could never be satisfied. When the scheduler publishes its own ceiling, the decoder uses the smaller of the two.
         * @param {Function} [options.onError] - Called with a {@link LuminaAudioPlayer.PlayerError} when the decoder fails. An engine must forward it to host.reportError.
         * @throws {TypeError} When no scheduler with a schedule method and a getScheduledAheadSeconds method is provided.
         * @throws {LuminaAudioPlayer.PlayerError} When an initial options.config is given and it is invalid or the browser rejects it.
         */
        constructor(options = {}) {
            const settings = options || {};
            this.scheduler = settings.scheduler || null;
            if (!this.scheduler || typeof this.scheduler.schedule !== 'function')
                throw new TypeError('The audio decoder requires a scheduler with a schedule method.');
            // The decode-ahead bound is the only thing that keeps a fast decoder from retaining a whole decoded track, so a
            // scheduler that cannot report its scheduled ahead duration is refused instead of silently decoding without a
            // bound beyond the small encoded queue.
            if (typeof this.scheduler.getScheduledAheadSeconds !== 'function')
                throw new TypeError('The audio decoder requires a scheduler that reports its scheduled ahead duration.');
            this.decoderFactory = typeof settings.decoderFactory === 'function' ? settings.decoderFactory : createDefaultAudioDecoder;
            this.maxQueueSize = toPositiveInteger(settings.maxQueueSize) || DEFAULT_MAX_QUEUE_SIZE;
            // The ceiling must be strictly positive to be satisfiable: the scheduled ahead duration is never negative, so a
            // ceiling of zero or below would block every chunk forever instead of refusing one buffer, and a wait for room
            // would then never settle. An absent key, an explicit undefined and an invalid or non-positive value all keep
            // the default, and an explicit null is the only way to disable the ceiling.
            if (!Object.prototype.hasOwnProperty.call(settings, 'maxDecodeAheadSeconds')) {
                this.maxDecodeAheadSeconds = DEFAULT_MAX_SCHEDULED_AHEAD_SECONDS;
            } else if (settings.maxDecodeAheadSeconds === null) {
                this.maxDecodeAheadSeconds = null;
            } else {
                const ceiling = toFiniteNumber(settings.maxDecodeAheadSeconds);
                this.maxDecodeAheadSeconds = ceiling === null || ceiling <= 0 ? DEFAULT_MAX_SCHEDULED_AHEAD_SECONDS : ceiling;
            }
            this.onError = typeof settings.onError === 'function' ? settings.onError : null;
            /** @type {AudioDecoder|null} */
            this.decoder = null;
            /** @type {Object|null} */
            this.config = null;
            /** @type {LuminaAudioPlayer.PlayerError|null} */
            this.error = null;
            this.configured = false;
            this.destroyed = false;
            /** @type {Array<Function>} */
            this.capacityWaiters = [];
            /** @type {number|null} */
            this.capacityTimer = null;
            this.chunksFed = 0;
            this.framesDecoded = 0;
            // The scheduled ahead duration below which feeding reopens after the scheduler refused a frame, if any. It
            // only ever tightens for the stream, so a scheduler whose own ceiling is tighter than this decoder's settles at
            // that ceiling instead of refusing one frame per decoded frame.
            /** @type {number|null} */
            this.scheduleBackoffSeconds = null;
            // True once the scheduled ahead mismatch of the current stream was logged, so a sustained mismatch, which is a
            // configuration slip, does not flood the console.
            this.scheduleBackoffWarned = false;
            // Incremented every time the stream the decoder is decoding changes, which is a configure or a reset. A decode
            // that was waiting for room across such a change belongs to the previous stream and must not be submitted,
            // so it is identified by the generation it started with and dropped when the generation moved on.
            this.generation = 0;

            if (settings.config)
                this.configure(settings.config);
        }

        //+======================================================================================+
        //|                                      Lifecycle                                       |
        //+======================================================================================+

        /**
         * Configures the decoder for a track, creating the native decoder on first use. The configuration is kept, so
         * that a reset can reconfigure the decoder on a seek without the caller supplying it again.
         * @param {Object} config - The decoder configuration.
         * @param {string} config.codec - The WebCodecs codec string of the track, for example 'mp3', 'flac', 'opus', 'mp4a.40.2', 'vorbis' or 'pcm-s16'.
         * @param {number} config.sampleRate - The sample rate of the encoded audio, in Hz.
         * @param {number} config.numberOfChannels - The number of channels of the encoded audio.
         * @param {ArrayBuffer|ArrayBufferView} [config.description] - The codec private data the container carries, for a codec that needs one.
         * @returns {LuminaAudioPlayer.TrackDecoder} This decoder, for chaining.
         * @throws {LuminaAudioPlayer.PlayerError} When the decoder was destroyed, already failed, or the configuration is missing a required value or is rejected by the browser.
         */
        configure(config) {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder was destroyed.');
            if (this.error)
                throw this.error;
            // The configuration is validated before the native decoder is created, so an invalid configuration never
            // allocates one and a decoder that already exists is left untouched.
            const normalized = this.normalizeConfig(config);
            this.ensureDecoder();
            // A reconfigure starts a new stream at the native boundary too. configure() resets an already configured
            // decoder itself, but the reset is done explicitly here so that the module's stream boundary and the native one
            // cannot drift apart, and so a track change drops the queued chunks and the pending frames deterministically
            // instead of leaving it to configure(). A reset returns the decoder to the unconfigured state, so it is only
            // done when a configuration is already in effect.
            if (this.configured && typeof this.decoder.reset === 'function') {
                try {
                    this.decoder.reset();
                } catch (error) {
                    throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'reset-failed' });
                }
            }
            this.applyConfiguration(normalized);
            this.config = normalized;
            this.configured = true;
            // A reconfigure starts a new stream, so a decode that is waiting for room belongs to the previous one and is
            // released as a clean stop instead of being submitted against the new configuration, and a backoff learned
            // from the previous stream no longer applies. There is no pump after this: resolveWaiters already emptied the
            // waiters, so a pump would return at once.
            this.generation++;
            this.scheduleBackoffSeconds = null;
            this.scheduleBackoffWarned = false;
            this.clearCapacityTimer();
            this.resolveWaiters(false);
            return this;
        }

        /**
         * Releases the native decoder, and every wait that was resting on it. It is safe to call more than once.
         *
         * A direct call releases the pending capacity waiters and the poll timer exactly as fail and destroyAsync do, so a
         * caller that closes the decoder itself cannot strand a wait or leave the 200 ms poll running. The decoder is left
         * unconfigured but neither stopped nor destroyed, so a later configure creates a fresh one.
         * @returns {void}
         */
        closeDecoder() {
            // The release happens before the decoder existence check, so a call on an already closed decoder still settles
            // anything that is waiting, rather than relying on the decoder reference being non-null.
            this.clearCapacityTimer();
            this.resolveWaiters(false);
            if (!this.decoder)
                return;
            const decoder = this.decoder;
            this.decoder = null;
            this.configured = false;
            if (typeof decoder.close !== 'function')
                return;
            try {
                const closing = decoder.close();
                // close never rejects, but a decoder implementation that returns a rejected promise must not leave an
                // unhandled rejection behind, so the outcome is deliberately discarded.
                if (closing && typeof closing.catch === 'function')
                    closing.catch(() => {});
            } catch (error) {
                console.debug('Audio player: the audio decoder could not be closed.', error);
            }
        }

        /**
         * Releases every resource of the decoder: the pending capacity waiters, the poll timer and the native decoder.
         * @returns {Promise<void>} A promise that completes when the native decoder is closed.
         */
        async destroyAsync() {
            if (this.destroyed)
                return;
            this.destroyed = true;
            this.configured = false;
            this.clearCapacityTimer();
            this.resolveWaiters(false);
            const decoder = this.decoder;
            this.decoder = null;
            if (!decoder || typeof decoder.close !== 'function')
                return;
            try {
                await decoder.close();
            } catch (error) {
                console.debug('Audio player: the audio decoder could not be closed.', error);
            }
        }

        //+======================================================================================+
        //|                                     Decode surface                                   |
        //+======================================================================================+

        /**
         * Performs the decoder half of a seek: it resets the decoder, dropping the queued chunks and the frames that have
         * not been emitted, and reconfigures it with the configuration it was last given. A decode that is waiting for
         * room when the seek arrives is released as a clean stop, because its packet belongs to the stream before the
         * seek. The scheduler half of the seek, renderer.seekTo, is the caller's to perform.
         * @returns {Promise<void>} A promise that resolves once the decoder is configured again.
         * @throws {LuminaAudioPlayer.PlayerError} When the decoder was destroyed, already failed, or could not be reset.
         */
        async resetAsync() {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder was destroyed.');
            if (this.error)
                throw this.error;
            if (!this.decoder)
                return;
            this.configured = false;
            // The seek starts a new stream, so every decode that is waiting for room belongs to the stream before the
            // seek and is released as a clean stop, instead of resuming to submit a stale pre-seek packet once the
            // reset empties the queue. A backoff learned from the stream before the seek no longer applies either.
            this.generation++;
            this.scheduleBackoffSeconds = null;
            this.scheduleBackoffWarned = false;
            this.clearCapacityTimer();
            this.resolveWaiters(false);
            try {
                this.decoder.reset();
            } catch (error) {
                throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'reset-failed' });
            }
            // A reset returns the decoder to the unconfigured state, so the last configuration is applied again before
            // any new packet is fed; without it the decoder would refuse the first packet of the seek.
            if (this.config) {
                this.applyConfiguration(this.config);
                this.configured = true;
            }
        }

        /**
         * Feeds one encoded packet to the decoder, submitting an EncodedAudioChunk with its microsecond timestamp and
         * duration. The chunk is submitted, not decoded: the decoded frames arrive later, in the output callback.
         * @param {Object|EncodedAudioChunk} packet - The encoded packet, or a prebuilt EncodedAudioChunk. See {@link LuminaAudioPlayer.TrackDecoder#buildEncodedChunk} for the packet shape.
         * @returns {void}
         * @throws {LuminaAudioPlayer.PlayerError} With {@link LuminaAudioPlayer.PlayerErrorCode.DECODER_BUSY} and <c>details.reason === 'queue-full'</c> when there is no room right now, which is a transient condition a caller resolves by waiting for {@link LuminaAudioPlayer.TrackDecoder#waitForCapacityAsync}. Otherwise, with {@link LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE} when the decoder is destroyed or not configured, or with a fatal recorded failure when it rejects the packet.
         */
        decode(packet) {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder was destroyed.');
            if (this.error)
                throw this.error;
            if (!this.configured)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder is not configured.');
            if (!this.canAccept())
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODER_BUSY, 'The audio decoder cannot accept another chunk yet.', { reason: 'queue-full' });
            const chunk = this.buildEncodedChunk(packet);
            try {
                this.decoder.decode(chunk);
            } catch (error) {
                throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'decode-rejected' });
            }
            this.chunksFed++;
        }

        /**
         * Waits for room in the decoder, then feeds one encoded packet. This is the entry point a decode loop uses; a loop
         * that paces itself with canAccept may call decode directly instead.
         * @param {Object|EncodedAudioChunk} packet - The encoded packet, per {@link LuminaAudioPlayer.TrackDecoder#decode}.
         * @returns {Promise<boolean>} True when the packet was submitted, false when the decoder stopped or the stream
         *     changed before there was room, which the caller reads as a clean stop rather than a failure.
         * @throws {LuminaAudioPlayer.PlayerError} When the decoder is in a state that can never accept the packet, or rejects it.
         */
        async decodeAsync(packet) {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder was destroyed.');
            if (this.error)
                throw this.error;
            if (!this.configured)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder is not configured.');
            const generation = this.generation;
            // A single pump releases a batch of waiters at once, and their continuations run one after another, so another
            // caller can take the last slot before this one reaches decode. The wait is therefore repeated until this
            // caller actually has room, which keeps the race from surfacing as a queue-full throw in the decode loop.
            while (true) {
                const accepted = await this.waitForCapacityAsync();
                if (!accepted)
                    return false;
                // The state may have changed while the wait was pending, so it is re-read before the packet is submitted.
                // The generation is a configure or a reset that happened while this packet waited, which makes the packet
                // stale: it belongs to the previous stream and must not be decoded against the new one.
                if (this.destroyed || this.error || !this.configured || generation !== this.generation)
                    return false;
                if (!this.canAccept())
                    continue;
                this.decode(packet);
                return true;
            }
        }

        /**
         * Emits every decoded frame of the chunks fed so far, and resolves once none is left pending. A caller invokes it
         * after the last packet was fed and before it declares the stream complete to the scheduler, so that no frame is
         * lost behind the end of the track. It is a no-op, resolving without touching the native decoder, when the decoder
         * is not configured, so a caller that is unsure whether it ever configured one may call it unconditionally.
         * @returns {Promise<void>} A promise that resolves once the decoder has emitted every pending frame.
         * @throws {LuminaAudioPlayer.PlayerError} When the decoder was destroyed, already failed, or refused to flush.
         */
        async flushAsync() {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The audio decoder was destroyed.');
            if (this.error)
                throw this.error;
            if (!this.decoder || !this.configured)
                return;
            try {
                await this.decoder.flush();
            } catch (error) {
                // A reset or a close interrupts a pending flush; that is a teardown of the decode pipeline, not a failure
                // of the track, so it is observed as a clean stop.
                if (this.destroyed || this.error || isAbortError(error))
                    return;
                throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'flush-failed' });
            }
        }

        //+======================================================================================+
        //|                                      Backpressure                                    |
        //+======================================================================================+

        /**
         * Checks whether the decoder can take another chunk right now, without waiting.
         * @returns {boolean} True when there is room in the decode queue and the scheduler is not too far ahead.
         */
        canAccept() {
            return this.getAvailableSlots() > 0;
        }

        /**
         * Gets the number of chunks the decoder can take right now. It is zero when the decoder is stopped, unconfigured,
         * its decode queue is full, the scheduler is at or beyond the effective scheduled ahead ceiling, or the scheduler
         * refused a frame and the schedule has not yet drained below the refusal point.
         * @returns {number} The number of available slots.
         */
        getAvailableSlots() {
            if (this.destroyed || this.error || !this.configured)
                return 0;
            const slots = this.maxQueueSize - this.getDecodeQueueSize();
            if (slots <= 0)
                return 0;
            const ahead = this.getScheduledAheadSeconds();
            const ceiling = this.getEffectiveCeiling();
            if (ceiling !== null && ahead >= ceiling)
                return 0;
            // A refused frame means the scheduler enforced a tighter ceiling than this decoder's, so feeding stays paused
            // until the playhead has consumed the schedule below the point of the refusal.
            if (this.scheduleBackoffSeconds !== null && ahead >= this.scheduleBackoffSeconds)
                return 0;
            return slots;
        }

        /**
         * Gets the scheduled ahead ceiling the decoder actually feeds to, in seconds, or null when it feeds without a
         * bound. It is the smaller of the decoder's own ceiling and the scheduler's published one, the latter kept a margin
         * below, so that a frame the decoder feeds fits under the scheduler's ceiling and is not refused and lost. Reading
         * the scheduler's ceiling here is what enforces the lossless ordering instead of leaving it to the caller to keep
         * the two options in the right relation.
         * @returns {number|null} The effective ceiling, in seconds, or null when there is none.
         */
        getEffectiveCeiling() {
            const schedulerCeiling = this.getSchedulerCeiling();
            if (schedulerCeiling === null)
                return this.maxDecodeAheadSeconds;
            // The margin is one frame's wall time, and the floor keeps a scheduler ceiling smaller than a frame from
            // closing the gate entirely, so the refusal handler can still detect an unsatisfiable ceiling.
            const safe = Math.max(MINIMUM_SCHEDULE_BACKOFF_SECONDS, schedulerCeiling - SCHEDULER_CEILING_MARGIN_SECONDS);
            if (this.maxDecodeAheadSeconds === null)
                return safe;
            return Math.min(this.maxDecodeAheadSeconds, safe);
        }

        /**
         * Gets the scheduled ahead ceiling the scheduler publishes, in seconds, or null when it publishes none or its
         * accessor throws.
         * @returns {number|null} The scheduler's ceiling, in seconds, or null.
         */
        getSchedulerCeiling() {
            if (!this.scheduler || typeof this.scheduler.getMaxScheduledAheadSeconds !== 'function')
                return null;
            try {
                const ceiling = toFiniteNumber(this.scheduler.getMaxScheduledAheadSeconds());
                return ceiling === null || ceiling < 0 ? null : ceiling;
            } catch (error) {
                console.debug('Audio player: the scheduler could not report its scheduled ahead ceiling.', error);
                return null;
            }
        }

        /**
         * Resolves as soon as the decoder has room for another chunk. It resolves false when the decoder will never have
         * room for the caller: when it is stopped or not configured, or when a configure, a reset, a failure or a destroy
         * releases the pending wait, because the packet the caller holds belongs to the stream that just ended. A wait that
         * finds room immediately resolves true without any generation check; decodeAsync is the entry point that drops a
         * packet whose stream changed under it.
         * @returns {Promise<boolean>} True when there is room, false when the decoder stopped, is not configured, or the wait was released by a configure, reset, failure or destroy.
         */
        waitForCapacityAsync() {
            if (this.canAccept())
                return Promise.resolve(true);
            if (this.destroyed || this.error || !this.configured)
                return Promise.resolve(false);
            return new Promise((resolve) => {
                this.capacityWaiters.push(resolve);
                // The decoder reports no event while the audio clock consumes the schedule, so the wait is also polled.
                this.scheduleCapacityCheck();
            });
        }

        //+======================================================================================+
        //|                                      Query surface                                   |
        //+======================================================================================+

        /**
         * Gets the number of encoded chunks the decoder holds right now.
         * @returns {number} The decode queue size.
         */
        getDecodeQueueSize() {
            if (!this.decoder)
                return 0;
            const size = Number(this.decoder.decodeQueueSize);
            return Number.isFinite(size) && size > 0 ? size : 0;
        }

        /**
         * Gets the decode queue ceiling the decoder enforces.
         * @returns {number} The maximum decode queue size.
         */
        getMaxQueueSize() {
            return this.maxQueueSize;
        }

        /**
         * Gets how far ahead of the audio clock the scheduler currently runs, in seconds. The scheduler is required to
         * expose the value, so this is what the scheduler reports; a scheduler whose accessor throws, for example once its
         * audio context is closed, is treated as an empty schedule rather than failing the caller, because this is called
         * from a polling timer and from the output callback, where an escaping exception could not be reported.
         * @returns {number} The scheduled ahead duration, in seconds.
         */
        getScheduledAheadSeconds() {
            try {
                const ahead = Number(this.scheduler.getScheduledAheadSeconds());
                return Number.isFinite(ahead) ? Math.max(0, ahead) : 0;
            } catch (error) {
                console.debug('Audio player: the scheduler could not report its scheduled ahead duration.', error);
                return 0;
            }
        }

        /**
         * Gets the playback rate the scheduler is running at. It converts a frame's media duration into the wall time the
         * frame adds to the schedule ahead, which the scheduled ahead gate is measured in. The scheduler is not required to
         * expose it, so a scheduler that does not, or one whose accessor throws, is assumed to run at rate one.
         * @returns {number} The playback rate, or one when the scheduler does not report it.
         */
        getPlaybackRate() {
            if (!this.scheduler || typeof this.scheduler.getPlaybackRate !== 'function')
                return 1;
            try {
                const rate = Number(this.scheduler.getPlaybackRate());
                return Number.isFinite(rate) && rate > 0 ? rate : 1;
            } catch (error) {
                console.debug('Audio player: the scheduler could not report its playback rate.', error);
                return 1;
            }
        }

        /**
         * Gets the configuration the decoder is running with.
         * @returns {Object|null} A shallow copy of the configuration, or null when the decoder was never configured. The
         *     codec description buffer is shared with the decoder, not cloned.
         */
        getConfig() {
            return this.config ? Object.assign({}, this.config) : null;
        }

        /**
         * Gets the last failure the decoder recorded, or null when there was none.
         * @returns {LuminaAudioPlayer.PlayerError|null} The recorded failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Checks whether the decoder is configured and ready to take encoded packets.
         * @returns {boolean} True when the decoder is configured.
         */
        isConfigured() {
            return this.configured && !this.destroyed && !this.error;
        }

        /**
         * Checks whether the decoder was destroyed.
         * @returns {boolean} True when the decoder was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Checks whether the decoder can no longer accept encoded packets, because it was destroyed or because it
         * failed.
         * @returns {boolean} True when the decoder has stopped.
         */
        isStopped() {
            return this.destroyed || this.error !== null;
        }

        /**
         * Gets a snapshot of the decoder counters, for diagnostics and tests.
         * @returns {{ chunksFed: number, framesDecoded: number, decodeQueueSize: number }} The snapshot.
         */
        getStatistics() {
            return Object.freeze({
                chunksFed: this.chunksFed,
                framesDecoded: this.framesDecoded,
                decodeQueueSize: this.getDecodeQueueSize()
            });
        }

        //+======================================================================================+
        //|                                   Internal helpers                                   |
        //+======================================================================================+

        /**
         * Makes sure the native decoder exists, creating it on first use through the decoder factory.
         * @returns {AudioDecoder} The native decoder.
         * @throws {LuminaAudioPlayer.PlayerError} When the environment provides no decoder, or it could not be created.
         */
        ensureDecoder() {
            if (this.decoder)
                return this.decoder;
            let decoder = null;
            try {
                decoder = this.decoderFactory({
                    output: (audioData) => this.handleOutput(audioData),
                    error: (error) => this.handleError(error)
                });
            } catch (error) {
                throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED);
            }
            if (!decoder || typeof decoder.configure !== 'function' || typeof decoder.decode !== 'function') {
                // A factory that returns something unusable may still hold native resources, so the object is released
                // before the failure is raised, exactly as a decoder that fails later is released by fail().
                if (decoder && typeof decoder.close === 'function') {
                    try {
                        const closing = decoder.close();
                        if (closing && typeof closing.catch === 'function')
                            closing.catch(() => {});
                    } catch (closeError) {
                        console.debug('Audio player: the unusable audio decoder could not be closed.', closeError);
                    }
                }
                throw this.fail(this.createError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'This environment provides no audio decoder for the player.'));
            }
            this.decoder = decoder;
            if (typeof decoder.addEventListener === 'function') {
                // One dequeue event fires per chunk that leaves the decoder, which is the moment a slot is freed and a
                // waiting caller can be released without polling.
                try {
                    decoder.addEventListener('dequeue', () => this.pump());
                } catch (error) {
                    console.debug('Audio player: the decoder dequeue event could not be observed.', error);
                }
            }
            return decoder;
        }

        /**
         * Validates and normalizes a configuration into the exact shape the browser is given.
         * @param {Object} config - The configuration to normalize.
         * @returns {Object} The normalized configuration.
         * @throws {LuminaAudioPlayer.PlayerError} When the configuration is missing a required value or carries an invalid one.
         */
        normalizeConfig(config) {
            if (!config || typeof config !== 'object')
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The audio decoder configuration is missing.', { reason: 'invalid-config' });
            const codec = typeof config.codec === 'string' ? config.codec.trim() : '';
            if (codec === '')
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The audio decoder configuration names no codec.', { reason: 'invalid-config' });
            const sampleRate = toPositiveInteger(config.sampleRate);
            if (sampleRate === null)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The audio decoder configuration carries no valid sample rate.', { reason: 'invalid-config' });
            const numberOfChannels = toPositiveInteger(config.numberOfChannels);
            if (numberOfChannels === null)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The audio decoder configuration carries no valid channel count.', { reason: 'invalid-config' });
            const normalized = { codec, sampleRate, numberOfChannels };
            if (config.description !== null && config.description !== undefined) {
                const description = toBufferSource(config.description);
                if (description === null)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The audio decoder configuration carries an invalid codec description.', { reason: 'invalid-config' });
                normalized.description = description;
            }
            return normalized;
        }

        /**
         * Applies a normalized configuration to the native decoder.
         * @param {Object} normalized - The configuration, per {@link LuminaAudioPlayer.TrackDecoder#normalizeConfig}.
         * @returns {void}
         * @throws {LuminaAudioPlayer.PlayerError} When the browser rejects the configuration, for example a codec it cannot decode.
         */
        applyConfiguration(normalized) {
            try {
                this.decoder.configure(normalized);
            } catch (error) {
                throw this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'configure-failed' });
            }
        }

        /**
         * Builds a WebCodecs EncodedAudioChunk from the packet a demuxer produced, or accepts one that a demuxer built itself.
         *
         * The packet shape is { data, timestamp, duration, type? }: data is the encoded bytes, as a buffer source; the
         * timestamp is the presentation timestamp of the first sample, in microseconds; the duration is the number of
         * microseconds the packet covers; and the type is 'key' or 'delta', defaulting to 'key', which is the type of
         * every audio packet of the formats the player supports directly. A missing or non finite timestamp is refused,
         * because a decoder cannot place a packet without one; a missing duration is treated as zero.
         * @param {Object|EncodedAudioChunk} packet - The packet to convert.
         * @returns {EncodedAudioChunk} The chunk to feed the decoder.
         * @throws {LuminaAudioPlayer.PlayerError} When the packet is not a usable encoded chunk.
         */
        buildEncodedChunk(packet) {
            const ChunkConstructor = getEncodedAudioChunkConstructor();
            if (ChunkConstructor && packet instanceof ChunkConstructor)
                return packet;
            if (!packet || typeof packet !== 'object')
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The demuxer provided no encoded packet to decode.', { reason: 'invalid-packet' });
            const data = toBufferSource(packet.data);
            if (data === null)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The encoded packet carries no decodable data.', { reason: 'invalid-packet' });
            const timestamp = toFiniteNumber(packet.timestamp);
            if (timestamp === null)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The encoded packet carries no valid timestamp.', { reason: 'invalid-packet' });
            const duration = toFiniteNumber(packet.duration);
            if (!ChunkConstructor)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'This environment provides no encoded audio chunk type for the player.');
            try {
                return new ChunkConstructor({
                    type: packet.type === 'delta' ? 'delta' : 'key',
                    timestamp: Math.round(timestamp),
                    duration: duration === null ? 0 : Math.round(duration),
                    data
                });
            } catch (error) {
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, 'The demuxer provided an encoded packet the decoder cannot use.', { reason: 'invalid-packet' });
            }
        }

        /**
         * Consumes one decoded frame out of the decoder and hands it to the scheduler.
         *
         * The scheduler takes ownership of the frame and closes it, so this callback never closes a frame it handed over.
         * A frame that arrives after the decoder stopped or failed is closed here instead, so its native memory is always
         * released. A refusal caused by the scheduler's own scheduled ahead ceiling is treated as backpressure rather than
         * corruption, through handleScheduleRefusal.
         * @param {AudioData} audioData - The decoded frame.
         */
        handleOutput(audioData) {
            if (this.destroyed || this.error) {
                this.closeAudioData(audioData);
                return;
            }
            // The wall time the frame would add to the schedule ahead, which is its media duration scaled by the playback
            // rate. It is read before the frame is handed over, because the scheduler closes it before returning or throwing.
            const frameWallSeconds = getAudioDataDurationSeconds(audioData) / this.getPlaybackRate();
            try {
                this.scheduler.schedule(audioData);
            } catch (error) {
                if (isScheduledAheadRefusal(error)) {
                    // The scheduler closed the frame already, so it is not closed here.
                    this.handleScheduleRefusal(error, frameWallSeconds);
                    return;
                }
                // The scheduler closes the frame before it throws, so it must not be closed here as well.
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'schedule-failed' });
                return;
            }
            this.framesDecoded++;
            this.pump();
        }

        /**
         * Handles a frame the scheduler refused because it ran beyond the scheduler's own scheduled ahead ceiling. For a
         * ceiling that can still be satisfied that is backpressure, not corruption, so the decoder keeps running and only
         * tightens when it feeds again; a refusal of an empty schedule, which can never be satisfied, is the one case that
         * stops the decoder.
         *
         * A refusal is not free: the scheduler closes the refused frame before it throws, so that frame's audio is
         * discarded and cannot be replayed, and this method can only keep the following frames from being refused too. The
         * decoder normally avoids refusals by staying a margin below the scheduler's published ceiling, so reaching here
         * means the scheduler does not publish its ceiling or a frame was longer than the margin.
         *
         * The scheduler refuses a frame once the schedule ahead plus that frame's wall length would exceed its ceiling.
         * Feeding therefore reopens only after the playhead has drained the schedule below the point of the refusal by the
         * frame's wall length, which is the most the refused frame could have added and so is enough for the next frame of
         * a similar size to fit. The gate is floored, because a refusal only proves the ceiling is smaller than the
         * schedule ahead plus the frame, not that it is smaller than one frame, so a satisfiable ceiling must not be turned
         * into a failure. The gate only ever tightens for the stream, which is what makes a sustained mismatch settle at
         * the scheduler's ceiling instead of refusing a frame per decoded frame, and the mismatch, a configuration slip, is
         * logged once per stream.
         * @param {*} error - The scheduled ahead refusal the scheduler threw.
         * @param {number} frameWallSeconds - The wall time, in seconds, the refused frame would have added to the schedule.
         */
        handleScheduleRefusal(error, frameWallSeconds) {
            const ahead = this.getScheduledAheadSeconds();
            // Only a refusal of an empty schedule is genuinely unsatisfiable: it can only come from a ceiling smaller than
            // one frame plus the scheduling lead, which can never accept anything, so it is a configuration failure. Any
            // other refusal may still be a satisfiable ceiling that simply has to drain further, so it is recoverable.
            if (ahead <= 0) {
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: LuminaAudioPlayer.PlayerErrorReason.SCHEDULED_AHEAD_UNSATISFIABLE });
                return;
            }
            const duration = Number.isFinite(frameWallSeconds) ? Math.max(0, frameWallSeconds) : 0;
            const margin = duration > 0 ? duration : ahead / 2;
            const gate = Math.max(MINIMUM_SCHEDULE_BACKOFF_SECONDS, ahead - margin);
            this.scheduleBackoffSeconds = this.scheduleBackoffSeconds === null ? gate : Math.min(this.scheduleBackoffSeconds, gate);
            if (!this.scheduleBackoffWarned) {
                this.scheduleBackoffWarned = true;
                console.warn('Audio player: the scheduler refused a decoded frame it had no room for, so the decode-ahead is reduced for this track and the refused frame is lost. Keep the decoder scheduled ahead ceiling below the scheduler ceiling.');
            }
            this.pump();
        }

        /**
         * Handles a failure reported by the native decoder itself. It is a failure of the track, not of a caller, so it is
         * recorded and stops the decoder cleanly. A failure that arrives after the decoder was torn down is not a failure
         * of the track, so it is ignored, exactly as a frame that arrives after a teardown is dropped by handleOutput.
         * @param {*} error - The failure the decoder reported.
         */
        handleError(error) {
            if (this.destroyed || this.error)
                return;
            this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, { reason: 'decoder-error' });
        }

        /**
         * Releases the native memory of a decoded frame the scheduler will not take.
         * @param {AudioData} audioData - The frame to close.
         */
        closeAudioData(audioData) {
            if (!audioData || typeof audioData.close !== 'function')
                return;
            try {
                audioData.close();
            } catch (error) {
                console.debug('Audio player: a decoded frame could not be released.', error);
            }
        }

        /**
         * Records a fatal failure, stops the decoder cleanly and notifies the error callback. The first failure wins, so
         * a burst of callbacks after the decoder stopped never overwrites the cause or reports twice. The error is copied
         * into one this decoder owns, and a failure that another component already typed with a player error keeps that
         * component's code, which is more specific than the code passed here.
         * @param {*} error - The failure to record.
         * @param {string} [code] - The code to use when the failure is not already a player error.
         * @param {Object} [details] - Optional structured details about the failure.
         * @returns {LuminaAudioPlayer.PlayerError} The recorded typed error.
         */
        fail(error, code = LuminaAudioPlayer.PlayerErrorCode.DECODE_FAILED, details = null) {
            if (this.error)
                return this.error;
            // The error is owned by this decoder: a PlayerError raised elsewhere is copied rather than adopted, so its
            // details are never mutated and getError never returns an object the raising component still holds.
            const typed = toOwnedError(error, code, details);
            this.error = typed;
            this.clearCapacityTimer();
            this.closeDecoder();
            this.resolveWaiters(false);
            if (this.onError) {
                try {
                    this.onError(typed);
                } catch (callbackError) {
                    console.error('Audio player: the decoder error callback threw.', callbackError);
                }
            }
            return typed;
        }

        /**
         * Builds a typed error without recording it, for a caller error that does not stop the decoder, such as feeding a
         * packet when the decoder is not configured.
         * @param {string} code - One of the {@link LuminaAudioPlayer.PlayerErrorCode} values.
         * @param {string} message - The developer facing description of the failure.
         * @param {Object} [details] - Optional structured details about the failure.
         * @returns {LuminaAudioPlayer.PlayerError} The typed error.
         */
        createError(code, message, details = null) {
            return new LuminaAudioPlayer.PlayerError(code, message, details);
        }

        /**
         * Resolves the waiters whose room is now available, and reschedules the poll while some are still waiting.
         */
        pump() {
            if (this.capacityWaiters.length === 0)
                return;
            if (this.destroyed || this.error) {
                this.resolveWaiters(false);
                return;
            }
            let slots = this.getAvailableSlots();
            while (slots > 0 && this.capacityWaiters.length > 0) {
                const resolve = this.capacityWaiters.shift();
                resolve(true);
                slots--;
            }
            if (this.capacityWaiters.length > 0)
                this.scheduleCapacityCheck();
            else
                this.clearCapacityTimer();
        }

        /**
         * Schedules a poll that re-checks the room once, while at least one waiter is pending. It is the fallback for the
         * wait that only the audio clock can release, because the scheduler emits no event when it consumes its schedule.
         */
        scheduleCapacityCheck() {
            if (this.destroyed || this.capacityTimer !== null || this.capacityWaiters.length === 0)
                return;
            this.capacityTimer = setTimeout(() => {
                this.capacityTimer = null;
                this.pump();
            }, CAPACITY_POLL_INTERVAL_MS);
        }

        /**
         * Clears the poll timer, when one is pending.
         */
        clearCapacityTimer() {
            if (this.capacityTimer === null)
                return;
            clearTimeout(this.capacityTimer);
            this.capacityTimer = null;
        }

        /**
         * Resolves and clears every pending capacity waiter.
         * @param {boolean} value - The value to resolve each waiter with.
         */
        resolveWaiters(value) {
            if (this.capacityWaiters.length === 0)
                return;
            const waiters = this.capacityWaiters;
            this.capacityWaiters = [];
            for (const resolve of waiters)
                resolve(value);
        }
    }

    LuminaAudioPlayer.TrackDecoder = TrackDecoder;

    /**
     * Creates a WebCodecs track decoder. See {@link LuminaAudioPlayer.TrackDecoder} for the options.
     * @param {Object} options - The decoder options.
     * @returns {LuminaAudioPlayer.TrackDecoder} The created decoder.
     */
    LuminaAudioPlayer.createTrackDecoder = function (options) {
        return new TrackDecoder(options);
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
