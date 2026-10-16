/**
 * Download engine of the audio player.
 *
 * The player never uses a native audio element, so it fetches the bytes of a track itself and keeps them until the track
 * ends. This module performs that fetch: it issues HTTP range requests against the same origin streaming proxy of the web
 * application, reads the body through a ReadableStream, and stores the bytes in a buffer that the UI and the decode
 * pipeline can query. It produces no audio; it only brings the bytes into the page.
 *
 * What it guarantees
 * ------------------
 *   - It handles both a 200 (the whole resource) and a 206 (a partial response) answer, so a server that ignores the
 *     range header still ends up with a correct buffer.
 *   - Overlapping and adjacent ranges are merged, so a byte that was already downloaded is never downloaded twice, and
 *     seeking back into an already downloaded range does not fetch it again.
 *   - It never discards bytes: seeking to a new position keeps everything that was already downloaded and continues from
 *     the requested position. The buffer is only released when the track ends (destroyAsync).
 *   - It supports open ended ranges ('bytes=start-'), bounded ranges ('bytes=start-end') and suffix ranges ('bytes=-n').
 *   - It supports abort and resume: an in flight request is cancelled through an AbortController, and resuming issues a
 *     new request from the first byte that is still missing, so no work is lost.
 *   - It maps downloaded byte positions to time through the manifest duration, so the fraction it reports is
 *     time accurate and can drive the streamed indicator of the player bar directly.
 *
 * Continue while paused
 * ---------------------
 * The player keeps downloading while playback is paused only when the user turned the 'keep downloading while paused'
 * setting on. The downloader therefore tracks two independent inputs: whether playback is paused (setPlaybackPaused) and
 * the setting itself (setKeepDownloadingWhilePaused). When playback is paused and the setting is off, the in flight
 * request is aborted and the download stops; when the setting is on, the download runs to the end of the file regardless
 * of the paused state. The setting is also read from the player event bus, so a preference that changes mid track is
 * honored without the caller having to push it again.
 *
 * Using it
 * --------
 * The engine of a track creates the downloader with the stream path and the duration of the playback manifest, seeds the
 * 'keep downloading while paused' setting from the player preferences, wires the progress callback to host.reportProgress,
 * and drives the paused state from its own play and pause methods:
 *
 *   const preferences = host.getPreferences();
 *   const downloader = LuminaAudioPlayer.createTrackDownloader({
 *       url: manifest.streamPath,
 *       durationInSeconds: manifest.durationInSeconds,
 *       shouldKeepDownloadingWhilePaused: preferences.shouldKeepDownloadingWhilePaused,
 *       events: host.events,
 *       onProgress: (progress) => host.reportProgress(progress.downloadedFraction),
 *       onError: (error) => host.reportError(error)
 *   });
 *   // Fire and forget: the promise settles only when the loop stops, which is when the whole file is downloaded, the region
 *   // from the target on is fully buffered, playback is paused with keep-downloading off, a failure or a no-progress
 *   // response stops it, or it is aborted or destroyed. Awaiting it in loadAsync would stall the engine, and the first
 *   // audio, until the entire file is in the buffer. Observe progress and completion through the callbacks.
 *   downloader.startAsync();
 *
 * The option is the creation-time snapshot and the event is the update. The player starts loading the stored preferences
 * without awaiting the request, so a play that lands before that load resolves creates the engine, and its downloader,
 * first; the engine therefore seeds the value it holds at that moment through getPreferences, and the subscription is what
 * makes a change that arrives afterwards, including the load that finishes late, win. From then on PREFERENCES_CHANGED
 * (and setKeepDownloadingWhilePaused) override the seed, so a preference that changes mid track is applied without
 * recreating the downloader.
 *
 * Reporting a failure
 * -------------------
 * The downloader is a sub component, so it never moves the player state machine and never emits on the event bus itself.
 * A stream or configuration failure is reported to the onError callback as a {@link LuminaAudioPlayer.PlayerError}
 * carrying {@link LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED}, and kept on the downloader for polling through
 * getError. The engine must forward that callback to <c>host.reportError(error)</c>, exactly as above, so the failure
 * reaches the player and the event bus; an engine that wires only onProgress leaves the failure unobserved.
 *
 * An intentional stop is not a failure: abort and destroyAsync never call onError and never set getError. They only reject
 * the reads that are waiting for a window, with {@link LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED}, so a seek
 * teardown is never mistaken for a stream failure. A read therefore resolves once the buffer covers its window, and is
 * rejected when the window can no longer be filled: on a destroy or an abort (DOWNLOAD_ABORTED), with the recorded failure
 * when the loop stopped after a failure, or with DOWNLOAD_FAILED and <c>details.reason === 'window-unavailable'</c> when
 * the file completed or the loop ended with no gap left to fill. A read that is waiting while the download is merely
 * paused, or before it has started, is left pending, because a resume or a start can still fill it. The loop callbacks and
 * getError only ever see DOWNLOAD_FAILED.
 *
 * Rejection policy
 * ----------------
 * One rule, everywhere in this module: the control surface never throws and never rejects. startAsync and destroyAsync
 * return a promise that only ever resolves; seek, setPlaybackPaused, setKeepDownloadingWhilePaused and abort are
 * synchronous and return nothing. A failure reaches a caller through the error callback and getError, never through a
 * rejected control promise. The awaited reads are the exception and do reject: readBytesAsync and whenRangeAvailableAsync
 * reject with a typed error when the window can never arrive. A caller awaits a read to get an error, and observes the
 * loop through its callbacks, never by catching its promise.
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
     * Parses the total byte count of a Content-Length header.
     * @param {string|null} value - The header value.
     * @returns {number|null} The length in bytes, or null when it is absent or malformed.
     */
    function parseContentLength(value) {
        if (!value)
            return null;
        const length = Number(value);
        return Number.isInteger(length) && length >= 0 ? length : null;
    }

    /**
     * Parses a Content-Range header of the form 'bytes start-end/total', where the total may be '*' when the server does
     * not know it.
     * @param {string|null} value - The header value.
     * @returns {{ start: number, end: number, total: number|null }|null} The parsed range, or null when the header is absent or malformed.
     */
    function parseContentRange(value) {
        if (!value)
            return null;
        const match = /^bytes\s+(\d+)-(\d+)\/(\d+|\*)$/i.exec(value.trim());
        if (!match)
            return null;
        const start = Number(match[1]);
        const end = Number(match[2]);
        const total = match[3] === '*' ? null : Number(match[3]);
        if (!Number.isInteger(start) || !Number.isInteger(end) || start > end)
            return null;
        return { start, end, total };
    }

    /**
     * Checks whether a value is the AbortError a cancelled fetch or stream read rejects with. It is tested by name rather
     * than by type, because in a browser an AbortError is a DOMException, which is not an Error.
     * @param {*} error - The value to check.
     * @returns {boolean} True when the value is an AbortError.
     */
    function isAbortError(error) {
        return !!error && error.name === 'AbortError';
    }

    /**
     * Checks whether a Content-Type is one the track stream may answer with: an audio type, or the generic binary type
     * the stream endpoint falls back to for a container it cannot name. Any other type, in particular the text/html of a
     * sign in page, is not the track.
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
     * A byte buffer that stores downloaded chunks as merged, contiguous segments.
     *
     * A segment is a contiguous run of downloaded bytes, held as the list of chunks it was assembled from. Chunks are kept
     * by reference, never re-copied, so a sequential download appends in constant time and the memory cost is the file
     * itself. A chunk that is already covered adds nothing, a chunk that is only partly new contributes just its new bytes,
     * and touching runs are merged, so segment boundaries are exactly the gaps in the download, which makes the range
     * reporting and the byte count exact. It is the structure the UI queries for the downloaded ranges and the fraction,
     * and the structure the decode pipeline reads bytes from.
     */
    class ByteBufferStore {
        /**
         * Creates an instance of ByteBufferStore.
         */
        constructor() {
            /** @type {Array<{ start: number, length: number, chunks: Array<Uint8Array> }>} */
            this.segments = [];
            /** @type {{ segmentIndex: number, chunkIndex: number, chunkStart: number }|null} */
            this.readHint = null;
        }

        /**
         * Appends a chunk at the given byte offset, keeping only the bytes that are not stored yet and merging them with
         * the segments they touch.
         *
         * A sequential download, the common case, extends the last segment in constant time. A chunk that is fully covered
         * by the buffer adds nothing and is dropped without a copy, and a chunk that is only partly new contributes just its
         * new sub-ranges. There is therefore no path that copies a whole downloaded span, which matters when a server
         * answers a range request with the entire resource while the buffer already holds a large prefix.
         * @param {number} offset - The byte offset the chunk starts at.
         * @param {Uint8Array} bytes - The chunk to store.
         */
        append(offset, bytes) {
            if (!bytes || bytes.byteLength === 0)
                return;
            const start = Math.max(0, Math.floor(offset));
            const end = start + bytes.byteLength;
            const last = this.segments[this.segments.length - 1];
            if (last && last.start + last.length === start) {
                last.chunks.push(bytes);
                last.length += bytes.byteLength;
                return;
            }
            for (const range of this.subtract(start, end))
                this.insertSegment(range.start, bytes.subarray(range.start - start, range.end - start));
        }

        /**
         * Computes the sub-ranges of a span that are not stored yet.
         * @param {number} start - The first byte of the span.
         * @param {number} end - The exclusive end of the span.
         * @returns {Array<{ start: number, end: number }>} The uncovered sub-ranges, in ascending order.
         */
        subtract(start, end) {
            // Reducing a chunk to only its new bytes is what keeps an overlapping append cheap: the buffer never has to copy
            // or rebuild the span a refill overlaps, because the already stored part is simply not handed on.
            const result = [];
            let position = start;
            for (const segment of this.segments) {
                const segmentEnd = segment.start + segment.length;
                if (segmentEnd <= position)
                    continue;
                if (segment.start >= end)
                    break;
                if (segment.start > position)
                    result.push({ start: position, end: Math.min(segment.start, end) });
                position = Math.max(position, segmentEnd);
                if (position >= end)
                    break;
            }
            if (position < end)
                result.push({ start: position, end });
            return result;
        }

        /**
         * Inserts a run of bytes that is known not to overlap any stored byte, merging it with the segments it touches.
         * @param {number} start - The first byte of the run.
         * @param {Uint8Array} bytes - The bytes of the run.
         */
        insertSegment(start, bytes) {
            // The run is known to be new, so merging only concatenates chunk references; no byte is copied into a new buffer.
            const end = start + bytes.byteLength;
            const kept = [];
            const parts = [{ start, length: bytes.byteLength, chunks: [bytes] }];
            let mergedStart = start;
            let mergedEnd = end;
            for (const segment of this.segments) {
                const segmentEnd = segment.start + segment.length;
                // Touching segments are merged as well: the comparison is strict, so a segment that ends exactly where
                // another begins joins the run instead of leaving a one byte gap.
                if (segmentEnd < mergedStart || segment.start > mergedEnd) {
                    kept.push(segment);
                    continue;
                }
                mergedStart = Math.min(mergedStart, segment.start);
                mergedEnd = Math.max(mergedEnd, segmentEnd);
                parts.push(segment);
            }
            parts.sort((left, right) => left.start - right.start);
            // The parts are non-overlapping by construction, so their chunks concatenate in offset order into the run.
            const chunks = [];
            for (const part of parts)
                for (const chunk of part.chunks)
                    chunks.push(chunk);
            kept.push({ start: mergedStart, length: mergedEnd - mergedStart, chunks });
            kept.sort((left, right) => left.start - right.start);
            this.segments = kept;
            this.readHint = null;
        }

        /**
         * Lists the downloaded byte ranges, as half open intervals.
         * @returns {Array<{ start: number, end: number }>} The downloaded ranges, in ascending order.
         */
        getRanges() {
            return this.segments.map((segment) => ({ start: segment.start, end: segment.start + segment.length }));
        }

        /**
         * Gets the number of distinct bytes held by the buffer.
         * @returns {number} The byte count.
         */
        getByteCount() {
            let count = 0;
            for (const segment of this.segments)
                count += segment.length;
            return count;
        }

        /**
         * Checks whether the whole window is already stored, without copying its bytes.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The length of the window, in bytes.
         * @returns {boolean} True when the window is fully stored.
         */
        covers(offset, length) {
            const start = Math.max(0, Math.floor(offset));
            const size = Math.max(0, Math.floor(length));
            if (size === 0)
                return true;
            for (const segment of this.segments) {
                const segmentEnd = segment.start + segment.length;
                if (start < segment.start)
                    return false;
                if (start < segmentEnd)
                    return start + size <= segmentEnd;
            }
            return false;
        }

        /**
         * Copies a window of bytes out of the buffer.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The length of the window, in bytes.
         * @returns {Uint8Array|null} A copy of the bytes, or null when any byte of the window is missing.
         */
        read(offset, length) {
            const start = Math.max(0, Math.floor(offset));
            const size = Math.max(0, Math.floor(length));
            if (size === 0)
                return new Uint8Array(0);
            let segmentIndex = -1;
            for (let index = 0; index < this.segments.length; index++) {
                const candidate = this.segments[index];
                if (start < candidate.start)
                    return null;
                if (start < candidate.start + candidate.length) {
                    if (start + size > candidate.start + candidate.length)
                        return null;
                    segmentIndex = index;
                    break;
                }
            }
            if (segmentIndex === -1)
                return null;
            const segment = this.segments[segmentIndex];
            const local = start - segment.start;
            const result = new Uint8Array(size);
            let filled = 0;
            let position = 0;
            let chunkIndex = 0;
            // A sequential reader continues where the previous read stopped, so the chunk search resumes at that chunk
            // instead of walking the whole segment again.
            if (this.readHint && this.readHint.segmentIndex === segmentIndex && this.readHint.chunkStart <= local) {
                chunkIndex = this.readHint.chunkIndex;
                position = this.readHint.chunkStart;
            }
            for (; chunkIndex < segment.chunks.length; chunkIndex++) {
                const chunk = segment.chunks[chunkIndex];
                if (position + chunk.byteLength <= local) {
                    position += chunk.byteLength;
                    continue;
                }
                const from = Math.max(0, local - position);
                const take = Math.min(chunk.byteLength - from, size - filled);
                result.set(chunk.subarray(from, from + take), filled);
                filled += take;
                if (filled >= size) {
                    this.readHint = { segmentIndex, chunkIndex, chunkStart: position };
                    return result;
                }
                position += chunk.byteLength;
            }
            return null;
        }

        /**
         * Finds the first byte at or after an offset that is not stored yet.
         * @param {number} offset - The byte to start from.
         * @param {number|null} limit - The exclusive upper bound, or null when it is unknown.
         * @returns {number|null} The first missing byte, or null when every byte up to the limit is stored.
         */
        findFirstGap(offset, limit) {
            // This is the loop's scheduling primitive: it answers both "which byte do I fetch next" (a number) and "is there
            // anything left to fetch" (null). The limit is the known end of the file; with it the search can prove there is
            // no gap left, while a null limit (an unknown total) can only ever point at the next byte to try.
            let position = Math.max(0, Math.floor(offset));
            for (const segment of this.segments) {
                const segmentEnd = segment.start + segment.length;
                if (segmentEnd <= position)
                    continue;
                if (segment.start > position)
                    return position;
                // The position is inside this segment, so the search continues at its end.
                position = segmentEnd;
                if (limit !== null && position >= limit)
                    return null;
            }
            if (limit !== null && position >= limit)
                return null;
            return position;
        }

        /**
         * Gets the end of the contiguous run of stored bytes that starts at or after an offset.
         * @param {number} offset - The byte to start from.
         * @returns {number} The exclusive end of the contiguous run.
         */
        getContiguousEnd(offset) {
            let position = Math.max(0, Math.floor(offset));
            for (const segment of this.segments) {
                const segmentEnd = segment.start + segment.length;
                if (segmentEnd <= position)
                    continue;
                if (segment.start > position)
                    break;
                position = Math.max(position, segmentEnd);
            }
            return position;
        }

        /**
         * Gets the exclusive end of the last stored byte. It is the end of the file only when an open ended read proved it
         * (streamComplete); otherwise it is merely the end of what has been downloaded so far.
         * @returns {number} The exclusive end of the stored data, or zero when nothing is stored.
         */
        getEnd() {
            if (this.segments.length === 0)
                return 0;
            const last = this.segments[this.segments.length - 1];
            return last.start + last.length;
        }

        /**
         * Releases every stored byte.
         */
        clear() {
            this.segments = [];
            this.readHint = null;
        }
    }

    /**
     * Downloads a single track into a buffer, with range support, abort and resume, and time accurate progress.
     */
    class TrackDownloader {
        /**
         * Creates an instance of TrackDownloader.
         * @param {Object} options - The downloader options.
         * @param {string} options.url - The stream path of the track, relative to the page or absolute on the same origin. Nothing is fetched when it is empty.
         * @param {number} options.durationInSeconds - The duration of the track, in seconds, from the playback manifest. It maps byte positions to time.
         * @param {number} [options.totalBytes] - The known size of the file, in bytes. It is discovered from the first response when omitted.
         * @param {Function} [options.fetch] - The fetch implementation to use, for injection in a test. Defaults to window.fetch.
         * @param {string} [options.credentials] - The credentials mode of the request. Defaults to 'same-origin', so the session cookie is sent.
         * @param {Object} [options.events] - The player event bus. When provided, the 'keep downloading while paused' preference is kept current from it.
         * @param {boolean} [options.shouldKeepDownloadingWhilePaused] - The value of the 'keep downloading while paused' setting, seeded by the engine from host.getPreferences(). It is the initial value; later PREFERENCES_CHANGED events and setKeepDownloadingWhilePaused override it.
         * @param {Function} [options.onProgress] - Called with the progress snapshot every time the buffer grows.
         * @param {Function} [options.onComplete] - Called once when every byte of the file has been downloaded.
         * @param {Function} [options.onError] - Called with a {@link LuminaAudioPlayer.PlayerError} when the download fails.
         */
        constructor(options = {}) {
            this.url = typeof options.url === 'string' ? options.url : '';
            this.duration = Number.isFinite(Number(options.durationInSeconds)) && Number(options.durationInSeconds) > 0
                ? Number(options.durationInSeconds)
                : 0;
            this.buffer = new ByteBufferStore();
            this.fetchImpl = typeof options.fetch === 'function'
                ? options.fetch
                : (typeof window !== 'undefined' && typeof window.fetch === 'function' ? window.fetch.bind(window) : null);
            this.credentials = typeof options.credentials === 'string' && options.credentials !== '' ? options.credentials : 'same-origin';
            this.events = options.events || null;
            this.shouldKeepDownloadingWhilePaused = options.shouldKeepDownloadingWhilePaused === true;
            this.onProgress = typeof options.onProgress === 'function' ? options.onProgress : null;
            this.onComplete = typeof options.onComplete === 'function' ? options.onComplete : null;
            this.onError = typeof options.onError === 'function' ? options.onError : null;
            this.totalBytes = Number.isInteger(options.totalBytes) && options.totalBytes >= 0 ? options.totalBytes : null;
            this.targetByte = 0;
            this.pendingSeekSeconds = null;
            this.isPlaybackPaused = false;
            this.isAborted = false;
            // Boolean state keeps its conventional is/has prefix, except where a public is*() accessor already owns that name:
            // isDestroyed() is that accessor here, so the backing field keeps the participle instead of shadowing the method.
            this.destroyed = false;
            this.isFinished = false;
            // True once the loop stopped with no gap left to fill from the target and without completing the file: from
            // then on, the loop can no longer append anything for the current target, so a reader waiting for a window that
            // is not covered can never be filled. It is cleared whenever a new loop starts.
            this.isDrained = false;
            // True once an open ended read both delivered its whole declared range and reached the end of its body, which is
            // what proves the end of the file when the server never reported a total size.
            this.isStreamComplete = false;
            // True when the downloader can never fetch, because it has no url or no fetch implementation, so a start does
            // not keep re-reporting the same configuration failure.
            this.isUnusable = false;
            this.error = null;
            // A downloader may have one loop request and, at most, an explicit one shot range request in flight at the same
            // time, so every controller is tracked and an abort or a destroy cancels all of them, not just the newest.
            this.abortControllers = new Set();
            this.loopPromise = null;
            this.waiters = [];
            this.unsubscribePreferences = null;
            this.subscribeToPreferences();
        }

        //+======================================================================================+
        //|                                   Control surface                                    |
        //+======================================================================================+

        /**
         * Starts, or continues, the download loop from the current target position.
         *
         * The loop keeps downloading ahead until the whole file is stored, playback is paused with the 'keep downloading
         * while paused' setting off, or the downloader is aborted or destroyed. Bytes that are already stored are never
         * fetched again, so a call after a seek only fetches what is still missing.
         *
         * A caller starts it without awaiting it: the returned promise settles only when the loop stops, which can be the
         * end of the whole track. Progress and completion are observed through the callbacks.
         * @param {Object} [options] - The optional starting position.
         * @param {number} [options.fromByte] - A byte position to start from, overriding the current target.
         * @param {number} [options.fromSeconds] - A time position to start from, overriding the current target. It is mapped to a byte through the duration.
         * @returns {Promise<void>} A promise that resolves when the loop stops. It never rejects; a failure is reported through the error callback and getError.
         */
        startAsync(options = {}) {
            // A destroyed downloader, or one that can never fetch (no url, no fetch implementation), resolves without
            // starting a loop, so a reconcile does not keep re-reporting the same configuration failure.
            if (this.destroyed || this.isUnusable)
                return Promise.resolve();
            this.isAborted = false;
            let isRepositioned = false;
            if (Number.isFinite(Number(options.fromByte))) {
                this.targetByte = Math.max(0, Math.floor(Number(options.fromByte)));
                // A new target makes a previous drain stale, exactly as a seek does.
                this.isDrained = false;
                isRepositioned = true;
            } else if (Number.isFinite(Number(options.fromSeconds))) {
                this.setTargetFromSeconds(Number(options.fromSeconds));
                isRepositioned = true;
            }
            if (this.loopPromise) {
                // The loop re-evaluates the target at every iteration, so aborting the in flight request makes it pivot to
                // the new position right away instead of waiting for the current response to reach its end.
                if (isRepositioned)
                    this.cancelActiveRequest();
                return this.loopPromise;
            }
            // A restart clears the previous failure, so getError does not report a stale one after a successful retry, and it
            // clears the drained state, because the new loop may fill windows the previous one left alone.
            this.error = null;
            this.isDrained = false;
            const loop = this.runLoopAsync();
            this.loopPromise = loop;
            // The handle is cleared only while it still points at this loop, so a retry started from a callback (onError or
            // onComplete) keeps its own handle instead of having it nulled by the loop that just stopped.
            loop.finally(() => {
                if (this.loopPromise === loop)
                    this.loopPromise = null;
            });
            return loop;
        }

        /**
         * Records a playback position and downloads from it when the download loop is allowed to run.
         *
         * It never discards stored bytes: a position inside an already downloaded range continues from the first missing
         * byte after it, so seeking back into downloaded data never fetches it again. A seek over an incomplete download
         * aborts the in flight request, so the loop fetches the new position first instead of finishing the bytes it was
         * already reading.
         * @param {number} seconds - The playback position, in seconds.
         */
        seek(seconds) {
            this.setTargetFromSeconds(seconds);
            if (!this.shouldKeepRunning())
                return;
            if (this.loopPromise)
                this.cancelActiveRequest();
            else
                this.startAsync();
        }

        /**
         * Maps a playback position to a byte position, remembering it for later when the total size is not known yet.
         * @param {number} seconds - The playback position, in seconds.
         */
        setTargetFromSeconds(seconds) {
            const position = Math.max(0, Number(seconds) || 0);
            if (this.duration > 0 && this.totalBytes !== null) {
                this.targetByte = Math.round(clamp(position / this.duration, 0, 1) * this.totalBytes);
                this.pendingSeekSeconds = null;
            } else {
                // The size of the file is not known yet, so the position is remembered and mapped once the first response reveals it.
                this.pendingSeekSeconds = position;
            }
            // A reposition makes the previous drain stale: the target moved, so a window that the previous drain could not
            // fill may now be the very window a resume or a later start fetches. A read must not be rejected against it.
            this.isDrained = false;
        }

        /**
         * Sets whether playback is paused, and reconciles the download loop with the 'keep downloading while paused' setting.
         * @param {boolean} paused - True when playback is paused, false when it is playing.
         */
        setPlaybackPaused(paused) {
            const next = paused === true;
            if (next === this.isPlaybackPaused)
                return;
            this.isPlaybackPaused = next;
            this.reconcile();
        }

        /**
         * Sets the 'keep downloading while paused' setting, and reconciles the download loop.
         * @param {boolean} value - True to keep downloading while playback is paused, false to stop while paused.
         */
        setKeepDownloadingWhilePaused(value) {
            const next = value === true;
            if (next === this.shouldKeepDownloadingWhilePaused)
                return;
            this.shouldKeepDownloadingWhilePaused = next;
            this.reconcile();
        }

        /**
         * Aborts the in flight requests and stops the loop, keeping every byte that was already downloaded. It is
         * synchronous, like the other immediate controls, and it never returns a promise.
         *
         * The downloader stays usable: a later startAsync resumes from the first missing byte, because the abort only
         * cancels the requests, never the buffer. destroyAsync is the operation that releases the buffer.
         */
        abort() {
            this.isAborted = true;
            this.cancelActiveRequest();
            // The download is stopping and no byte will append until it is started again, so a reader that is waiting for a
            // byte window is released with the aborted code instead of waiting forever.
            this.rejectWaiters(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The track download was aborted.'));
        }

        /**
         * Releases every resource of the downloader: it aborts the in flight request, clears the buffer, and stops
         * listening to the preference bus. It is used when the track ends, never when playback merely pauses.
         * @returns {Promise<void>} A promise that resolves when the loop has stopped.
         */
        async destroyAsync() {
            this.destroyed = true;
            this.isAborted = true;
            this.cancelActiveRequest();
            if (this.unsubscribePreferences) {
                this.unsubscribePreferences();
                this.unsubscribePreferences = null;
            }
            this.rejectWaiters(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The track download was destroyed.'));
            if (this.loopPromise) {
                try {
                    await this.loopPromise;
                } catch (error) {
                    // The loop reports its own failures; a teardown must not surface one.
                }
            }
            this.buffer.clear();
        }

        //+======================================================================================+
        //|                                   Query surface                                      |
        //+======================================================================================+

        /**
         * Gets the downloaded byte ranges, as half open intervals. This is the structure the UI queries to draw the
         * downloaded parts of a progress bar.
         * @returns {Array<{ start: number, end: number }>} The downloaded ranges, in ascending order.
         */
        getRanges() {
            return this.buffer.getRanges();
        }

        /**
         * Gets the downloaded time ranges, mapped through either the known total size and the duration or, when the size
         * is not known yet, through the bytes themselves once the whole file is in the buffer.
         * @returns {Array<{ start: number, end: number }>} The downloaded time ranges, in seconds.
         */
        getTimeRanges() {
            // The duration and the total size give a single average rate, so a byte position maps to a time by proportion. A
            // variable bit rate makes this an approximation, but it stays monotonic and it is all the manifest offers, which
            // is why the streamed indicator is described as time accurate rather than exact.
            const total = this.resolveTotalForMapping();
            if (total === null || this.duration <= 0)
                return [];
            return this.buffer.getRanges().map((range) => ({
                start: (range.start / total) * this.duration,
                end: (range.end / total) * this.duration
            }));
        }

        /**
         * Gets the fraction of the track that has been downloaded, between 0 and 1.
         *
         * The fraction is time accurate because a byte position maps linearly to a time through the known total size and
         * the manifest duration.
         * @returns {number} The downloaded fraction.
         */
        getDownloadedFraction() {
            const total = this.resolveTotalForMapping();
            if (total !== null && total > 0)
                return clamp(this.buffer.getByteCount() / total, 0, 1);
            return this.isComplete() ? 1 : 0;
        }

        /**
         * Gets the time, in seconds, up to which the track has been downloaded contiguously from its start. It is the
         * value a single handed streamed indicator shows, while getTimeRanges is what a segmented bar uses.
         * @returns {number} The contiguous downloaded time, in seconds.
         */
        getDownloadedSeconds() {
            const total = this.resolveTotalForMapping();
            if (total === null || total <= 0 || this.duration <= 0)
                return 0;
            const end = this.buffer.getContiguousEnd(0);
            return clamp((end / total) * this.duration, 0, this.duration);
        }

        /**
         * Gets the number of distinct bytes in the buffer.
         * @returns {number} The downloaded byte count.
         */
        getDownloadedBytes() {
            return this.buffer.getByteCount();
        }

        /**
         * Gets the total size of the file, in bytes, once it is known.
         * @returns {number|null} The total size, or null when the first response has not revealed it yet.
         */
        getTotalBytes() {
            return this.totalBytes;
        }

        /**
         * Gets the duration the downloader maps bytes to, in seconds.
         * @returns {number} The duration, in seconds, or zero when it is unknown.
         */
        getDuration() {
            return this.duration;
        }

        /**
         * Checks whether the whole file is stored.
         * @returns {boolean} True when every byte of the file is in the buffer.
         */
        isComplete() {
            if (this.isFinished)
                return true;
            if (this.totalBytes !== null && this.totalBytes > 0)
                return this.buffer.findFirstGap(0, this.totalBytes) === null;
            // The total size was never reported, so the file is complete only when an open ended read reached the end of
            // the body and the buffer covers a single contiguous run that starts at the beginning of the file.
            return this.isStreamComplete && this.buffer.segments.length === 1 && this.buffer.segments[0].start === 0;
        }

        /**
         * Checks whether the downloader was destroyed.
         * @returns {boolean} True when the downloader was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Gets the last failure, or null when there was none.
         * @returns {LuminaAudioPlayer.PlayerError|null} The last failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Builds a snapshot of the download progress.
         * @returns {Object} The progress snapshot, with the downloaded byte count, the total size, the downloaded fraction, the downloaded time ranges, and the contiguous downloaded seconds.
         */
        getProgress() {
            return Object.freeze({
                downloadedBytes: this.buffer.getByteCount(),
                totalBytes: this.totalBytes,
                downloadedFraction: this.getDownloadedFraction(),
                downloadedSeconds: this.getDownloadedSeconds(),
                durationInSeconds: this.duration,
                timeRanges: this.getTimeRanges()
            });
        }

        /**
         * Reads a window of bytes out of the buffer, waiting for it when it has not been downloaded yet.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The length of the window, in bytes.
         * @returns {Promise<Uint8Array>} A copy of the requested bytes.
         * @throws {LuminaAudioPlayer.PlayerError} The rejection of {@link LuminaAudioPlayer.TrackDownloader#whenRangeAvailableAsync}, which a read propagates, or <c>DOWNLOAD_FAILED</c> with <c>details.reason === 'window-unavailable'</c> if the window was released without ever arriving.
         */
        async readBytesAsync(offset, length) {
            const direct = this.buffer.read(offset, length);
            if (direct !== null)
                return direct;
            await this.whenRangeAvailableAsync(offset, length);
            const resolved = this.buffer.read(offset, length);
            if (resolved === null)
                throw this.createWindowUnavailableError();
            return resolved;
        }

        /**
         * Resolves once a window of bytes has been downloaded.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The length of the window, in bytes.
         * @returns {Promise<void>} A promise that resolves once the buffer covers the window. It is rejected when the window can no longer be filled: on a destroy or an abort, with the recorded failure when the loop stopped after a failure, or with the window-unavailable error when the file completed or the loop ended with no gap left to fill. A read issued while the download is merely paused, or before it has started, stays pending, because a resume or a start can still fill it.
         * @throws {LuminaAudioPlayer.PlayerError} With <c>DOWNLOAD_ABORTED</c> when the downloader is destroyed or was aborted, with the recorded failure when the loop stopped after a failure, or with <c>DOWNLOAD_FAILED</c> and <c>details.reason === 'window-unavailable'</c> when the file completed or the loop ended with no gap left to fill.
         */
        whenRangeAvailableAsync(offset, length) {
            if (this.buffer.covers(offset, length))
                return Promise.resolve();
            if (this.destroyed)
                return Promise.reject(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The track download was destroyed.'));
            // A window that is not buffered and a download that is intentionally stopped or has failed can never be filled,
            // so the read is rejected now instead of waiting forever.
            if (this.isAborted)
                return Promise.reject(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The track download was aborted.'));
            if (this.error && !this.loopPromise)
                return Promise.reject(this.error);
            // A completed file, or a loop that stopped with nothing left to fetch, can never fill an uncovered window, so the
            // read is rejected instead of waiting. A pause leaves both unset, so a read issued during a pause still waits.
            if ((this.isFinished || this.isDrained) && !this.loopPromise)
                return Promise.reject(this.createWindowUnavailableError());
            return new Promise((resolve, reject) => {
                this.waiters.push({ offset: Math.max(0, Math.floor(offset)), length: Math.max(0, Math.floor(length)), resolve, reject });
            });
        }

        //+======================================================================================+
        //|                                   Range requests                                     |
        //+======================================================================================+

        /**
         * Downloads a bounded byte range and stores it. It never throws: a failure is returned as <c>{ isFailed: true, error }</c>
         * and, unlike the download loop, it never runs the complete or error callbacks, because it is an explicit request
         * whose outcome the caller inspects.
         * @param {number} start - The first byte of the range.
         * @param {number} end - The last byte of the range, inclusive, as HTTP defines the end of a Range header.
         * @returns {Promise<Object>} The outcome, per {@link LuminaAudioPlayer.TrackDownloader#performFetchAsync}.
         */
        downloadRangeAsync(start, end) {
            return this.performFetchAsync({ start, end });
        }

        /**
         * Downloads an open ended byte range, from a position to the end of the file, and stores it. It never throws: a
         * failure is returned as <c>{ isFailed: true, error }</c> and it never runs the complete or error callbacks.
         * @param {number} start - The first byte of the range.
         * @returns {Promise<Object>} The outcome, per {@link LuminaAudioPlayer.TrackDownloader#performFetchAsync}.
         */
        downloadOpenEndedAsync(start) {
            return this.performFetchAsync({ start, openEnded: true });
        }

        /**
         * Downloads a suffix byte range, the last bytes of the file, and stores it. It never throws: a failure is returned
         * as <c>{ isFailed: true, error }</c> and it never runs the complete or error callbacks.
         * @param {number} length - The number of trailing bytes to request.
         * @returns {Promise<Object>} The outcome, per {@link LuminaAudioPlayer.TrackDownloader#performFetchAsync}.
         */
        downloadSuffixAsync(length) {
            return this.performFetchAsync({ suffix: length });
        }

        //+======================================================================================+
        //|                                   Internal helpers                                   |
        //+======================================================================================+

        /**
         * Subscribes to the preference changes of the player, so that a 'keep downloading while paused' setting that
         * changes mid track is applied without the caller pushing it again. The initial value is never delivered by this
         * subscription, because the player loads the stored preferences before any downloader exists, so the engine seeds
         * it through the constructor option.
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
         * Reconciles the download loop with the current paused state and setting: it starts the loop when the download is
         * allowed to run, and aborts the in flight request when it is not.
         */
        reconcile() {
            if (this.destroyed)
                return;
            if (this.shouldKeepRunning())
                this.startAsync();
            else
                this.cancelActiveRequest();
        }

        /**
         * Checks whether the download loop is allowed to run right now.
         * @returns {boolean} True when the loop may run.
         */
        shouldKeepRunning() {
            if (this.destroyed || this.isAborted)
                return false;
            if (this.isPlaybackPaused)
                return this.shouldKeepDownloadingWhilePaused;
            return true;
        }

        /**
         * Cancels every in flight request through its AbortController. A pause, a pivot, an abort and a destroy all route
         * here, so no request, and no connection, is left open behind one that another controller replaced.
         */
        cancelActiveRequest() {
            if (this.abortControllers.size === 0)
                return;
            for (const controller of [...this.abortControllers]) {
                try {
                    controller.abort();
                } catch (error) {
                    console.debug('Audio player: the in flight download request could not be aborted.', error);
                }
            }
        }

        /**
         * Runs the download loop: it fetches from the first missing byte at or after the target until the loop is no
         * longer allowed to run, or the whole file is stored.
         * @returns {Promise<void>} A promise that resolves when the loop stops.
         */
        async runLoopAsync() {
            // The body is deferred past a microtask so that it can never run to completion (drain, fail or complete) inside
            // the synchronous prologue of startAsync, before startAsync has taken the handle. Without this a loop that ends
            // at once would let startAsync re-arm its handle after the loop already released its readers, reopening a window
            // in which a read waits for a loop that has finished. Awaiting a resolved promise, rather than a bare value,
            // keeps the microtask yield without the "await has no effect on the type" hint an IDE raises for `await null`.
            await Promise.resolve();
            if (this.fetchImpl === null) {
                // A missing fetch implementation is a permanent configuration failure, so the downloader is marked unusable
                // and later starts resolve without re-reporting it.
                this.isUnusable = true;
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'This environment provides no fetch implementation for the player download.'));
                return;
            }
            if (this.url === '') {
                this.isUnusable = true;
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The play descriptor carries no stream path.'));
                return;
            }
            let hasDrained = false;
            try {
                while (this.shouldKeepRunning()) {
                    // Completion is re-checked every iteration, not only through the gap, because with an unknown total a
                    // gap search against a null limit can never return null: without this the loop would request one byte
                    // past the end, get a 416, and report a failure on a file that is in fact fully buffered.
                    if (this.isComplete())
                        break;
                    const gap = this.buffer.findFirstGap(this.targetByte, this.resolveKnownEnd());
                    if (gap === null) {
                        // There is no gap left to fill from the target, so the loop has nothing more to do for this run.
                        hasDrained = true;
                        break;
                    }
                    const bytesBefore = this.buffer.getByteCount();
                    const result = await this.performFetchAsync({ start: gap, openEnded: true, stopOnPause: true });
                    if (this.destroyed || result.isDestroyed)
                        break;
                    if (result.isFailed) {
                        this.fail(result.error);
                        return;
                    }
                    if (result.isAborted) {
                        // The request was aborted to pivot to a new target while the loop is still allowed to run, so the
                        // loop re-evaluates the target instead of stopping; any other abort is caught by the loop condition.
                        if (this.shouldKeepRunning())
                            continue;
                        break;
                    }
                    // A response that added no byte can fill nothing: either the file is already complete, or the download
                    // stalled. A stall is reported so the loop does not spin and every reader waiting for a window is
                    // released with the failure instead of hanging.
                    if (this.buffer.getByteCount() <= bytesBefore) {
                        if (this.isComplete())
                            break;
                        this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The download made no progress.'));
                        return;
                    }
                }
                // Completion is reported inside the guarded block, so nothing the loop does can reject its promise.
                if (this.destroyed) {
                    // A destroyed downloader releases its own waiters in destroyAsync.
                } else if (this.isComplete()) {
                    this.complete();
                } else if (hasDrained) {
                    // The loop stopped with nothing left to fetch for the current target and without completing the file, so
                    // nothing will append again and a reader waiting for a window that is not covered can never be filled.
                    // The handle is cleared first, so a read issued after this point is rejected at once instead of waiting
                    // for a loop that is no longer running.
                    this.isDrained = true;
                    this.loopPromise = null;
                    this.rejectWaiters(this.createWindowUnavailableError());
                }
            } catch (error) {
                // No failure may escape the loop as a rejected promise, because a caller starts it without awaiting it.
                this.fail(error);
                return;
            }
        }

        /**
         * Performs a single range request, reads its body, and stores the bytes.
         * @param {Object} spec - The range specification.
         * @param {number} [spec.start] - The first byte of the range.
         * @param {number} [spec.end] - The last byte of the range, inclusive.
         * @param {number} [spec.suffix] - The number of trailing bytes to request.
         * @param {boolean} [spec.openEnded] - True to request an open ended range, from start to the end of the file.
         * @param {boolean} [spec.stopOnPause] - True when a paused player must stop this request at the next chunk. The download loop sets it; a one shot range request does not, so an explicit request always completes.
         * @returns {Promise<Object>} The outcome, one of: <c>{ isFailed: true, error }</c> when the request failed, <c>{ isAborted: true }</c> when it was aborted, <c>{ isDestroyed: true }</c> when the downloader was destroyed, <c>{ isSatisfied: true }</c> when the range was already satisfied, or <c>{ isAborted, isFailed: false, isDestroyed, isStopped }</c> for a read that ended. It never rejects.
         */
        async performFetchAsync(spec = {}) {
            if (this.destroyed)
                return { isDestroyed: true, isAborted: false, isFailed: false };
            if (this.fetchImpl === null || this.url === '') {
                this.isUnusable = true;
                return { isFailed: true, error: new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The download request could not be built.') };
            }
            const controller = new AbortController();
            this.abortControllers.add(controller);
            let response = null;
            try {
                response = await this.fetchImpl(this.url, {
                    method: 'GET',
                    headers: { 'Range': this.buildRangeHeader(spec) },
                    credentials: this.credentials,
                    signal: controller.signal
                });
                const resolved = this.resolveResponseRange(response);
                if (resolved === null) {
                    // The requested range is already satisfied, so the response body, which may be a small error document,
                    // is discarded instead of being left open until it is garbage collected.
                    this.discardResponseBody(response);
                    return { isAborted: false, isFailed: false, isDestroyed: this.destroyed, isSatisfied: true };
                }
                this.noteTotal(resolved.total);
                return await this.readResponseIntoBufferAsync(response, resolved, controller, spec.stopOnPause === true, spec.openEnded === true);
            } catch (error) {
                if (isAbortError(error))
                    return { isAborted: true, isFailed: false, isDestroyed: this.destroyed };
                // The failure left the response body unread, so it is discarded and the request is aborted to release the
                // connection instead of waiting for the browser to drop the stream.
                this.discardResponseBody(response);
                try {
                    if (!controller.signal.aborted)
                        controller.abort();
                } catch (abortError) {
                    console.debug('Audio player: the failed download request could not be aborted.', abortError);
                }
                return { isFailed: true, error: LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED) };
            } finally {
                this.abortControllers.delete(controller);
            }
        }

        /**
         * Cancels the body of a response that is not going to be read, so its connection is released right away.
         * @param {Response|null} response - The response whose body is discarded. Nothing is done when it has no body.
         */
        discardResponseBody(response) {
            if (!response || !response.body || typeof response.body.cancel !== 'function')
                return;
            try {
                const cancellation = response.body.cancel();
                // Cancel returns a promise; a rejection is only a stream already being gone, so it is swallowed.
                if (cancellation && typeof cancellation.catch === 'function')
                    cancellation.catch(() => {});
            } catch (error) {
                console.debug('Audio player: the discarded download response body could not be cancelled.', error);
            }
        }

        /**
         * Builds the Range header of a request.
         * @param {Object} spec - The range specification.
         * @returns {string} The Range header value.
         */
        buildRangeHeader(spec) {
            // A suffix wins over an explicit span, and an open ended request ignores an end even when one is present, so a
            // caller that sets openEnded always gets 'start-', never a bounded range.
            if (Number.isFinite(Number(spec.suffix)) && Number(spec.suffix) > 0)
                return `bytes=-${Math.floor(Number(spec.suffix))}`;
            const start = Number.isFinite(Number(spec.start)) ? Math.max(0, Math.floor(Number(spec.start))) : 0;
            if (!spec.openEnded && Number.isFinite(Number(spec.end))) {
                const end = Math.max(start, Math.floor(Number(spec.end)));
                return `bytes=${start}-${end}`;
            }
            return `bytes=${start}-`;
        }

        /**
         * Resolves what a response says about the requested range, handling both a 206 partial response and a 200 full
         * response, and rejecting an unusable answer.
         * @param {Response} response - The response to inspect.
         * @returns {{ start: number, endInclusive: number|null, total: number|null }|null} The resolved range, or null when the request is already satisfied.
         * @throws {LuminaAudioPlayer.PlayerError} When the response was a followed redirect, carries a content type that is not audio, or is an error that is not a 'range not satisfiable'.
         */
        resolveResponseRange(response) {
            // An expired or cleared session makes the authenticated stream route answer with a redirect to the sign in
            // page, and fetch follows it by default, so the followed redirect is rejected before any byte is stored. The
            // request never asked for a redirect to a track, so a redirect is always a failure, never audio.
            if (response.redirected)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The stream request was redirected, which means the session expired.');
            if (response.status === 416) {
                // A range not satisfiable means the request started at or past the end of the file. It is only "already
                // satisfied" when the buffer really covers the whole file, which is what isComplete measures, whether the
                // total was reported or an open ended read already proved the end of the file. A one shot range whose start
                // differs from the current target, or a file that was replaced or truncated, can answer 416 while bytes are
                // still missing, and that must be a failure, not a silent end.
                if (this.isComplete())
                    return null;
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The stream rejected the requested byte range.', { status: response.status });
            }
            if (response.status !== 200 && response.status !== 206)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, `The stream answered with status ${response.status}.`, { status: response.status });
            // A success that does not carry an audio type is not the track either, so the sign in page, or any other HTML
            // or JSON answer of the same status, is rejected before its body is read into the buffer.
            if (!isStreamContentType(response.headers.get('Content-Type')))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The stream answered with a content type that is not audio.');
            if (response.status === 206) {
                const parsed = parseContentRange(response.headers.get('Content-Range'));
                if (parsed === null)
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The stream answered a partial response without a usable Content-Range.', { status: response.status });
                return { start: parsed.start, endInclusive: parsed.end, total: parsed.total };
            }
            // A 200 means the server ignored the range and sent the whole resource, so the buffer is filled from its start.
            return { start: 0, endInclusive: null, total: parseContentLength(response.headers.get('Content-Length')) };
        }

        /**
         * Reads the body of a response into the buffer, chunk by chunk, through its ReadableStream.
         * @param {Response} response - The response whose body is read.
         * @param {{ start: number, endInclusive: number|null }} range - The resolved range of the response.
         * @param {AbortController} controller - The controller of the request, aborted when the loop must stop.
         * @param {boolean} stopOnPause - True when a paused player must stop the read at the next chunk boundary.
         * @param {boolean} openEnded - True when the request asked for a range that runs to the end of the file. Such a read is driven to the end of the body, and that end proves the end of the file, rather than the range the response happens to declare.
         * @returns {Promise<Object>} The outcome of the read.
         * @throws {LuminaAudioPlayer.PlayerError} When the response carries no readable body.
         */
        async readResponseIntoBufferAsync(response, range, controller, stopOnPause, openEnded) {
            if (!response.body || typeof response.body.getReader !== 'function')
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The stream response carried no readable body.');
            const reader = response.body.getReader();
            let offset = range.start;
            let isStopped = false;
            let hasReachedEnd = false;
            // True when a bounded request read its whole declared range and stopped before the body ended, so any remaining
            // bytes are discarded. An open ended request instead reads the body to its end, because that end, and not the
            // range the response happens to declare, is the only signal that proves the end of the file.
            let isStoppedAtRangeEnd = false;
            try {
                while (true) {
                    if (stopOnPause && !this.shouldKeepRunning()) {
                        isStopped = true;
                        break;
                    }
                    let result;
                    try {
                        result = await reader.read();
                    } catch (error) {
                        if (isAbortError(error)) {
                            isStopped = true;
                            break;
                        }
                        throw error;
                    }
                    if (result.done) {
                        hasReachedEnd = true;
                        break;
                    }
                    const value = result.value;
                    if (!value || value.byteLength === 0)
                        continue;
                    this.buffer.append(offset, value);
                    offset += value.byteLength;
                    this.notifyProgress();
                    this.resolveWaiters();
                    // A bounded request stops once it has read the range it asked for, while an open ended request reads on
                    // to the end of the body, so a capped response is not mistaken for the end of the file.
                    if (!openEnded && range.endInclusive !== null && offset > range.endInclusive) {
                        isStoppedAtRangeEnd = true;
                        break;
                    }
                }
            } finally {
                try {
                    reader.releaseLock();
                } catch (error) {
                    console.debug('Audio player: the download stream reader could not be released.', error);
                }
                // When the loop stops before the body ended, the request is aborted so the connection is released, and a
                // later resume issues a fresh request from the first missing byte.
                if (isStopped && !controller.signal.aborted) {
                    try {
                        controller.abort();
                    } catch (error) {
                        console.debug('Audio player: the paused download request could not be aborted.', error);
                    }
                }
                // A bounded request that read its whole declared range may leave unread bytes in the body, so the body is
                // discarded to release the connection.
                if (isStoppedAtRangeEnd)
                    this.discardResponseBody(response);
            }
            // The whole declared range was delivered. A response with no declared end (a plain 200) always counts as covered.
            const isDeclaredRangeCovered = range.endInclusive === null || offset > range.endInclusive;
            // A response that read from the start of the resource to the end of its full declared range tells the total size
            // even when the server reported none, which is what lets the fraction reach one for a server that answers with a
            // plain 200. A short body is not the end of the file, so it does not set a bogus total.
            if (hasReachedEnd && this.totalBytes === null && range.start === 0 && offset > 0 && isDeclaredRangeCovered)
                this.totalBytes = offset;
            // An open ended request proves the end of the file only when its body actually ended and it delivered the whole
            // range it declared, which is what lets isComplete recognize a fully buffered file when the server never
            // reported a total size. Requiring the body end rejects a short body (a stalled or truncated download), and
            // requiring the declared range rejects a response that stopped early without ending its body. The residual
            // assumption of this path is that the server did not cap an open ended answer to a shorter range, which is
            // indistinguishable from the end of the file while the total is unknown.
            if (openEnded && hasReachedEnd && isDeclaredRangeCovered)
                this.isStreamComplete = true;
            // A stopped read whose controller was aborted is reported as an abort, so the loop can tell a pivot from a stall.
            return { isAborted: controller.signal.aborted, isFailed: false, isDestroyed: this.destroyed, isStopped };
        }

        /**
         * Records the total size of the file when a response reveals it, and applies a seek that was waiting for it.
         * @param {number|null} total - The total size reported by the response, or null when it is unknown.
         */
        noteTotal(total) {
            // The size is latched on the first response that reports it and never overwritten afterwards: a later response
            // that disagrees would mean the file changed under the download, and re-deriving the mapping from it would move
            // the playhead and the fraction instead of surfacing the change.
            if (Number.isInteger(total) && total >= 0 && this.totalBytes === null)
                this.totalBytes = total;
            if (this.pendingSeekSeconds !== null && this.totalBytes !== null && this.duration > 0) {
                this.targetByte = Math.round(clamp(this.pendingSeekSeconds / this.duration, 0, 1) * this.totalBytes);
                this.pendingSeekSeconds = null;
                // The waiting seek just landed on a byte position, so a previous drain is stale.
                this.isDrained = false;
            }
        }

        /**
         * Gets the exclusive end of the file when it is known, used as the limit of the loop's gap search.
         *
         * It is the reported total when there is one, or, when the server never reported a total, the end of the stored
         * data once an open ended read has proved it (streamComplete). A null result means the end of the file is still
         * unknown, so the loop searches open ended.
         * @returns {number|null} The exclusive end of the file, or null when it is unknown.
         */
        resolveKnownEnd() {
            if (this.totalBytes !== null)
                return this.totalBytes;
            if (this.isStreamComplete)
                return this.buffer.getEnd();
            return null;
        }

        /**
         * Gets the total size used to map bytes to time: the known size, or the downloaded byte count once the whole file
         * is in the buffer and the server never reported a size.
         * @returns {number|null} The size to map against, or null when it is still unknown.
         */
        resolveTotalForMapping() {
            if (this.totalBytes !== null && this.totalBytes > 0)
                return this.totalBytes;
            if (this.isComplete())
                return this.buffer.getByteCount() || null;
            return null;
        }

        /**
         * Reports the current progress to the progress callback. A callback that throws is logged and swallowed, so a
         * misbehaving consumer can neither stop the download nor turn its own failure into a download failure.
         */
        notifyProgress() {
            if (!this.onProgress)
                return;
            try {
                this.onProgress(this.getProgress());
            } catch (error) {
                console.error('Audio player: the download progress callback threw.', error);
            }
        }

        /**
         * Marks the download as complete and reports the final progress. A callback that throws is logged and swallowed.
         */
        complete() {
            const isFirstCompletion = !this.isFinished;
            this.isFinished = true;
            if (isFirstCompletion) {
                // The loop is stopping, so its handle is cleared before the callback: a retry started from onComplete then
                // takes the fresh loop path instead of being dropped by a still pending loop promise.
                this.loopPromise = null;
                this.notifyProgress();
            }
            // A completed file can never fill a window that is still missing, so every remaining reader is released, even
            // on a later completion call that has no progress or callback left to report.
            this.rejectWaiters(this.createWindowUnavailableError());
            if (!isFirstCompletion || !this.onComplete)
                return;
            try {
                this.onComplete(this.getProgress());
            } catch (error) {
                console.error('Audio player: the download complete callback threw.', error);
            }
        }

        /**
         * Records a failure and reports it to the error callback. The callback is guarded, so a throwing consumer does not
         * escape as a rejected promise from a loop entry point.
         * @param {*} error - The failure to record.
         */
        fail(error) {
            const typed = LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED);
            this.error = typed;
            // The loop that failed has returned and no byte will append until a restart, so every reader waiting for a byte
            // window is released with the failure instead of hanging. The loop handle is cleared before the callback, so a
            // retry started from onError is not dropped by the loop promise that is still pending at this point.
            this.loopPromise = null;
            this.rejectWaiters(typed);
            if (!this.onError)
                return;
            try {
                this.onError(typed);
            } catch (callbackError) {
                console.error('Audio player: the download error callback threw.', callbackError);
            }
        }

        /**
         * Resolves every waiter whose byte window is now available.
         */
        resolveWaiters() {
            // Called after every append, so a reader is released the moment its window is covered instead of polling the
            // buffer. A waiter whose window is still partial stays queued and is checked again after the next chunk.
            if (this.waiters.length === 0)
                return;
            const remaining = [];
            for (const waiter of this.waiters) {
                if (this.buffer.covers(waiter.offset, waiter.length))
                    waiter.resolve();
                else
                    remaining.push(waiter);
            }
            this.waiters = remaining;
        }

        /**
         * Rejects and clears every pending byte window waiter, so a reader never waits for a download that has stopped.
         * @param {LuminaAudioPlayer.PlayerError} error - The reason to reject the waiters with.
         */
        rejectWaiters(error) {
            if (this.waiters.length === 0)
                return;
            for (const waiter of this.waiters.splice(0))
                waiter.reject(error);
        }

        /**
         * Builds the single error used whenever a byte window can no longer be filled: a completed file, a loop that
         * drained with nothing left to fetch, or a window that was released without ever arriving. Every path that reports
         * this outcome uses it, so a consumer sees one consistent code and reason.
         * @returns {LuminaAudioPlayer.PlayerError} The window unavailable error, with <c>details.reason === 'window-unavailable'</c>.
         */
        createWindowUnavailableError() {
            return new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The requested byte window can no longer be filled.', { reason: 'window-unavailable' });
        }
    }

    LuminaAudioPlayer.ByteBufferStore = ByteBufferStore;
    LuminaAudioPlayer.TrackDownloader = TrackDownloader;

    /**
     * Creates a track downloader. See {@link LuminaAudioPlayer.TrackDownloader} for the options.
     * @param {Object} options - The downloader options.
     * @returns {LuminaAudioPlayer.TrackDownloader} The created downloader.
     */
    LuminaAudioPlayer.createTrackDownloader = function (options) {
        return new TrackDownloader(options);
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
