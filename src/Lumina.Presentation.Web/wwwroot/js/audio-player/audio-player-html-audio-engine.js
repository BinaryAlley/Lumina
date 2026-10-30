/**
 * HTML audio fallback engine of the audio player.
 *
 * The media pipeline decodes with WebCodecs and renders with the Web Audio API, so a browser that hides either of those
 * APIs cannot use it. A privacy hardened browser can hide WebCodecs even though its native HTML audio element still plays
 * common formats, so this engine gives such a browser a degraded but usable player instead of a player that refuses every
 * track. It implements the engine interface documented in audio-player-engine.js with a single HTMLAudioElement.
 *
 * What it owns
 * ------------
 * A track is the playback manifest of the track, used to obtain the validated stream path, the declared duration and the
 * ReplayGain values, and one HTMLAudioElement that fetches the original stream through the range capable stream route.
 * The browser decodes the audio with its own native media stack, so a format the browser cannot decode fails normally
 * instead of being normalized by the server, and a normalized delivery cannot be stitched from its independent segments by this engine.
 *
 * ReplayGain and the VU meter
 * ---------------------------
 * When the browser also provides the Web Audio API, the element is routed through a graph that mirrors the full renderer:
 * the element feeds a ReplayGain gain, which feeds the analyzer of the VU meter, which feeds an output gain, which feeds
 * the destination. ReplayGain and the VU meter therefore keep working in the fallback. If the Web Audio API is missing
 * too, the element plays through its own output without them, because there is no graph through which to apply or measure the gain.
 *
 * Genuine limits
 * --------------
 * The engine provides no true gapless transition, because an HTMLMediaElement cannot append the next track sample
 * accurately. It also provides no server normalized delivery, because the independent segments cannot be stitched into
 * one element source. Seeking itself is delegated to the element, which fetches the range of the target.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The name under which the HTML audio fallback engine is registered.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.HTML_AUDIO_ENGINE_NAME = 'htmlAudio';

    /**
     * The timeout, in milliseconds, of a load that waits for the metadata of the element.
     * @readonly
     * @type {number}
     */
    const METADATA_TIMEOUT_MS = 30000;

    /**
     * The transform size of the analyzer that taps the output for the VU meter, matching the analyzer of the full player.
     * @readonly
     * @type {number}
     */
    const OUTPUT_ANALYSER_FFT_SIZE = 2048;

    /**
     * The ramp, in seconds, of a volume or mute change, so a change of the output gain never clicks.
     * @readonly
     * @type {number}
     */
    const OUTPUT_GAIN_RAMP_SECONDS = 0.02;

    /**
     * The ramp, in seconds, of a ReplayGain change, applied when a track starts or the preference changes.
     * @readonly
     * @type {number}
     */
    const REPLAY_GAIN_RAMP_SECONDS = 0.05;

    /**
     * The native formats the fallback advertises to the playback manifest, as the container and codec pairs the playback
     * format service understands. The first type the element reports it can play selects the pair, so a browser that
     * exposes several MIME aliases for one format still advertises the pair once.
     * @readonly
     * @type {ReadonlyArray<{ types: ReadonlyArray<string>, container: string, codec: string }>}
     */
    const NATIVE_FORMAT_PROBES = Object.freeze([
        Object.freeze({ types: Object.freeze(['audio/mpeg', 'audio/mp3']), container: 'mp3', codec: 'mp3' }),
        Object.freeze({ types: Object.freeze(['audio/flac', 'audio/x-flac']), container: 'flac', codec: 'flac' }),
        Object.freeze({ types: Object.freeze(['audio/ogg; codecs="opus"']), container: 'ogg', codec: 'opus' }),
        Object.freeze({ types: Object.freeze(['audio/ogg; codecs="vorbis"']), container: 'ogg', codec: 'vorbis' }),
        Object.freeze({ types: Object.freeze(['audio/wav', 'audio/wave', 'audio/x-wav']), container: 'wav', codec: 'pcm' }),
        Object.freeze({ types: Object.freeze(['audio/aac', 'audio/aacp']), container: 'aac', codec: 'aac' }),
        Object.freeze({ types: Object.freeze(['audio/mp4; codecs="mp4a.40.2"', 'audio/x-m4a']), container: 'mp4', codec: 'aac' })
    ]);

    /**
     * Coerces a value to a finite number, or null when it is absent or not finite.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The finite number, or null.
     */
    function toFiniteNumber(value) {
        return LuminaAudioPlayer.toFiniteNumber(value);
    }

    /**
     * Clamps a number into an inclusive range.
     * @param {number} value - The value to clamp.
     * @param {number} minimum - The inclusive minimum.
     * @param {number} maximum - The inclusive maximum.
     * @returns {number} The clamped value.
     */
    function clamp(value, minimum, maximum) {
        return Math.min(maximum, Math.max(minimum, value));
    }

    /**
     * Builds the container and codec pairs the native media stack of the browser reports it can play.
     * @param {HTMLAudioElement} audio - The element whose canPlayType probe answers the question.
     * @returns {string} The comma separated capability list, or an empty string when nothing can be played.
     */
    function describeNativeCapabilities(audio) {
        const pairs = [];
        for (const probe of NATIVE_FORMAT_PROBES) {
            const canPlay = probe.types.some((type) => {
                try {
                    return audio.canPlayType(type) !== '';
                } catch (error) {
                    return false;
                }
            });
            if (canPlay)
                pairs.push(`${probe.container}/${probe.codec}`);
        }
        return pairs.join(',');
    }

    /**
     * The HTMLAudioElement fallback engine. See the module documentation for its behavior.
     */
    class HtmlAudioPlayerEngine {
        /**
         * Creates an instance of HtmlAudioPlayerEngine.
         * @param {Object} host - The engine host provided by the player controller.
         * @param {Object} [options] - The engine options, for injection in a test.
         * @param {HTMLAudioElement} [options.audio] - The audio element to use instead of creating one.
         */
        constructor(host, options = {}) {
            this.host = host;
            this.options = options || {};
            this.audio = this.options.audio || new Audio();
            this.audio.preload = 'auto';
            this.descriptor = null;
            this.manifest = null;
            this.duration = 0;
            this.loadToken = null;
            this.isLoading = false;
            this.isDestroyed = false;
            this.eventHandlers = [];
            // The output gain carries the volume and mute state, the track gain carries ReplayGain, and the analyzer taps the
            // track level before the output gain, exactly like the full renderer. The graph is optional: a browser without
            // the Web Audio API keeps the plain element output, without ReplayGain or a VU meter.
            this.audioContext = null;
            this.mediaSourceNode = null;
            this.trackGainNode = null;
            this.analyserNode = null;
            this.outputGainNode = null;
            this.ownsAudioContext = !this.options.audioContext;
            this.hasTriedAudioGraph = false;
            this.isAudioGraphReady = false;
            this.volume = 1;
            this.replayGainMode = this.readReplayGainMode();
            this.unsubscribePreferences = null;
            this.bindAudioEvents();
            this.subscribeToPreferences();
        }

        /**
         * Reads the ReplayGain mode the player currently holds.
         * @returns {string} The mode, or the off mode when the host provides none.
         */
        readReplayGainMode() {
            const preferences = typeof this.host.getPreferences === 'function' ? this.host.getPreferences() : null;
            return preferences && typeof preferences.replayGainMode === 'string'
                ? preferences.replayGainMode
                : 'Off';
        }

        /**
         * Follows the ReplayGain mode of the player while a track plays, so a settings change applies without a new load.
         */
        subscribeToPreferences() {
            const events = this.host && this.host.events;
            if (!events || typeof events.on !== 'function' || !LuminaAudioPlayer.PlayerEvent)
                return;
            this.unsubscribePreferences = events.on(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, (preferences) => {
                if (!preferences || typeof preferences.replayGainMode !== 'string')
                    return;
                this.replayGainMode = preferences.replayGainMode;
                this.applyReplayGain(REPLAY_GAIN_RAMP_SECONDS);
            });
        }

        /**
         * Subscribes to the events of the element that the engine forwards to the player.
         */
        bindAudioEvents() {
            const listen = (event, handler) => {
                const bound = handler.bind(this);
                this.audio.addEventListener(event, bound);
                this.eventHandlers.push({ event, handler: bound });
            };
            listen('timeupdate', this.handleTimeUpdate);
            listen('durationchange', this.handleDurationChange);
            listen('loadedmetadata', this.handleDurationChange);
            listen('progress', this.handleProgress);
            listen('waiting', this.handleWaiting);
            listen('stalled', this.handleWaiting);
            listen('playing', this.handleCanPlay);
            listen('canplay', this.handleCanPlay);
            listen('canplaythrough', this.handleCanPlay);
            listen('seeked', this.handleSeeked);
            listen('ended', this.handleEnded);
            listen('error', this.handleError);
        }

        /**
         * Releases every listener the engine added to the element.
         */
        unbindAudioEvents() {
            for (const entry of this.eventHandlers)
                this.audio.removeEventListener(entry.event, entry.handler);
            this.eventHandlers = [];
        }

        /**
         * Creates the optional Web Audio graph around the element: the element feeds the ReplayGain gain, which feeds the
         * analyzer of the VU meter, which feeds the output gain, which feeds the destination. The output gain carries the
         * volume and mute state, so the meter stays independent of them, exactly like the full renderer.
         *
         * The graph is only created once. A browser that provides no AudioContext, or a context that cannot wrap the element,
         * leaves the element on its own native output, without ReplayGain or a VU meter, instead of failing the track.
         * @returns {boolean} True when the graph is available, false when the element plays natively.
         */
        ensureAudioGraph() {
            if (this.isAudioGraphReady)
                return true;
            if (this.hasTriedAudioGraph)
                return false;
            this.hasTriedAudioGraph = true;
            let context = this.options.audioContext || null;
            if (!context) {
                const targetWindow = typeof window !== 'undefined' ? window : null;
                const ContextConstructor = targetWindow
                    ? (targetWindow.AudioContext || targetWindow.webkitAudioContext)
                    : null;
                if (typeof ContextConstructor !== 'function')
                    return false;
                try {
                    context = new ContextConstructor();
                } catch (error) {
                    console.debug('Audio player: the fallback audio context could not be created.', error);
                    return false;
                }
            }
            if (!context
                || typeof context.createMediaElementSource !== 'function'
                || typeof context.createGain !== 'function') {
                if (this.ownsAudioContext && context && typeof context.close === 'function') {
                    try {
                        context.close();
                    } catch (error) {
                        console.debug('Audio player: the unusable fallback audio context could not be closed.', error);
                    }
                }
                return false;
            }
            this.audioContext = context;
            this.ownsAudioContext = !this.options.audioContext;
            try {
                const mediaSource = context.createMediaElementSource(this.audio);
                const trackGain = context.createGain();
                const outputGain = context.createGain();
                this.mediaSourceNode = mediaSource;
                this.trackGainNode = trackGain;
                this.outputGainNode = outputGain;
                let analyser = null;
                if (typeof context.createAnalyser === 'function') {
                    try {
                        analyser = context.createAnalyser();
                        analyser.fftSize = OUTPUT_ANALYSER_FFT_SIZE;
                    } catch (error) {
                        console.debug('Audio player: the fallback output analyzer could not be created.', error);
                        analyser = null;
                    }
                }
                mediaSource.connect(trackGain);
                if (analyser) {
                    this.analyserNode = analyser;
                    trackGain.connect(analyser);
                    analyser.connect(outputGain);
                } else {
                    trackGain.connect(outputGain);
                }
                outputGain.connect(context.destination);
                this.isAudioGraphReady = true;
                // The element output is now routed through the graph, so its own volume stays at full scale and the output
                // gain is the only place the volume is applied. Otherwise the two would multiply.
                this.audio.volume = 1;
                this.audio.muted = false;
                this.applyReplayGain(0);
                this.applyOutputGain(0);
                return true;
            } catch (error) {
                console.debug('Audio player: the fallback audio graph could not be connected.', error);
                this.disconnectAudioGraph();
                return false;
            }
        }

        /**
         * Applies the ReplayGain normalization selected by the current mode to the track gain.
         * @param {number} rampSeconds - The duration of the ramp to the new value, in seconds. Zero applies it at once.
         */
        applyReplayGain(rampSeconds) {
            if (!this.trackGainNode || typeof LuminaAudioPlayer.computeReplayGainTrackGain !== 'function')
                return;
            const gain = LuminaAudioPlayer.computeReplayGainTrackGain(this.manifest, this.replayGainMode);
            this.setAudioParamValue(this.trackGainNode.gain, gain, rampSeconds);
        }

        /**
         * Applies the stored output volume to the output gain.
         * @param {number} rampSeconds - The duration of the ramp to the new value, in seconds. Zero applies it at once.
         */
        applyOutputGain(rampSeconds) {
            if (!this.outputGainNode)
                return;
            this.setAudioParamValue(this.outputGainNode.gain, this.volume, rampSeconds);
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
                && this.audioContext) {
                try {
                    const now = this.audioContext.currentTime;
                    const current = Number.isFinite(param.value) ? param.value : target;
                    param.cancelScheduledValues(now);
                    param.setValueAtTime(current, now);
                    param.linearRampToValueAtTime(target, now + rampSeconds);
                    return;
                } catch (error) {
                    console.debug('Audio player: a fallback gain ramp could not be scheduled.', error);
                }
            }
            try {
                param.value = target;
            } catch (error) {
                console.debug('Audio player: a fallback gain value could not be applied.', error);
            }
        }

        /**
         * Disconnects the optional Web Audio graph and closes the context this engine owns.
         * @returns {Promise<void>} A promise that completes when the graph was released.
         */
        async disconnectAudioGraph() {
            for (const node of [this.mediaSourceNode, this.trackGainNode, this.analyserNode, this.outputGainNode]) {
                if (!node || typeof node.disconnect !== 'function')
                    continue;
                try {
                    node.disconnect();
                } catch (error) {
                    console.debug('Audio player: a fallback audio node could not be disconnected.', error);
                }
            }
            const context = this.audioContext;
            this.mediaSourceNode = null;
            this.trackGainNode = null;
            this.analyserNode = null;
            this.outputGainNode = null;
            this.audioContext = null;
            this.isAudioGraphReady = false;
            if (this.ownsAudioContext && context && typeof context.close === 'function') {
                try {
                    await context.close();
                } catch (error) {
                    console.debug('Audio player: the fallback audio context could not be closed.', error);
                }
            }
        }

        /**
         * Reports the playhead the element reached.
         */
        handleTimeUpdate() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportTime(this.audio.currentTime, this.getDuration());
        }

        /**
         * Reports the duration the element discovered, and refreshes the playhead against it.
         */
        handleDurationChange() {
            if (this.isDestroyed || !this.descriptor)
                return;
            const duration = this.getDuration();
            if (duration > 0) {
                this.duration = duration;
                this.host.reportDuration(duration);
            }
            this.host.reportTime(this.audio.currentTime, duration);
            this.reportProgress();
        }

        /**
         * Reports how much of the track the element buffered.
         */
        handleProgress() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.reportProgress();
        }

        /**
         * Reports that the element is waiting for data.
         */
        handleWaiting() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportBuffering(true);
        }

        /**
         * Reports that the element has enough data to play again.
         */
        handleCanPlay() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportBuffering(false);
        }

        /**
         * Reports that a seek completed and clears the waiting state it may have raised.
         */
        handleSeeked() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportBuffering(false);
            this.host.reportTime(this.audio.currentTime, this.getDuration());
            this.reportProgress();
        }

        /**
         * Reports the end of the track to the player.
         */
        handleEnded() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportTime(this.getDuration(), this.getDuration());
            this.host.reportEnded();
        }

        /**
         * Reports a media load or playback failure. A failure during load is reported by the load promise, so this handler
         * only reports failures of a track that already loaded, which keeps one failure from being reported twice.
         */
        handleError() {
            if (this.isDestroyed || this.isLoading || !this.descriptor)
                return;
            this.host.reportError(this.createMediaError());
        }

        /**
         * Creates the typed error of a media element failure.
         * @returns {LuminaAudioPlayer.PlayerError} The typed error.
         */
        createMediaError() {
            const mediaError = this.audio.error;
            return new LuminaAudioPlayer.PlayerError(
                LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR,
                'The browser could not play the track with its native audio player.',
                mediaError && Number.isFinite(Number(mediaError.code)) ? { mediaErrorCode: Number(mediaError.code) } : null);
        }

        /**
         * Reports the downloaded fraction of the track to the player.
         */
        reportProgress() {
            const duration = this.getDuration();
            if (!(duration > 0))
                return;
            try {
                const ranges = this.audio.buffered;
                if (!ranges || ranges.length === 0)
                    return;
                const end = ranges.end(ranges.length - 1);
                if (Number.isFinite(end))
                    this.host.reportProgress(clamp(end / duration, 0, 1));
            } catch (error) {
                console.debug('Audio player: the buffered ranges of the native audio element could not be read.', error);
            }
        }

        /**
         * Loads a track: fetches its manifest, resolves its validated stream path, and waits for the element metadata.
         * @param {Object} descriptor - The play descriptor of the track.
         * @param {Object} [options] - The load options. The fallback ignores the buffering hint of the player.
         * @returns {Promise<void>} A promise that completes when the element is ready to play.
         * @throws {LuminaAudioPlayer.PlayerError} When the manifest cannot be read, the stream path is not safe, or the element cannot load the track.
         */
        async loadAsync(descriptor, options = {}) {
            if (this.isDestroyed)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The HTML audio fallback engine was destroyed.');
            const token = {};
            this.loadToken = token;
            this.isLoading = true;
            try {
                this.releaseSource();
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                const configuration = typeof this.host.getConfiguration === 'function' ? this.host.getConfiguration() : {};
                const transport = typeof this.host.fetchJsonAsync === 'function'
                    ? (url, signal, label, requestOptions) => this.host.fetchJsonAsync(url, signal, label, requestOptions)
                    : null;
                const manifest = await LuminaAudioPlayer.fetchPlaybackManifestAsync({
                    configuration: configuration,
                    descriptor: descriptor,
                    capabilities: describeNativeCapabilities(this.audio),
                    transport: transport
                });
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                const streamUrl = LuminaAudioPlayer.resolveStreamUrl(manifest, descriptor, configuration);
                const duration = Number(manifest.durationInSeconds);
                this.descriptor = descriptor;
                this.manifest = manifest;
                this.duration = Number.isFinite(duration) && duration > 0 ? duration : 0;
                this.audio.src = streamUrl;
                this.audio.load();
                // The graph is built before playback starts, so the analyzer is ready for the VU meter the player binds
                // after this load, and the ReplayGain of the new manifest is applied before the first sample is heard.
                this.ensureAudioGraph();
                this.applyReplayGain(0);
                await this.waitForMetadataAsync(token);
                if (!this.isLoadCurrent(token))
                    throw this.createSupersededLoadError();
                const loadedDuration = this.getDuration();
                if (loadedDuration > 0) {
                    this.duration = loadedDuration;
                    this.host.reportDuration(loadedDuration);
                }
                this.host.reportTime(0, loadedDuration);
                this.reportProgress();
            } finally {
                if (this.loadToken === token)
                    this.isLoading = false;
            }
        }

        /**
         * Waits for the element to report its metadata, so a load resolves only when the track can actually play.
         * @param {Object} token - The token of the load that owns the wait.
         * @returns {Promise<void>} A promise that completes when the metadata arrived.
         */
        waitForMetadataAsync(token) {
            return new Promise((resolve, reject) => {
                let isSettled = false;
                const settle = (callback, value) => {
                    if (isSettled)
                        return;
                    isSettled = true;
                    clearTimeout(timer);
                    this.audio.removeEventListener('loadedmetadata', onLoaded);
                    this.audio.removeEventListener('error', onError);
                    callback(value);
                };
                const onLoaded = () => settle(resolve);
                const onError = () => settle(reject, this.createMediaError());
                const timer = setTimeout(() => {
                    if (!this.isLoadCurrent(token)) {
                        settle(reject, this.createSupersededLoadError());
                        return;
                    }
                    settle(reject, new LuminaAudioPlayer.PlayerError(
                        LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED,
                        'The browser did not report the metadata of the track in time.'));
                }, METADATA_TIMEOUT_MS);
                this.audio.addEventListener('loadedmetadata', onLoaded);
                this.audio.addEventListener('error', onError);
            });
        }

        /**
         * Checks whether a load still owns the element.
         * @param {Object} token - The token of the load.
         * @returns {boolean} True when the load is current and the engine was not destroyed.
         */
        isLoadCurrent(token) {
            return !this.isDestroyed && this.loadToken === token;
        }

        /**
         * Creates the typed error that tells the player a load was overtaken by a newer play request.
         * @returns {LuminaAudioPlayer.PlayerError} The superseded load error.
         */
        createSupersededLoadError() {
            return new LuminaAudioPlayer.PlayerError(
                LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE,
                'The load was superseded by a newer play request.',
                { reason: LuminaAudioPlayer.PlayerErrorReason.LOAD_SUPERSEDED });
        }

        /**
         * Starts, or resumes, audible playback.
         * @returns {Promise<void>} A promise that completes when the element started playing.
         */
        async playAsync() {
            if (this.isDestroyed || !this.descriptor)
                return;
            this.host.reportBuffering(false);
            // A context created before the first gesture starts suspended, and an element routed through a suspended context
            // produces no sound even when the element itself is playing. Resuming here is what starts the graph with playback.
            if (this.audioContext && this.audioContext.state !== 'running' && typeof this.audioContext.resume === 'function')
                await this.audioContext.resume();
            await this.audio.play();
        }

        /**
         * Suspends audible playback, keeping the position.
         * @returns {Promise<void>} A promise that completes when the element paused.
         */
        async pauseAsync() {
            if (this.isDestroyed)
                return;
            this.audio.pause();
            this.host.reportTime(this.audio.currentTime, this.getDuration());
        }

        /**
         * Moves the playhead to a position of the current track.
         * @param {number} seconds - The target position, in seconds.
         * @returns {Promise<void>} A promise that completes when the element accepted the position.
         */
        async seekAsync(seconds) {
            if (this.isDestroyed || !this.descriptor)
                return;
            const requested = toFiniteNumber(seconds);
            const duration = this.getDuration();
            const position = Math.max(0, requested === null ? 0 : requested);
            try {
                this.audio.currentTime = duration > 0 ? Math.min(position, duration) : position;
                this.host.reportTime(this.audio.currentTime, duration);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(
                    LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR,
                    'The browser could not seek the native audio element.',
                    { cause: error });
            }
        }

        /**
         * Sets the output volume.
         * @param {number} volume - The output volume, between 0 and 1.
         * @returns {Promise<void>} A promise that completes when the volume was applied.
         */
        async setVolumeAsync(volume) {
            if (this.isDestroyed)
                return;
            const value = toFiniteNumber(volume);
            this.volume = clamp(value === null ? 0 : value, 0, 1);
            if (this.isAudioGraphReady) {
                this.audio.volume = 1;
                this.audio.muted = false;
                this.applyOutputGain(OUTPUT_GAIN_RAMP_SECONDS);
                return;
            }
            this.audio.volume = this.volume;
        }

        /**
         * Gets the time ranges the element already buffered.
         * @returns {Array<{ start: number, end: number }>} The buffered ranges, in ascending order.
         */
        getBufferedTimeRanges() {
            const ranges = [];
            try {
                for (let index = 0; index < this.audio.buffered.length; index++) {
                    ranges.push({
                        start: this.audio.buffered.start(index),
                        end: this.audio.buffered.end(index)
                    });
                }
            } catch (error) {
                console.debug('Audio player: the buffered ranges of the native audio element could not be read.', error);
            }
            return ranges;
        }

        /**
         * Gets the analyzer that taps the track level after ReplayGain and before the output volume, so the VU meter stays
         * independent of the volume and mute state. It is null when the browser provides no usable Web Audio graph.
         * @returns {AnalyserNode|null} The output analyzer, or null.
         */
        getOutputAnalyser() {
            return this.analyserNode;
        }

        /**
         * Releases the currently prepared track without destroying the engine.
         * @returns {Promise<void>} A promise that completes when the source was released.
         */
        async discardCurrentAsync() {
            if (this.isDestroyed)
                return;
            this.loadToken = null;
            this.releaseSource();
            this.descriptor = null;
            this.manifest = null;
            this.duration = 0;
        }

        /**
         * Drops a prefetched or appended handoff. The fallback never prepares one, so there is nothing to cancel.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async cancelUpcomingAsync() {
        }

        /**
         * Releases every resource the engine holds.
         * @returns {Promise<void>} A promise that completes when the element was released.
         */
        async destroyAsync() {
            if (this.isDestroyed)
                return;
            this.isDestroyed = true;
            this.loadToken = null;
            this.isLoading = false;
            if (this.unsubscribePreferences) {
                this.unsubscribePreferences();
                this.unsubscribePreferences = null;
            }
            this.unbindAudioEvents();
            this.releaseSource();
            await this.disconnectAudioGraph();
            this.descriptor = null;
            this.manifest = null;
            this.duration = 0;
        }

        /**
         * Pauses the element and detaches its source, so a replaced track stops downloading immediately.
         */
        releaseSource() {
            try {
                this.audio.pause();
            } catch (error) {
                console.debug('Audio player: the native audio element could not be paused.', error);
            }
            try {
                this.audio.removeAttribute('src');
                this.audio.load();
            } catch (error) {
                console.debug('Audio player: the native audio element source could not be released.', error);
            }
        }

        /**
         * Gets the duration the element discovered, falling back to the duration the manifest declared.
         * @returns {number} The known duration, in seconds.
         */
        getDuration() {
            const elementDuration = Number(this.audio.duration);
            if (Number.isFinite(elementDuration) && elementDuration > 0)
                return elementDuration;
            return this.duration > 0 ? this.duration : 0;
        }
    }

    LuminaAudioPlayer.HtmlAudioPlayerEngine = HtmlAudioPlayerEngine;

    /**
     * Creates an HTML audio fallback engine. See {@link LuminaAudioPlayer.HtmlAudioPlayerEngine} for the options.
     * @param {Object} host - The engine host provided by the player controller.
     * @param {Object} [options] - The engine options.
     * @returns {LuminaAudioPlayer.HtmlAudioPlayerEngine} The created engine.
     */
    LuminaAudioPlayer.createHtmlAudioPlayerEngine = function (host, options) {
        return new HtmlAudioPlayerEngine(host, options);
    };

    /**
     * Registers the HTML audio engine as the fallback of a player, without changing the engine it runs with when its
     * capabilities are present.
     * @param {LuminaAudioPlayer.AudioPlayer} player - The player to install the fallback on.
     * @returns {LuminaAudioPlayer.AudioPlayer} The same player, for chaining.
     */
    LuminaAudioPlayer.installHtmlAudioFallbackEngine = function (player) {
        if (!player)
            return player;
        player.registerEngine(LuminaAudioPlayer.HTML_AUDIO_ENGINE_NAME, (host) => new HtmlAudioPlayerEngine(host));
        return player.registerFallbackEngine(LuminaAudioPlayer.HTML_AUDIO_ENGINE_NAME);
    };

    /**
     * Installs the fallback on the application singleton when the player module is loaded.
     */
    function installOnDefaultPlayer() {
        if (typeof LuminaAudioPlayer.createDefaultPlayer !== 'function')
            return;
        const player = LuminaAudioPlayer.createDefaultPlayer();
        LuminaAudioPlayer.installHtmlAudioFallbackEngine(player);
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', installOnDefaultPlayer);
    else
        installOnDefaultPlayer();
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
