/**
 * Seek-aware byte source of the audio player pipeline.
 *
 * It fetches an uncovered demuxer window on demand, coalesced into an aligned block, while a seek or a gapless prefetch
 * needs bytes the regular download loop is not fetching, and it never issues a duplicate request for a window the running loop is already reading.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The block size, in bytes, of an on-demand range fetch. A demuxer seek reads small windows one after another, so a
     * request per window would pay a network round trip for each read; the windows are coalesced into aligned blocks of
     * this size, which are still bounded and are stored in the same buffer the demuxer reads from.
     * @readonly
     * @type {number}
     */
    const SEEK_FETCH_BLOCK_BYTES = 256 * 1024;

    /**
     * How long the received cursor of the download loop may stay unchanged before a read that is waiting for that loop
     * falls back to a one-shot range request, in milliseconds. A loop response that sends headers and then stalls has no
     * deadline of its own, so without this bound a seek near the loop frontier would wait until the user resumes or aborts.
     * @readonly
     * @type {number}
     */
    const LOOP_STALL_FALLBACK_MS = 3000;

    /**
     * How often the progress of the download loop is checked while a read waits for it, in milliseconds.
     * @readonly
     * @type {number}
     */
    const LOOP_STALL_POLL_MS = 100;

    /**
     * A byte source for a demuxer that fetches an uncovered window on demand instead of waiting for a download loop.
     *
     * It serves two situations. A demuxer seek is not a sequential read: OGG binary-searches the byte range, and MP3 and
     * AAC scan forward from an estimate that may lie below the target. Those reads can fall into a gap below the byte
     * target the download loop is fetching, and a plain downloader read would then wait for a window the loop never
     * fetches, in practice for the rest of the file. A gapless prefetch has no download loop at all, by design, so that it
     * never runs a second open-ended download next to the current one. In both cases an uncovered window is fetched as a
     * one-shot bounded range, coalesced with its neighbours into an aligned block of a few hundred kilobytes, and stored
     * in the download buffer, so the demuxer can continue immediately and a later sequential read finds the same bytes in
     * the buffer instead of fetching them again. A window that the running download loop is already fetching is awaited
     * instead of being requested twice. The on-demand mode is switched off when a seek has scheduled its first frame, or
     * when a prefetch is handed over and its download loop starts.
     */
    class SeekAwareByteSource {
        /**
         * Creates an instance of SeekAwareByteSource.
         * @param {LuminaAudioPlayer.TrackDownloader} downloader - The downloader of the track.
         */
        constructor(downloader) {
            this.downloader = downloader;
            // The regular byte source owns the end-of-stream behavior of a downloader read: a window that reaches past the
            // end is shortened, and a window that can no longer be filled reads as the end of the file. Delegating to it
            // keeps that behavior identical whether the window came from a one-shot seek fetch or from the download loop.
            this.standard = new LuminaAudioPlayer.DemuxerByteSource(downloader);
            this.isSeeking = false;
            this.destroyed = false;
            // Incremented by every new seek, so a read that is still waiting for a previous seek can tell that a newer seek
            // owns the timeline and release the demuxer operation instead of fetching a window for the outdated one.
            this.seekGeneration = 0;
        }

        /**
         * Sets whether the demuxer is currently resolving a seek, which enables the on-demand range fetch. Every new seek
         * starts a new generation, which invalidates the reads of the seek it replaced.
         * @param {boolean} value - True while a seek is being resolved.
         */
        setSeeking(value) {
            const next = value === true;
            if (next)
                this.seekGeneration += 1;
            this.isSeeking = next;
        }

        /**
         * Gets the total size of the stream, when the downloader knows it.
         * @returns {number|null} The length in bytes, or null.
         */
        getLength() {
            return this.standard.getLength();
        }

        /**
         * Reads a window of bytes, fetching it with a one-shot range request when a seek needs a window below the byte
         * target of the download loop.
         * @param {number} offset - The first byte to read.
         * @param {number} length - The number of bytes to read.
         * @returns {Promise<Uint8Array>} The bytes, possibly shorter at the end of the stream.
         * @throws {LuminaAudioPlayer.PlayerError} When the seek range cannot be fetched, or the read was aborted.
         */
        async readAsync(offset, length) {
            if (this.destroyed)
                return new Uint8Array(0);
            const seekGeneration = this.seekGeneration;
            if (this.isDownloaderAborted())
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The seek range request was aborted.');
            const start = Math.max(0, Math.floor(offset));
            let size = Math.max(0, Math.floor(length));
            if (size === 0)
                return new Uint8Array(0);
            const total = this.downloader.getTotalBytes();
            if (total !== null) {
                if (start >= total)
                    return new Uint8Array(0);
                size = Math.min(size, total - start);
            }
            const targetByte = typeof this.downloader.getTargetByte === 'function'
                ? this.downloader.getTargetByte()
                : (Number.isInteger(this.downloader.targetByte) ? this.downloader.targetByte : null);
            // The read offset alone never ends the on-demand mode: a demuxer seek can probe far ahead of the seek target, and
            // the loop will not reach such a window for a long time. The engine clears the mode once the target's first frame
            // is scheduled; until then isWindowCovered and isRangeBeingFetched decide whether a one-shot is needed, and reads
            // behind the loop frontier stay on demand through isBelowLoopTarget.
            const isBelowLoopTarget = targetByte !== null && start < targetByte;
            // Serve the read from the cheapest source that can complete it: the buffer, the download loop that is already
            // reading the same region, or a bounded one-shot. The order matters: a covered read must not touch the network,
            // and a loop-covered read must not be requested twice.
            if ((this.isSeeking || isBelowLoopTarget) && !this.isWindowCovered(start, size)) {
                if (this.isLoopFetching(start, size)) {
                    // The regular read registers an event driven waiter that resolves the moment the loop stores the window,
                    // so a normal delivery adds no polling delay. A watchdog races it only to bound a loop that stalls or is replaced by a newer seek.
                    if (typeof this.downloader.getLoopProgress === 'function')
                        return this.readWhileLoopAsync(start, size, seekGeneration, total);
                    return this.standard.readAsync(start, size);
                }
                return this.fetchOnDemandAsync(start, size, total, seekGeneration);
            }
            return this.standard.readAsync(start, size);
        }

        /**
         * Reads a window the download loop is already fetching, racing the regular event driven read against a watchdog
         * for a stalled, aborted or superseded loop. The read wins immediately when the loop delivers the window.
         * @param {number} start - The first byte of the window.
         * @param {number} size - The number of bytes of the window.
         * @param {number} seekGeneration - The seek generation the read belongs to.
         * @param {number|null} total - The total size of the stream, or null when it is unknown.
         * @returns {Promise<Uint8Array>} The bytes, possibly shorter at the end of the stream.
         */
        async readWhileLoopAsync(start, size, seekGeneration, total) {
            const readPromise = this.standard.readAsync(start, size);
            const readOutcome = readPromise.then(
                (bytes) => ({ isRead: true, bytes }),
                (error) => ({ isRead: false, error })
            );
            const watchdog = this.createLoopStallWatchdog(start, size, seekGeneration);
            let result;
            try {
                result = await Promise.race([
                    readOutcome,
                    watchdog.promise.then((reason) => ({ isWatchdog: true, reason }))
                ]);
            } finally {
                watchdog.dispose();
            }
            if (result.isRead)
                return result.bytes;
            if (!result.isWatchdog) {
                // A read that the loop can no longer fill falls back to a one-shot, exactly like a watchdog that saw the
                // loop stop fetching. Any other failure, including an abort, propagates.
                if (!result.error)
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The seek range request failed.');
                if (LuminaAudioPlayer.isDownloadAbort(result.error)
                    || !result.error.details
                    || result.error.details.reason !== LuminaAudioPlayer.PlayerErrorReason.WINDOW_UNAVAILABLE)
                    throw result.error;
                if (this.isDownloaderAborted() || this.seekGeneration !== seekGeneration)
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The seek range request was aborted.');
                if (this.isWindowCovered(start, size))
                    return this.standard.readAsync(start, size);
                return this.fetchOnDemandAsync(start, size, total, seekGeneration);
            }
            // The regular read stays pending until the fallback one-shot stores the same window, or until the downloader is
            // torn down and rejects it. It must not become an unhandled rejection in either case.
            readPromise.catch(() => {});
            if (result.reason === 'aborted' || result.reason === 'superseded'
                || this.isDownloaderAborted() || this.seekGeneration !== seekGeneration)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The seek range request was aborted.');
            if (this.isWindowCovered(start, size))
                return this.standard.readAsync(start, size);
            return this.fetchOnDemandAsync(start, size, total, seekGeneration);
        }

        /**
         * Checks whether the download buffer already holds the whole requested window, without allocating a range list.
         * @param {number} start - The first byte of the window.
         * @param {number} size - The number of bytes of the window.
         * @returns {boolean} True when the window is fully buffered.
         */
        isWindowCovered(start, size) {
            if (typeof this.downloader.isRangeCovered === 'function')
                return this.downloader.isRangeCovered(start, size);
            const end = start + size;
            return this.downloader.getRanges().some((range) => range.start <= start && range.end >= end);
        }

        /**
         * Checks whether the download loop is already reading a range that covers the window, in which case the read waits for those bytes instead of issuing a duplicate request.
         * @param {number} start - The first byte of the window.
         * @param {number} size - The number of bytes of the window.
         * @returns {boolean} True when the active loop request covers the window.
         */
        isLoopFetching(start, size) {
            return typeof this.downloader.isRangeBeingFetched === 'function'
                && this.downloader.isRangeBeingFetched(start, size);
        }

        /**
         * Checks whether the downloader was intentionally aborted or torn down, so an on-demand read must not start a request for a timeline nobody owns anymore.
         * @returns {boolean} True when the downloader is aborted or destroyed.
         */
        isDownloaderAborted() {
            return this.destroyed
                || this.downloader.destroyed === true
                || this.downloader.isAborted === true;
        }

        /**
         * Creates a watchdog for a read that waits for the download loop. It resolves with the reason the wait must end
         * when the downloader is aborted, a newer seek took the timeline, the loop stopped fetching the window, or its
         * received cursor has not advanced for the stall bound. It stays pending while the loop is making progress, and the
         * caller disposes it as soon as the regular read resolves.
         * @param {number} start - The first byte of the window.
         * @param {number} size - The number of bytes of the window.
         * @param {number} seekGeneration - The seek generation the read belongs to.
         * @returns {{ promise: Promise<string>, dispose: Function }} The watchdog promise and its cleanup.
         */
        createLoopStallWatchdog(start, size, seekGeneration) {
            let disposed = false;
            let timer = null;
            let lastMarker = null;
            let lastProgressAt = Date.now();
            const promise = new Promise((resolve) => {
                const check = () => {
                    if (disposed)
                        return;
                    if (this.isDownloaderAborted()) {
                        resolve('aborted');
                        return;
                    }
                    if (this.seekGeneration !== seekGeneration) {
                        resolve('superseded');
                        return;
                    }
                    if (!this.isLoopFetching(start, size)) {
                        resolve('not-fetching');
                        return;
                    }
                    const progress = this.downloader.getLoopProgress();
                    const now = Date.now();
                    if (progress) {
                        const marker = `${progress.start}:${progress.cursor}`;
                        if (lastMarker === null || marker !== lastMarker) {
                            lastMarker = marker;
                            lastProgressAt = now;
                        }
                    }
                    if (now - lastProgressAt >= LOOP_STALL_FALLBACK_MS) {
                        resolve('stalled');
                        return;
                    }
                    timer = setTimeout(check, LOOP_STALL_POLL_MS);
                };
                check();
            });
            return {
                promise,
                dispose: () => {
                    disposed = true;
                    if (timer !== null)
                        clearTimeout(timer);
                }
            };
        }

        /**
         * Fetches an uncovered window as a bounded one-shot range request and then reads it through the regular source.
         * @param {number} start - The first byte of the window.
         * @param {number} size - The number of bytes of the window.
         * @param {number|null} total - The total size of the stream, or null when it is unknown.
         * @param {number} seekGeneration - The seek generation the read belongs to.
         * @returns {Promise<Uint8Array>} The bytes, possibly shorter at the end of the stream.
         */
        async fetchOnDemandAsync(start, size, total, seekGeneration) {
            if (this.isDownloaderAborted() || this.seekGeneration !== seekGeneration)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The seek range request was aborted.');
            const window = this.getFetchWindow(start, size, total);
            if (window === null)
                return new Uint8Array(0);
            // A probe far ahead of the loop frontier can duplicate bytes that the loop's open ended response will also
            // deliver. That duplication is accepted: without the one-shot an OGG binary search can wait for the rest of
            // the file, while the byte buffer merges the overlapping ranges.
            const outcome = await this.downloader.downloadRangeAsync(window.start, window.end);
            if (outcome.isAborted || outcome.isDestroyed)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The seek range request was aborted.');
            if (outcome.isFailed)
                throw outcome.error || new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The seek range request failed.');
            // Every one-shot fetch is validated, not only a satisfied response: a short body or a changed file must not fall
            // through to a read that waits for a loop window below its target that will never be fetched.
            if (!this.isWindowCovered(start, size) && total !== null)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The seek range did not cover the requested window.');
            return this.standard.readAsync(start, size);
        }

        /**
         * Expands a small demuxer read into an aligned block, so a seek that scans the stream does not pay one network round
         * trip per small window. The block never extends past the end of the file and is bounded to one aligned block around the requested window.
         * @param {number} start - The first byte of the requested window.
         * @param {number} size - The number of bytes of the requested window.
         * @param {number|null} total - The total size of the stream, or null when it is unknown.
         * @returns {{ start: number, end: number }|null} The inclusive byte range to fetch, or null when it is empty.
         */
        getFetchWindow(start, size, total) {
            if (size <= 0)
                return null;
            const blockStart = Math.floor(start / SEEK_FETCH_BLOCK_BYTES) * SEEK_FETCH_BLOCK_BYTES;
            let blockEndExclusive = Math.ceil((start + size) / SEEK_FETCH_BLOCK_BYTES) * SEEK_FETCH_BLOCK_BYTES;
            if (!Number.isFinite(blockEndExclusive) || blockEndExclusive <= blockStart)
                blockEndExclusive = blockStart + SEEK_FETCH_BLOCK_BYTES;
            if (total !== null)
                blockEndExclusive = Math.min(total, blockEndExclusive);
            if (blockEndExclusive <= blockStart)
                return null;
            // Start after the contiguous buffered run that covers the block start, so a seek into a partially buffered
            // block does not request a whole block of bytes the buffer already holds.
            let fetchStart = blockStart;
            if (typeof this.downloader.getContiguousEnd === 'function')
                fetchStart = Math.max(blockStart, this.downloader.getContiguousEnd(blockStart));
            // A run that already reaches the end of the requested window would have made isWindowCovered true; the clamp is
            // only a guard against asking for an empty or reversed range in an inconsistent buffer state.
            if (fetchStart >= start + size)
                fetchStart = start;
            if (fetchStart >= blockEndExclusive)
                fetchStart = blockStart;
            return { start: fetchStart, end: blockEndExclusive - 1 };
        }

        /**
         * Releases the source, so a late read of a released track returns an end of stream instead of touching the
         * downloader.
         */
        destroy() {
            this.destroyed = true;
        }
    }

    LuminaAudioPlayer.SeekAwareByteSource = SeekAwareByteSource;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
