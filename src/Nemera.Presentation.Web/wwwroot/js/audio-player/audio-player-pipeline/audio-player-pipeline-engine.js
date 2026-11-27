/**
 * Media pipeline engine of the audio player: the engine that actually plays a track.
 *
 * The controller of the player owns the state machine, the play descriptor, the queue and the bar, but it produces no
 * audio. This module is the engine the player runs with: it composes the download engine, the container demuxers, the
 * WebCodecs decode pipeline and the Web Audio scheduler into one pipeline, and implements the engine interface documented
 * in audio-player-engine.js. It is registered under {@link LuminaAudioPlayer.PIPELINE_ENGINE_NAME} and installed on the
 * application singleton when this module loads, so the bar drives a real pipeline instead of the silent engine.
 *
 * What it owns
 * ------------
 * A track is represented by a pipeline item: the playback manifest of the track, the downloader that fetches its bytes,
 * the demuxer that turns the bytes into encoded packets, the decoder that turns the packets into decoded audio, and the
 * scheduler that hands the decoded audio to the renderer. The renderer itself is shared by every track, because it owns
 * the AudioContext, the master gain and the ReplayGain gain, and because a gapless transition is produced by appending
 * the next track to the schedule of the current one instead of resetting it.
 *
 * Seeking
 * -------
 * A seek is a coordinated reset of every stage. The in flight download request is aborted so the demuxer read that is
 * waiting on it is released, the decode loop is stopped, the decoder is reset, the scheduler is moved to the new media
 * position, the demuxer is repositioned to the packet boundary that covers that position, and the decode loop is started
 * again from there. The renderer adopts the requested position as the media origin of the first buffer it receives, so the
 * first decoded frame after a seek is trimmed to the requested position when the demuxer reported a timeline that starts
 * before it. A container that seeds the packet timeline with the requested time, such as MP3 or AAC, needs no trim; a
 * container that reports the real frame time, such as FLAC, OGG or WAV, is trimmed. A demuxer seek is not sequential: OGG
 * binary-searches the byte range, and the MP3 and AAC estimates can land below the target, so the demuxer may read a gap
 * below the byte position the download loop fetches. The seek aware byte source serves such a window with a one-shot
 * bounded range request, so the demuxer scan never waits for a download that would only reach the gap at the end of the
 * file. Seeks into bytes that are already downloaded do not fetch them again: the downloader keeps every byte it already
 * has, as the download engine documents.
 *
 * Underrun
 * --------
 * A decoder that catches the download frontier leaves the schedule empty while the track is still playing. The engine
 * watches the scheduled ahead duration of the renderer: when it reaches zero before the stream is complete, the engine
 * suspends the renderer and reports buffering through the host, then resumes the renderer as soon as enough audio has been
 * scheduled again. A draining schedule at the very end of the track is not an underrun, because the renderer reports the
 * end on its own once the decode pipeline declared the stream complete.
 *
 * Gapless transitions
 * -------------------
 * When the user enabled gapless playback and the player queue holds another item, the engine waits until the current track is within a short lead
 * of its end, then fetches the next manifest and pre-decodes its first audio. Nothing is requested at load time: a normalized next track would start
 * a full server side production that is expensive and bounded in concurrency, and a direct track would hold bytes for a track the user may never
 * reach. Normalized gapless therefore only happens when the production is already cached or finishes within the lead; otherwise the next track
 * starts normally at the boundary. Once the current decoder has scheduled its last frame, the pre-decoded audio is appended to the renderer
 * schedule, exactly after the audio of the current track, the download of the next track resumes from its first missing byte, and the rest of the
 * track is decoded into the same schedule. The engine then switches the current item at the exact media position where the next
 * audio starts, reports the new duration and asks the player to advance its queue, so the bar shows the new track while
 * the audio never stops. Only the last track of a continuous chain declares the stream complete, which is what keeps the
 * renderer from reporting an end between two gapless items.
 *
 * Failures
 * --------
 * A download, demux or decode failure that happens mid-stream is reported once, through host.reportError, and stops the
 * pipeline cleanly; it never escapes as an unhandled rejection and never leaves the renderer or the downloader running.
 * A failure of a prefetch is dropped instead, because it must not interrupt the track that is playing. A track whose
 * manifest is not a direct play is played from the fixed duration segments the server produces, through the normalized
 * delivery pipeline (audio-player-pipeline-normalized.js), and it is seeked by segment index exactly like a direct play is
 * seeked by byte range. A play descriptor must name a track, not an album or an artist, because the manifest endpoint is
 * addressed by the track and its album and artist; expanding an album or an artist context into its tracks is the queue
 * engine's job.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The name the media pipeline engine is registered and selected under.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PIPELINE_ENGINE_NAME = 'pipeline';

    /**
     * The scheduled ahead duration, in seconds, below which a playing renderer is treated as having underrun.
     * @readonly
     * @type {number}
     */
    const UNDERRUN_THRESHOLD_SECONDS = 0.05;

    /**
     * The scheduled ahead duration, in seconds, that must be reached again before an underrun pause is resumed.
     * @readonly
     * @type {number}
     */
    const UNDERRUN_RESUME_SECONDS = 0.25;

    /**
     * The interval, in milliseconds, of the monitor that detects underruns and gapless track boundaries.
     * @readonly
     * @type {number}
     */
    const MONITOR_INTERVAL_MS = 150;

    /**
     * The margin, in seconds, that is kept between the end of the scheduled audio and the deadline for a gapless prefetch.
     * The handoff must be appended before the current audio drains, so the wait never runs to the very end of the schedule.
     * @readonly
     * @type {number}
     */
    const PREFETCH_HANDOFF_MARGIN_SECONDS = 0.15;

    /**
     * The longest, in seconds, that the end of a decode waits for a gapless prefetch. A prefetch that is not ready by this
     * deadline is abandoned and the track ends normally, so a stalled next track can never hold the current one open.
     * @readonly
     * @type {number}
     */
    const PREFETCH_HANDOFF_MAX_WAIT_SECONDS = 5;

    /**
     * The playback time, in seconds, before the end of the current track at which the next item is prefetched. A direct play needs a few seconds
     * to pre-decode its first audio, and a normalized production is expensive and bounded in concurrency, so starting either at load time would
     * hold resources for a track the user may never reach.
     * @readonly
     * @type {number}
     */
    const PREFETCH_LEAD_SECONDS = 15;

    /**
     * Coerces a value to a finite number, or null when it is absent or not finite.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The finite number, or null.
     */
    function toFiniteNumber(value) {
        return LuminaAudioPlayer.toFiniteNumber(value);
    }

    /**
     * The media pipeline engine. See the module documentation for its behavior. It fulfills the engine interface
     * documented in audio-player-engine.js.
     */
    class MediaPipelineEngine {
        /**
         * Creates an instance of MediaPipelineEngine.
         * @param {Object} host - The engine host provided by the player controller.
         * @param {Object} [options] - The engine options, for injection in a test.
         * @param {LuminaAudioPlayer.TrackRenderer} [options.renderer] - The renderer to use instead of creating one.
         * @param {LuminaAudioPlayer.PlayerCapabilities} [options.capabilities] - The capability detector to use.
         * @param {Object} [options.configuration] - The endpoints configuration. It overrides the host configuration and exists as an injection seam for a test or a composition that provides its own transport.
         * @param {Function} [options.fetchJsonAsync] - The JSON transport to use instead of the host transport, as an injection seam. It has the signature of the host method.
         */
        constructor(host, options = {}) {
            this.host = host;
            this.options = options || {};
            this.events = host.events;
            // The player's detector is reused when the host exposes it, so the engine never runs a second, independent capability detection or keeps a second memoization cache.
            this.capabilities = this.options.capabilities
                || (typeof host.getCapabilityDetector === 'function' ? host.getCapabilityDetector() : null)
                || new LuminaAudioPlayer.PlayerCapabilities();
            this.renderer = this.options.renderer || new LuminaAudioPlayer.TrackRenderer({
                events: this.events,
                replayGainMode: host.getPreferences().replayGainMode,
                onTimeUpdate: (currentTime) => this.handleTimeUpdate(currentTime),
                onEnded: () => this.handleEnded()
            });
            // The current track of the pipeline. The renderer is shared by every track.
            this.current = null;
            // The next item whose audio was already appended to the renderer schedule but whose media position has not started yet. It becomes current at the exact position its audio begins.
            this.nextItem = null;
            this.monitorTimer = null;
            this.isMonitoring = false;
            this.isUserPaused = false;
            this.isUnderrunPaused = false;
            this.destroyed = false;
            // The token of the load that owns the pipeline, so a second load that overtakes the first one stops it.
            this.loadToken = null;
            // True while a load is between the start of its teardown and the installation of its item. The old gapless timeline must not advance its queue or end under a load that is about to replace it.
            this.isLoadingTrack = false;
            this.unsubscribePreferences = null;
            this.subscribeToPreferences();
        }

        //+======================================================================================+
        //|                                    Engine interface                                  |
        //+======================================================================================+

        /**
         * Loads a track: fetches its playback manifest, prepares its download, demux and decode stages, and starts decoding
         * ahead. Playback itself is started by playAsync, so the caller can keep the preparation and the audio separate.
         * @param {Object} descriptor - The play descriptor of the track, per {@link LuminaAudioPlayer.createPlayDescriptor}.
         * @param {Object} [options] - The load options.
         * @param {boolean} [options.keepBuffering] - True to keep a loading state that was handed to the load by the player, used by an automatic continuation so the teardown of the previous track does not clear it.
         * @returns {Promise<void>} A promise that completes when the track is ready to play.
         * @throws {LuminaAudioPlayer.PlayerError} When the manifest cannot be read, the track cannot be played directly, or a stage of the pipeline cannot be created.
         */
        async loadAsync(descriptor, options = {}) {
            if (this.destroyed)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The media pipeline engine was destroyed.');
            // The manifest request of the load that is being replaced is aborted, so a slow session never keeps a stale request alive behind the track that the newer load owns.
            this.abortLoad(this.loadToken);
            const token = {
                abortController: typeof AbortController === 'function' ? new AbortController() : null
            };
            this.loadToken = token;
            this.isLoadingTrack = true;
            try {
                await this.teardownPlaybackAsync(null, token, options.keepBuffering === true);
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                this.isUserPaused = false;
                this.isUnderrunPaused = false;
                const signal = token.abortController ? token.abortController.signal : undefined;
                const manifest = await this.fetchManifestAsync(descriptor, signal);
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                this.renderer.configure(manifest);
                const item = await this.buildItemAsync(descriptor, manifest, false);
                if (!this.isLoadCurrent(token) || this.destroyed) {
                    await this.disposeItemAsync(item);
                    throw this.createSupersededLoadError();
                }
                item.base = 0;
                this.current = item;
                this.isLoadingTrack = false;
                if (item.duration > 0)
                    this.renderer.setTimelineDuration(item.duration);
                this.host.reportDuration(item.duration);
                this.startDecodeLoop(item);
                this.startGaplessPrefetch(item);
            } catch (error) {
                // A failure of a load that was overtaken by a newer one belongs to a playback the user already replaced, so it is reported as a superseded load, which the player ignores,
                // instead of surfacing an error over the track that the newer load owns. The rejection also stops the stale playAsync continuation before it can set READY or PLAYING over
                // the newer request.A stale load leaves the loading flag to the newer load.
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                this.isLoadingTrack = false;
                throw error;
            }
        }

        /**
         * Creates the typed error that tells the player a load was overtaken by a newer play request.
         * @returns {LuminaAudioPlayer.PlayerError} The superseded load error.
         */
        createSupersededLoadError() {
            return new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The load was superseded by a newer play request.', { reason: LuminaAudioPlayer.PlayerErrorReason.LOAD_SUPERSEDED });
        }

        /**
         * Aborts the manifest request of a load, so a teardown or a newer load never leaves a stale request running.
         * @param {Object|null} token - The load token whose request is aborted.
         */
        abortLoad(token) {
            if (!token || !token.abortController)
                return;
            try {
                token.abortController.abort();
            } catch (error) {
                console.debug('Audio player: the manifest request of the replaced load could not be aborted.', error);
            }
        }

        /**
         * Checks whether a load is still the one that owns the pipeline.
         * @param {Object} token - The token of the load.
         * @returns {boolean} True when this load is still current and the engine was not destroyed.
         */
        isLoadCurrent(token) {
            return !this.destroyed && this.loadToken === token;
        }

        /**
         * Releases the currently prepared track without destroying the engine, which keeps its renderer and audio context for a later play.
         *  The player calls it when a stop arrives while an automatic continuation is loading: the stopped player should not keep a track loaded that it started only to continue a queue.
         * @returns {Promise<void>} A promise that completes when the prepared track was released.
         */
        async discardCurrentAsync() {
            if (this.destroyed)
                return;
            await this.teardownPlaybackAsync(null, null, false);
        }

        /**
         * Drops the next item and the prefetch of the current track, without touching the audio that is already playing.
         *
         * The player calls it when it replaces the queue under a playing track, for example when the user turns shuffle on or
         * off. A prefetched or appended handoff was prepared from the queue that no longer exists, so leaving it in place
         * would play a track the queue no longer describes. When the handoff is already audible it is left alone, because
         * truncating it would rewind audio that has played.
         * @returns {Promise<void>} A promise that completes when the upcoming handoff was released.
         */
        async cancelUpcomingAsync() {
            if (this.destroyed)
                return;
            const item = this.current;
            if (!item)
                return;
            await this.cancelPrefetchAsync(item);
            item.continueToEntry = null;
            const next = this.nextItem;
            if (!next)
                return;
            // A handoff that has not reached the playhead can be removed from the schedule; one that is already audible stays.
            if (typeof this.renderer.truncateScheduleAt !== 'function'
                || next.base <= this.renderer.getCurrentTime())
                return;
            this.nextItem = null;
            if (typeof this.renderer.cancelTrackGainChange === 'function')
                this.renderer.cancelTrackGainChange();
            this.renderer.truncateScheduleAt(next.base);
            this.renderer.markStreamComplete();
            this.disposeItemAsync(next).catch((error) => {
                console.debug('Audio player: the upcoming track of the replaced queue could not be released.', error);
            });
        }

        /**
         * Starts, or resumes, audible playback.
         * @returns {Promise<void>} A promise that completes when the renderer is running.
         */
        async playAsync() {
            this.isUserPaused = false;
            this.isUnderrunPaused = false;
            this.host.reportBuffering(false);
            if (this.current)
                this.current.downloader.setPlaybackPaused(false);
            // An attached next item is about to own the playhead, so its sequential download resumes with playback. The detached prefetch stays stopped until the handoff and is never started by a resume.
            if (this.nextItem && this.nextItem.downloader) {
                this.nextItem.downloader.setPlaybackPaused(false);
                this.nextItem.downloader.startAsync();
                if (!this.nextItem.isDecodeComplete)
                    this.resumeDecodeAfterSettledLoopAsync(this.nextItem);
            }
            await this.renderer.playAsync();
            this.startMonitor();
        }

        /**
         * Suspends audible playback, keeping the position. The downloader stops with the pause only when the stored 'keep downloading while paused' preference is off, which the download engine enforces itself.
         * @returns {Promise<void>} A promise that completes when playback was suspended.
         */
        async pauseAsync() {
            this.isUserPaused = true;
            this.isUnderrunPaused = false;
            this.stopMonitor();
            this.host.reportBuffering(false);
            if (this.current)
                this.current.downloader.setPlaybackPaused(true);
            // The attached next item started its sequential download at the handoff preparation, so it follows the same
            // pause policy as the current item. The detached prefetch is deliberately not touched: it has no sequential
            // download, and forwarding the paused state to it would start one when keep-downloading-while-paused is enabled.
            if (this.nextItem && this.nextItem.downloader)
                this.nextItem.downloader.setPlaybackPaused(true);
            await this.renderer.pauseAsync();
        }

        /**
         * Moves the playhead to a position of the current track. A position that is already downloaded is not fetched
         * again; a position that is not downloaded triggers the range fetch of the download engine.
         * @param {number} seconds - The target position of the current track, in seconds.
         * @param {boolean} [isRetry=false] - True when this is the one automatic retry after an interrupted seek.
         * @returns {Promise<void>} A promise that completes when the decode pipeline resumed at the target.
         */
        async seekAsync(seconds, isRetry = false) {
            const item = this.current;
            if (!item || this.destroyed)
                return;
            // The order is deliberate: stop the old timeline, reset the decoder, reposition the demuxer, and only then let
            // the decode loop schedule frames at the target. Every await is followed by an isSeekCurrent check, so a newer
            // seek can abandon this one without touching the pipeline the newer seek already owns.
            // Every seek owns a token, so a seek that is overtaken by a newer one returns at its next await instead of
            // resetting the demuxer, the decoder or the shared renderer that the newer seek is already using.
            const token = {};
            item.seekToken = token;
            // A decode completion that is parked while it waits for a prefetch is released at once, so the seek never waits for a stalled next track that is kept across the seek.
            this.cancelPrefetchWait(item);
            // A seek is a resume when the audio was playing or when an underrun had paused it. A seek requested by a user
            // pause stays paused, and a seek in the ended state is refused by the player. The state is captured before the seek resets it.
            const wasPlaying = this.renderer.isPlaying();
            const shouldResume = !this.isUserPaused && (wasPlaying || this.isUnderrunPaused);
            const requested = Math.max(0, toFiniteNumber(seconds) === null ? 0 : Number(seconds));
            const position = item.duration > 0 ? Math.min(requested, item.duration) : requested;
            const nextEntry = this.getNextQueueEntry();
            // From here until the target is scheduled, the seek owns the timeline: the monitor does not pause it and a boundary crossing of the old timeline does not advance the queue under it.
            item.isSeeking = true;
            item.isDecodeComplete = false;
            item.continueToEntry = null;
            item.skipUntilSeconds = null;
            item.hasScheduledAudio = false;
            item.scheduler.setSuppressed(true);
            // A seek into bytes that are already downloaded needs no loading state: the player's own buffered range check already knows the target is available, so only an uncovered target reports engine buffering.
            if (!this.isSeekTargetBuffered(item, position))
                this.host.reportBuffering(true);

            // The old sources are stopped and the playhead is moved before any teardown is awaited, so the seek is heard immediately and the prefetch teardown cannot extend the latency of the reposition.
            item.loopToken = null;
            item.downloader.abort();
            let decoderReset = null;
            let decoderResetError = null;
            try {
                decoderReset = item.decoder.resetAsync();
            } catch (error) {
                decoderResetError = error;
            }
            try {
                this.renderer.seekTo(item.base + position);
            } catch (error) {
                this.failItem(item, error);
                return;
            }

            // An attached next item had its audio in the schedule that the reset above stopped, so it is released and a fresh prefetch is started after the seek.
            // A detached prefetch does not touch the renderer, and it still describes the same successor, so it is kept without being torn down and rebuilt.
            if (this.nextItem) {
                const detached = this.nextItem;
                this.nextItem = null;
                this.renderer.cancelTrackGainChange();
                await this.disposeItemAsync(detached);
                if (!this.isSeekCurrent(item, token))
                    return;
            }
            if (item.prefetch && !this.canKeepPrefetch(item.prefetch, nextEntry)) {
                await this.cancelPrefetchAsync(item);
                if (!this.isSeekCurrent(item, token))
                    return;
            }
            if (decoderResetError) {
                if (LuminaAudioPlayer.isDownloadAbort(decoderResetError))
                    return this.recoverInterruptedSeekAsync(item, token, position, isRetry);
                this.failItem(item, decoderResetError);
                return;
            }
            if (decoderReset) {
                try {
                    await decoderReset;
                } catch (error) {
                    if (LuminaAudioPlayer.isDownloadAbort(error))
                        return this.recoverInterruptedSeekAsync(item, token, position, isRetry);
                    this.failItem(item, error);
                    return;
                }
            }
            if (!this.isSeekCurrent(item, token))
                return;
            await this.waitForDecodeLoopAsync(item);
            if (!this.isSeekCurrent(item, token))
                return;
            // The old loop may have reached the end of the stream while it was being stopped, after the seek already cleared
            // the flag. It is cleared again here, so the new loop is not mistaken for one whose decode already finished.
            item.isDecodeComplete = false;
            // The loading state belongs to the frames of the seek target only, so the first frame notification is re-armed
            // once the old decode loop has definitively stopped, and the frames of that loop are no longer dropped.
            item.scheduler.rearm();
            item.scheduler.setSeekTarget(position);
            item.scheduler.setSuppressed(false);
            if (item.source)
                item.source.setSeeking(true);
            item.downloader.seek(position);
            // startAsync clears the aborted state that the abort above set. The sequential loop only runs when playback is
            // active and the keep-downloading-while-paused preference allows it, but the seek itself does not depend on it:
            // the seek aware source fetches every window the demuxer needs for the target with bounded range requests.
            item.downloader.startAsync();
            const seekOutcome = await this.runDemuxerOperationAsync(item, token, () => item.demuxer.seekToTimeAsync(position));
            if (seekOutcome.skipped)
                return;
            if (seekOutcome.error) {
                if (!this.isSeekCurrent(item, token))
                    return;
                if (LuminaAudioPlayer.isDownloadAbort(seekOutcome.error))
                    return this.recoverInterruptedSeekAsync(item, token, position, isRetry);
                this.failItem(item, seekOutcome.error);
                return;
            }
            if (!this.isSeekCurrent(item, token))
                return;
            if (seekOutcome.value === null) {
                // A stream the demuxer cannot reposition, for example one whose length is not known, is re-read from its
                // start and the packets that end before the target are dropped, which still lands on the requested audio.
                const rebuildOutcome = await this.runDemuxerOperationAsync(item, token, () => this.rebuildDemuxerAsync(item));
                if (rebuildOutcome.skipped)
                    return;
                if (rebuildOutcome.error) {
                    if (!this.isSeekCurrent(item, token))
                        return;
                    if (LuminaAudioPlayer.isDownloadAbort(rebuildOutcome.error))
                        return this.recoverInterruptedSeekAsync(item, token, position, isRetry);
                    this.failItem(item, rebuildOutcome.error);
                    return;
                }
                item.skipUntilSeconds = position;
            }
            if (!this.isSeekCurrent(item, token))
                return;
            // A resume requested while the seek was running also counts, which is why the renderer is consulted again: a
            // paused track that the user resumed during the seek is playing even though the seek started without playback.
            const shouldResumeNow = !this.isUserPaused && (shouldResume || this.renderer.isPlaying());
            if (shouldResumeNow) {
                // A seek requested while the audio was playing, or while an underrun had paused it, resumes the renderer,
                // so the schedule of the new position actually advances the playhead and reports its end. A pause that the user requested while the seek was being resolved still wins.
                this.isUnderrunPaused = false;
                try {
                    await this.renderer.playAsync();
                } catch (error) {
                    this.failItem(item, error);
                    return;
                }
                if (!this.isSeekCurrent(item, token))
                    return;
                this.startMonitor();
            }
            this.startDecodeLoop(item);
            // The decode of the seek may already have reached the end of the track, in which case the completion has
            // already made its end decision and a prefetch started now would never be appended.
            if (!item.isDecodeComplete)
                this.startGaplessPrefetch(item);
        }

        /**
         * Runs an operation that mutates the demuxer of an item under a gate, so two seeks of the same track can never
         * reposition or rebuild the same demuxer at the same time. A seek that is overtaken while it waits for the gate returns without running its operation.
         * @param {Object} item - The item whose demuxer is operated on.
         * @param {Object} token - The token of the seek that requests the operation.
         * @param {Function} operation - The operation to run, returning a value or a promise of one.
         * @returns {Promise<Object>} The outcome, with a value, an error, or the skipped flag when a newer seek owns the item.
         */
        async runDemuxerOperationAsync(item, token, operation) {
            // A demuxer is not safe to reposition or rebuild concurrently. A previous seek may still be operating on it, so
            // the new operation waits for that gate before it reads the current seek token and takes the gate itself.
            if (item.demuxerOperation) {
                try {
                    await item.demuxerOperation;
                } catch (error) {
                    // The gate never rejects, so this is only a teardown of the operation that held it.
                }
            }
            if (!this.isSeekCurrent(item, token))
                return { skipped: true };
            let release = null;
            // The gate resolves rather than rejects. One seek's failure must not become the next seek's failure just
            // because the next seek was waiting behind it.
            const gate = new Promise((resolve) => {
                release = resolve;
            });
            item.demuxerOperation = gate;
            try {
                return { value: await operation() };
            } catch (error) {
                return { error };
            } finally {
                // Clear the handle only if it is still ours. A newer seek may already have replaced it, and clearing that
                // newer gate would let a third operation run concurrently with it.
                if (item.demuxerOperation === gate)
                    item.demuxerOperation = null;
                // Release in every outcome, including a failed operation, or the next seek waits forever.
                release();
            }
        }

        /**
         * Checks whether a seek is still the one that owns the current track, so a seek that was overtaken by a newer one stops before it touches shared state.
         * @param {Object} item - The item the seek was requested on.
         * @param {Object} token - The token of the seek.
         * @returns {boolean} True when this seek is still the current one.
         */
        isSeekCurrent(item, token) {
            return !this.destroyed && this.current === item && item.seekToken === token
                && !item.failed && !item.stopped && !item.disposed;
        }

        /**
         * Checks whether a position of an item is already covered by its downloaded time ranges, so a seek into it does not need to report engine buffering.
         * @param {Object} item - The item whose downloaded ranges are checked.
         * @param {number} seconds - The position to check.
         * @returns {boolean} True when the position is already downloaded.
         */
        isSeekTargetBuffered(item, seconds) {
            if (!item || !item.downloader)
                return false;
            const value = Number(seconds);
            if (!Number.isFinite(value))
                return false;
            // The fraction check answers from the byte buffer without allocating a time range list, which matters because this runs on a seek rather than on a network chunk.
            if (typeof item.downloader.coversTime === 'function') {
                try {
                    return item.downloader.coversTime(value);
                } catch (error) {
                    console.debug('Audio player: the buffered state of the seek target could not be read.', error);
                    return false;
                }
            }
            if (typeof item.downloader.getTimeRanges !== 'function')
                return false;
            try {
                return item.downloader.getTimeRanges().some((range) =>
                    Number.isFinite(range.start) && Number.isFinite(range.end)
                    && value >= range.start && value <= range.end);
            } catch (error) {
                console.debug('Audio player: the downloaded ranges of the seek target could not be read.', error);
                return false;
            }
        }

        /**
         * Recovers an interrupted seek. The seek is retried once, which succeeds when the interruption was a transient
         * request abort such as a pause that reached the downloader; if the retry is interrupted too, the seek is reported
         * as a clean download failure instead of leaving the item in a silent, permanently buffering state.
         * @param {Object} item - The item whose seek was interrupted.
         * @param {Object} token - The token of the interrupted seek.
         * @param {number} position - The seek target, in seconds.
         * @param {boolean} isRetry - True when the interrupted seek was already the automatic retry.
         * @returns {Promise<void>} A promise that completes when the seek was retried or failed.
         */
        async recoverInterruptedSeekAsync(item, token, position, isRetry) {
            if (!this.isSeekCurrent(item, token))
                return;
            this.clearSeekState(item);
            if (!isRetry) {
                await this.seekAsync(position, true);
                return;
            }
            this.failItem(item, new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED, 'The seek was interrupted before its target could be read.'));
        }

        /**
         * Clears the state a seek owns, so a seek that cannot continue leaves a pipeline that can be resumed or retried instead of an item whose monitor, boundary and end checks all return early.
         * @param {Object} item - The item whose seek state is cleared.
         */
        clearSeekState(item) {
            this.cancelPrefetchWait(item);
            item.isSeeking = false;
            if (item.scheduler) {
                item.scheduler.setSuppressed(false);
                item.scheduler.rearm();
            }
            if (item.source)
                item.source.setSeeking(false);
        }

        /**
         * Sets the output volume. The player folds the mute state into the volume it sends, so the renderer receives zero while the player is muted.
         * @param {number} volume - The output volume, between 0 and 1.
         * @returns {Promise<void>} A promise that completes when the renderer applied the volume.
         */
        async setVolumeAsync(volume) {
            await this.renderer.setVolumeAsync(volume);
        }

        /**
         * Gets the time ranges of the current track that were already downloaded, so the player can tell whether a seek
         * target is buffered and can show the streamed part of the progress bar.
         * @returns {Array<{ start: number, end: number }>} The downloaded time ranges, in seconds.
         */
        getBufferedTimeRanges() {
            if (!this.current)
                return [];
            try {
                return this.current.downloader.getTimeRanges();
            } catch (error) {
                console.debug('Audio player: the downloaded ranges of the current track could not be read.', error);
                return [];
            }
        }

        /**
         * Gets the analyzer that taps the track level of the engine, which the VU meter of the bar reads. The analyzer of
         * the shared renderer sits before the output volume, so the meter does not change with the volume setting, and it
         * stays valid across the tracks the engine plays, because the renderer and its audio context are reused.
         * @returns {AnalyserNode|null} The output analyzer, or null while no audio context has been created yet.
         */
        getOutputAnalyser() {
            return this.renderer.getOutputAnalyser();
        }

        /**
         * Checks whether the engine was destroyed.
         * @returns {boolean} True when the engine was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Reports the downloaded fraction of an item to the player once, used at a gapless handoff where the prefetched downloader has nothing left to emit.
         * @param {Object} item - The item whose progress is reported.
         */
        reportItemProgress(item) {
            if (!item || !item.downloader)
                return;
            try {
                const fraction = Number(item.downloader.getDownloadedFraction());
                if (Number.isFinite(fraction))
                    this.host.reportProgress(Math.min(1, Math.max(0, fraction)));
            } catch (error) {
                console.debug('Audio player: the downloaded fraction of the next track could not be read.', error);
            }
        }

        /**
         * Releases every resource of the engine, including the audio context of the renderer.
         * @returns {Promise<void>} A promise that completes when every resource was released.
         */
        async destroyAsync() {
            if (this.destroyed)
                return;
            this.destroyed = true;
            this.isLoadingTrack = false;
            this.stopMonitor();
            if (this.unsubscribePreferences) {
                this.unsubscribePreferences();
                this.unsubscribePreferences = null;
            }
            await this.teardownPlaybackAsync(this.loadToken, null);
            try {
                await this.renderer.destroyAsync();
            } catch (error) {
                console.error('Audio player: the media pipeline renderer could not be destroyed.', error);
            }
        }

        //+======================================================================================+
        //|                                    Pipeline setup                                    |
        //+======================================================================================+

        /**
         * Builds the pipeline item of a track: its downloader, its demuxer, its decoder and its scheduler.
         * @param {Object} descriptor - The play descriptor of the track.
         * @param {Object} manifest - The playback manifest of the track.
         * @param {boolean} isPrefetch - True to build the item for a gapless prefetch, which buffers instead of scheduling.
         * @param {Function} [onItemCreated] - Called with the item as soon as its downloader exists, so a cancellation can
         * stop the prefetch before the rest of its stages were built.
         * @returns {Promise<Object>} The prepared item.
         */
        async buildItemAsync(descriptor, manifest, isPrefetch, onItemCreated) {
            // Build in dependency order: downloader and byte source first, then demuxer, scheduler and decoder. The catch
            // below disposes the partial item if a later stage fails, so a failed load never leaves a half-built pipeline.
            const preferences = this.host.getPreferences();
            const duration = this.resolveDuration(manifest, descriptor, null);
            const item = {
                descriptor,
                // The identity of the queue entry this item represents, when it came from a gapless prefetch.
                entryId: null,
                manifest,
                downloader: null,
                source: null,
                demuxer: null,
                decoder: null,
                scheduler: null,
                base: 0,
                duration: duration,
                isDecodeComplete: false,
                isPreloading: isPrefetch,
                isGainScheduled: false,
                isSeeking: false,
                seekToken: null,
                demuxerOperation: null,
                // The next queue entry to start normally when a gapless handoff could not be prepared before the track ended, so a slow or failed prefetch never strands the queue.
                continueToEntry: null,
                skipUntilSeconds: null,
                loopToken: null,
                loopPromise: null,
                prefetch: null,
                prefetchWaitCancel: null,
                disposed: false,
                failed: false,
                stopped: false,
                hasScheduledAudio: false,
                // A delivery refresh is attempted once per item. If the refreshed handle is rejected again, the failure is terminal, so a stale or
                // broken delivery cannot make the player refetch the manifest forever.
                deliveryRecoveryAttempted: false,
                deliveryRecoveryInProgress: false
            };
            try {
                const isNormalizedDelivery = manifest.isDirectPlay !== true;
                let demuxer = null;
                if (isNormalizedDelivery) {
                    const segmentDurationSeconds = Number(manifest.segmentDurationSeconds);
                    if (!manifest.deliveryContainer || !manifest.deliveryCodec || !Number.isInteger(segmentDurationSeconds) || segmentDurationSeconds <= 0)
                        throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The normalized delivery of the track is incomplete.');
                    const configuration = this.getConfiguration();
                    item.downloader = LuminaAudioPlayer.createNormalizedSegmentDownloader({
                        durationInSeconds: duration,
                        segmentDurationSeconds: segmentDurationSeconds,
                        urlBuilder: (segmentIndex) => LuminaAudioPlayer.resolveSegmentStreamUrl(manifest, descriptor, configuration, segmentIndex),
                        shouldRetryUnavailable: !isPrefetch,
                        shouldKeepDownloadingWhilePaused: preferences.shouldKeepDownloadingWhilePaused,
                        events: this.events,
                        onProgress: (downloadedFraction) => {
                            if (item === this.current)
                                this.host.reportProgress(downloadedFraction);
                        },
                        onError: (error) => this.handleDownloadFailure(item, error)
                    });
                    if (typeof onItemCreated === 'function')
                        onItemCreated(item);
                    // The normalized demuxer fetches complete segments itself, so it needs no byte source. The engine still drives a source on
                    // seek and teardown, so a no-op source of the same shape keeps that path uniform without a download loop behind it.
                    item.source = {
                        setSeeking: () => {},
                        destroy: () => {}
                    };
                    if (!isPrefetch)
                        item.downloader.startAsync();
                    demuxer = await LuminaAudioPlayer.createNormalizedSegmentDemuxer(manifest, {
                        downloader: item.downloader,
                        capabilities: this.capabilities,
                        events: this.events,
                        onError: (error) => this.failItem(item, error)
                    });
                } else {
                    const streamPath = LuminaAudioPlayer.resolveStreamUrl(manifest, descriptor, this.getConfiguration());
                    item.downloader = LuminaAudioPlayer.createTrackDownloader({
                        url: streamPath,
                        durationInSeconds: duration,
                        shouldKeepDownloadingWhilePaused: preferences.shouldKeepDownloadingWhilePaused,
                        events: this.events,
                        onProgress: (downloadedFraction) => {
                            if (item === this.current)
                                this.host.reportProgress(downloadedFraction);
                        },
                        onError: (error) => this.handleDownloadFailure(item, error)
                    });
                    if (typeof onItemCreated === 'function')
                        onItemCreated(item);
                    // The demuxer reads through the seek aware source, so a seek can fetch a window below the byte target of the download loop instead of waiting for bytes the loop never fetches.
                    item.source = new LuminaAudioPlayer.SeekAwareByteSource(item.downloader);
                    if (isPrefetch) {
                        // A prefetch fetches only the windows its pre-decode needs, through one-shot range requests, instead of
                        // running a second open-ended download from the moment the current track starts. Its sequential download is
                        // started at the handoff, from the first byte that is still missing.
                        item.source.setSeeking(true);
                    } else {
                        item.downloader.startAsync();
                    }
                }
                if (!demuxer) {
                    demuxer = await LuminaAudioPlayer.createDemuxerForManifest(manifest, {
                        source: item.source,
                        capabilities: this.capabilities,
                        events: this.events,
                        onError: (error) => this.failItem(item, error)
                    });
                }
                if (!demuxer) {
                    // A cancelled or aborted header probe and a transport failure are not properties of the track, so
                    // they are reported as themselves. Only a probe that actually concluded the bundled player cannot
                    // handle the bytes is reported as a track the server has to normalize.
                    if (item.downloader.isAborted || item.stopped || item.disposed)
                        throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED, 'The header probe of the track was aborted.');
                    const downloadError = item.downloader.getError();
                    if (downloadError)
                        throw downloadError;
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The track cannot be demuxed and decoded by the bundled player.', { reason: LuminaAudioPlayer.PlayerErrorReason.NORMALIZED_DELIVERY_REQUIRED });
                }
                item.demuxer = demuxer;
                const configuration = demuxer.getConfiguration();
                if (!configuration)
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The demuxer of the track reported no decoder configuration.');
                const schedulerTarget = isPrefetch ? new LuminaAudioPlayer.BufferingScheduler() : this.renderer;
                item.scheduler = new LuminaAudioPlayer.SeekableScheduler(schedulerTarget, () => {
                    item.hasScheduledAudio = true;
                    // The first frame of a seek is what ends its loading state, not the moment the seek was requested. The same point ends the on-demand range fetch mode of the source:
                    // from here the demuxer reads forward from the seek target, which the regular download loop fetches. A prefetch keeps its on-demand mode until
                    // the handoff, because its download loop does not run while it pre-decodes.
                    if (item.source && item === this.current)
                        item.source.setSeeking(false);
                    if (item.isSeeking) {
                        item.isSeeking = false;
                        if (item === this.current)
                            this.host.reportBuffering(false);
                    }
                });
                item.decoder = LuminaAudioPlayer.createTrackDecoder({
                    scheduler: item.scheduler,
                    config: configuration,
                    onError: (error) => this.failItem(item, error)
                });
                item.duration = this.resolveDuration(manifest, descriptor, demuxer);
                return item;
            } catch (error) {
                await this.disposeItemAsync(item);
                throw error;
            }
        }

        /**
         * Resolves the duration of a track: the exact duration the demuxer stated, then the manifest duration, then the duration of the play descriptor.
         * @param {Object} manifest - The playback manifest of the track.
         * @param {Object} descriptor - The play descriptor of the track.
         * @param {LuminaAudioPlayer.Demuxer|null} demuxer - The demuxer of the track, or null before it exists.
         * @returns {number} The duration in seconds, or zero when no source knows it.
         */
        resolveDuration(manifest, descriptor, demuxer) {
            const candidates = [
                demuxer ? demuxer.getDuration() : null,
                manifest && manifest.durationInSeconds,
                descriptor && descriptor.durationInSeconds
            ];
            for (const candidate of candidates) {
                const value = toFiniteNumber(candidate);
                if (value !== null && value > 0)
                    return value;
            }
            return 0;
        }

        /**
         * Starts the decode loop of an item when one is not already running.
         * @param {Object} item - The item to decode.
         * @returns {Promise<void>|null} The loop promise, or null when the loop was already running or the item stopped.
         */
        startDecodeLoop(item) {
            if (!item || item.stopped || item.disposed || this.destroyed)
                return null;
            if (item.loopPromise)
                return item.loopPromise;
            const token = {};
            item.loopToken = token;
            const loop = this.runDecodeLoopAsync(item, token);
            item.loopPromise = loop;
            loop.finally(() => {
                if (item.loopToken === token)
                    item.loopToken = null;
                if (item.loopPromise === loop)
                    item.loopPromise = null;
            }).catch(() => {
                // The loop reports its own failures through the engine, so a rejection is only a teardown and is swallowed
                // here, so the bookkeeping promise can never surface as an unhandled rejection.
            });
            return loop;
        }

        /**
         * Resumes the decode of an attached next item after its previous loop settles. A pause may have cancelled the loop
         * request, and the loop is still finishing when the user resumes; starting a second loop immediately would return the dying one and never decode the track.
         * @param {Object} item - The next item whose decode is resumed.
         */
        resumeDecodeAfterSettledLoopAsync(item) {
            if (!item || item.isDecodeComplete || item.stopped || item.failed || this.destroyed)
                return;
            const pending = item.loopPromise;
            if (!pending) {
                this.startDecodeLoop(item);
                return;
            }
            Promise.resolve(pending).catch(() => {}).finally(() => {
                // The item may have advanced to the playhead while its old loop was settling, so it is resumed when it is either still the attached next item or the current one.
                if ((this.nextItem === item || this.current === item) && !item.isDecodeComplete && !item.stopped
                    && !item.failed && !this.destroyed && !this.isUserPaused)
                    this.startDecodeLoop(item);
            }).catch(() => {
                console.debug('Audio player: the resumed next track decode could not be started.');
            });
        }

        /**
         * Runs the decode loop of an item: it reads encoded packets from the demuxer, feeds them to the decoder, and stops
         * when the stream ends, the preload of a gapless prefetch is satisfied, or the item is stopped.
         * @param {Object} item - The item to decode.
         * @param {Object} token - The token that identifies this loop, so a seek can retire it.
         * @returns {Promise<void>} A promise that completes when the loop stops.
         */
        async runDecodeLoopAsync(item, token) {
            try {
                while (!this.destroyed && !item.stopped && !item.failed && item.loopToken === token) {
                    if (item.isPreloading && item.scheduler.getScheduledAheadSeconds() >= LuminaAudioPlayer.GAPLESS_PRELOAD_SECONDS)
                        return;
                    if (item.isDecodeComplete)
                        return;
                    const packet = await item.demuxer.readPacketAsync();
                    if (item.loopToken !== token || item.stopped || item.failed || this.destroyed)
                        return;
                    if (packet === null) {
                        await item.decoder.flushAsync();
                        // A seek can retire the loop while the final flush is pending, and the decoder reset makes that
                        // flush resolve normally instead of rejecting. The completion must not be recorded for a loop that
                        // no longer owns the item, or the fresh seek loop would exit immediately at the completed flag.
                        if (item.loopToken !== token || item.stopped || item.failed || this.destroyed)
                            return;
                        item.isDecodeComplete = true;
                        await this.handleDecodeCompleteAsync(item, token);
                        return;
                    }
                    if (this.shouldSkipPacket(item, packet))
                        continue;
                    const accepted = await item.decoder.decodeAsync(packet);
                    if (!accepted) {
                        if (item.decoder.isStopped() && item.decoder.getError())
                            this.failItem(item, item.decoder.getError());
                        return;
                    }
                }
            } catch (error) {
                if (this.destroyed || item.stopped || LuminaAudioPlayer.isDownloadAbort(error))
                    return;
                this.failItem(item, error);
            }
        }

        /**
         * Checks whether a packet of a demuxer that was restarted from the beginning ends before the seek target, in which
         * case it is dropped. The packet that covers the target arms the trim of the scheduler.
         * @param {Object} item - The item the packet belongs to.
         * @param {Object} packet - The encoded packet.
         * @returns {boolean} True when the packet must be skipped.
         */
        shouldSkipPacket(item, packet) {
            if (item.skipUntilSeconds === null)
                return false;
            const target = item.skipUntilSeconds;
            const start = toFiniteNumber(packet.timestamp) === null ? 0 : Number(packet.timestamp) / 1000000;
            const duration = toFiniteNumber(packet.duration) === null ? 0 : Number(packet.duration) / 1000000;
            if (start + duration <= target + LuminaAudioPlayer.SEEK_TRIM_EPSILON_SECONDS)
                return true;
            item.skipUntilSeconds = null;
            item.scheduler.setSeekTarget(target);
            return false;
        }

        /**
         * Handles the end of the decode of an item. A prefetched item only records it; the current item either hands over
         * to the prefetched next track or declares the stream complete, which is what lets the renderer report the end.
         * @param {Object} item - The item whose decode ended.
         * @param {Object} [loopToken] - The token of the decode loop that reached the end, when the completion was observed
         * by that loop. It is omitted when the completion is re-evaluated for an item that already became current.
         * @returns {Promise<void>} A promise that completes when the completion was handled or abandoned as stale.
         */
        async handleDecodeCompleteAsync(item, loopToken) {
            // The state of the item at the moment the completion was observed, so a seek that overtakes the possibly long wait for a gapless prefetch is detected before it is acted on.
            const seekToken = item.seekToken;
            if (item !== this.current) {
                // A prefetched item records its own completion; the handoff that consumes it decides when that matters.
                item.isDecodeComplete = true;
                item.isPreloading = false;
                return;
            }
            if (!this.isCompletionCurrent(item, loopToken, seekToken))
                return;
            item.isDecodeComplete = true;
            item.isPreloading = false;
            if (item.isSeeking) {
                // A seek whose decode produced no frame, for example one to the very end of the track, has nothing left to
                // schedule, so its loading state is cleared and the track is reported as ended instead of spinning.
                item.isSeeking = false;
                if (item.source)
                    item.source.setSeeking(false);
                this.host.reportBuffering(false);
                if (!item.hasScheduledAudio) {
                    item.downloader.abort();
                    this.stopMonitor();
                    this.renderer.markStreamComplete();
                    this.renderer.pauseAsync().catch((error) => {
                        console.debug('Audio player: the renderer could not be paused after an empty seek.', error);
                    });
                    // The track ended and a finished pipeline is not retained: its decoder, demuxer, downloader and byte
                    // buffer are released. The disposal is scheduled after this callback because the decode loop that is calling it is the loop the disposal waits for.
                    this.current = null;
                    this.host.reportEnded();
                    Promise.resolve().then(() => this.disposeItemAsync(item)).catch((error) => {
                        console.debug('Audio player: the track that ended at an empty seek could not be released.', error);
                    });
                    return;
                }
            }
            if (this.nextItem)
                return;
            const preferences = this.host.getPreferences();
            if (preferences.isGaplessPlaybackEnabled) {
                // The fallback and the completion are recorded before the wait, so a stalled prefetch can never hold the
                // track open: the renderer ends at the end of the scheduled audio and the player starts the fallback track
                // non-gaplessly. The wait only tries to improve that into a seamless handoff.
                if (!this.nextItem)
                    item.continueToEntry = this.getNextQueueEntry();
                this.renderer.markStreamComplete();
                const token = item.prefetch;
                if (token && !token.cancelled && !token.consumed) {
                    const prefetched = await this.waitForPrefetchAsync(item, token, loopToken, seekToken);
                    if (!this.isCompletionCurrent(item, loopToken, seekToken))
                        return;
                    if (prefetched && this.isPrefetchUsable(prefetched) && !this.nextItem)
                        this.attachNextItem(item, prefetched);
                }
                return;
            }
            if (!this.isCompletionCurrent(item, loopToken, seekToken))
                return;
            this.renderer.markStreamComplete();
        }

        /**
         * Checks whether a decode completion still belongs to the current item and to the decode loop and seek that
         * observed it, so a completion that was overtaken while it waited is abandoned instead of acted on.
         * @param {Object} item - The item whose completion is checked.
         * @param {Object} [loopToken] - The token of the decode loop that observed the completion, when there is one.
         * @param {Object|null} seekToken - The seek token the item carried when the completion was observed.
         * @returns {boolean} True when the completion is still current.
         */
        isCompletionCurrent(item, loopToken, seekToken) {
            return !this.destroyed && this.current === item && !item.stopped && !item.failed && !item.disposed
                && item.seekToken === seekToken
                && (loopToken === undefined || item.loopToken === loopToken);
        }

        /**
         * Waits for a gapless prefetch, bounded by the remaining scheduled audio and by a short maximum. A seek cancels the
         * wait immediately through {@link LuminaAudioPlayer.MediaPipelineEngine#cancelPrefetchWait}, so a seek never waits for a stalled next track that is kept across it.
         * @param {Object} item - The current item whose decode completion is waiting.
         * @param {Object} token - The prefetch token of the current item.
         * @param {Object} [loopToken] - The token of the decode loop that observed the completion.
         * @param {Object|null} seekToken - The seek token the item carried when the completion was observed.
         * @returns {Promise<Object|null>} The ready prefetched item, or null when it is not ready in time.
         */
        waitForPrefetchAsync(item, token, loopToken, seekToken) {
            if (!token || token.cancelled || token.consumed)
                return Promise.resolve(null);
            if (token.ready)
                return Promise.resolve(token.item);
            if (!token.promise)
                return Promise.resolve(null);
            const deadline = this.getPrefetchWaitSeconds();
            if (deadline <= 0)
                return Promise.resolve(null);
            return new Promise((resolve) => {
                let hasSettled = false;
                let timer = null;
                const finish = (value) => {
                    if (hasSettled)
                        return;
                    hasSettled = true;
                    clearTimeout(timer);
                    if (item.prefetchWaitCancel === cancel)
                        item.prefetchWaitCancel = null;
                    resolve(value && token.ready && !token.cancelled && !token.consumed ? value : null);
                };
                const cancel = () => finish(null);
                item.prefetchWaitCancel = cancel;
                timer = setTimeout(() => finish(null), deadline * 1000);
                token.promise.then((prefetched) => {
                    // A completion that a seek already overtook is not handed over, even if the prefetch finished in time.
                    if (item.seekToken !== seekToken || (loopToken !== undefined && item.loopToken !== loopToken))
                        finish(null);
                    else
                        finish(prefetched || null);
                }, () => finish(null));
            });
        }

        /**
         * Gets how long the end of a decode may wait for a gapless prefetch: the remaining scheduled audio minus the handoff margin, capped by the maximum wait.
         * @returns {number} The wait, in seconds, never below zero.
         */
        getPrefetchWaitSeconds() {
            const ahead = Number(this.renderer.getScheduledAheadSeconds());
            const remaining = Number.isFinite(ahead) && ahead > 0 ? ahead : 0;
            return Math.max(0, Math.min(PREFETCH_HANDOFF_MAX_WAIT_SECONDS, remaining - PREFETCH_HANDOFF_MARGIN_SECONDS));
        }

        /**
         * Releases a decode completion that is waiting for a gapless prefetch, used when a seek takes over the timeline.
         * @param {Object} item - The item whose prefetch wait is cancelled.
         */
        cancelPrefetchWait(item) {
            if (!item || !item.prefetchWaitCancel)
                return;
            const cancel = item.prefetchWaitCancel;
            item.prefetchWaitCancel = null;
            cancel();
        }

        /**
         * Checks whether a prefetched item can be appended to the current schedule: it is ready, still alive, and holds
         * decoded audio that can be handed to the renderer.
         * @param {Object} prefetched - The prefetched item.
         * @returns {boolean} True when the item can be handed over.
         */
        isPrefetchUsable(prefetched) {
            return Boolean(prefetched) && !prefetched.failed && !prefetched.stopped && !prefetched.disposed
                && prefetched.scheduler
                && prefetched.scheduler.getScheduledAheadSeconds() > 0;
        }

        //+======================================================================================+
        //|                                  Gapless transitions                                 |
        //+======================================================================================+

        /**
         * Starts prefetching the next item of the player queue for a gapless transition, when the stored preference is on.
         * The prefetch is dropped silently when it fails, because it must never interrupt the track that is playing.
         * @param {Object} item - The item whose next queue entry is prefetched.
         */
        startGaplessPrefetch(item) {
            // Only the item that owns the playhead can have a successor to prefetch. The item that is merely attached as
            // the next handoff starts its own prefetch when it becomes current, so the queue is never traversed by hand.
            if (this.destroyed || !item || item !== this.current || item.stopped || item.disposed || item.prefetch)
                return;
            // A completed decode is not rejected here. A short track, or a short remainder after a seek, can finish its decode before its prefetch is started, and the handoff re-evaluation still consumes a prefetch for it.
            // The paths where completion really makes a handoff impossible, the seek completion and a preference change, check completion before calling this method.
            const preferences = this.host.getPreferences();
            if (!preferences.isGaplessPlaybackEnabled)
                return;
            const nextEntry = this.getNextQueueEntry();
            if (!nextEntry)
                return;
            const nextDescriptor = nextEntry.descriptor;
            const token = {
                cancelled: false,
                consumed: false,
                ready: false,
                entryId: nextEntry.entryId,
                descriptor: nextDescriptor,
                item: null,
                promise: null,
                abortController: typeof AbortController === 'function' ? new AbortController() : null
            };
            item.prefetch = token;
            const phase = (async () => {
                try {
                    // The prefetch waits for the monitor to report that the current track is close enough to its end. No manifest is requested and no
                    // normalized production is started before then, so a skipped or abandoned track never holds a delivery handle or a normalization slot.
                    const isWindowOpen = await this.waitForPrefetchWindowAsync(item, token);
                    if (!isWindowOpen)
                        return null;
                    const signal = token.abortController ? token.abortController.signal : undefined;
                    const manifest = await this.fetchManifestAsync(nextDescriptor, signal);
                    if (token.cancelled || this.destroyed || item.stopped)
                        return null;
                    const prefetched = await this.buildItemAsync(nextDescriptor, manifest, true, (created) => {
                        token.item = created;
                    });
                    prefetched.entryId = token.entryId;
                    token.item = prefetched;
                    // The prefetch downloader is deliberately not started here, even when the player is paused with keep
                    // downloading enabled: its pre-decode was fetched on demand, and its sequential download starts only at the handoff.
                    if (token.cancelled || this.destroyed || item.stopped) {
                        await this.disposeItemAsync(prefetched);
                        return null;
                    }
                    await this.startDecodeLoop(prefetched);
                    if (token.cancelled || prefetched.failed || prefetched.stopped)
                        return null;
                    // The pre-decode window is buffered, so no more bytes are fetched until the handoff. The downloader
                    // never ran for a prefetch, and the abort clears any range request that was still completing, while
                    // keeping the bytes that were fetched. The sequential download starts at the handoff.
                    if (prefetched.source)
                        prefetched.source.setSeeking(false);
                    try {
                        prefetched.downloader.abort();
                    } catch (abortError) {
                        console.debug('Audio player: the prefetch download could not be stopped after pre-decoding.', abortError);
                    }
                    // Ready means the preload loop has stopped and the buffered audio can be appended to the shared schedule without two decode loops feeding the same item.
                    token.ready = true;
                    return prefetched;
                } catch (error) {
                    if (!token.cancelled && !this.destroyed && !LuminaAudioPlayer.isDownloadAbort(error))
                        console.debug('Audio player: the gapless prefetch of the next track failed.', error);
                    return null;
                }
            })();
            token.promise = phase.then((prefetched) => {
                // A prefetch that settled without a usable item is forgotten, so a later preference change or a later decode completion can try again after a transient manifest or network failure.
                if (!prefetched && item.prefetch === token)
                    item.prefetch = null;
                return prefetched;
            });
        }

        /**
         * Waits until the current track is close enough to its end for the prefetch to start. The wait is resolved by the playback monitor when it
         * crosses the lead, and is rejected as false when the prefetch is cancelled, so no timer polls the whole track and a paused track starts nothing until it resumes near the end.
         * @param {Object} item - The current item whose successor is prefetched.
         * @param {Object} token - The prefetch token of the current item.
         * @returns {Promise<boolean>} True when the prefetch window opened, false when the prefetch was cancelled or the item stopped.
         */
        waitForPrefetchWindowAsync(item, token) {
            return new Promise((resolve) => {
                token.windowResolve = resolve;
                this.tryOpenPrefetchWindow(item, token);
            });
        }

        /**
         * Opens the prefetch window when the current track is within the lead of its end, or closes it when the prefetch is no longer wanted.
         * @param {Object} item - The current item whose successor is prefetched.
         * @param {Object} token - The prefetch token of the current item.
         */
        tryOpenPrefetchWindow(item, token) {
            if (!token || !token.windowResolve)
                return;
            if (token.cancelled || this.destroyed || !item || item.stopped || item.disposed || this.current !== item) {
                this.closePrefetchWindow(token);
                return;
            }
            // A completion that already fell back to a normal next-track start has no use for a prefetch, so it is not started only to be thrown away.
            if (item.continueToEntry) {
                this.closePrefetchWindow(token);
                return;
            }
            if (this.getRemainingPlaybackSeconds(item) > PREFETCH_LEAD_SECONDS)
                return;
            const resolve = token.windowResolve;
            token.windowResolve = null;
            resolve(true);
        }

        /**
         * Closes a pending prefetch window as false, so the prefetch phase ends instead of waiting for a track that is no longer current.
         * @param {Object} token - The prefetch token whose window is closed.
         */
        closePrefetchWindow(token) {
            if (!token || !token.windowResolve)
                return;
            const resolve = token.windowResolve;
            token.windowResolve = null;
            resolve(false);
        }

        /**
         * Gets how much playback time is left in an item, from the shared renderer playhead.
         * @param {Object} item - The item whose remaining time is read.
         * @returns {number} The remaining time, in seconds, or zero when the duration is unknown or already reached.
         */
        getRemainingPlaybackSeconds(item) {
            const duration = Number(item && item.duration);
            if (!Number.isFinite(duration) || duration <= 0)
                return 0;
            const absolute = Number(this.renderer.getCurrentTime());
            const local = Number.isFinite(absolute) ? Math.max(0, absolute - Number(item.base || 0)) : 0;
            return Math.max(0, duration - local);
        }

        /**
         * Appends the pre-decoded audio of the next item to the renderer schedule, exactly after the audio of the current
         * track, and lets its decode continue into the same schedule. The current item stays current until the media position of the next item begins.
         * @param {Object} current - The item that is finishing.
         * @param {Object} next - The prefetched item that hands over.
         */
        attachNextItem(current, next) {
            const token = current.prefetch;
            if (token) {
                token.consumed = true;
                next.entryId = token.entryId;
            }
            current.prefetch = null;
            current.continueToEntry = null;
            next.isPreloading = false;
            // The prefetch fetched only the windows its pre-decode needed, so the sequential download of the next track starts now: the decode loop that continues after the buffered frames needs those bytes.
            if (next.source)
                next.source.setSeeking(false);
            // The sequential download of the next track starts only when the player is not paused. While paused it stays stopped, keeping its bytes, and is started by the next resume or by the handoff.
            if (!this.isUserPaused) {
                next.downloader.setPlaybackPaused(false);
                next.downloader.startAsync();
            }
            // Prefetches are built with retries disabled, because an unavailable production abandons the prefetch instead of holding it open.
            // After the handoff this item is the track that owns the playhead, so a temporarily unavailable segment is retried like any current track.
            if (typeof next.downloader.setShouldRetryUnavailable === 'function')
                next.downloader.setShouldRetryUnavailable(true);
            // The end of the scheduled audio of the current track is the exact media position the next track starts at.
            next.base = this.renderer.getScheduledMediaDuration();
            this.nextItem = next;
            // The current decode completion marked the stream complete before it waited for the prefetch. The handoff
            // appends more audio after the current track, so completion must be cleared: a drained schedule from here on
            // is an underrun that the monitor pauses and resumes, not the end of the next track.
            this.renderer.clearStreamComplete();
            if (next.duration > 0)
                this.renderer.setTimelineDuration(next.base + next.duration);
            // The ReplayGain change is scheduled at the context time the next track starts, so it is not applied a reporter tick late after the boundary was detected.
            next.isGainScheduled = false;
            try {
                next.isGainScheduled = this.renderer.scheduleManifestGain(next.manifest, this.getContextTimeForMediaPosition(next.base));
            } catch (error) {
                console.debug('Audio player: the ReplayGain change of the next track could not be scheduled.', error);
            }
            next.scheduler.attach(this.renderer);
            this.startDecodeLoop(next);
        }

        /**
         * Switches the current item to the next one at the media position its audio already started at. It reports the new
         * descriptor to the player queue and applies the ReplayGain values of the new track.
         */
        advanceToNextItem() {
            if (this.isLoadingTrack)
                return;
            const previous = this.current;
            const next = this.nextItem;
            if (!next)
                return;
            // The audio of the next track is already scheduled, so the handoff is bookkeeping: move the engine cursor, move
            // the player cursor by identity, release the previous item and adopt the renderer manifest. The audio cannot be
            // rolled back, so a failure is handled on the new item rather than by undoing the transition.
            this.nextItem = null;
            if (next.failed || next.disposed || next.stopped) {
                // A next track that failed after it was attached can no longer be switched to, so the chain ends with the
                // audio that is already in the schedule instead of advancing the player to a dead item.
                this.disposeItemAsync(next).catch((error) => {
                    console.debug('Audio player: the failed handoff track could not be released.', error);
                });
                this.renderer.cancelTrackGainChange();
                this.renderer.markStreamComplete();
                return;
            }
            this.current = next;
            // A loading state that belonged to the previous track does not follow the gapless handoff, unless the next track is itself waiting for a seek target to arrive.
            if (!next.isSeeking)
                this.host.reportBuffering(false);
            // The audio of the previous track is already in the schedule of the renderer, so its pipeline can be released
            // without affecting the transition, and it is released before anything that can fail below can leak it.
            if (previous)
                this.disposeItemAsync(previous).catch((error) => {
                    console.debug('Audio player: the finished track of the pipeline could not be released.', error);
                });
            // The handoff is committed against the queue entry identity that was actually scheduled. If the player queue
            // changed under it, the bar is moved to that entry instead of describing a different track.
            const scheduledEntryId = next.entryId;
            // The gapless handoff and a normal continuation use the same identity based commit, so a failed advance can never succeed through a second adoption call.
            const advanced = this.host.advanceQueueTo
                ? this.host.advanceQueueTo(scheduledEntryId)
                : null;
            if (advanced)
                next.descriptor = advanced;
            // If the item was attached while the player was paused, a resume that happened before the handoff still left its downloader aborted. It is started now that the item owns the playhead.
            if (!this.isUserPaused && next.downloader)
                next.downloader.startAsync();
            try {
                this.renderer.adoptManifest(next.manifest, next.isGainScheduled === true);
            } catch (error) {
                this.failItem(next, error);
                return;
            }
            next.isGainScheduled = false;
            if (next.duration > 0)
                this.renderer.setTimelineDuration(next.base + next.duration);
            this.host.reportDuration(next.duration);
            // The prefetched downloader fetched its pre-decode window before the handoff, and it may already have all of
            // it, so it will emit no further progress after the player reset the streamed indicator for the new track. The
            // fraction is reported once here so the three-state bar starts at the real streamed state of the new item.
            this.reportItemProgress(next);
            const absolute = this.renderer.getCurrentTime();
            const local = Math.max(0, absolute - next.base);
            this.host.reportTime(next.duration > 0 ? Math.min(local, next.duration) : local, next.duration);
            // The item now owns the engine's playhead, but its successor can only be prefetched when the player queue
            // actually moved to it. If the commit was refused, the player cursor still points at the previous entry, so
            // resolving its next entry would prefetch the track that just started as its own successor.
            if (advanced)
                this.startGaplessPrefetch(next);
            if (next.isDecodeComplete) {
                this.handleDecodeCompleteAsync(next).catch((error) => {
                    console.debug('Audio player: the completion of the handed off track could not be handled.', error);
                });
            } else {
                this.startDecodeLoop(next);
            }
        }

        /**
         * Gets the queue entry that follows the current item, from the player's own queue cursor. The entry carries its
         * identity and its descriptor, so two occurrences of the same track are never confused.
         * @returns {{ entryId: string, descriptor: Object }|null} The following entry, or null at the end of the queue.
         */
        getNextQueueEntry() {
            if (typeof this.host.getNextDescriptor !== 'function')
                return null;
            try {
                return this.host.getNextDescriptor() || null;
            } catch (error) {
                console.debug('Audio player: the next queue entry could not be read.', error);
                return null;
            }
        }

        /**
         * Checks whether a prefetch can survive a seek. A detached prefetch never touched the renderer schedule, so a seek
         * can keep it as long as it still describes the same queue entry identity.
         * @param {Object} token - The prefetch token of the current item.
         * @param {{ entryId: string, descriptor: Object }|null} nextEntry - The successor of the current track after the seek.
         * @returns {boolean} True when the prefetch is still valid and must not be cancelled.
         */
        canKeepPrefetch(token, nextEntry) {
            if (!token || token.cancelled || token.consumed || !token.entryId || !nextEntry)
                return false;
            const prefetched = token.item;
            if (prefetched && (prefetched.failed || prefetched.stopped || prefetched.disposed))
                return false;
            return token.entryId === nextEntry.entryId;
        }

        /**
         * Converts an absolute media position of the shared renderer into the context time at which it will play, so a
         * change can be scheduled to happen exactly at a track boundary rather than when the boundary is noticed.
         * @param {number} mediaTime - The absolute media position, in seconds.
         * @returns {number} The context time, in seconds. Zero when the renderer has no context yet.
         */
        getContextTimeForMediaPosition(mediaTime) {
            // The renderer owns the anchor, the rate and the schedule lead, so the conversion is delegated to it instead of
            // being derived a second time with values that can drift from the audio clock.
            return typeof this.renderer.getContextTimeForMediaPosition === 'function'
                ? this.renderer.getContextTimeForMediaPosition(mediaTime)
                : 0;
        }

        /**
         * Checks whether the media position that the next item starts at was reached, and switches to it when it was.
         * @param {number} absoluteTime - The absolute media position of the shared renderer, in seconds.
         */
        checkForTrackBoundary(absoluteTime) {
            // A seek owns the timeline until it has repositioned every stage, so a boundary that the old timeline crosses
            // during the seek must not invalidate it. A load in progress owns the queue and the player state, so the old timeline must not advance under it either.
            if (!this.nextItem || this.isLoadingTrack || (this.current && this.current.isSeeking))
                return;
            const value = toFiniteNumber(absoluteTime);
            if (value === null || value < this.nextItem.base)
                return;
            this.advanceToNextItem();
        }

        /**
         * Cancels and releases the prefetch of an item, including a prefetched item that was never attached.
         * @param {Object} item - The item whose prefetch is cancelled.
         * @returns {Promise<void>} A promise that completes when the prefetch was released.
         */
        async cancelPrefetchAsync(item) {
            const token = item ? item.prefetch : null;
            if (!token)
                return;
            item.prefetch = null;
            token.cancelled = true;
            // A prefetch that is still waiting for its window is released immediately instead of keeping the teardown open.
            this.closePrefetchWindow(token);
            if (token.abortController) {
                try {
                    token.abortController.abort();
                } catch (abortError) {
                    console.debug('Audio player: the manifest request of the cancelled prefetch could not be aborted.', abortError);
                }
            }
            let prefetched = token.item;
            // A prefetch that is already built is stopped before its promise is awaited, so a decode that is waiting for
            // bytes of a track nobody wants anymore is released instead of holding the teardown open.
            if (prefetched && !prefetched.disposed && !token.consumed) {
                prefetched.stopped = true;
                if (prefetched.scheduler)
                    prefetched.scheduler.stop();
                try {
                    prefetched.downloader.abort();
                } catch (abortError) {
                    console.debug('Audio player: the prefetched download could not be aborted.', abortError);
                }
                // Destroying the decoder releases a decode that is waiting for room, which a prefetch whose buffer is full
                // would otherwise wait for forever, because its schedule is never consumed while it is detached.
                if (prefetched.decoder) {
                    try {
                        await prefetched.decoder.destroyAsync();
                    } catch (decodeError) {
                        console.debug('Audio player: the prefetched decoder could not be destroyed.', decodeError);
                    }
                }
            }
            try {
                prefetched = (await token.promise) || prefetched;
            } catch (error) {
                // The prefetch reports its own failures and resolves null, so a rejection is only a teardown.
            }
            if (token.consumed || !prefetched)
                return;
            await this.disposeItemAsync(prefetched);
        }

        //+======================================================================================+
        //|                                  Playback monitoring                                 |
        //+======================================================================================+

        /**
         * Starts the monitor that detects underruns and gapless track boundaries. It is idempotent.
         */
        startMonitor() {
            if (this.monitorTimer !== null || this.destroyed)
                return;
            this.monitorTimer = setInterval(() => {
                this.monitorPlayback();
            }, MONITOR_INTERVAL_MS);
        }

        /**
         * Stops the playback monitor.
         */
        stopMonitor() {
            if (this.monitorTimer === null)
                return;
            clearInterval(this.monitorTimer);
            this.monitorTimer = null;
        }

        /**
         * Inspects the schedule and the shared timeline: it switches to the next track at its media position and it pauses
         * and resumes the renderer around an underrun. It never runs while the user paused the player.
         * @returns {Promise<void>} A promise that completes when the monitor pass is done.
         */
        async monitorPlayback() {
            if (this.isMonitoring || this.destroyed || this.isUserPaused || !this.current)
                return;
            // A seek owns the timeline until its first frame is scheduled, and a load in progress owns the player state, so
            // the monitor neither pauses the renderer nor advances a boundary of the timeline that is being replaced.
            if (this.current.isSeeking || this.isLoadingTrack)
                return;
            this.tryOpenPrefetchWindow(this.current, this.current.prefetch);
            this.isMonitoring = true;
            try {
                const absolute = this.renderer.getCurrentTime();
                this.checkForTrackBoundary(absolute);
                if (this.isUnderrunPaused) {
                    if (this.renderer.getScheduledAheadSeconds() >= UNDERRUN_RESUME_SECONDS) {
                        this.isUnderrunPaused = false;
                        this.host.reportBuffering(false);
                        await this.renderer.playAsync();
                    }
                    return;
                }
                if (!this.renderer.isPlaying())
                    return;
                if (this.renderer.getScheduledAheadSeconds() > UNDERRUN_THRESHOLD_SECONDS)
                    return;
                // A drained schedule that is also declared complete is the end of the track, not an underrun.
                if (this.renderer.isStreamComplete() && this.current.isDecodeComplete)
                    return;
                // Before the first frame of a freshly loaded or seeked track there is nothing to play yet. Pausing that
                // state would only flap the renderer between paused and playing, so the pause waits for audio to have been scheduled at least once.
                if (!this.current.hasScheduledAudio)
                    return;
                // A seek that started while this pass was running owns the timeline, so the pass does not pause it.
                if (this.current.isSeeking)
                    return;
                this.isUnderrunPaused = true;
                this.host.reportBuffering(true);
                await this.renderer.pauseAsync();
            } catch (error) {
                if (!this.destroyed)
                    this.failItem(this.current, error);
            } finally {
                this.isMonitoring = false;
            }
        }

        /**
         * Handles a playhead report from the renderer. The report is on the shared timeline, so it is translated to the
         * position of the current track and the boundary of the next track is checked first.
         * @param {number} absoluteTime - The absolute media position of the shared renderer, in seconds.
         */
        handleTimeUpdate(absoluteTime) {
            if (this.destroyed)
                return;
            this.checkForTrackBoundary(absoluteTime);
            const item = this.current;
            if (!item)
                return;
            const local = Math.max(0, Number(absoluteTime) - item.base);
            const bounded = item.duration > 0 ? Math.min(local, item.duration) : local;
            this.host.reportTime(bounded, item.duration);
        }

        /**
         * Handles the end of the scheduled audio reported by the renderer.
         */
        handleEnded() {
            if (this.destroyed)
                return;
            // An end of the timeline that a new load is replacing is not reported: the load owns the player state now.
            if (this.isLoadingTrack)
                return;
            // An end that the old timeline reports while a seek is repositioning the pipeline belongs to the timeline the seek is replacing, not to the target it is moving to.
            if (this.current && this.current.isSeeking)
                return;
            this.stopMonitor();
            this.isUnderrunPaused = false;
            const current = this.current;
            // The next entry is re-resolved at the end, not taken from the value cached at decode completion: a queue that
            // changed in between may no longer offer a successor, in which case the track simply ends.
            const nextEntry = this.getNextQueueEntry();
            if (current && current.continueToEntry && nextEntry && !this.isUserPaused
                && typeof this.host.requestNextTrackAsync === 'function') {
                // The engine reports the structural fact and lets the player's own play path start the next entry, so the
                // controller keeps ownership of the state, the pause and stop intent, and the capability gate. The loading
                // state spans the end gap: the continuation load keeps it across the teardown of the finished track.
                current.continueToEntry = null;
                this.host.reportBuffering(true);
                Promise.resolve(this.host.requestNextTrackAsync('gapless-prefetch-failed'))
                    .then((hasTakenOwnership) => {
                        if (!hasTakenOwnership)
                            this.finishEndedTrack(current);
                    })
                    .catch((error) => {
                        console.debug('Audio player: the player could not start the next track after a gapless fallback.', error);
                        this.finishEndedTrack(current);
                    });
                return;
            }
            this.host.reportBuffering(false);
            if (current)
                this.finishEndedTrack(current);
            else
                this.host.reportEnded();
        }

        /**
         * Releases a finished item and reports the end to the player, unless a newer load already took ownership of the pipeline in the meantime.
         * @param {Object} item - The item that ended.
         */
        finishEndedTrack(item) {
            if (this.current !== item)
                return;
            this.current = null;
            this.host.reportBuffering(false);
            this.host.reportEnded();
            this.disposeItemAsync(item).catch((error) => {
                console.debug('Audio player: the ended track of the pipeline could not be released.', error);
            });
        }

        /**
         * Follows the stored playback preferences. Gapless playback is honored from the moment it is turned on, and a
         * prefetch that becomes unwanted is released before its audio is appended to the schedule.
         * @param {Object} preferences - The preferences payload of the player.
         */
        handlePreferencesChanged(preferences) {
            if (this.destroyed || !preferences || typeof preferences.isGaplessPlaybackEnabled !== 'boolean')
                return;
            const item = this.current;
            if (!item)
                return;
            if (preferences.isGaplessPlaybackEnabled === false) {
                this.cancelPrefetchAsync(item).catch((error) => {
                    console.debug('Audio player: the disabled gapless prefetch could not be released.', error);
                });
                // A next item that was already appended to the schedule is cancelled too: the appended audio is removed, the
                // current track is allowed to end at the boundary, and the player then starts the next entry non-gaplessly.
                // The boundary may already be behind the playhead, in which case the next track is already audible and
                // truncating it would rewind the timeline and replay audio that has played. That transition is left alone.
                const next = this.nextItem;
                if (next && typeof this.renderer.truncateScheduleAt === 'function'
                    && next.base > this.renderer.getCurrentTime()) {
                    this.nextItem = null;
                    this.renderer.cancelTrackGainChange();
                    this.renderer.truncateScheduleAt(next.base);
                    item.continueToEntry = this.getNextQueueEntry();
                    this.renderer.markStreamComplete();
                    this.disposeItemAsync(next).catch((error) => {
                        console.debug('Audio player: the disabled gapless handoff could not be released.', error);
                    });
                }
                return;
            }
            // A decode that has already completed made its end decision before gapless was enabled, so a prefetch started now could never be appended to the schedule.
            if (!this.nextItem && !item.isDecodeComplete)
                this.startGaplessPrefetch(item);
        }

        /**
         * Subscribes the engine to the stored preference changes of the player.
         */
        subscribeToPreferences() {
            if (!this.events || !LuminaAudioPlayer.PlayerEvent || typeof this.events.on !== 'function')
                return;
            this.unsubscribePreferences = this.events.on(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, (preferences) => {
                this.handlePreferencesChanged(preferences);
            });
        }

        //+======================================================================================+
        //|                                  Failures and teardown                               |
        //+======================================================================================+

        /**
         * Handles a download failure of an item. A prefetch failure is dropped; the failure of the current track is
         * reported to the player, which moves to its error state without the pipeline leaving anything running.
         * @param {Object} item - The item that failed.
         * @param {*} error - The failure.
         */
        handleDownloadFailure(item, error) {
            if (LuminaAudioPlayer.isDownloadAbort(error))
                return;
            this.failItem(item, error);
        }

        /**
         * Checks whether a failure is a delivery that the server no longer accepts, which the player can fix by fetching the manifest again.
         * @param {Object} item - The item that failed.
         * @param {*} error - The failure.
         * @returns {boolean} True when the delivery should be refreshed and playback resumed at the current position.
         */
        shouldRecoverDelivery(item, error) {
            if (!item || item !== this.current || item.deliveryRecoveryAttempted || item.deliveryRecoveryInProgress || this.destroyed)
                return false;
            if (!error || error.code !== LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED || !error.details)
                return false;
            const reason = error.details.reason;
            return reason === LuminaAudioPlayer.PlayerErrorReason.SEGMENT_NOT_FOUND
                || reason === LuminaAudioPlayer.PlayerErrorReason.PLAYBACK_DELIVERY_CHANGED;
        }

        /**
         * Refetches the manifest of the current track and rebuilds the normalized pipeline at the current playback position, used when the
         * delivery handle expired, the API restarted, or the source file changed since the manifest was read.
         * @param {Object} item - The item whose delivery must be refreshed.
         * @param {*} originalError - The failure that triggered the refresh.
         * @returns {Promise<void>} A promise that completes when playback resumed on the refreshed delivery.
         */
        async recoverDeliveryAsync(item, originalError) {
            // The attempt is marked before the first await, because the teardown below can raise failures, and a second recovery started
            // from one of them would race this one for the same item.
            item.deliveryRecoveryInProgress = true;
            item.deliveryRecoveryAttempted = true;
            // An underrun pause is a temporary buffer stall, not a user pause, so it must be resumed after the refresh just like active playback.
            const wasPlaying = !this.isUserPaused && (this.renderer.isPlaying() || this.isUnderrunPaused);
            // The renderer time is shared by the whole queue, while the position to resume at is relative to this track.
            const position = Math.max(0, Number(this.renderer.getCurrentTime()) - Number(item.base || 0));
            // Stop observing the old pipeline before pausing it, so the monitor does not treat the intentional pause as an underrun and does
            // not schedule more work against the delivery that is being replaced.
            this.stopMonitor();
            this.isUnderrunPaused = false;
            this.host.reportBuffering(true);
            try {
                await this.renderer.pauseAsync();
            } catch (pauseError) {
                // The old item is released below regardless, so a renderer that refuses to pause must not abort the recovery.
                console.debug('Audio player: the renderer could not be paused before refreshing the delivery.', pauseError);
            }
            // A prefetched successor was built from the old manifest, so keeping it would hand off to the expired delivery handle.
            this.cancelPrefetchWait(item);
            if (item.prefetch) {
                try {
                    await this.cancelPrefetchAsync(item);
                } catch (prefetchError) {
                    console.debug('Audio player: the prefetch of the failed delivery could not be released.', prefetchError);
                }
            }
            // An already attached next track may have audio scheduled from the old delivery, so it is released before the new manifest
            // is read instead of letting two deliveries meet at the track boundary.
            if (this.nextItem) {
                const attachedNextItem = this.nextItem;
                this.nextItem = null;
                await this.disposeItemAsync(attachedNextItem).catch((disposeError) => {
                    console.debug('Audio player: the attached next track of the failed delivery could not be released.', disposeError);
                });
            }
            // The old decode loop and downloads must stop before the replacement is built, because their aborts and failures belong to the
            // delivery being replaced and must not be mistaken for failures of the new one.
            item.loopToken = null;
            if (item.downloader)
                item.downloader.abort();

            let manifest;
            try {
                manifest = await this.fetchManifestAsync(item.descriptor);
            } catch (manifestError) {
                // The user needs the failure that interrupted playback, not the recovery attempt that also failed.
                console.debug('Audio player: the manifest of the failed delivery could not be refreshed.', manifestError);
                throw originalError;
            }
            // The awaits above can be overtaken by a new load or by teardown, and recovery must never touch a different current item.
            if (this.destroyed || this.current !== item)
                return;
            if (manifest.isDirectPlay === true) {
                // The server now considers the track a direct play. Converting the running normalized pipeline to the direct stream is a different
                // transition, so the track ends with the original failure instead of playing mixed deliveries.
                throw originalError;
            }
            try {
                // ReplayGain and timeline metadata can change with the refreshed manifest, so the renderer is configured before the
                // replacement pipeline starts producing audio.
                this.renderer.configure(manifest);
            } catch (configureError) {
                console.debug('Audio player: the renderer could not be configured for the refreshed delivery.', configureError);
                throw originalError;
            }

            let replacement;
            try {
                // The replacement owns fresh downloader, demuxer and decoder state bound to the new delivery handle.
                replacement = await this.buildItemAsync(item.descriptor, manifest, false);
            } catch (buildError) {
                console.debug('Audio player: the refreshed delivery could not be prepared.', buildError);
                throw originalError;
            }
            // Building the replacement took another await, so it is discarded if the user left this track in the meantime.
            if (this.destroyed || this.current !== item) {
                await this.disposeItemAsync(replacement);
                return;
            }
            // The replacement starts at the beginning of this track on the shared timeline. The attempted flag is carried over so a delivery
            // that is still rejected after the refresh cannot make the player refetch the manifest forever.
            replacement.base = 0;
            replacement.deliveryRecoveryAttempted = true;
            this.current = replacement;
            if (replacement.duration > 0)
                this.renderer.setTimelineDuration(replacement.duration);
            this.host.reportDuration(replacement.duration);
            await this.disposeItemAsync(item);
            // A seek rebuilds every stage at the requested position. At zero there is nothing to seek, so the decode loop and its prefetch are started explicitly.
            if (position > 0)
                await this.seekAsync(position);
            else {
                this.startDecodeLoop(replacement);
                this.startGaplessPrefetch(replacement);
            }
            // Seeking is asynchronous, so the user may have paused, stopped or replaced the track while it ran. Do not resume a pipeline that no longer owns the playhead.
            if (this.current !== replacement || replacement.failed || replacement.stopped || this.destroyed)
                return;
            // Resume only if audio was actually being heard before the refresh and the user has not paused since. A user pause must stay a pause, while an underrun pause is restored.
            if (wasPlaying && !this.isUserPaused) {
                await this.renderer.playAsync();
                this.startMonitor();
            }
            this.host.reportBuffering(false);
        }

        /**
         * Records a failure of an item once and reports it to the player when the item is the current one.
         * @param {Object} item - The item that failed.
         * @param {*} error - The failure.
         */
        failItem(item, error) {
            if (!item || item.failed || item.stopped || item.disposed || this.destroyed)
                return;
            if (LuminaAudioPlayer.isDownloadAbort(error))
                return;
            // Errors raised by the old pipeline while it is being replaced by a refreshed delivery belong to that teardown, not to a new failure.
            if (item.deliveryRecoveryInProgress)
                return;
            if (this.shouldRecoverDelivery(item, error)) {
                this.recoverDeliveryAsync(item, error).catch((recoveryError) => {
                    console.debug('Audio player: the refreshed delivery could not be applied.', recoveryError);
                    item.deliveryRecoveryInProgress = false;
                    this.failItem(item, error);
                });
                return;
            }
            item.failed = true;
            item.stopped = true;
            try {
                item.downloader.abort();
            } catch (abortError) {
                console.debug('Audio player: the failed download could not be aborted.', abortError);
            }
            if (item === this.nextItem) {
                // The next track was already appended to the schedule but has not started yet, so the chain ends with the
                // audio that is already scheduled and the player is not advanced to an item that can no longer play.
                this.nextItem = null;
                this.renderer.cancelTrackGainChange();
                this.renderer.markStreamComplete();
                this.host.reportBuffering(false);
                this.disposeItemAsync(item).catch((disposeError) => {
                    console.debug('Audio player: the failed handoff track could not be released.', disposeError);
                });
                return;
            }
            if (item !== this.current) {
                console.debug('Audio player: a prefetched track of the pipeline failed.', error);
                // A failed prefetch is released here instead of waiting for the next load or for the teardown, so its
                // decoder, demuxer, downloader and buffered frames never outlive the failure.
                this.disposeItemAsync(item).catch((disposeError) => {
                    console.debug('Audio player: the failed prefetch could not be released.', disposeError);
                });
                return;
            }
            this.stopMonitor();
            this.isUnderrunPaused = false;
            item.isSeeking = false;
            if (item.scheduler)
                item.scheduler.stop();
            if (item.source)
                item.source.setSeeking(false);
            this.host.reportBuffering(false);
            // The audio that is already scheduled is suspended before the failure is reported, so a failed track never keeps playing while the player shows its error state.
            this.renderer.pauseAsync().catch((pauseError) => {
                console.debug('Audio player: the renderer could not be paused after a failure.', pauseError);
            });
            this.host.reportError(LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR));
            // The failed item is not retained in the error state: its decoder, demuxer, downloader, prefetch and scheduled
            // sources are released now. The player refuses seeks and resumes in the error state anyway, and a replay starts
            // a fresh load.
            if (this.current === item)
                this.current = null;
            this.disposeItemAsync(item).catch((disposeError) => {
                console.debug('Audio player: the failed track could not be released.', disposeError);
            });
        }

        /**
         * Waits for the decode loop of an item to stop.
         * @param {Object} item - The item whose loop is awaited.
         * @returns {Promise<void>} A promise that completes when the loop stopped.
         */
        async waitForDecodeLoopAsync(item) {
            const loop = item.loopPromise;
            if (!loop)
                return;
            try {
                await loop;
            } catch (error) {
                // The loop reports its own failures, so a teardown never surfaces them.
            }
        }

        /**
         * Rebuilds the demuxer of an item from the beginning of its stream, used when a seek cannot be performed by time.
         * @param {Object} item - The item whose demuxer is rebuilt.
         * @returns {Promise<void>} A promise that completes when the new demuxer is ready.
         */
        async rebuildDemuxerAsync(item) {
            const previous = item.demuxer;
            item.demuxer = null;
            if (previous)
                await previous.destroyAsync();
            if (item.stopped || item.disposed)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The track was released while its demuxer was rebuilt.');
            const demuxer = await LuminaAudioPlayer.createDemuxerForManifest(item.manifest, {
                source: item.source || item.downloader,
                capabilities: this.capabilities,
                events: this.events,
                onError: (error) => this.failItem(item, error)
            });
            if (!demuxer)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The track cannot be demuxed again after a seek.');
            if (item.stopped || item.disposed) {
                // The item was released while the demuxer probed its header, so the new demuxer must not be installed after the disposal already walked past a null demuxer.
                await demuxer.destroyAsync();
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The track was released while its demuxer was rebuilt.');
            }
            item.demuxer = demuxer;
            const configuration = demuxer.getConfiguration();
            if (configuration)
                item.decoder.configure(configuration);
        }

        /**
         * Releases every resource of a pipeline item. It is idempotent.
         * @param {Object} item - The item to release.
         * @returns {Promise<void>} A promise that completes when the item was released.
         */
        async disposeItemAsync(item) {
            if (!item || item.disposed)
                return;
            // The flags are set before any await, so callbacks raised by the teardown itself see the item as dead instead of failing it again.
            item.disposed = true;
            item.stopped = true;
            item.loopToken = null;
            this.cancelPrefetchWait(item);
            // Stop scheduling before the decoder is torn down, so a frame produced during teardown can never enter the renderer schedule.
            if (item.scheduler)
                item.scheduler.stop();
            // The item may itself have prefetched the track after it, which must not keep downloading or decoding once the
            // item is released, for example when a seek drops a whole gapless chain.
            if (item.prefetch) {
                try {
                    await this.cancelPrefetchAsync(item);
                } catch (error) {
                    console.debug('Audio player: the prefetch of a released track could not be released.', error);
                }
            }
            if (item.downloader) {
                try {
                    item.downloader.abort();
                } catch (abortError) {
                    console.debug('Audio player: the download of a released track could not be aborted.', abortError);
                }
            }
            // The decoder is destroyed before the decode loop is awaited, because destroying it releases a decode that is
            // waiting for room, which is the only thing that can keep the loop from noticing the item stopped.
            if (item.decoder) {
                try {
                    await item.decoder.destroyAsync();
                } catch (error) {
                    console.debug('Audio player: the decoder of a released track could not be destroyed.', error);
                }
            }
            await this.waitForDecodeLoopAsync(item);
            // The loop has exited, so the scheduler and the demuxer it could still touch are released next.
            if (item.scheduler) {
                try {
                    item.scheduler.dispose();
                } catch (error) {
                    console.debug('Audio player: the scheduler of a released track could not be released.', error);
                }
            }
            if (item.demuxer) {
                try {
                    await item.demuxer.destroyAsync();
                } catch (error) {
                    console.debug('Audio player: the demuxer of a released track could not be destroyed.', error);
                }
            }
            if (item.source)
                item.source.destroy();
            // The downloader is destroyed last, because the demuxer and the byte source may hold its reads until they are released.
            if (item.downloader) {
                try {
                    await item.downloader.destroyAsync();
                } catch (error) {
                    console.debug('Audio player: the downloader of a released track could not be destroyed.', error);
                }
            }
        }

        /**
         * Releases the item that is playing, the prefetch that feeds it and the item that was already attached to the renderer schedule, without touching the renderer itself.
         * @param {Object|null} [loadTokenToAbort] - A load token whose manifest request must be aborted, used by the
         * destroy path. A load never aborts its own token here, because the request it is about to make belongs to it.
         * @param {Object|null} [loadToken] - The token of the load that owns this teardown. Shared state is only touched
         * after the awaited disposals when this is still the current token.
         * @param {boolean} [keepBuffering=false] - True to keep a loading state that was handed to the load by the player,
         * used by an automatic continuation so the end gap keeps showing that the next track is being prepared.
         * @returns {Promise<void>} A promise that completes when the playback resources were released.
         */
        async teardownPlaybackAsync(loadTokenToAbort = null, loadToken = null, keepBuffering = false) {
            // The items are captured and detached synchronously, so a teardown that is overtaken while it awaits can only
            // release the pipeline it captured and can never null or dispose the item a newer load installed.
            const current = this.current;
            const next = this.nextItem;
            this.current = null;
            this.nextItem = null;
            const prefetchedInFlight = current && current.prefetch ? current.prefetch.item : null;
            this.abortLoad(loadTokenToAbort);
            this.stopMonitor();
            this.isUnderrunPaused = false;
            // Every stage is stopped before the renderer is reset: the schedulers drop the frames that are still in flight,
            // the downloaders release the reads that are waiting, and only then does the shared renderer stop its schedule,
            // so a decoder cannot append a frame of the replaced track after the reset.
            for (const item of [current, next, prefetchedInFlight]) {
                if (!item)
                    continue;
                item.stopped = true;
                item.loopToken = null;
                if (item.scheduler)
                    item.scheduler.stop();
                if (item.downloader) {
                    try {
                        item.downloader.abort();
                    } catch (abortError) {
                        console.debug('Audio player: the download of the replaced track could not be aborted.', abortError);
                    }
                }
            }
            try {
                this.renderer.cancelTrackGainChange();
            } catch (error) {
                console.debug('Audio player: the scheduled ReplayGain change of the replaced track could not be cleared.', error);
            }
            try {
                this.renderer.seekTo(0);
            } catch (error) {
                console.debug('Audio player: the schedule of the replaced track could not be stopped.', error);
            }
            // The captured items are released even when this teardown was overtaken, because the newer teardown already saw
            // a null current and would otherwise leak them. No shared state is read or written after these awaits.
            if (next)
                await this.disposeItemAsync(next);
            if (current)
                await this.disposeItemAsync(current);
            if (!keepBuffering && (!loadToken || this.loadToken === loadToken))
                this.host.reportBuffering(false);
        }

        //+======================================================================================+
        //|                                     Manifest access                                  |
        //+======================================================================================+

        /**
         * Fetches the playback manifest of a track from the web application.
         * @param {Object} descriptor - The play descriptor of the track.
         * @param {AbortSignal} [signal] - An optional signal that cancels the request, used by a cancelled prefetch.
         * @returns {Promise<Object>} The playback manifest.
         * @throws {LuminaAudioPlayer.PlayerError} When the descriptor is incomplete, the session expired, or the manifest
         * could not be read.
         */
        async fetchManifestAsync(descriptor, signal) {
            // The injected transport wins over the host, exactly like the injected configuration does.
            const transport = (typeof this.options.fetchJsonAsync === 'function')
                ? this.options.fetchJsonAsync
                : ((this.host && typeof this.host.fetchJsonAsync === 'function')
                    ? (requestUrl, requestSignal, label, requestOptions) => this.host.fetchJsonAsync(requestUrl, requestSignal, label, requestOptions)
                    : null);
            return LuminaAudioPlayer.fetchPlaybackManifestAsync({
                configuration: this.getConfiguration(),
                descriptor: descriptor,
                capabilities: await this.describeCapabilitiesAsync(),
                transport: transport,
                signal: signal
            });
        }

        /**
         * Describes the container and codec pairs the browser can play directly, which the playback format service uses to decide between direct play, remux and transcode.
         * @returns {Promise<string>} The comma separated capability list, or an empty string when it cannot be built.
         */
        async describeCapabilitiesAsync() {
            if (typeof LuminaAudioPlayer.describeDemuxerFormatsAsync !== 'function')
                return '';
            try {
                return await LuminaAudioPlayer.describeDemuxerFormatsAsync({
                    capabilities: this.capabilities,
                    events: this.events
                });
            } catch (error) {
                // The list is an optimization of the delivery decision, so a failed probe falls back to the static registry rather than making the track unplayable.
                console.debug('Audio player: the decode aware capability list could not be built.', error);
                return typeof LuminaAudioPlayer.describeDemuxerFormats === 'function' ? LuminaAudioPlayer.describeDemuxerFormats() : '';
            }
        }

        /**
         * Gets the endpoints configuration of the player.
         * @returns {Object} The endpoints configuration.
         */
        getConfiguration() {
            if (this.options.configuration)
                return this.options.configuration;
            return this.host && typeof this.host.getConfiguration === 'function' ? this.host.getConfiguration() : {};
        }

    }

    LuminaAudioPlayer.MediaPipelineEngine = MediaPipelineEngine;

    /**
     * Creates a media pipeline engine. See {@link LuminaAudioPlayer.MediaPipelineEngine} for the options.
     * @param {Object} host - The engine host provided by the player controller.
     * @param {Object} [options] - The engine options.
     * @returns {LuminaAudioPlayer.MediaPipelineEngine} The created engine.
     */
    LuminaAudioPlayer.createMediaPipelineEngine = function (host, options) {
        return new MediaPipelineEngine(host, options);
    };

    /**
     * Registers the media pipeline engine on a player and makes it the selected engine. The player is the singleton of
     * the application, so this is what turns the control API and the bar into an actually audible player.
     * @param {LuminaAudioPlayer.AudioPlayer} player - The player to install the engine on.
     * @returns {LuminaAudioPlayer.AudioPlayer} The same player, for chaining.
     */
    LuminaAudioPlayer.installMediaPipelineEngine = function (player) {
        if (!player)
            return player;
        player.registerEngine(LuminaAudioPlayer.PIPELINE_ENGINE_NAME, (host) => new MediaPipelineEngine(host));
        return player.useEngine(LuminaAudioPlayer.PIPELINE_ENGINE_NAME);
    };

    /**
     * Creates the singleton player when it does not exist yet and installs the media pipeline engine on it. It is the one
     * composition point of the engine, and it requires the player module to be loaded first, which is the order the layout
     * uses. When the player module is absent the install is skipped instead of throwing, so loading this module early is harmless; the engine is then simply not selected.
     */
    function installOnDefaultPlayer() {
        if (typeof LuminaAudioPlayer.createDefaultPlayer !== 'function')
            return;
        const player = LuminaAudioPlayer.createDefaultPlayer();
        LuminaAudioPlayer.installMediaPipelineEngine(player);
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', installOnDefaultPlayer);
    else
        installOnDefaultPlayer();
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
