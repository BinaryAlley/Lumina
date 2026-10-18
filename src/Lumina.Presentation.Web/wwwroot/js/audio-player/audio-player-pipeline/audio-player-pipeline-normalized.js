/**
 * Normalized delivery pipeline of the audio player.
 *
 * A track the player cannot play as stored is produced by the server as a sequence of fixed duration, time addressed
 * segments. A byte range cannot be mapped to a time in such a delivery, so the player addresses it by segment index
 * instead: the playback manifest carries a segment path template, the downloader fetches one complete segment at a time,
 * and the segment demuxer parses each segment with the demuxer of the delivered container and codec, presenting them to
 * the decode pipeline as one continuous timeline.
 *
 * What it owns
 * ------------
 *   - NormalizedSegmentDownloader fetches the segments named by the manifest, keeps every fetched segment in memory for
 *     the lifetime of the track so a seek back into a segment never fetches it again, and reports the downloaded time
 *     ranges and fraction to the player. A segment is an independent container file, so it is fetched whole; the server
 *     has already cut it at a fixed duration, which is what makes a segment a seek unit. The end of the audio is declared
 *     by the segment count header of every response, so a segment that returns not found is a failure (a deleted track, a
 *     moved file or an evicted cache entry), never a silent end of stream. A response that reports every normalization job
 *     slot busy is retried with backoff while the track is playing, because the production is bounded and shared.
 *     A bounded lookahead of one segment keeps the next segment in memory before the decoder needs it. It stops while
 *     playback is paused unless the keep-downloading-while-paused preference is on, and it never advances past the window,
 *     so the whole track is not downloaded at once.
 *   - NormalizedSegmentDemuxer wraps the per segment demuxers behind the same interface the bundled demuxers expose. It
 *     initializes the demuxer of the delivered container, offsets every packet timestamp by the start of its segment, drops
 *     the packets that end before a seek target, and moves to the next segment when the current one ends.
 *
 * Seeking
 * -------
 * A seek selects the segment that covers the target time, reloads it from its first byte, seeks inside it when the
 * container supports a time seek, and reports a segment covering position so the engine's existing seek trim applies. No
 * range request is ever issued for a segment, and no byte range is mapped to a time.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The response header that carries the number of segments a normalized audio production holds.
     * @readonly
     * @type {string}
     */
    const SEGMENT_COUNT_HEADER = 'X-Lumina-Segment-Count';

    /**
     * The response header that carries the exact start of a segment on the track timeline, in seconds.
     * @readonly
     * @type {string}
     */
    const SEGMENT_START_SECONDS_HEADER = 'X-Segment-Start-Seconds';

    /**
     * The response header that carries the produced duration of the normalized track, in seconds.
     * @readonly
     * @type {string}
     */
    const TRACK_DURATION_SECONDS_HEADER = 'X-Lumina-Track-Duration-Seconds';

    /**
     * The delays, in milliseconds, between the attempts of a segment request that the server answered as temporarily unavailable.
     * The production that serves a segment is bounded in concurrency and shared by every user, so a request that arrives while every
     * job slot is in use is a transient condition, not a failure of the track.
     * @readonly
     * @type {ReadonlyArray<number>}
     */
    const UNAVAILABLE_RETRY_DELAYS_MS = Object.freeze([500, 1000, 2000, 4000, 8000, 16000, 30000]);

    /**
     * The longest time, in milliseconds, a playing segment waits for a normalization slot before the request becomes a playback failure. It is
     * longer than the default normalization job timeout, so a production that legitimately started before this request is not reported as an error.
     * @readonly
     * @type {number}
     */
    const MAX_UNAVAILABLE_RETRY_MS = 16 * 60 * 1000;

    /**
     * The number of segments kept fetched ahead of the segment the demuxer is reading. A segment is about six seconds and the
     * decoder schedules about five seconds ahead, so one segment of lookahead is enough to keep the link busy and to have the
     * next segment ready before it is needed, while keeping the concurrent stream requests and the memory bounded.
     * @readonly
     * @type {number}
     */
    const LOOKAHEAD_SEGMENTS = 1;

    /**
     * Converts a time in seconds to whole microseconds, the unit of every packet timestamp of the player.
     * @param {number} seconds - The time in seconds.
     * @returns {number} The time in microseconds.
     */
    function toMicroseconds(seconds) {
        const value = Number(seconds);
        return Number.isFinite(value) && value > 0 ? Math.round(value * 1000000) : 0;
    }

    /**
     * Checks whether a Content-Type is one a segment stream may answer with: an audio type, or the generic binary type
     * the API falls back to for a container it cannot name. Any other type, in particular the text/html of a sign in page, is not the segment.
     * @param {string|null} value - The Content-Type header value.
     * @returns {boolean} True when the value is an audio type or application/octet-stream.
     */
    function isStreamContentType(value) {
        if (!value)
            return false;
        const type = value.split(';', 1)[0].trim().toLowerCase();
        return type.startsWith('audio/') || type === 'application/octet-stream';
    }

    /**
     * Parses the number of segments a normalized audio production declares.
     * @param {string|null} value - The header value.
     * @returns {number|null} The segment count, or null when the value is absent or malformed.
     */
    function parseSegmentCount(value) {
        if (typeof value !== 'string' || value.trim() === '')
            return null;
        const count = Number(value.trim());
        return Number.isInteger(count) && count >= 0 ? count : null;
    }

    /**
     * Parses the exact start of a segment on the track timeline.
     * @param {string|null} value - The header value.
     * @returns {number|null} The start in seconds, or null when the value is absent or malformed.
     */
    function parseSegmentStartSeconds(value) {
        if (typeof value !== 'string' || value.trim() === '')
            return null;
        const seconds = Number(value.trim());
        return Number.isFinite(seconds) && seconds >= 0 ? seconds : null;
    }

    /**
     * A random access byte source over the bytes of one segment, so the bundled demuxer of the delivered container reads the segment exactly as it would read a file.
     */
    class SegmentByteSource {
        /**
         * Creates an instance of SegmentByteSource.
         * @param {Uint8Array} bytes - The complete bytes of the segment.
         */
        constructor(bytes) {
            this.bytes = bytes instanceof Uint8Array ? bytes : new Uint8Array(0);
        }

        /**
         * Gets the length of the segment.
         * @returns {number} The length in bytes.
         */
        getLength() {
            return this.bytes.length;
        }

        /**
         * Reads a window of the segment at an absolute offset.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The number of bytes of the window.
         * @returns {Promise<Uint8Array>} The bytes, possibly shorter at the end of the segment.
         */
        async readAsync(offset, length) {
            const start = Math.max(0, Math.floor(Number(offset) || 0));
            const size = Math.max(0, Math.floor(Number(length) || 0));
            if (size === 0 || start >= this.bytes.length)
                return new Uint8Array(0);
            return this.bytes.subarray(start, Math.min(this.bytes.length, start + size));
        }
    }

    /**
     * Fetches, keeps and reports the fixed duration segments of a normalized track.
     */
    class NormalizedSegmentDownloader {
        /**
         * Creates an instance of NormalizedSegmentDownloader.
         * @param {Object} options - The downloader options.
         * @param {Function} options.urlBuilder - Builds the URL of a segment from its zero based index.
         * @param {number} options.durationInSeconds - The duration of the track, in seconds.
         * @param {number} options.segmentDurationSeconds - The duration of one segment, in seconds.
         * @param {Function} [options.onProgress] - Called with the downloaded fraction, between 0 and 1.
         * @param {Function} [options.onError] - Called with a typed error when a segment cannot be fetched.
         * @param {boolean} [options.shouldRetryUnavailable=true] - Whether a request the server reports as temporarily unavailable is retried with backoff.
         * @param {boolean} [options.shouldKeepDownloadingWhilePaused=false] - Whether the lookahead keeps fetching while playback is paused.
         * @param {Object} [options.events] - The player event bus, used to follow a change of the keep-downloading-while-paused preference.
         */
        constructor(options) {
            const settings = options || {};
            this.urlBuilder = settings.urlBuilder;
            this.durationInSeconds = Math.max(0, Number(settings.durationInSeconds) || 0);
            this.segmentDurationSeconds = Math.max(0, Number(settings.segmentDurationSeconds) || 0);
            this.onProgress = typeof settings.onProgress === 'function' ? settings.onProgress : null;
            this.onError = typeof settings.onError === 'function' ? settings.onError : null;
            this.events = settings.events || null;
            this.shouldRetryUnavailable = settings.shouldRetryUnavailable !== false;
            this.shouldKeepDownloadingWhilePaused = settings.shouldKeepDownloadingWhilePaused === true;
            this.isPlaybackPaused = false;
            // The last segment the demuxer actually consumed. The lookahead is measured from it, so background fetches can never
            // advance it and turn the bounded read ahead into a full download of the track.
            this.lookaheadAnchor = -1;
            this.unsubscribePreferences = null;
            // The production states how many segments it holds, so the end of the audio is known without a request that returns not found.
            // Until the first segment response states it, the count is unknown and a missing segment is a failure, never an end of stream.
            this.segmentCount = null;
            // The server reports the produced duration, which can differ from the probed duration in the manifest. It is kept separately so the
            // progress bar and the seek clamp use what was actually produced once the first segment response arrives.
            this.producedDurationSeconds = null;
            /** @type {Map<number, number>} */
            this.segmentStartTimes = new Map();
            /** @type {Map<number, Uint8Array>} */
            this.segments = new Map();
            /** @type {Map<number, Promise<Uint8Array|null>>} */
            this.pending = new Map();
            // Every request has its own controller and all of them are aborted together, because a seek or a teardown must cancel
            // requests whose headers have arrived and whose bodies are still being read, not only the newest one.
            this.controllers = new Set();
            this.error = null;
            this.isAborted = false;
            // A set, not one handle, because a required read and a speculative lookahead can be waiting on a retry at the same time and an abort
            // must release both immediately.
            this.retryWaitCancels = new Set();
            // Incremented by every abort, so a request that was in flight when the downloader stopped can be told apart from a
            // request that was started after it, which must be allowed to proceed.
            this.abortGeneration = 0;
            this.destroyed = false;
            this.subscribeToPreferences();
        }

        /**
         * Gets the start of a segment on the timeline of the track. The exact start the server indexed is used when it is known, because ffmpeg
         * cuts at the first packet at or after a segment boundary, so a segment does not always start at an exact multiple of the duration.
         * @param {number} index - The zero based index of the segment.
         * @returns {number} The start, in seconds.
         */
        getSegmentStartSeconds(index) {
            const declaredStart = this.segmentStartTimes.get(index);
            if (declaredStart !== undefined)
                return declaredStart;
            return Math.max(0, index) * this.segmentDurationSeconds;
        }

        /**
         * Selects the segment that covers a time from the exact starts the server declared, falling back to the nominal grid only for segments whose start is not known yet.
         * @param {number} seconds - The time to locate, in seconds.
         * @returns {number} The zero based index of the covering segment.
         */
        findSegmentIndexForTime(seconds) {
            const value = Number(seconds);
            const target = Number.isFinite(value) && value > 0 ? value : 0;
            if (this.segmentDurationSeconds <= 0)
                return 0;
            let index = Math.max(0, Math.floor(target / this.segmentDurationSeconds));
            while (index > 0 && this.segmentStartTimes.has(index) && this.segmentStartTimes.get(index) > target)
                index--;
            while (this.segmentStartTimes.has(index + 1) && this.segmentStartTimes.get(index + 1) <= target)
                index++;
            return index;
        }

        /**
         * Gets the duration of a segment on the timeline of the track, from the start of the next segment when it is known, otherwise from the configured segment duration and the end of the track.
         * @param {number} index - The zero based index of the segment.
         * @returns {number} The duration, in seconds.
         */
        getSegmentDurationSeconds(index) {
            const start = this.getSegmentStartSeconds(index);
            const nextStart = this.segmentStartTimes.get(index + 1);
            if (nextStart !== undefined)
                return Math.max(0, nextStart - start);
            const totalDuration = this.getTotalDurationSeconds();
            if (totalDuration <= 0)
                return this.segmentDurationSeconds;
            return Math.max(0, Math.min(this.segmentDurationSeconds, totalDuration - start));
        }

        /**
         * Gets the duration of the produced normalized audio, which can differ from the probed duration reported by the manifest.
         * @returns {number|null} The produced duration in seconds, or null when no segment response has declared it.
         */
        getProducedDurationSeconds() {
            return this.producedDurationSeconds;
        }

        /**
         * Gets the duration used for progress, preferring the produced duration over the probed one.
         * @returns {number} The duration in seconds, or zero when it is not known.
         */
        getTotalDurationSeconds() {
            const producedDuration = Number(this.producedDurationSeconds);
            if (Number.isFinite(producedDuration) && producedDuration > 0)
                return producedDuration;
            return this.durationInSeconds;
        }

        /**
         * Gets one segment, from memory when it was already fetched, otherwise from the segment route.
         * @param {number} index - The zero based index of the segment.
         * @returns {Promise<Uint8Array|null>} The bytes of the segment, or null when the index is past the produced audio.
         * @throws {LuminaAudioPlayer.PlayerError} When the segment cannot be fetched, or the download was aborted.
         */
        async getSegmentAsync(index) {
            if (this.destroyed)
                throw this.createAbortError();
            if (!Number.isInteger(index) || index < 0)
                throw this.recordError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The segment index is not valid.'));
            // The production stated how many segments it holds, so an index at or past that count is the end of the audio and needs no request.
            if (this.segmentCount !== null && index >= this.segmentCount)
                return null;
            let bytes;
            if (this.segments.has(index)) 
                bytes = this.segments.get(index);
            else if (this.pending.has(index)) // The segment may already be in flight as lookahead, so the read waits for that request instead of starting a second one.                
                bytes = await this.pending.get(index);
            else 
                bytes = await this.createSegmentRequest(index, false);
            if (bytes) // Only a segment the demuxer actually consumed moves the anchor, so the read ahead stays bounded to the lookahead window.                
                this.advanceLookaheadAnchor(index);
            return bytes;
        }

        /**
         * Starts a segment request and records it as pending, so a concurrent read or a lookahead never asks for the same segment twice.
         * @param {number} index - The zero based index of the segment.
         * @param {boolean} isSpeculative - True for a background lookahead, whose failure must not become the failure of the track.
         * @returns {Promise<Uint8Array|null>} The bytes of the segment, or null when it does not exist.
         */
        createSegmentRequest(index, isSpeculative) {
            const request = this.fetchSegmentAsync(index, isSpeculative);
            this.pending.set(index, request);
            request.then(
                () => {
                    if (this.pending.get(index) === request)
                        this.pending.delete(index);
                },
                () => {
                    // A failed request is forgotten, so a later read of the same segment can try again. The failure itself reaches the
                    // caller that awaited it, and is only reported through onError when it belongs to the track that is playing.
                    if (this.pending.get(index) === request)
                        this.pending.delete(index);
                }
            );
            return request;
        }

        /**
         * Moves the read ahead anchor to a consumed segment and starts the lookahead that follows it.
         * @param {number} index - The zero based index of the segment the demuxer consumed.
         */
        advanceLookaheadAnchor(index) {
            if (index <= this.lookaheadAnchor)
                return;
            this.lookaheadAnchor = index;
            this.scheduleLookahead();
        }

        /**
         * Fetches up to the configured number of segments after the consumed one, so the next segment is already in memory when the
         * decoder needs it. The lookahead is bounded, and it stops while playback is paused unless the user kept downloading enabled.
         */
        scheduleLookahead() {
            if (this.destroyed || this.isAborted || this.segmentCount === null || this.lookaheadAnchor < 0)
                return;
            if (this.isPlaybackPaused && !this.shouldKeepDownloadingWhilePaused)
                return;
            for (let offset = 1; offset <= LOOKAHEAD_SEGMENTS; offset++) {
                const index = this.lookaheadAnchor + offset;
                if (index >= this.segmentCount)
                    return;
                if (this.segments.has(index) || this.pending.has(index))
                    continue;
                this.createSegmentRequest(index, true);
            }
        }

        /**
         * Subscribes to the preference changes of the player, so a change of the keep-downloading-while-paused setting is applied to
         * the lookahead of a track that is already loaded.
         */
        subscribeToPreferences() {
            if (!this.events || !LuminaAudioPlayer.PlayerEvent || typeof this.events.on !== 'function')
                return;
            this.unsubscribePreferences = this.events.on(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, (preferences) => {
                if (preferences && typeof preferences.shouldKeepDownloadingWhilePaused === 'boolean')
                    this.setKeepDownloadingWhilePaused(preferences.shouldKeepDownloadingWhilePaused);
            });
        }

        /**
         * Fetches one complete segment and stores it, so it is never fetched again for this track.
         * @param {number} index - The zero based index of the segment.
         * @param {boolean} [isSpeculative=false] - True for a background lookahead, whose failure must not be recorded as the failure of the track.
         * @returns {Promise<Uint8Array|null>} The bytes of the segment, or null when the server reports no such segment.
         * @throws {LuminaAudioPlayer.PlayerError} When the request fails or is aborted.
         */
        async fetchSegmentAsync(index, isSpeculative = false) {
            if (this.destroyed)
                throw this.createAbortError();
            // The generation is captured for the whole call, so an abort or restart invalidates every retry and the body read, even when a later attempt uses a new controller.
            const abortGeneration = this.abortGeneration;
            // The retry window starts at the first attempt, not at each retry, so a production that stays unavailable cannot keep this request alive indefinitely.
            const retryStartedAt = Date.now();
            let attempt = 0;
            // Only a transient unavailable status loops back here; every other result leaves through a return or a throw below.
            while (true) {
                if (this.destroyed || this.abortGeneration !== abortGeneration)
                    throw this.createAbortError();
                const controller = typeof AbortController === 'function' ? new AbortController() : null;
                // The controller stays registered until the body has been read, so an abort during arrayBuffer cancels the transfer instead
                // of leaving it to download the discarded segment in full while holding a stream slot on both hops.
                if (controller)
                    this.controllers.add(controller);
                try {
                    let response;
                    try {
                        const url = typeof this.urlBuilder === 'function' ? this.urlBuilder(index) : null;
                        if (typeof url !== 'string' || url.length === 0)
                            throw this.createRequestFailure('The segment URL is not configured.', null, isSpeculative, LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED);
                        response = await fetch(url, {
                            method: 'GET',
                            credentials: 'same-origin',
                            redirect: 'manual',
                            headers: { 'Accept': 'audio/*, application/octet-stream' },
                            signal: controller ? controller.signal : undefined
                        });
                    } catch (error) {
                        if (LuminaAudioPlayer.isAbortError(error) || this.destroyed || this.abortGeneration !== abortGeneration)
                            throw this.createAbortError();
                        // A speculative read must not record the first failure, because the required read that follows owns the
                        // track's error state; the recorded failure is the one the user should see.
                        if (error instanceof LuminaAudioPlayer.PlayerError)
                            throw isSpeculative ? error : this.recordError(error);
                        throw this.createRequestFailure('The segment could not be requested.', { cause: error }, isSpeculative);
                    }
                    if (!response)
                        throw this.createRequestFailure('The segment request returned no response.', null, isSpeculative);
                    // Redirects are not followed: an expired session answers with a redirect to the login page, whose HTML must never be parsed as audio.
                    if (response.type === 'opaqueredirect' || (response.status >= 300 && response.status < 400))
                        throw this.createRequestFailure('The session expired while a segment was requested.', { reason: LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED }, isSpeculative);
                    if (!response.ok) {
                        const status = response.status;
                        // Every non-success path discards the body first, so the connection is released instead of being held by a response nobody reads.
                        await this.discardResponseAsync(response);
                        // Every job slot can be in use because productions are shared and bounded, so a temporarily unavailable segment is
                        // retried for the track that is playing. A prefetch does not retry: it is abandoned and the track starts normally.
                        if (this.shouldRetryUnavailable && (status === 503 || status === 429)
                            && Date.now() - retryStartedAt < MAX_UNAVAILABLE_RETRY_MS) {
                            // The last delay is reused after the array ends, so the backoff stays bounded while the retry window runs.
                            const delay = UNAVAILABLE_RETRY_DELAYS_MS[Math.min(attempt, UNAVAILABLE_RETRY_DELAYS_MS.length - 1)];
                            attempt++;
                            await this.waitBeforeRetryAsync(delay, abortGeneration);
                            continue;
                        }
                        // The reason is part of the error contract: the engine uses SEGMENT_NOT_FOUND and PLAYBACK_DELIVERY_CHANGED
                        // to refresh the manifest, and reports the other reasons as ordinary playback failures.
                        const reason = status === 503
                            ? LuminaAudioPlayer.PlayerErrorReason.NORMALIZATION_UNAVAILABLE
                            : (status === 409
                                ? LuminaAudioPlayer.PlayerErrorReason.PLAYBACK_DELIVERY_CHANGED
                                : (status === 404 ? LuminaAudioPlayer.PlayerErrorReason.SEGMENT_NOT_FOUND : LuminaAudioPlayer.PlayerErrorReason.HTTP_REQUEST_FAILED));
                        throw this.createRequestFailure('The segment request failed.', { reason, status }, isSpeculative);
                    }
                    // A proxy or an error page can answer 200 with a non-audio body, so the media type is checked before the bytes ever reach a demuxer.
                    if (!isStreamContentType(response.headers.get('Content-Type'))) {
                        await this.discardResponseAsync(response);
                        throw this.createRequestFailure('The segment response is not audio.', null, isSpeculative);
                    }
                    // The production declares how many segments exist. It is required, because a player cannot tell a missing
                    // final segment from the end of the track without it.
                    const declaredSegmentCount = parseSegmentCount(response.headers.get(SEGMENT_COUNT_HEADER));
                    if (declaredSegmentCount === null) {
                        await this.discardResponseAsync(response);
                        throw this.createRequestFailure('The segment response carried no segment count.', null, isSpeculative);
                    }
                    // A response beyond the declared count means the handle and the production disagree, not that the track ended.
                    if (index >= declaredSegmentCount) {
                        await this.discardResponseAsync(response);
                        throw this.createRequestFailure('The segment response is outside the declared production.', null, isSpeculative);
                    }
                    if (this.segmentCount !== null && this.segmentCount !== declaredSegmentCount) {
                        // A production is immutable, so a different count means the audio was replaced under the track. Mixing the two
                        // productions would corrupt the timeline, so the track fails instead of playing a splice of both.
                        await this.discardResponseAsync(response);
                        throw this.createRequestFailure('The normalized production changed during playback.', null, isSpeculative);
                    }
                    this.segmentCount = declaredSegmentCount;
                    // The produced duration and the exact start of this segment come from the server, because the probe duration and
                    // the nominal index times can differ slightly after transcoding.
                    const declaredDuration = parseSegmentStartSeconds(response.headers.get(TRACK_DURATION_SECONDS_HEADER));
                    if (declaredDuration !== null)
                        this.producedDurationSeconds = declaredDuration;
                    const declaredStart = parseSegmentStartSeconds(response.headers.get(SEGMENT_START_SECONDS_HEADER));
                    if (declaredStart !== null)
                        this.segmentStartTimes.set(index, declaredStart);
                    let bytes;
                    try {
                        // The controller is still registered here, so an abort now cancels this body read instead of after it completed.
                        bytes = new Uint8Array(await response.arrayBuffer());
                    } catch (error) {
                        if (LuminaAudioPlayer.isAbortError(error) || this.destroyed || this.abortGeneration !== abortGeneration)
                            throw this.createAbortError();
                        throw this.createRequestFailure('The segment body could not be read.', { cause: error }, isSpeculative);
                    }
                    if (this.destroyed || this.abortGeneration !== abortGeneration)
                        throw this.createAbortError();
                    // The bytes are cached only after the whole body was read, so a transfer that failed halfway can never be mistaken for a complete segment.
                    this.segments.set(index, bytes);
                    this.reportProgress();
                    return bytes;
                } finally {
                    if (controller)
                        this.controllers.delete(controller);
                }
            }
        }

        /**
         * Discards the body of a response that is not used, so a connection that will not be read is released.
         * @param {Response} response - The response whose body is discarded.
         * @returns {Promise<void>} A promise that completes when the body was discarded.
         */
        async discardResponseAsync(response) {
            try {
                if (response && response.body && typeof response.body.cancel === 'function')
                    await response.body.cancel();
            } catch (error) {
                console.debug('Audio player: the unused segment response could not be discarded.', error);
            }
        }

        /**
         * Waits before the next attempt of a temporarily unavailable segment request, unless the downloader is aborted first.
         * @param {number} delay - The delay, in milliseconds.
         * @param {number} abortGeneration - The abort generation the attempt belongs to.
         * @returns {Promise<void>} A promise that completes when the delay elapsed.
         * @throws {LuminaAudioPlayer.PlayerError} When the downloader was aborted or released during the delay.
         */
        waitBeforeRetryAsync(delay, abortGeneration) {
            return new Promise((resolve, reject) => {
                let isSettled = false;
                let timer = null;
                const finish = (isAborted) => {
                    if (isSettled)
                        return;
                    isSettled = true;
                    if (timer !== null)
                        clearTimeout(timer);
                    this.retryWaitCancels.delete(cancel);
                    if (isAborted)
                        reject(this.createAbortError());
                    else
                        resolve();
                };
                const cancel = () => finish(true);
                timer = setTimeout(() => finish(false), delay);
                this.retryWaitCancels.add(cancel);
                if (this.destroyed || this.abortGeneration !== abortGeneration)
                    cancel();
            });
        }

        /**
         * Reports the downloaded fraction of the track, mapped through the manifest duration so the streamed indicator is
         * time accurate.
         */
        reportProgress() {
            if (!this.onProgress)
                return;
            try {
                this.onProgress(this.getDownloadedFraction());
            } catch (error) {
                console.debug('Audio player: the downloaded fraction of a normalized track could not be reported.', error);
            }
        }

        /**
         * Gets the fraction of the track that was fetched, by the duration of the fetched segments.
         * @returns {number} The fraction, between 0 and 1.
         */
        getDownloadedFraction() {
            const totalDuration = this.getTotalDurationSeconds();
            if (totalDuration <= 0 || this.segmentDurationSeconds <= 0)
                return 0;
            let downloadedSeconds = 0;
            for (const index of this.segments.keys())
                downloadedSeconds += this.getSegmentDurationSeconds(index);
            return Math.min(1, Math.max(0, downloadedSeconds / totalDuration));
        }

        /**
         * Gets the time ranges of the track that were already fetched, so the three state progress bar can show the
         * streamed part and a seek into it needs no loading state.
         * @returns {Array<{ start: number, end: number }>} The downloaded time ranges, in seconds.
         */
        getTimeRanges() {
            const indexes = Array.from(this.segments.keys()).sort((first, second) => first - second);
            const ranges = [];
            for (const index of indexes) {
                const start = this.getSegmentStartSeconds(index);
                const end = start + this.getSegmentDurationSeconds(index);
                const last = ranges[ranges.length - 1];
                if (last && Math.abs(last.end - start) < 0.000001)
                    last.end = end;
                else
                    ranges.push({ start, end });
            }
            return ranges;
        }

        /**
         * Checks whether a time is covered by a fetched segment, so the player knows a seek into it needs no loading state.
         * @param {number} seconds - The time to check, in seconds.
         * @returns {boolean} True when the segment that covers the time was fetched.
         */
        coversTime(seconds) {
            if (this.segmentDurationSeconds <= 0)
                return false;
            const value = Number(seconds);
            if (!Number.isFinite(value) || value < 0)
                return false;
            // When the exact starts are known, the segment that covers the time is the one with the greatest declared start at or before it,
            // not necessarily the one the assumed grid would pick.
            let candidateIndex = null;
            let candidateStart = -1;
            for (const [index, start] of this.segmentStartTimes) {
                if (start <= value && start > candidateStart) {
                    candidateIndex = index;
                    candidateStart = start;
                }
            }
            if (candidateIndex !== null && value <= candidateStart + this.getSegmentDurationSeconds(candidateIndex))
                return this.segments.has(candidateIndex);
            return this.segments.has(Math.floor(value / this.segmentDurationSeconds));
        }

        /**
         * Sets whether playback is paused. The lookahead keeps fetching while paused only when the user enabled the
         * keep-downloading-while-paused preference; a request that is already in flight is left to finish, so pausing never
         * turns a segment the decoder is waiting for into a failure.
         * @param {boolean} isPaused - Whether playback is paused.
         */
        setPlaybackPaused(isPaused) {
            const next = isPaused === true;
            if (next === this.isPlaybackPaused)
                return;
            this.isPlaybackPaused = next;
            if (!next || this.shouldKeepDownloadingWhilePaused)
                this.scheduleLookahead();
        }

        /**
         * Sets the keep-downloading-while-paused preference. When it is enabled while playback is paused, the lookahead resumes;
         * when it is disabled, no new lookahead is started, while a request that is already in flight is left to finish.
         * @param {boolean} shouldKeepDownloading - Whether downloading should continue while paused.
         */
        setKeepDownloadingWhilePaused(shouldKeepDownloading) {
            const next = shouldKeepDownloading === true;
            if (next === this.shouldKeepDownloadingWhilePaused)
                return;
            this.shouldKeepDownloadingWhilePaused = next;
            if (next)
                this.scheduleLookahead();
        }

        /**
         * Enables or disables the retry of a temporarily unavailable segment request. A prefetch starts with it disabled, so an
         * unavailable production is abandoned instead of held; the item enables it when it becomes the track that is playing.
         * @param {boolean} shouldRetry - Whether a temporarily unavailable request is retried with backoff.
         */
        setShouldRetryUnavailable(shouldRetry) {
            this.shouldRetryUnavailable = shouldRetry === true;
        }

        /**
         * Starts, or resumes, the downloader after an abort. It clears the abort flag and resumes the lookahead, because a seek and a resume both call it.
         * @returns {Promise<void>} A promise that completes when the downloader is usable again.
         */
        async startAsync() {
            this.isAborted = false;
            this.scheduleLookahead();
        }

        /**
         * Moves the read ahead anchor to the segment that covers a seek target, so the lookahead around the new position is fetched
         * before the decoder reaches it. The segment of the target itself is fetched by the demuxer.
         * @param {number} seconds - The target position, in seconds.
         */
        seek(seconds) {
            if (this.segmentDurationSeconds <= 0)
                return;
            const value = Number(seconds);
            const target = Number.isFinite(value) && value > 0 ? value : 0;
            this.lookaheadAnchor = this.findSegmentIndexForTime(target);
            this.scheduleLookahead();
        }

        /**
         * Aborts the segment request that is in flight, if any, and rejects the reads that are waiting for it.
         */
        abort() {
            this.isAborted = true;
            // Every pending read belongs to the request that is being stopped, so the entries are dropped here; a read that starts after
            // this abort is a new request and is not rejected by the abort that precedes it.
            this.abortGeneration += 1;
            this.pending.clear();
            for (const cancelRetryWait of this.retryWaitCancels) {
                try {
                    cancelRetryWait();
                } catch (error) {
                    console.debug('Audio player: a segment retry wait could not be cancelled.', error);
                }
            }
            this.retryWaitCancels.clear();
            // Every request is aborted, including one whose headers arrived and whose body is still being read. The controllers stay
            // registered until each body read finishes, which is what makes this cancellation effective on a slow link.
            for (const controller of this.controllers) {
                try {
                    controller.abort();
                } catch (error) {
                    console.debug('Audio player: a segment request could not be aborted.', error);
                }
            }
            this.controllers.clear();
        }

        /**
         * Gets the last failure the downloader recorded, or null.
         * @returns {LuminaAudioPlayer.PlayerError|null} The recorded failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Gets the total size of the track in bytes, which the segment delivery does not expose as a stream length.
         * @returns {null} Always null.
         */
        getTotalBytes() {
            return null;
        }

        /**
         * Builds the typed failure of a segment request. A failure of a background lookahead is returned without being recorded, so a
         * speculative request that is not needed yet can never make the track that is playing fail.
         * @param {string} message - The developer facing description of the failure.
         * @param {Object|null} details - Optional structured details.
         * @param {boolean} isSpeculative - True when the failure belongs to a background lookahead.
         * @param {string} [code] - The player error code. Defaults to DOWNLOAD_FAILED.
         * @returns {LuminaAudioPlayer.PlayerError} The typed failure, recorded when it is not speculative.
         */
        createRequestFailure(message, details, isSpeculative, code = LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED) {
            const error = new LuminaAudioPlayer.PlayerError(code, message, details);
            return isSpeculative ? error : this.recordError(error);
        }

        /**
         * Records a failure once and reports it to the caller, so a failed segment is never retried silently and never overwrites the cause of the first failure.
         * @param {LuminaAudioPlayer.PlayerError} error - The failure to record.
         * @returns {LuminaAudioPlayer.PlayerError} The recorded failure.
         */
        recordError(error) {
            if (this.error)
                return this.error;
            this.error = error;
            if (this.onError) {
                try {
                    this.onError(error);
                } catch (callbackError) {
                    console.error('Audio player: the normalized download error callback threw.', callbackError);
                }
            }
            return error;
        }

        /**
         * Creates the typed abort of an intentionally stopped segment request.
         * @returns {LuminaAudioPlayer.PlayerError} The abort.
         */
        createAbortError() {
            return new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The segment request was aborted.');
        }

        /**
         * Releases every fetched segment and aborts the request that is in flight.
         * @returns {Promise<void>} A promise that completes when the downloader was released.
         */
        async destroyAsync() {
            if (this.destroyed)
                return;
            this.destroyed = true;
            if (this.unsubscribePreferences) {
                try {
                    this.unsubscribePreferences();
                } catch (error) {
                    console.debug('Audio player: the normalized download preference subscription could not be removed.', error);
                }
                this.unsubscribePreferences = null;
            }
            this.abort();
            this.segments.clear();
            this.pending.clear();
        }
    }

    /**
     * Presents the segments of a normalized track to the decode pipeline as one continuous stream.
     */
    class NormalizedSegmentDemuxer {
        /**
         * Creates an instance of NormalizedSegmentDemuxer.
         * @param {Object} options - The demuxer options.
         * @param {Object} options.manifest - The playback manifest of the normalized track.
         * @param {LuminaAudioPlayer.NormalizedSegmentDownloader} options.downloader - The downloader of the segments.
         * @param {Object} [options.registry] - The demuxer registry. Defaults to the process wide registry.
         * @param {Object} [options.capabilities] - The browser capability detector.
         * @param {Object} [options.events] - The player event bus.
         * @param {Function} [options.onError] - Called with a typed error when the normalized stream cannot be read.
         */
        constructor(options) {
            const settings = options || {};
            const manifest = settings.manifest || {};
            this.manifest = manifest;
            this.downloader = settings.downloader;
            this.registry = settings.registry || LuminaAudioPlayer.demuxerRegistry;
            this.capabilities = settings.capabilities || null;
            this.events = settings.events || null;
            this.onError = typeof settings.onError === 'function' ? settings.onError : null;
            this.container = LuminaAudioPlayer.canonicalizeDemuxerContainer(manifest.deliveryContainer);
            this.codec = LuminaAudioPlayer.canonicalizeDemuxerCodec(manifest.deliveryCodec);
            this.segmentDurationSeconds = Math.max(0, Number(manifest.segmentDurationSeconds) || 0);
            this.durationInSeconds = Math.max(0, Number(manifest.durationInSeconds) || 0);
            this.inner = null;
            this.innerIndex = -1;
            this.initialized = false;
            this.destroyed = false;
            this.error = null;
            // Set only when the produced segment count is exhausted, so an ordinary read failure is never treated as the end of the track.
            this.hasReachedEnd = false;
            this.seekTargetMicros = null;
        }

        /**
         * Gets the start of a segment on the timeline of the track, from the exact value the server indexed when it is known.
         * @param {number} index - The zero based index of the segment.
         * @returns {number} The start, in seconds.
         */
        getSegmentStartSeconds(index) {
            if (this.downloader && typeof this.downloader.getSegmentStartSeconds === 'function')
                return this.downloader.getSegmentStartSeconds(index);
            return Math.max(0, index) * this.segmentDurationSeconds;
        }

        /**
         * Loads the demuxer of one segment, replacing the previous one.
         * @param {number} index - The zero based index of the segment.
         * @param {boolean} [forceReload=false] - True to reload a segment that is already open, used by a seek.
         * @returns {Promise<boolean>} True when the segment was loaded, false when it does not exist.
         * @throws {LuminaAudioPlayer.PlayerError} When the segment or its format cannot be read.
         */
        async loadSegmentAsync(index, forceReload = false) {
            if (this.destroyed)
                throw this.createAbortError();
            // A normal sequential read reuses the open segment. A seek forces a reload even for the same index, because its demuxer must
            // restart at the first packet so readPacketAsync can drop the frames before the new target.
            if (!forceReload && this.inner && this.innerIndex === index)
                return true;
            // The old segment is detached before it is awaited, so a failure while releasing it can never leave the wrapper pointing at a demuxer that is being destroyed.
            const previous = this.inner;
            this.inner = null;
            this.innerIndex = -1;
            if (previous) {
                try {
                    await previous.destroyAsync();
                } catch (error) {
                    console.debug('Audio player: the previous normalized segment could not be released.', error);
                }
            }
            // A seek can move back before the produced end, so the end marker is only valid until the next forced load.
            if (forceReload)
                this.hasReachedEnd = false;
            if (this.hasReachedEnd)
                return false;
            let bytes;
            try {
                bytes = await this.downloader.getSegmentAsync(index);
            } catch (error) {
                if (LuminaAudioPlayer.isDownloadAbort(error))
                    throw this.createAbortError();
                // The downloader has already classified the failure, but this wrapper owns the stream error that the decode pipeline
                // reads, so the failure is recorded with the download code before it is thrown.
                this.recordFailure(LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED));
                throw this.error;
            }
            if (bytes === null) {
                // No bytes for this index is the declared end of the production, not a corrupt segment.
                this.hasReachedEnd = true;
                return false;
            }
            const descriptor = this.registry ? this.registry.find(this.container, this.codec) : null;
            if (!descriptor) {
                this.recordFailure(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The bundled player has no demuxer for the normalized delivery.', { reason: LuminaAudioPlayer.PlayerErrorReason.NO_DEMUXER }));
                throw this.error;
            }
            // The inner demuxer reads only from this segment's bytes. Its error callback is left null because this wrapper records and classifies every failure itself.
            const inner = descriptor.create({
                source: new SegmentByteSource(bytes),
                capabilities: this.capabilities,
                events: this.events,
                onError: null
            });
            try {
                await inner.initializeAsync();
            } catch (error) {
                // A demuxer that failed to initialize may still hold its byte source or parser state, so it is released before the wrapper records the failure.
                try {
                    await inner.destroyAsync();
                } catch (destroyError) {
                    console.debug('Audio player: the failed normalized segment could not be released.', destroyError);
                }
                if (LuminaAudioPlayer.isDownloadAbort(error))
                    throw this.createAbortError();
                this.recordFailure(LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED));
                throw this.error;
            }
            // Initialization is asynchronous, so the stream may have been released while it ran. The new demuxer must be released too,
            // and the caller must see an abort rather than a segment that belongs to a track nobody is playing anymore.
            if (this.destroyed) {
                try {
                    await inner.destroyAsync();
                } catch (destroyError) {
                    console.debug('Audio player: the normalized segment of a released stream could not be released.', destroyError);
                }
                throw this.createAbortError();
            }
            // The segment becomes current only after every check and await succeeded, so a failed load never leaves a half usable inner demuxer in place.
            this.inner = inner;
            this.innerIndex = index;
            return true;
        }

        /**
         * Loads the first segment and prepares the decoder configuration of the normalized stream.
         * @returns {Promise<void>} A promise that completes when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the first segment does not exist or is not in the delivered format.
         */
        async initializeAsync() {
            if (this.destroyed)
                throw this.createAbortError();
            if (this.segmentDurationSeconds <= 0)
                throw this.recordFailure(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The normalized delivery has no segment duration.'));
            const loaded = await this.loadSegmentAsync(0);
            if (!loaded || !this.inner)
                throw this.recordFailure(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The normalized delivery has no segments.'));
            const configuration = this.inner.getConfiguration();
            if (!configuration) {
                const failure = this.recordFailure(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The normalized delivery reported no decoder configuration.'));
                // The wrapper is not returned to the engine after a failed initialization, so the already opened segment and its demuxer are
                // released here instead of waiting for garbage collection.
                await this.destroyAsync();
                throw failure;
            }
            if (this.capabilities && typeof this.capabilities.supportsAudioConfigAsync === 'function') {
                const isSupported = await this.capabilities.supportsAudioConfigAsync(configuration);
                if (!isSupported) {
                    const failure = this.recordFailure(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The browser cannot decode the normalized delivery.', { reason: LuminaAudioPlayer.PlayerErrorReason.CODEC_UNSUPPORTED }));
                    await this.destroyAsync();
                    throw failure;
                }
            }
            this.initialized = true;
        }

        /**
         * Gets the decoder configuration of the normalized stream.
         * @returns {Object|null} The configuration, or null before initialization.
         */
        getConfiguration() {
            return this.inner ? this.inner.getConfiguration() : null;
        }

        /**
         * Gets the duration of the whole normalized stream. The produced duration reported by the server takes precedence over the probed duration
         * of the manifest, because a production can run past the probe.
         * @returns {number|null} The duration in seconds, or null when it is not known.
         */
        getDuration() {
            if (this.downloader && typeof this.downloader.getProducedDurationSeconds === 'function') {
                const producedDuration = Number(this.downloader.getProducedDurationSeconds());
                if (Number.isFinite(producedDuration) && producedDuration > 0)
                    return producedDuration;
            }
            return this.durationInSeconds > 0 ? this.durationInSeconds : null;
        }

        /**
         * Reads the next encoded packet of the normalized stream, moving to the next segment when the current one ends.
         * @returns {Promise<Object|null>} The packet with an absolute timestamp, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When a segment cannot be read.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            // The loop only continues when a segment boundary is crossed; every packet returned to the decoder leaves through the return at the bottom.
            while (!this.destroyed) {
                if (!this.inner) {
                    // This happens after a forced seek load failed or before the first packet. A missing next segment is the end of the production, not an error.
                    const loaded = await this.loadSegmentAsync(this.innerIndex + 1);
                    if (!loaded)
                        return null;
                }
                const packet = await this.inner.readPacketAsync();
                // The index is captured before moving on, because the packet timestamp is relative to the segment that produced it, even after the next segment was loaded.
                const currentIndex = this.innerIndex;
                if (packet === null) {
                    // The current segment is fully drained. Load the following one and continue reading without exposing a null packet
                    // to the decoder, because null means the whole track ended.
                    const loaded = await this.loadSegmentAsync(currentIndex + 1);
                    if (!loaded)
                        return null;
                    continue;
                }
                // Demuxers report timestamps relative to their own segment. Adding the exact start the server indexed places the packet
                // on the shared track timeline, so a boundary does not jump or overlap after transcoding.
                const startMicros = toMicroseconds(this.getSegmentStartSeconds(currentIndex));
                const timestamp = packet.timestamp + startMicros;
                if (this.seekTargetMicros !== null) {
                    // A demuxer that could not reposition by time starts the segment at its first frame. The frames that end
                    // before the target belong to audio the user already skipped, so they are dropped before the decoder sees them.
                    if (timestamp + packet.duration <= this.seekTargetMicros)
                        continue;
                    this.seekTargetMicros = null;
                }
                return {
                    data: packet.data,
                    timestamp: timestamp,
                    duration: packet.duration,
                    type: packet.type
                };
            }
            return null;
        }

        /**
         * Repositions the normalized stream to the segment that covers a time, from the start of that segment.
         * @param {number} seconds - The target position, in seconds.
         * @returns {Promise<number>} Zero, which is not a byte offset: the normalized stream is always repositioned by loading a new segment
         * and dropping the packets before the target, so no container level byte seek is left for the engine to perform.
         * @throws {LuminaAudioPlayer.PlayerError} When the target segment cannot be read.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const value = Number(seconds);
            const requested = Number.isFinite(value) && value > 0 ? value : 0;
            const knownDuration = Number(this.getDuration());
            const target = Number.isFinite(knownDuration) && knownDuration > 0 ? Math.min(requested, knownDuration) : requested;
            const index = this.downloader && typeof this.downloader.findSegmentIndexForTime === 'function'
                ? this.downloader.findSegmentIndexForTime(target)
                : Math.max(0, Math.floor(target / this.segmentDurationSeconds));
            this.seekTargetMicros = toMicroseconds(target);
            let loaded = await this.loadSegmentAsync(index, true);
            if (!loaded && index > 0) {
                // The produced end of the normalized audio can be shorter than the probed duration, so a seek at the very end
                // falls back to the last segment that was actually produced instead of ending the track silently.
                loaded = await this.loadSegmentAsync(index - 1, true);
            }
            if (!loaded || !this.inner)
                return 0;
            // A segment starts at the first packet at or after its boundary, so a target in the gap just before that packet belongs to the
            // previous segment. The exact start the server indexed is known now, so the seek does not lose the frame at the boundary.
            const loadedIndex = this.innerIndex;
            if (this.getSegmentStartSeconds(loadedIndex) > target && loadedIndex > 0) {
                const previousLoaded = await this.loadSegmentAsync(loadedIndex - 1, true);
                if (!previousLoaded)
                    await this.loadSegmentAsync(loadedIndex, true);
            }
            // A segment is at most a few seconds long and is already in memory, so it is always read from its first packet. A segment
            // is an independent container file, and a demuxer that seeds its timeline from a frame number counts that number across
            // the whole track, so an inner seek would double the offset the segment already carries. Reading from the start and
            // dropping the packets that end before the target in readPacketAsync is exact for every container.
            return 0;
        }

        /**
         * Checks whether the stream was initialized and can be read.
         * @returns {boolean} True when the stream is initialized.
         */
        isInitialized() {
            return this.initialized && !this.destroyed && this.error === null;
        }

        /**
         * Checks whether the stream can no longer be read, because it was destroyed or failed.
         * @returns {boolean} True when the stream stopped.
         */
        isStopped() {
            return this.destroyed || this.error !== null;
        }

        /**
         * Checks whether the stream was destroyed.
         * @returns {boolean} True when the stream was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Gets the last failure the stream recorded, or null.
         * @returns {LuminaAudioPlayer.PlayerError|null} The recorded failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Records a failure once and reports it to the caller, so the decode pipeline stops with a typed error.
         * @param {LuminaAudioPlayer.PlayerError} error - The failure to record.
         * @returns {LuminaAudioPlayer.PlayerError} The recorded failure.
         */
        recordFailure(error) {
            if (this.error)
                return this.error;
            const typed = LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED);
            this.error = typed;
            this.initialized = false;
            if (this.onError) {
                try {
                    this.onError(typed);
                } catch (callbackError) {
                    console.error('Audio player: the normalized demux error callback threw.', callbackError);
                }
            }
            return typed;
        }

        /**
         * Creates the typed abort of an intentionally stopped normalized stream.
         * @returns {LuminaAudioPlayer.PlayerError} The abort.
         */
        createAbortError() {
            return new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The normalized segment stream was aborted.');
        }

        /**
         * Ensures the stream was initialized before it is read.
         * @returns {void}
         * @throws {LuminaAudioPlayer.PlayerError} When the stream was destroyed, failed, or was not initialized.
         */
        ensureInitialized() {
            if (this.destroyed)
                throw this.createAbortError();
            if (this.error)
                throw this.error;
            if (!this.initialized)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The normalized segment stream is not initialized.');
        }

        /**
         * Releases the segment demuxer that is open.
         * @returns {Promise<void>} A promise that completes when the stream was released.
         */
        async destroyAsync() {
            if (this.destroyed)
                return;
            this.destroyed = true;
            this.initialized = false;
            if (this.inner) {
                const inner = this.inner;
                this.inner = null;
                try {
                    await inner.destroyAsync();
                } catch (error) {
                    console.debug('Audio player: the normalized segment demuxer could not be released.', error);
                }
            }
        }
    }

    LuminaAudioPlayer.NormalizedSegmentDownloader = NormalizedSegmentDownloader;
    LuminaAudioPlayer.NormalizedSegmentDemuxer = NormalizedSegmentDemuxer;

    /**
     * Creates the downloader of the segments of a normalized track.
     * @param {Object} options - The downloader options, per {@link LuminaAudioPlayer.NormalizedSegmentDownloader}.
     * @returns {LuminaAudioPlayer.NormalizedSegmentDownloader} The downloader.
     */
    LuminaAudioPlayer.createNormalizedSegmentDownloader = function (options) {
        return new NormalizedSegmentDownloader(options);
    };

    /**
     * Creates and initializes the demuxer of a normalized playback manifest.
     * @param {Object} manifest - The playback manifest of the normalized track.
     * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.NormalizedSegmentDemuxer}.
     * @returns {Promise<LuminaAudioPlayer.NormalizedSegmentDemuxer>} The ready demuxer.
     * @throws {LuminaAudioPlayer.PlayerError} When the normalized delivery cannot be read.
     */
    LuminaAudioPlayer.createNormalizedSegmentDemuxer = async function (manifest, options) {
        const demuxer = new NormalizedSegmentDemuxer(Object.assign({}, options, { manifest: manifest }));
        try {
            await demuxer.initializeAsync();
            return demuxer;
        } catch (error) {
            // A partially initialized wrapper still holds segment 0 and its demuxer, so it is released before the failure is reported.
            try {
                await demuxer.destroyAsync();
            } catch (cleanupError) {
                console.debug('Audio player: the partially initialized normalized demuxer could not be released.', cleanupError);
            }
            throw error;
        }
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
