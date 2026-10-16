/**
 * The audio player of Lumina: state machine, play descriptor, lifecycle, bar and the public control API.
 *
 * The player is a singleton created once, in the application layout, outside the content area that the
 * in-place navigation manager swaps. Because it is never torn down by a page swap, playback and its
 * UI survive navigation. It produces no audio itself: audio is delegated to a swappable engine that
 * fulfills the interface documented in audio-player-engine.js. This module only owns the state machine, the
 * event bus, capability detection, the play descriptor, the bar and the public API; the download,
 * decode and render pipeline is plugged in as an engine by the later stages.
 *
 * Public control API (window.LuminaAudioPlayer.instance)
 * ----------------------------------------
 * Methods:
 *   - playAsync(descriptor): starts playback of a track. The descriptor is documented by createPlayDescriptor.
 *                            When the descriptor carries a queue, that queue becomes the player queue.
 *   - pauseAsync(): suspends playback, keeping the position.
 *   - resumeAsync(): resumes playback from the current position.
 *   - toggleAsync(): pauses when playing, resumes otherwise.
 *   - stopAsync(): stops playback. While the bar is condensed, it also hides the bar; while the bar is
 *                  expanded, it stays on screen.
 *   - seekAsync(seconds): moves the playhead to an absolute position, in seconds.
 *   - setVolumeAsync(volume): sets the output volume, between 0 and 1.
 *   - setMutedAsync(muted): sets the mute state. Volume and mute are independent: unmuting restores the volume.
 *   - toggleMuteAsync(): flips the mute state.
 *   - toggleFullHeight(): expands or collapses the bar.
 *   - toggleVisibility(): shows or hides the bar.
 *   - on(event, handler): subscribes to a player event, returns an unsubscribe function.
 *   - once(event, handler), off(event, handler): one shot subscribe, and unsubscribe.
 *   - getState(), getDescriptor(), getQueue(), getCurrentTime(), getDuration(),
 *     getVolume(), isMuted(), getPreferences(), getCapabilities(), getLastError(), getSubTemplates().
 *   - destroyAsync(): releases every resource of the player. It is only used for an explicit teardown, never
 *                     by a navigation, because the navigation manager never recreates the player.
 *   - registerEngine(name, factory): registers an engine factory on this player, by name.
 *   - useEngine(name): selects a registered engine; it takes effect on the next load.
 *   - getEngineName(): the name of the engine the player runs with.
 * Properties:
 *   - capabilitiesReady: a Promise resolving to the capability report.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Enum-like object for the states of the player state machine.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerState = Object.freeze({
        // Nothing is loaded. This is also the state a stop returns to.
        IDLE: 'idle',
        // A track is being prepared. The download engine opens its stream here.
        LOADING: 'loading',
        // A track is prepared and can start producing audio.
        READY: 'ready',
        // Audio is being produced.
        PLAYING: 'playing',
        // Playback is suspended, and the position is kept.
        PAUSED: 'paused',
        // The current track reached its end.
        ENDED: 'ended',
        // A failure moved the player out of a normal state.
        ERROR: 'error'
    });

    /**
     * The states each state may move to. A transition outside this map is applied anyway, but it is
     * logged, because it signals a bug in the caller rather than a recoverable condition.
     * @readonly
     * @type {Object<string, ReadonlyArray<string>>}
     */
    const STATE_TRANSITIONS = Object.freeze({
        idle: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.ERROR],
        loading: [LuminaAudioPlayer.PlayerState.READY, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR],
        ready: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.PLAYING, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR],
        playing: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.PAUSED, LuminaAudioPlayer.PlayerState.ENDED, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR],
        paused: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.PLAYING, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR],
        ended: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.PLAYING, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR],
        error: [LuminaAudioPlayer.PlayerState.LOADING, LuminaAudioPlayer.PlayerState.IDLE, LuminaAudioPlayer.PlayerState.ERROR]
    });

    /**
     * The result of a background request whose answer was a redirect, which means the session expired and the page is
     * being sent to the sign in page. It is distinct from a missing value, which is a failure, so that a caller does not
     * report an error for a redirect it is already handling.
     * @readonly
     * @type {Object}
     */
    const REDIRECTED = Object.freeze({ redirected: true });

    /**
     * The attribute that marks an element as a play trigger the player handles by event delegation.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_DESCRIPTOR_ATTRIBUTE = 'data-audio-player-play';

    /**
     * Maps each play descriptor field to the data attribute the pages set on a play trigger, so the
     * pages and the player share a single documented set of names.
     * @readonly
     * @type {Object<string, string>}
     */
    LuminaAudioPlayer.PLAY_DESCRIPTOR_DATA_KEYS = Object.freeze({
        trackId: 'trackId',
        albumId: 'albumId',
        artistId: 'artistId',
        libraryId: 'libraryId',
        title: 'title',
        durationInSeconds: 'duration',
        coverArtPath: 'coverArtPath',
        coverArtUrl: 'coverArtUrl'
    });

    /**
     * The marker shown in place of a duration that is not known, so that an absent duration reads as unknown
     * instead of as a real zero.
     * @readonly
     * @type {string}
     */
    const UNKNOWN_TIME = '--:--';

    /**
     * Formats a known, positive time in seconds as m:ss, or h:mm:ss when it is an hour or longer.
     * @param {number} totalSeconds - The time to format, in seconds.
     * @returns {string} The formatted time.
     */
    function formatClock(totalSeconds) {
        const seconds = Math.floor(totalSeconds % 60);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const hours = Math.floor(totalSeconds / 3600);
        const paddedSeconds = String(seconds).padStart(2, '0');
        if (hours > 0)
            return `${hours}:${String(minutes).padStart(2, '0')}:${paddedSeconds}`;
        return `${minutes}:${paddedSeconds}`;
    }

    /**
     * Formats a playhead in seconds as m:ss, or h:mm:ss when it is an hour or longer. A playhead is always known,
     * so a missing or negative value is the start of the track, and it is formatted as 0:00.
     * @param {number} totalSeconds - The playhead to format, in seconds.
     * @returns {string} The formatted playhead.
     */
    function formatTime(totalSeconds) {
        const value = Number(totalSeconds);
        return Number.isFinite(value) && value > 0 ? formatClock(value) : '0:00';
    }

    /**
     * Formats a duration in seconds as m:ss, or h:mm:ss when it is an hour or longer, and shows the unknown time
     * marker while the duration is not known.
     * @param {number} totalSeconds - The duration to format, in seconds.
     * @returns {string} The formatted duration, or --:-- when the duration is not known.
     */
    function formatDuration(totalSeconds) {
        const value = Number(totalSeconds);
        return Number.isFinite(value) && value > 0 ? formatClock(value) : UNKNOWN_TIME;
    }

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
     * Resolves the cover art of a track against the base URL of the API server, using the same artwork path the album page uses.
     *
     * Artwork is served by the API server, so a value is resolved against the API server base (or, when no base is
     * configured, against the origin of the page) and is accepted only when it ends up on that same origin. A value on
     * any other origin, a protocol relative value, any scheme other than http or https, and any value carrying a control
     * character are rejected. This keeps a crafted cover art value from making the page fetch an image from a third
     * party, which would leak the location of the user, and, by association, what they listen to.
     * @param {string} value - The relative artwork path, or an absolute cover art URL on the API server.
     * @returns {string} The normalized cover art URL, or an empty string when the value is missing or rejected.
     */
    function resolveCoverArtUrl(value) {
        if (!value)
            return '';
        const raw = String(value).trim();
        if (raw === '')
            return '';
        // A control character is rejected, a protocol relative value is rejected, and any scheme that is not http or
        // https is rejected: the URL parser strips leading control characters, so a value such as "\0//host" would
        // otherwise be treated as a different reference than the checks above see.
        if (/[\u0000-\u001f\u007f]/.test(raw) || raw.startsWith('//') || (/^[a-z][a-z0-9+.-]*:/i.test(raw) && !/^https?:/i.test(raw)))
            return '';
        const configuredBase = (window.globalConfiguration && window.globalConfiguration.serverBasePath) || '';
        const base = configuredBase || (window.location && window.location.origin) || '';
        // Without a base there is nothing to check the value against, so it is rejected rather than trusted.
        if (!base)
            return '';
        try {
            const resolved = new URL(raw, base);
            if (resolved.protocol !== 'http:' && resolved.protocol !== 'https:')
                return '';
            // The artwork of a library must stay on the server that serves it, so the resolved origin is compared to the
            // origin of the base the value was resolved against, and anything else is dropped instead of being loaded.
            const allowedOrigin = new URL(base).origin;
            return resolved.origin === allowedOrigin ? resolved.href : '';
        } catch (error) {
            console.debug('Audio player: a cover art value could not be resolved.', error);
            return '';
        }
    }

    /**
     * Normalizes a play descriptor, a plain object that describes what to play.
     *
     * Field rules:
     *   - libraryId (string, required): the Id of the media library the context belongs to.
     *   - trackId (string, required for a single track): the Id of the track.
     *   - albumId (string, optional): the Id of the album. A descriptor without a track Id describes an album play context.
     *   - artistId (string, optional): the Id of the artist. A descriptor without a track or album Id describes an artist play context.
     *   - title (string, optional): the display title of the track or context.
     *   - durationInSeconds (number, optional): the duration of the track, in seconds.
     *   - coverArtPath (string, optional): the relative artwork path of the album, resolved against the server base path.
     *   - coverArtUrl (string, optional): an absolute cover art URL, or a relative artwork path.
     *   - queue (Array, optional): the ordered track descriptors of the play context, used to render the expanded bar.
     * @param {Object} input - The descriptor to normalize.
     * @returns {Object|null} The normalized descriptor, or null when it identifies no library, or no track, album or artist.
     */
    function createPlayDescriptor(input) {
        if (!input || typeof input !== 'object')
            return null;
        const trackId = toIdString(input.trackId);
        const albumId = toIdString(input.albumId);
        const artistId = toIdString(input.artistId);
        const libraryId = toIdString(input.libraryId);
        // A track descriptor carries its track Id, while an album or artist context descriptor carries only the Id it is built from.
        if (!libraryId || (!trackId && !albumId && !artistId))
            return null;
        const coverArtSource = input.coverArtUrl || input.coverArtPath || '';
        const descriptor = {
            trackId,
            libraryId,
            albumId,
            artistId,
            title: typeof input.title === 'string' ? input.title : '',
            durationInSeconds: Number.isFinite(Number(input.durationInSeconds)) ? Number(input.durationInSeconds) : 0,
            coverArtPath: typeof input.coverArtPath === 'string' ? input.coverArtPath : '',
            coverArtUrl: resolveCoverArtUrl(coverArtSource)
        };
        if (Array.isArray(input.queue)) {
            const queue = input.queue.map(item => createPlayDescriptor(item)).filter(item => item !== null);
            descriptor.queue = Object.freeze(queue);
        }
        return Object.freeze(descriptor);
    }

    /**
     * Coerces an Id into a non empty string, or an empty string when it carries no value.
     * @param {*} value - The Id to coerce.
     * @returns {string} The Id as a string, or an empty string.
     */
    function toIdString(value) {
        if (value === null || value === undefined)
            return '';
        const text = String(value).trim();
        return text === '' ? '' : text;
    }

    /**
     * Reads the raw source of a theme sub template element of the player bar, or null when the theme does not provide one.
     *
     * The theme inlines the sub template sources into plain text script elements of the bar, so that the browser never
     * parses the theme markup into live elements and requests their resources before the player renders them. The raw
     * source is recovered by reading the element text, exactly like the file system browser does it.
     * @param {string} elementId - The id of the plain text template element.
     * @returns {string|null} The raw template source, or null when the element or the template engine is missing.
     */
    function readSubTemplate(elementId) {
        const templateElement = document.getElementById(elementId);
        if (!templateElement)
            return null;
        // The theme template engine may be absent on a partially loaded page, in which case the built-in markup is used.
        if (typeof window.renderThemeTemplate !== 'function')
            return null;
        // An empty template element means the theme does not provide the template, in which case the built-in markup is used.
        return templateElement.textContent || null;
    }

    /**
     * Reads a play descriptor from the data attributes of a play trigger element, per
     * {@link LuminaAudioPlayer.PLAY_DESCRIPTOR_DATA_KEYS}.
     * @param {Element} element - The element carrying the descriptor attributes.
     * @returns {Object|null} The normalized descriptor, or null when the element does not describe a valid track.
     */
    function readPlayDescriptorFromElement(element) {
        if (!element || !element.dataset)
            return null;
        const keys = LuminaAudioPlayer.PLAY_DESCRIPTOR_DATA_KEYS;
        return createPlayDescriptor({
            trackId: element.dataset[keys.trackId],
            albumId: element.dataset[keys.albumId],
            artistId: element.dataset[keys.artistId],
            libraryId: element.dataset[keys.libraryId],
            title: element.dataset[keys.title],
            durationInSeconds: element.dataset[keys.durationInSeconds],
            coverArtPath: element.dataset[keys.coverArtPath],
            coverArtUrl: element.dataset[keys.coverArtUrl]
        });
    }

    /**
     * The audio player. See the module documentation for the public control API.
     */
    class AudioPlayer {
        /**
         * Creates an instance of AudioPlayer.
         * @param {Object} [options] - The player options.
         * @param {HTMLElement} [options.container] - The player bar element. Defaults to the first element with the audio-player-container class.
         * @param {HTMLElement} [options.main] - The main content element, whose class tracks the bar visibility. Defaults to the first main element.
         * @param {LuminaAudioPlayer.PlayerEventBus} [options.events] - The event bus to publish to. A new one is created when omitted.
         * @param {LuminaAudioPlayer.PlayerCapabilities} [options.capabilities] - The capability detector. A new one is created when omitted.
         * @param {LuminaAudioPlayer.PlayerEngineRegistry} [options.engineRegistry] - The engine registry. The default registry is created when omitted.
         * @param {string} [options.engineName] - The name of the engine to run. Defaults to the silent engine.
         * @param {Object} [options.configuration] - The endpoints configuration. Defaults to window.audioPlayerConfiguration.
         */
        constructor(options = {}) {
            this.options = options;
            this.events = options.events || new LuminaAudioPlayer.PlayerEventBus();
            this.capabilities = options.capabilities || new LuminaAudioPlayer.PlayerCapabilities();
            this.engineRegistry = options.engineRegistry || LuminaAudioPlayer.createDefaultEngineRegistry();
            this.engineName = options.engineName || LuminaAudioPlayer.NULL_ENGINE_NAME;

            this.state = LuminaAudioPlayer.PlayerState.IDLE;
            this.descriptor = null;
            this.queue = [];
            this.currentIndex = 0;
            this.currentTime = 0;
            this.duration = 0;
            this.downloadedFraction = 0;
            this.volume = 1;
            // Boolean state keeps its conventional is/has prefix, except where a public is*() accessor already owns that name:
            // isMuted() is that accessor here, so the backing field keeps the participle instead of shadowing the method.
            this.muted = false;
            this.lastError = null;
            this.preferences = {
                shouldKeepDownloadingWhilePaused: false,
                shouldAutoplay: false,
                isGaplessPlaybackEnabled: false,
                replayGainMode: 'Off',
                streamingQuality: 'Auto'
            };
            this.capabilityReport = null;
            this.capabilitiesReady = null;
            this.engine = null;
            this.isAvailable = false;
            this.isInitialized = false;
            this.container = null;
            this.main = null;
            this.dom = {};
            // The raw theme sub templates of the dynamic content, read once with the bar elements and null when the theme does not provide them.
            this.subTemplates = { queueItem: null, playlistItem: null };
            this.descriptorClickHandler = null;
            this.controlHandlers = [];
            this.isProgressRenderScheduled = false;
        }

        /**
         * Binds the player to the bar in the layout, prepares the hidden state, and starts capability
         * detection and the load of the stored playback preferences.
         *
         * The method is idempotent: a second call returns immediately. When the layout does not render
         * a bar (an unauthenticated visitor), the player marks itself unavailable and every control
         * method becomes a no-op.
         * @returns {LuminaAudioPlayer.AudioPlayer} This player, for chaining.
         */
        initialize() {
            if (this.isInitialized)
                return this;
            this.isInitialized = true;

            this.container = this.options.container || document.querySelector('.audio-player-container');
            this.main = this.options.main || document.querySelector('main');
            if (!this.container) {
                this.isAvailable = false;
                return this;
            }
            this.isAvailable = true;

            this.collectDomReferences();
            this.bindPlaybackControls();
            this.bindDescriptorDelegation();
            // The bar is hidden until the user starts playback, so a refresh never flashes an empty player.
            this.suppressInitialTransitions();
            this.applyHiddenState();
            this.releaseInitialHiddenState();

            this.capabilitiesReady = this.detectCapabilitiesAsync();
            // The stored preferences are read in the background; a slow or failing request never blocks the bar.
            this.loadPreferencesAsync();
            return this;
        }

        /**
         * Collects the references to the bar elements the player drives, once, so that a navigation
         * that swaps the content area never invalidates them.
         */
        collectDomReferences() {
            const query = (selector) => this.container.querySelector(selector);
            this.dom.trackName = query('[data-audio-player-track-name]');
            this.dom.coverThumbnail = query('[data-audio-player-cover-thumbnail]');
            this.dom.coverImage = query('[data-audio-player-cover-image]');
            this.dom.currentTime = query('[data-audio-player-current-time]');
            this.dom.totalTime = query('[data-audio-player-total-time]');
            this.dom.progress = query('[data-audio-player-progress]');
            this.dom.progressPlayed = query('[data-audio-player-progress-played]');
            this.dom.progressDownloaded = query('[data-audio-player-progress-downloaded]');
            this.dom.playPause = query('[data-audio-player-play-pause]');
            this.dom.queue = query('[data-audio-player-queue]');
            // The sub template sources are read once, with the bar elements, because the theme markup is part of the bar
            // the player was bound to, and the player is never recreated by a navigation.
            this.subTemplates = {
                queueItem: readSubTemplate('audio-player-template-queue-item'),
                playlistItem: readSubTemplate('audio-player-template-playlist-item')
            };
        }

        /**
         * Binds the transport controls the player owns. The full height and close affordances are bound
         * by the global site script, which delegates them to this player, so that a single click never
         * toggles them twice.
         */
        bindPlaybackControls() {
            if (this.dom.playPause)
                this.addControlHandler(this.dom.playPause, 'click', () => this.toggleAsync());
            if (this.dom.progress)
                this.addControlHandler(this.dom.progress, 'click', (event) => this.handleProgressClick(event));
        }

        /**
         * Adds a click handler to a bar element, and remembers it so that an explicit teardown can remove it.
         * @param {HTMLElement} element - The element to bind.
         * @param {string} event - The DOM event to listen to.
         * @param {Function} handler - The handler to invoke.
         */
        addControlHandler(element, event, handler) {
            element.addEventListener(event, handler);
            this.controlHandlers.push({ element, event, handler });
        }

        /**
         * Binds the global click delegation that turns an element carrying the play trigger attribute
         * into a play request, so that pages never need a direct reference to the player.
         */
        bindDescriptorDelegation() {
            // One listener on the document rather than one per affordance, because the pages and their theme templates are
            // swapped in place by the navigation manager and never re-bound, and because a trigger can be rendered at any
            // time without the player being told about it.
            this.descriptorClickHandler = (event) => {
                if (!(event.target instanceof Element))
                    return;
                const trigger = event.target.closest(`[${LuminaAudioPlayer.PLAY_DESCRIPTOR_ATTRIBUTE}]`);
                if (!trigger)
                    return;
                event.preventDefault();
                const descriptor = readPlayDescriptorFromElement(trigger);
                this.playAsync(descriptor).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR));
            };
            document.addEventListener('click', this.descriptorClickHandler);
        }

        /**
         * Starts playback of a track, and shows the bar.
         * @param {Object} descriptorInput - The play descriptor to play, per {@link LuminaAudioPlayer.createPlayDescriptor}.
         * @returns {Promise<boolean>} True when playback started, false when the request was rejected.
         */
        async playAsync(descriptorInput) {
            // The player is only available when the layout rendered the bar, so a caller on a page without one is ignored instead of failing.
            if (!this.isAvailable)
                return false;
            const descriptor = createPlayDescriptor(descriptorInput);
            if (!descriptor) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor identifies no library, or no track, album or artist.'));
                return false;
            }
            // Capability detection runs in the background while the page loads, so the first play waits for its answer instead
            // of starting a track the browser may not be able to decode.
            const report = await this.capabilitiesReady;
            if (report && !report.isSupported) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }));
                return false;
            }

            // A new descriptor starts a new timeline, so the playhead and the progress of the previous track are dropped.
            this.descriptor = descriptor;
            this.duration = descriptor.durationInSeconds;
            this.currentTime = 0;
            this.downloadedFraction = 0;
            // An album or artist descriptor carries no track id of its own, so the queue starts at its first item in that case.
            this.queue = descriptor.queue && descriptor.queue.length > 0 ? [...descriptor.queue] : [descriptor];
            this.currentIndex = Math.max(0, this.queue.findIndex(item => item.trackId === descriptor.trackId));

            // The bar is shown and the state moves to loading before the engine is touched, so a slow engine still gives the user
            // immediate feedback that the request was accepted.
            this.renderDescriptor();
            this.renderProgress();
            this.renderQueue();
            this.showBar();
            this.setState(LuminaAudioPlayer.PlayerState.LOADING);
            this.events.emit(LuminaAudioPlayer.PlayerEvent.DESCRIPTOR_CHANGED, descriptor);
            this.events.emit(LuminaAudioPlayer.PlayerEvent.QUEUE_CHANGED, this.queue);

            try {
                // The state follows the engine step by step, so a consumer can tell preparation apart from actual playback.
                const engine = await this.ensureEngineAsync();
                await engine.loadAsync(descriptor);
                this.setState(LuminaAudioPlayer.PlayerState.READY);
                await engine.playAsync();
                this.setState(LuminaAudioPlayer.PlayerState.PLAYING);
                this.renderTransportState();
            } catch (error) {
                // A failure before playback starts is a load failure; later failures are reported by the engine through its host.
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED);
                return false;
            }
            return true;
        }

        /**
         * Suspends playback, keeping the current position.
         * @returns {Promise<void>} A promise that completes when the engine paused.
         */
        async pauseAsync() {
            // Pausing anything that is not playing is a no-op, so the pause and close controls stay harmless when clicked twice.
            if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                return;
            if (this.engine)
                await this.engine.pauseAsync();
            this.setState(LuminaAudioPlayer.PlayerState.PAUSED);
            this.renderTransportState();
        }

        /**
         * Resumes playback from the current position.
         * @returns {Promise<void>} A promise that completes when the engine resumed.
         */
        async resumeAsync() {
            // A ready track has not started yet, and a paused one keeps its position, so both can resume.
            if (this.state !== LuminaAudioPlayer.PlayerState.PAUSED && this.state !== LuminaAudioPlayer.PlayerState.READY)
                return;
            // Without an engine there is nothing to resume; the next play builds one and applies the stored volume.
            if (!this.engine)
                return;
            await this.engine.playAsync();
            this.setState(LuminaAudioPlayer.PlayerState.PLAYING);
            this.renderTransportState();
        }

        /**
         * Pauses when playing, resumes when paused or ready, and replays the current track when idle.
         * @returns {Promise<void>} A promise that completes when the toggle was applied.
         */
        async toggleAsync() {
            if (this.state === LuminaAudioPlayer.PlayerState.PLAYING)
                await this.pauseAsync();
            else if (this.state === LuminaAudioPlayer.PlayerState.PAUSED || this.state === LuminaAudioPlayer.PlayerState.READY)
                await this.resumeAsync();
            else if (this.descriptor)
                await this.playAsync(this.descriptor);
        }

        /**
         * Stops playback. While the bar is condensed, it slides out of view; while the bar is expanded,
         * it stays on screen with the current track, and only playback stops.
         * @returns {Promise<void>} A promise that completes when playback stopped.
         */
        async stopAsync() {
            if (this.engine) {
                try {
                    // The engine is paused rather than destroyed, because it is reused for the next track and rebuilding it would
                    // drop resources such as the audio context that a user gesture is needed to resume.
                    await this.engine.pauseAsync();
                } catch (error) {
                    // A failure while stopping must not prevent the player from returning to idle.
                    console.error('Audio player: the engine failed while stopping.', error);
                }
            }
            // The playhead and the progress are reset while the descriptor is kept, so the bar can still show what was stopped.
            this.currentTime = 0;
            this.downloadedFraction = 0;
            this.renderProgress();
            this.setState(LuminaAudioPlayer.PlayerState.IDLE);
            this.renderTransportState();
            // A condensed bar slides out when playback stops, while an expanded one stays so the user keeps the track and its queue in view.
            if (!this.isExpanded())
                this.hideBar();
        }

        /**
         * Moves the playhead to an absolute position.
         * @param {number} seconds - The target position, in seconds.
         * @returns {Promise<void>} A promise that completes when the seek was requested.
         */
        async seekAsync(seconds) {
            // Without a descriptor there is no timeline to seek in.
            if (!this.descriptor)
                return;
            // A known duration bounds the target, while without one the position is only kept non negative, so a seek is never
            // silently snapped to zero just because the duration has not been reported yet.
            const request = Number(seconds);
            const position = this.duration > 0
                ? clamp(request, 0, this.duration)
                : Math.max(0, Number.isFinite(request) ? request : 0);
            this.currentTime = position;
            if (this.engine) {
                try {
                    await this.engine.seekAsync(position);
                } catch (error) {
                    this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                }
            }
            // The engine reports the new playhead itself, so the render is coalesced with that report instead of racing it.
            this.scheduleProgressRender();
        }

        /**
         * Sets the output volume. Mute is independent of the volume: a muted player remembers the volume.
         * @param {number} volume - The output volume, between 0 and 1.
         * @returns {Promise<number>} The applied volume.
         */
        async setVolumeAsync(volume) {
            const value = clamp(Number(volume), 0, 1);
            this.volume = value;
            await this.applyVolumeAsync();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.VOLUME_CHANGED, this.buildVolumePayload());
            return this.volume;
        }

        /**
         * Sets the mute state.
         * @param {boolean} muted - True to mute the output, false to unmute it.
         * @returns {Promise<void>} A promise that completes when the mute was applied.
         */
        async setMutedAsync(muted) {
            this.muted = muted === true;
            await this.applyVolumeAsync();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.MUTE_CHANGED, this.buildVolumePayload());
        }

        /**
         * Flips the mute state.
         * @returns {Promise<void>} A promise that completes when the mute was applied.
         */
        async toggleMuteAsync() {
            await this.setMutedAsync(!this.muted);
        }

        /**
         * Expands the bar into its full height form, or collapses it back to the condensed bar.
         * @returns {boolean} True when the bar is expanded after the toggle.
         */
        toggleFullHeight() {
            if (!this.container)
                return false;
            const isExpanded = this.container.classList.toggle('full-height');
            if (this.main)
                this.main.classList.toggle('audio-player-full-height', isExpanded);
            return isExpanded;
        }

        /**
         * Shows the bar when it is hidden, and hides it when it is visible.
         * @returns {boolean} True when the bar is visible after the toggle.
         */
        toggleVisibility() {
            if (!this.container)
                return false;
            if (this.container.classList.contains('not-shown'))
                this.showBar();
            else
                this.hideBar();
            return !this.container.classList.contains('not-shown');
        }

        /**
         * Shows the bar and reflows the content area around it. A bar that is already shown is left untouched, and no
         * visibility event is emitted, so a consumer is never asked to animate a change that did not happen.
         */
        showBar() {
            if (!this.container)
                return;
            const wasHidden = this.container.classList.contains('not-shown');
            this.container.removeAttribute('hidden');
            this.container.classList.remove('not-shown');
            if (this.main)
                this.main.classList.remove('audio-player-not-shown');
            if (wasHidden)
                this.events.emit(LuminaAudioPlayer.PlayerEvent.VISIBILITY_CHANGED, { isVisible: true });
        }

        /**
         * Hides the bar and reflows the content area over it. A bar that is already hidden is left untouched, and no
         * visibility event is emitted, so a consumer is never asked to animate a change that did not happen.
         */
        hideBar() {
            if (!this.container)
                return;
            if (this.container.classList.contains('not-shown'))
                return;
            this.container.classList.add('not-shown');
            if (this.main)
                this.main.classList.add('audio-player-not-shown');
            this.events.emit(LuminaAudioPlayer.PlayerEvent.VISIBILITY_CHANGED, { isVisible: false });
        }

        /**
         * Checks whether the bar is in its expanded form.
         * @returns {boolean} True when the bar is expanded.
         */
        isExpanded() {
            return this.container !== null && this.container.classList.contains('full-height');
        }

        /**
         * Gets the current state of the state machine.
         * @returns {string} One of the {@link LuminaAudioPlayer.PlayerState} values.
         */
        getState() {
            return this.state;
        }

        /**
         * Gets the descriptor of the track the player last started.
         * @returns {Object|null} The play descriptor, or null.
         */
        getDescriptor() {
            return this.descriptor;
        }

        /**
         * Gets the ordered queue of the current play context.
         * @returns {Array<Object>} The ordered play descriptors.
         */
        getQueue() {
            return [...this.queue];
        }

        /**
         * Gets the raw theme sub templates the player renders its dynamic content from.
         * @returns {Object} The sub templates, with a queueItem and a playlistItem source, each null when the theme provides none.
         */
        getSubTemplates() {
            return { ...this.subTemplates };
        }

        /**
         * Gets the playhead of the current track.
         * @returns {number} The playhead, in seconds.
         */
        getCurrentTime() {
            return this.currentTime;
        }

        /**
         * Gets the known duration of the current track.
         * @returns {number} The duration, in seconds.
         */
        getDuration() {
            return this.duration;
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
         * Gets the stored playback preferences loaded for the user.
         * @returns {Object} The preferences.
         */
        getPreferences() {
            // A shallow frozen copy, so a consumer cannot mutate the preferences the player uses.
            return Object.freeze({ ...this.preferences });
        }

        /**
         * Gets the last capability report, or null when detection has not completed yet.
         * @returns {Object|null} The capability report.
         */
        getCapabilities() {
            return this.capabilityReport;
        }

        /**
         * Gets the last typed error the player raised.
         * @returns {LuminaAudioPlayer.PlayerError|null} The last error, or null.
         */
        getLastError() {
            return this.lastError;
        }

        /**
         * Subscribes to a player event.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler invoked with the event payload.
         * @returns {Function} A function that unsubscribes the handler.
         */
        on(event, handler) {
            return this.events.on(event, handler);
        }

        /**
         * Subscribes to a player event once.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler invoked once with the event payload.
         * @returns {Function} A function that unsubscribes the handler.
         */
        once(event, handler) {
            return this.events.once(event, handler);
        }

        /**
         * Unsubscribes a handler from a player event.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler to remove.
         */
        off(event, handler) {
            this.events.off(event, handler);
        }

        /**
         * Releases every resource the player holds. It is intended for an explicit teardown only: the
         * in-place navigation manager never destroys the player, so that playback survives navigation.
         * @returns {Promise<void>} A promise that completes when the engine was destroyed.
         */
        async destroyAsync() {
            if (this.descriptorClickHandler) {
                document.removeEventListener('click', this.descriptorClickHandler);
                this.descriptorClickHandler = null;
            }
            for (const control of this.controlHandlers)
                control.element.removeEventListener(control.event, control.handler);
            this.controlHandlers = [];
            if (this.engine) {
                try {
                    await this.engine.destroyAsync();
                } catch (error) {
                    console.error('Audio player: the engine failed to destroy itself.', error);
                }
                this.engine = null;
            }
            this.events.clear();
            this.setState(LuminaAudioPlayer.PlayerState.IDLE);
            // The singleton is no longer usable, so it is cleared, and a later createDefaultPlayer builds a fresh one.
            if (LuminaAudioPlayer.instance === this)
                LuminaAudioPlayer.instance = null;
        }

        //+======================================================================================+
        //|                                   Internal helpers                                   |
        //+======================================================================================+

        /**
         * Moves the state machine to another state and publishes the change.
         * @param {string} nextState - One of the {@link LuminaAudioPlayer.PlayerState} values.
         */
        setState(nextState) {
            if (this.state === nextState)
                return;
            const previousState = this.state;
            const allowed = STATE_TRANSITIONS[previousState] || [];
            if (!allowed.includes(nextState))
                console.warn(`Audio player: the transition '${previousState}' -> '${nextState}' is not part of the state machine.`);
            this.state = nextState;
            this.events.emit(LuminaAudioPlayer.PlayerEvent.STATE_CHANGED, { previousState, state: nextState });
        }

        /**
         * Raises a typed error without moving the state machine.
         * @param {*} error - The error to raise, wrapped into a {@link LuminaAudioPlayer.PlayerError}.
         * @param {string} [code] - The code to use when the error is not already typed.
         * @returns {LuminaAudioPlayer.PlayerError} The raised typed error.
         */
        raiseError(error, code = LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR) {
            const typed = LuminaAudioPlayer.PlayerError.from(error, code);
            this.lastError = typed;
            console.error(`Audio player: ${typed.code}.`, typed.message);
            this.events.emit(LuminaAudioPlayer.PlayerEvent.ERROR, typed);
            return typed;
        }

        /**
         * Raises a typed error and moves the state machine to the error state.
         * @param {*} error - The error to raise.
         * @param {string} [code] - The code to use when the error is not already typed.
         * @returns {LuminaAudioPlayer.PlayerError} The raised typed error.
         */
        fail(error, code = LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR) {
            const typed = this.raiseError(error, code);
            this.setState(LuminaAudioPlayer.PlayerState.ERROR);
            return typed;
        }

        /**
         * Registers an engine factory on this player, by name.
         *
         * This is the documented seam through which the later stages plug the real download, decode and render
         * engine into the player, without reaching into its internals.
         * @param {string} name - The name the engine is selected by, later passed to {@link LuminaAudioPlayer.AudioPlayer#useEngine}.
         * @param {Function} factory - A factory that receives the engine host and returns an engine.
         * @returns {LuminaAudioPlayer.AudioPlayer} This player, for chaining.
         */
        registerEngine(name, factory) {
            this.engineRegistry.register(name, factory);
            return this;
        }

        /**
         * Selects the registered engine the player runs with. The selection takes effect on the next load.
         *
         * When an engine instance is already created, it is destroyed and dropped, so that the next load builds the
         * newly selected engine. Selecting an engine while a track plays therefore stops that track and returns the
         * player to its idle state, so it is meant to be called during setup, before playback starts.
         * @param {string} name - The name of a registered engine.
         * @returns {LuminaAudioPlayer.AudioPlayer} This player, for chaining.
         * @throws {LuminaAudioPlayer.PlayerError} When no engine with that name is registered.
         */
        useEngine(name) {
            if (name === this.engineName)
                return this;
            if (!this.engineRegistry.hasEngine(name))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, `No player engine named '${name}' is registered.`);
            this.engineName = name;
            if (this.engine) {
                const previous = this.engine;
                this.engine = null;
                // The engine that was playing is gone, so the player returns to idle instead of claiming it is still playing.
                if (this.state !== LuminaAudioPlayer.PlayerState.IDLE) {
                    this.setState(LuminaAudioPlayer.PlayerState.IDLE);
                    this.renderTransportState();
                }
                try {
                    Promise.resolve(previous.destroyAsync()).catch((error) => console.error('Audio player: the previous engine failed to destroy itself.', error));
                } catch (error) {
                    console.error('Audio player: the previous engine failed to destroy itself.', error);
                }
            }
            return this;
        }

        /**
         * Gets the name of the engine the player runs with.
         * @returns {string} The selected engine name.
         */
        getEngineName() {
            return this.engineName;
        }

        /**
         * Ensures an engine exists, creating it from the registry on first use.
         * @returns {Promise<Object>} The engine.
         */
        async ensureEngineAsync() {
            if (this.engine)
                return this.engine;
            this.engine = this.engineRegistry.create(this.engineName, this.createEngineHost());
            if (!this.engine)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, `No player engine named '${this.engineName}' is registered.`);
            // The stored volume is applied at creation, so the very first track starts at the volume the user last chose.
            await this.applyVolumeAsync();
            return this.engine;
        }

        /**
         * Builds the host object through which an engine reports back to the player.
         * @returns {Object} The engine host.
         */
        createEngineHost() {
            // The host is the only channel an engine uses to talk back, so the player stays the single source of truth and
            // an engine never reaches into the controller state; a report is routed to the matching handler here.
            return {
                events: this.events,
                // A shallow frozen copy, so an engine or a test cannot mutate the controller state through the host.
                getPreferences: () => Object.freeze({ ...this.preferences }),
                reportTime: (currentTime, duration) => this.handleEngineTime(currentTime, duration),
                reportDuration: (duration) => this.handleEngineDuration(duration),
                reportProgress: (fraction) => this.handleEngineProgress(fraction),
                reportEnded: () => this.handleEngineEnded(),
                reportError: (error) => this.fail(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR)
            };
        }

        /**
         * Handles a playhead report from the engine.
         *
         * The report only updates the player state and schedules the render, so that a burst of reports coalesces into
         * a single set of DOM writes and a single event emission per animation frame.
         * @param {number} currentTime - The current playhead, in seconds.
         * @param {number} duration - The known duration, in seconds.
         */
        handleEngineTime(currentTime, duration) {
            this.handleEngineDuration(duration);
            const position = Number(currentTime);
            this.currentTime = Number.isFinite(position) && position > 0 ? position : 0;
            this.scheduleProgressRender();
        }

        /**
         * Handles a duration report from the engine.
         * @param {number} duration - The known duration, in seconds.
         */
        handleEngineDuration(duration) {
            // An invalid, non positive or unchanged value is ignored: an engine may report its duration repeatedly, and
            // re-rendering or re-emitting for a value that did not move would churn the bar and its subscribers for nothing.
            const value = Number(duration);
            if (!Number.isFinite(value) || value <= 0 || value === this.duration)
                return;
            this.duration = value;
            // The total time is written by the coalesced progress render, so it is not written twice for one report.
            this.scheduleProgressRender();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.DURATION_CHANGED, { duration: this.duration });
        }

        /**
         * Handles a download progress report from the engine.
         * @param {number} fraction - The downloaded fraction, between 0 and 1.
         */
        handleEngineProgress(fraction) {
            const value = clamp(Number(fraction), 0, 1);
            this.downloadedFraction = value;
            this.scheduleProgressRender();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.PROGRESS, { downloadedFraction: value });
        }

        /**
         * Coalesces the progress bar DOM writes and the playhead event into a single animation frame, so that a
         * high frequency stream of engine reports does not turn into a high frequency stream of DOM writes.
         */
        scheduleProgressRender() {
            if (this.isProgressRenderScheduled)
                return;
            this.isProgressRenderScheduled = true;
            const render = () => {
                this.isProgressRenderScheduled = false;
                this.renderProgress();
                this.events.emit(LuminaAudioPlayer.PlayerEvent.TIME_UPDATE, { currentTime: this.currentTime, duration: this.duration });
            };
            if (typeof requestAnimationFrame === 'function')
                requestAnimationFrame(render);
            else
                render();
        }

        /**
         * Handles the end of the current track reported by the engine.
         */
        handleEngineEnded() {
            this.setState(LuminaAudioPlayer.PlayerState.ENDED);
            this.renderTransportState();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.ENDED);
        }

        /**
         * Detects the browser capabilities and publishes a typed error when one is missing.
         * @returns {Promise<Object>} The capability report.
         */
        async detectCapabilitiesAsync() {
            const report = await this.capabilities.detectAsync();
            this.capabilityReport = report;
            this.events.emit(LuminaAudioPlayer.PlayerEvent.CAPABILITIES_CHANGED, report);
            if (!report.isSupported) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }));
            }
            return report;
        }

        /**
         * Reads the stored playback preferences of the user: the behavior preferences from the user
         * settings endpoint, and the last volume and mute state from the playback state endpoint.
         * @returns {Promise<void>} A promise that completes when the preferences were read.
         */
        async loadPreferencesAsync() {
            const configuration = this.getConfiguration();
            const [settings, playbackState] = await Promise.all([
                this.fetchJsonAsync(configuration.getUserSettingsUrl),
                this.fetchJsonAsync(configuration.getPlaybackStateUrl)
            ]);
            // A redirect means the session expired and the page is already navigating to the sign in page, so nothing is
            // reported and nothing is applied, instead of treating an expected outcome as a failure.
            if (settings === REDIRECTED || playbackState === REDIRECTED)
                return;
            if (settings === undefined || playbackState === undefined) 
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.PREFERENCES_LOAD_FAILED, 'The stored playback preferences of the user could not be read.'));
            if (settings) {
                this.preferences = {
                    shouldKeepDownloadingWhilePaused: settings.shouldKeepDownloadingWhilePaused === true,
                    shouldAutoplay: settings.shouldAutoplay === true,
                    isGaplessPlaybackEnabled: settings.isGaplessPlaybackEnabled === true,
                    replayGainMode: settings.replayGainMode || 'Off',
                    streamingQuality: settings.streamingQuality || 'Auto'
                };
                // The payload is a frozen copy, so a subscriber that mutates it cannot change the preferences the player uses.
                this.events.emit(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, Object.freeze({ ...this.preferences }));
            }
            if (playbackState) {
                if (Number.isFinite(Number(playbackState.volume)))
                    await this.setVolumeAsync(clamp(Number(playbackState.volume), 0, 1));
                if (typeof playbackState.isMuted === 'boolean')
                    await this.setMutedAsync(playbackState.isMuted);
            }
        }

        /**
         * Gets the endpoints configuration of the player.
         * @returns {Object} The endpoints configuration.
         */
        getConfiguration() {
            // Options win, so a composition or a test can inject endpoints; otherwise the global object the layout emits is
            // used, and an empty object is returned when neither exists, so a read of a missing endpoint is a no-op rather
            // than a crash.
            return this.options.configuration || window.audioPlayerConfiguration || {};
        }

        /**
         * Reads a JSON payload from a web endpoint, without surfacing a notification on failure, because
         * the request is a background one that must never disturb the page.
         *
         * An expired session is answered by the server with a redirect to the login page, so the redirect is followed
         * here, exactly like the application API helper does it, instead of being mistaken for a failed request.
         * @param {string} url - The endpoint URL. Nothing is requested when it is empty.
         * @returns {Promise<*|null|undefined|Object>} The unwrapped data, null when the endpoint answered without data, undefined when the request failed, or the REDIRECTED marker when the session expired and the page is navigating away.
         */
        async fetchJsonAsync(url) {
            if (!url)
                return null;
            try {
                const response = await fetch(url, {
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                if (response.redirected) {
                    // The session expired and the server answered with the login page; follow it, and report the redirect, not a failure.
                    window.location.href = response.url;
                    return REDIRECTED;
                }
                if (!response.ok)
                    return undefined;
                const payload = await response.json();
                return payload && payload.success === true ? payload.data : null;
            } catch (error) {
                console.error('Audio player: a preferences request failed.', error);
                return undefined;
            }
        }

        /**
         * Applies the effective volume to the engine.
         * @returns {Promise<void>} A promise that completes when the engine received the volume.
         */
        async applyVolumeAsync() {
            // Before the first play there is no engine to receive the volume, so it is remembered and applied when the engine is
            // created, which keeps a volume set before playback from being lost.
            if (!this.engine)
                return;
            try {
                await this.engine.setVolumeAsync(this.getEffectiveVolume());
            } catch (error) {
                this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
            }
        }

        /**
         * Gets the volume actually sent to the output, which is zero while muted.
         * @returns {number} The effective volume, between 0 and 1.
         */
        getEffectiveVolume() {
            // Mute is a flag of its own, so the volume is kept untouched and only the value sent to the output is zeroed;
            // that is what lets unmuting restore the volume the user had set rather than a default.
            return this.muted ? 0 : this.volume;
        }

        /**
         * Builds the payload shared by the volume and mute events.
         * @returns {Object} The volume payload.
         */
        buildVolumePayload() {
            return { volume: this.volume, muted: this.muted, effectiveVolume: this.getEffectiveVolume() };
        }

        /**
         * Handles a click on the progress bar as a seek to the clicked ratio.
         * @param {MouseEvent} event - The click event on the progress bar.
         */
        handleProgressClick(event) {
            // Without a track, without a known duration, without the bar, or with a zero width bar there is no position to
            // map the click to, and dividing by the width would produce a NaN seek, so the click is ignored instead.
            if (!this.descriptor || this.duration <= 0 || !this.dom.progress)
                return;
            const bounds = this.dom.progress.getBoundingClientRect();
            if (bounds.width <= 0)
                return;
            const ratio = clamp((event.clientX - bounds.left) / bounds.width, 0, 1);
            this.seekAsync(ratio * this.duration);
        }

        /**
         * Ensures the bar starts hidden, so that a page load never flashes an empty player.
         */
        applyHiddenState() {
            this.container.classList.add('not-shown');
            if (this.main)
                this.main.classList.add('audio-player-not-shown');
        }

        /**
         * Suppresses the transitions of the bar and of the content area while the initial hidden state is applied, so
         * that a page load never slides either of them into place. The suppression is removed after the next frame,
         * once the hidden state is already in effect, so later show and hide transitions keep working.
         */
        suppressInitialTransitions() {
            const root = document.documentElement;
            if (!root || !root.classList)
                return;
            root.classList.add('audio-player-initializing');
            const release = () => root.classList.remove('audio-player-initializing');
            if (typeof requestAnimationFrame === 'function')
                requestAnimationFrame(() => requestAnimationFrame(release));
            else
                release();
        }

        /**
         * Removes the initial 'hidden' attribute of the bar, now that the 'not-shown' class already keeps it hidden.
         *
         * The markup ships the bar with the 'hidden' attribute, so that it cannot be painted before this script runs,
         * no matter how the stylesheets settle. By the time the attribute is removed the bar is already at its hidden
         * height, so the removal is never visible; the class that remains is what later show and hide transitions act on.
         */
        releaseInitialHiddenState() {
            const release = () => {
                if (this.container)
                    this.container.removeAttribute('hidden');
            };
            if (typeof requestAnimationFrame === 'function')
                requestAnimationFrame(release);
            else
                release();
        }

        /**
         * Renders the track name, the cover art and the total time of the current descriptor.
         */
        renderDescriptor() {
            const descriptor = this.descriptor;
            if (this.dom.trackName)
                this.dom.trackName.textContent = descriptor ? descriptor.title : '';
            const coverArtUrl = descriptor && descriptor.coverArtUrl
                ? descriptor.coverArtUrl
                : '/images/ui/music-placeholder.svg';
            if (this.dom.coverThumbnail)
                this.dom.coverThumbnail.src = coverArtUrl;
            if (this.dom.coverImage)
                this.dom.coverImage.src = coverArtUrl;
            if (this.dom.totalTime)
                this.dom.totalTime.textContent = formatDuration(this.duration);
            this.renderTransportState();
        }

        /**
         * Renders the three state progress bar: the played part, the downloaded part, and the remainder.
         */
        renderProgress() {
            // Only widths are written here, as percentages; the colors, shape and layering of the three states belong to the
            // theme's CSS, which selects them through the classes on the bar elements, so the player never hardcodes a look.
            if (this.dom.currentTime)
                this.dom.currentTime.textContent = formatTime(this.currentTime);
            if (this.dom.totalTime)
                this.dom.totalTime.textContent = formatDuration(this.duration);
            const playedRatio = this.duration > 0 ? clamp(this.currentTime / this.duration, 0, 1) : 0;
            const downloadedRatio = clamp(this.downloadedFraction, 0, 1);
            if (this.dom.progressPlayed)
                this.dom.progressPlayed.style.width = `${playedRatio * 100}%`;
            if (this.dom.progressDownloaded)
                this.dom.progressDownloaded.style.width = `${downloadedRatio * 100}%`;
        }

        /**
         * Renders the play or pause affordance of the transport control.
         */
        renderTransportState() {
            const isPlaying = this.state === LuminaAudioPlayer.PlayerState.PLAYING;
            if (this.dom.playPause)
                this.dom.playPause.classList.toggle('is-playing', isPlaying);
        }

        /**
         * Renders the ordered queue in the expanded bar. The list is only shown when the play context
         * holds more than one track, so that a single track never renders a one item list.
         *
         * Each item is rendered from the 'shared/audio-player/queue-item' sub template of the active theme when it
         * provides one, so that the theme owns the structure of a queue item, and the built-in markup is used otherwise.
         */
        renderQueue() {
            if (!this.dom.queue)
                return;
            this.dom.queue.innerHTML = '';
            const items = this.queue.length > 1 ? this.queue : [];
            for (let index = 0; index < items.length; index++) {
                const entry = this.renderQueueItem(items[index], index, items.length);
                if (entry)
                    this.dom.queue.appendChild(entry);
            }
        }

        /**
         * Renders one queue item, from the theme sub template when the theme provides one and from the built-in markup
         * otherwise.
         * @param {Object} item - The play descriptor of the item.
         * @param {number} index - The zero based position of the item in the queue.
         * @param {number} totalItems - The number of items in the queue.
         * @returns {HTMLElement|null} The rendered queue item, or null when the markup produced no element.
         */
        renderQueueItem(item, index, totalItems) {
            // The model exposes only what a queue item can show, so a theme template never reaches into the player internals.
            const viewModel = {
                index: index + 1,
                trackId: item.trackId,
                albumId: item.albumId,
                artistId: item.artistId,
                libraryId: item.libraryId,
                title: item.title || '',
                duration: formatDuration(item.durationInSeconds),
                durationSeconds: item.durationInSeconds,
                coverArtUrl: item.coverArtUrl || '',
                isCurrent: index === this.currentIndex,
                isPlayed: index < this.currentIndex,
                isLast: index === totalItems - 1
            };
            if (this.subTemplates.queueItem) {
                try {
                    const container = document.createElement('div');
                    container.innerHTML = window.renderThemeTemplate(this.subTemplates.queueItem, viewModel);
                    const entry = container.firstElementChild;
                    if (entry)
                        return entry;
                } catch (error) {
                    // A broken theme sub template falls through to the built-in markup, so the queue never disappears.
                    console.error('Audio player: the theme queue item template could not be rendered.', error);
                }
            }
            return this.buildDefaultQueueItem(viewModel);
        }

        /**
         * Builds one queue item with the built-in markup, used when the theme provides no queue item sub template.
         * @param {Object} viewModel - The queue item model, per {@link LuminaAudioPlayer.AudioPlayer#renderQueueItem}.
         * @returns {HTMLElement} The built-in queue item.
         */
        buildDefaultQueueItem(viewModel) {
            const entry = document.createElement('li');
            entry.className = 'audio-player-queue-item';
            entry.dataset.trackId = viewModel.trackId;
            if (viewModel.isCurrent)
                entry.classList.add('is-current');
            const title = document.createElement('span');
            title.className = 'audio-player-queue-item-title';
            title.textContent = viewModel.title;
            const duration = document.createElement('span');
            duration.className = 'audio-player-queue-item-duration';
            duration.textContent = viewModel.duration;
            entry.appendChild(title);
            entry.appendChild(duration);
            return entry;
        }
    }

    LuminaAudioPlayer.AudioPlayer = AudioPlayer;

    // Expose the descriptor helpers on the namespace, so that a page or a test builds a descriptor the same way the player does.
    LuminaAudioPlayer.createPlayDescriptor = createPlayDescriptor;
    LuminaAudioPlayer.resolveCoverArtUrl = resolveCoverArtUrl;
    LuminaAudioPlayer.readPlayDescriptorFromElement = readPlayDescriptorFromElement;
    LuminaAudioPlayer.formatPlayerTime = formatTime;
    LuminaAudioPlayer.formatPlayerDuration = formatDuration;
    LuminaAudioPlayer.UNKNOWN_TIME = UNKNOWN_TIME;

    /**
     * Creates the singleton player of the application, binds it to the layout bar, and exposes it as
     * <code>window.LuminaAudioPlayer.instance</code>. The player is created exactly once: the in-place navigation
     * manager swaps the content area only, so the player and its playback survive every navigation.
     *
     * This function is invoked automatically when the module loads, so the singleton is already created before any
     * other script runs. The instance seam is therefore the way to configure it afterwards:
     * <code>window.LuminaAudioPlayer.instance.registerEngine(name, factory).useEngine(name)</code>. The optional
     * options are honored only when this is the first creation of the player, for a composition that creates the
     * player itself, such as a test.
     * @param {Object} [options] - The player options, per the {@link LuminaAudioPlayer.AudioPlayer} constructor. Honored only on the first creation.
     * @returns {LuminaAudioPlayer.AudioPlayer} The singleton player.
     */
    LuminaAudioPlayer.createDefaultPlayer = function (options) {
        if (LuminaAudioPlayer.instance)
            return LuminaAudioPlayer.instance;
        const player = new AudioPlayer(options);
        player.initialize();
        LuminaAudioPlayer.instance = player;
        return player;
    };

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', () => LuminaAudioPlayer.createDefaultPlayer());
    else
        LuminaAudioPlayer.createDefaultPlayer();
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
