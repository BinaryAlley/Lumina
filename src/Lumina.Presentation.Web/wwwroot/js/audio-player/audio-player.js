/**
 * The audio player of Lumina: state machine, play descriptor, lifecycle, bar and the public control API.
 *
 * The player is a singleton created once, in the application layout, outside the content area that the
 * in-place navigation manager swaps. Because it is never torn down by a page swap, playback and its
 * UI survive navigation. It produces no audio itself: audio is delegated to a swappable engine that
 * fulfills the interface documented in audio-player-engine.js. This module only owns the state machine, the
 * event bus, capability detection, the play descriptor, the bar, the transport controls it binds and renders, and the
 * public API. The download, decode and render pipeline is plugged in as a swappable engine. The bar holds
 * the three state progress bar (played, downloaded but not played, and not yet downloaded), the volume and mute
 * controls, the keyboard shortcuts, and the loading state a seek into a not yet downloaded position shows; the volume
 * and mute the controls change are persisted to the stored playback state of the user on their own.
 *
 * Public control API (window.LuminaAudioPlayer.instance)
 * ----------------------------------------
 * Methods:
 *   - playAsync(descriptor, options): starts playback of a track. The descriptor is documented by createPlayDescriptor.
 *                            When the descriptor carries a queue, that queue becomes the player queue. When it names only
 *                            an album or an artist, the album or the discography is loaded into the queue through the
 *                            music endpoints documented in audio-player-queue.js. The optional `shouldStartRandomTrack`
 *                            option starts a random track of that context instead of its first track.
 *   - playNextAsync(descriptor): queues a track or an album after the current entry, without starting playback. It
 *                            discards the pending autoplay continuation, because an explicit play next is a manual queue.
 *   - playShuffledAsync(descriptor, scope): starts the described context and turns shuffle on for the given scope. When the
 *                            scope is omitted, it shuffles the release of the started context.
 *   - playLibraryShuffledAsync(libraryId): starts a shuffled playback of a whole media library, drawing the first track
 *                            and every later track from the library shuffle source.
 *   - nextAsync(): starts the next entry of the queue, wrapping at the end only when repeat all is on.
 *   - previousAsync(): returns to the previous entry from the navigation history. When the playhead is past 3 seconds
 *                      it restarts the current track instead, and pressing it again at the start walks back.
 *   - setRepeatMode(mode), setRepeatModeAsync(mode): set repeat off, all or one.
 *   - cycleRepeatMode(), cycleRepeatModeAsync(): cycle off, all, one and back to off.
 *   - getRepeatMode(): the current repeat mode.
 *   - setShuffleAsync(enabled, scope), toggleShuffleAsync(scope): shuffle a release, an artist or the whole library,
 *                            one batch of randomized tracks at a time, as documented in audio-player-queue.js.
 *   - getShuffleState(): whether shuffle is on and the scope it draws from.
 *   - getQueueState(): the full queue entries, cursor, history, repeat mode, shuffle mode and play context.
 *   - getQueueStateSummary(): the compact queue state published to observers.
 *   - pauseAsync(): suspends playback, keeping the position.
 *   - resumeAsync(): resumes playback from the current position.
 *   - toggleAsync(): pauses when playing, resumes otherwise.
 *   - stopAsync(): stops playback. While the bar is condensed, it also hides the bar; while the bar is
 *                  expanded, it stays on screen.
 *   - seekAsync(seconds): moves the playhead to an absolute position, in seconds.
 *   - setVolumeAsync(volume): sets the output volume, between 0 and 1. Volume and mute are persisted to the stored
 *                             playback state of the user, so they survive a reload.
 *   - setMutedAsync(muted): sets the mute state. Volume and mute are independent: unmuting restores the volume.
 *   - toggleMuteAsync(): flips the mute state.
 *   - toggleExpanded(): expands or collapses the bar.
 *   - toggleVisibility(): shows or hides the bar.
 *   - on(event, handler): subscribes to a player event, returns an unsubscribe function.
 *   - once(event, handler), off(event, handler): one shot subscribe, and unsubscribe.
 *   - getState(), getDescriptor(), getQueue(), getCurrentTime(), getDuration(), getVolume(),
 *     isMuted(), isBuffering(), getBufferedTimeRanges(), getPreferences(), getCapabilities(), getLastError(),
 *     getSubTemplates().
 *   - getNextDescriptor(): the identity and descriptor of the next queue entry, or null.
 *   - advanceQueueTo(entryId): moves the queue cursor to the entry with that identity without starting playback, so a
 *                           linear advance, a wrap, a reordered queue and a gapless handoff share one commit. The advance
 *                           is refused when the identity is no longer part of the queue.
 *   - requestNextTrackAsync(reason): starts the next queue entry through the player's own play path, used by the engine
 *                           when a gapless handoff could not be prepared.
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

    // The redirect sentinel belongs to the dependency free utils module, so a consumer that is loaded without this
    // controller, such as the manifest client, can still recognize it. The local name keeps the references readable.
    const REDIRECTED = LuminaAudioPlayer.REDIRECTED;

    /**
     * Follows a server redirect, but only when the target is an http or https URL on the same origin and protocol as
     * the current page, and carries no credentials.
     *
     * An expired session is answered with a redirect to the sign in page, which is on this origin. A redirect to any
     * other origin, which a misconfigured proxy that trusts a forwarded host header could produce, is not followed, so
     * the player never turns a session expiry into an open redirect. The protocol is checked separately, so a target
     * that only matches because an opaque document and a javascript:, data: or about: target all report the origin
     * 'null' is not followed either. A target that embeds a user name or a password is dropped too, because the origin
     * check ignores the userinfo, so such a target would otherwise put attacker-chosen credentials in the address bar.
     * The target is resolved against the current page, so a relative target is handled, and a malformed one is dropped.
     * @param {string} url - The redirect target the server answered with.
     */
    function navigateToRedirect(url) {
        if (!url)
            return;
        try {
            const target = new URL(url, window.location.href);
            // The target must stay on the origin of the page and use its protocol. The protocol is checked separately
            // from the origin because an opaque document, such as about:blank, reports the origin 'null', which a
            // javascript: or data: target also reports, so the origin alone would let such a target through. The scheme
            // is also confined to http and https, so a redirect can never introduce one. A target that embeds a user
            // name or a password is dropped too, because the origin check ignores the userinfo, so such a target would
            // otherwise put attacker-chosen credentials in the address bar.
            const isHttp = target.protocol === 'http:' || target.protocol === 'https:';
            const isSameOrigin = target.origin === window.location.origin
                && target.protocol === window.location.protocol;
            if (!isHttp || !isSameOrigin || target.username !== '' || target.password !== '') {
                console.warn('Audio player: a redirect that is not to this page was not followed.');
                return;
            }
            window.location.href = target.href;
        } catch (error) {
            console.debug('Audio player: a redirect target could not be followed.', error);
        }
    }

    /**
     * Checks whether a response is a redirect that was not followed because the request asked for manual redirects.
     * @param {Response} response - The response to check.
     * @returns {boolean} True when the response is a redirect.
     */
    function isRedirectResponse(response) {
        if (!response)
            return false;
        if (response.type === 'opaqueredirect')
            return true;
        const status = Number(response.status);
        return status === 301 || status === 302 || status === 303 || status === 307 || status === 308;
    }

    /**
     * Resolves a configured endpoint URL against the origin of the page, so a root relative configuration value is always
     * requested from the page's own origin and not from whatever the server saw as its host or a base element points at.
     * @param {string} url - The configured URL.
     * @returns {string|null} The resolved absolute URL, or null when it is missing or not on this origin.
     */
    function resolveConfiguredUrl(url) {
        if (typeof url !== 'string' || url.length === 0)
            return null;
        try {
            const resolved = new URL(url, window.location.origin);
            if ((resolved.protocol !== 'http:' && resolved.protocol !== 'https:')
                || resolved.origin !== window.location.origin
                || resolved.username !== ''
                || resolved.password !== '')
                return null;
            return resolved.href;
        } catch (error) {
            return null;
        }
    }

    /**
     * The attribute that marks an element as a play trigger the player handles by event delegation.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_DESCRIPTOR_ATTRIBUTE = 'data-audio-player-play';

    /**
     * The attribute that marks an element as a play next trigger the player handles by event delegation. An explicit play
     * next queues the item after the current entry and discards the pending autoplay continuation.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_NEXT_ATTRIBUTE = 'data-audio-player-play-next';

    /**
     * The attribute that marks an element as a shuffled play trigger the player handles by event delegation. The trigger
     * starts the described context and turns shuffle on for the scope in {@link LuminaAudioPlayer.PLAY_SHUFFLE_SCOPE_ATTRIBUTE}.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_SHUFFLED_ATTRIBUTE = 'data-audio-player-play-shuffled';

    /**
     * The attribute that carries the shuffle scope of a shuffled play trigger. It holds one of the
     * {@link LuminaAudioPlayer.QueueShuffleScope} values.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_SHUFFLE_SCOPE_ATTRIBUTE = 'data-audio-player-shuffle-scope';

    /**
     * The attribute that marks an element as a library play trigger the player handles by event delegation. It carries
     * only a library Id, because a whole library play context starts at the first artist of the library.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_LIBRARY_ATTRIBUTE = 'data-audio-player-play-library';

    /**
     * The attribute that marks an element as a library shuffle trigger the player handles by event delegation. It carries
     * only a library Id, because a whole library shuffle has no track, album or artist context to start from.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.PLAY_LIBRARY_SHUFFLED_ATTRIBUTE = 'data-audio-player-play-library-shuffled';

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
        albumTitle: 'albumTitle',
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
     * The artwork the bar and the queue show when a track has no cover art.
     * @readonly
     * @type {string}
     */
    const MUSIC_PLACEHOLDER_URL = '/images/ui/music-placeholder.svg';

    /**
     * The placeholder album title the scanner stores for a track whose tags carry no release name. The bar leaves the
     * album line empty for it, so a track without an album shows only its own name.
     * @readonly
     * @type {string}
     */
    const UNKNOWN_ALBUM_TITLE = 'Unknown Album';

    /**
     * Gets the album title a bar shows for the provided descriptor value, or an empty string when there is none.
     *
     * The scanner stores a placeholder release name for a track whose tags carry no album, so the placeholder is treated
     * as an absent album. The comparison is case insensitive and ignores surrounding whitespace, because the stored
     * value can be edited or re-read from a file.
     * @param {string} albumTitle - The raw album title of the descriptor.
     * @returns {string} The album title to show, or an empty string when the track has no album name to show.
     */
    function getDisplayAlbumTitle(albumTitle) {
        const title = typeof albumTitle === 'string' ? albumTitle.trim() : '';
        return title.toLowerCase() === UNKNOWN_ALBUM_TITLE.toLowerCase() ? '' : title;
    }

    /**
     * Normalizes a stored artwork path for comparison. The stored paths can use either directory separator, and the
     * server stores them with the casing of the file system, so the comparison ignores both.
     * @param {string} path - The artwork path.
     * @returns {string} The normalized path, or an empty string.
     */
    function normalizeArtworkPath(path) {
        return typeof path === 'string' ? path.trim().replace(/\\/g, '/').toLowerCase() : '';
    }

    /**
     * Formats a music release type for display, expanding the compound enum names and the common abbreviations.
     * @param {string} type - The release type, as returned by the API.
     * @returns {string} The display label of the release type.
     */
    function formatReleaseType(type) {
        const value = typeof type === 'string' ? type.trim() : '';
        if (!value)
            return '';
        if (value === 'Ep')
            return 'EP';
        if (value === 'DjMix')
            return 'DJ Mix';
        return value.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    /**
     * The number of seconds the left and right arrow keys move the playhead by, matching the seek step of the
     * reference web player.
     * @readonly
     * @type {number}
     */
    const SEEK_STEP_SECONDS = 5;

    /**
     * The fraction of the full volume the up and down arrow keys change the volume by, matching the volume step of
     * the reference web player.
     * @readonly
     * @type {number}
     */
    const VOLUME_STEP = 0.05;

    /**
     * The delay, in milliseconds, that coalesces the volume and mute writes a drag of the volume control produces
     * into a single request, so that dragging across the control never sends one request per pixel.
     * @readonly
     * @type {number}
     */
    const PLAYBACK_STATE_PERSIST_DELAY_MS = 500;

    /**
     * The time, in milliseconds, after which a play context, continuation or shuffle fetch is abandoned. A request that
     * never answers must not leave the player loading forever or keep a queue extension promise pending, so the build is
     * aborted and the affected batch is treated as exhausted.
     * @readonly
     * @type {number}
     */
    const QUEUE_BUILD_TIMEOUT_MS = 60000;

    /**
     * The position, in seconds, after which the previous action restarts the current track instead of returning to the
     * previous entry when there is no navigation history. It matches the convention of the reference web players.
     * @readonly
     * @type {number}
     */
    const PREVIOUS_RESTART_THRESHOLD_SECONDS = 3;

    /**
     * The tolerance, in seconds, within which a playhead report counts as having reached a pending seek target, so
     * that the loading state of the seek is cleared once playback resumes there.
     * @readonly
     * @type {number}
     */
    const SEEK_SETTLE_TOLERANCE_SECONDS = 0.25;

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
     *   - trackId (string, optional): the Id of the track. A track descriptor requires trackId, albumId and artistId,
     *     because the playback manifest route needs all four Ids.
     *   - albumId (string, optional): the Id of the album. An album play context requires albumId and artistId.
     *   - artistId (string, optional): the Id of the artist. An artist play context is identified by artistId alone.
     *   - isLibraryContext (boolean, optional): true for a library play context, which is identified by libraryId alone.
     *   - title (string, optional): the display title of the track or context.
     *   - albumTitle (string, optional): the display title of the album the track belongs to, shown under the track name.
     *   - durationInSeconds (number, optional): the duration of the track, in seconds.
     *   - coverArtPath (string, optional): the relative artwork path of the album, resolved against the server base path.
     *   - coverArtUrl (string, optional): an absolute cover art URL, or a relative artwork path.
     *   - queue (Array, optional): the ordered track descriptors of the play context, used to render the expanded bar.
     *     A descriptor without any track, album or artist Id is valid when it is a library play context or when it carries a non empty queue.
     * @param {Object} input - The descriptor to normalize.
     * @returns {Object|null} The normalized descriptor, or null when the required Ids of its context are missing.
     */
    function createPlayDescriptor(input) {
        if (!input || typeof input !== 'object')
            return null;
        const trackId = LuminaAudioPlayer.toIdString(input.trackId);
        const albumId = LuminaAudioPlayer.toIdString(input.albumId);
        const artistId = LuminaAudioPlayer.toIdString(input.artistId);
        const libraryId = LuminaAudioPlayer.toIdString(input.libraryId);
        if (!libraryId)
            return null;
        const queue = Array.isArray(input.queue)
            ? input.queue
                .map(item => createPlayDescriptor(item))
                // An explicit queue holds playable tracks only. An album or artist context entry has no track to load, so it
                // is dropped instead of becoming a dead queue item.
                .filter(item => item !== null && item.trackId && item.albumId && item.artistId && item.libraryId)
            : null;
        const hasQueue = queue !== null && queue.length > 0;
        const isLibraryContext = input.isLibraryContext === true;
        // A track descriptor must carry all four Ids the manifest route needs, and an album context must carry its
        // artist. A context without any of them is valid only when it is an explicit library context or a queue.
        if (trackId) {
            if (!albumId || !artistId)
                return null;
        } else if (albumId) {
            if (!artistId)
                return null;
        } else if (!artistId && !hasQueue && !isLibraryContext) {
            return null;
        }
        const coverArtSource = input.coverArtUrl || input.coverArtPath || '';
        const descriptor = {
            trackId,
            libraryId,
            albumId,
            artistId,
            isLibraryContext,
            title: typeof input.title === 'string' ? input.title : '',
            albumTitle: typeof input.albumTitle === 'string' ? input.albumTitle : '',
            durationInSeconds: Number.isFinite(Number(input.durationInSeconds)) ? Number(input.durationInSeconds) : 0,
            coverArtPath: typeof input.coverArtPath === 'string' ? input.coverArtPath : '',
            coverArtUrl: resolveCoverArtUrl(coverArtSource)
        };
        if (queue !== null)
            descriptor.queue = Object.freeze(queue);
        return Object.freeze(descriptor);
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
     * {@link LuminaAudioPlayer.PLAY_DESCRIPTOR_DATA_KEYS}. A trigger can describe a track, an album context or an artist
     * context, so the descriptor is valid whenever its context Ids are complete, not only when it names a track.
     * @param {Element} element - The element carrying the descriptor attributes.
     * @returns {Object|null} The normalized descriptor, or null when the element does not describe a valid track, album or artist context.
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
            albumTitle: element.dataset[keys.albumTitle],
            durationInSeconds: element.dataset[keys.durationInSeconds],
            coverArtPath: element.dataset[keys.coverArtPath],
            coverArtUrl: element.dataset[keys.coverArtUrl],
            isLibraryContext: element.hasAttribute(LuminaAudioPlayer.PLAY_LIBRARY_ATTRIBUTE)
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
            // The queue is the single source of truth of what the player may play next: its entries carry an identity per
            // occurrence, so a repeated track is distinguishable from another occurrence of the same track. The model, the
            // play context and the client storage are documented in audio-player-queue.js.
            this.queue = options.queue || new LuminaAudioPlayer.AudioQueue();
            // The service that builds a queue from a play context through the web endpoints of the music pages, and the
            // source a shuffle draws its batches from. Both are created lazily when a context or a shuffle needs them.
            this.queueService = options.queueService || new LuminaAudioPlayer.QueueContextService({
                fetchJsonAsync: (url, signal, label) => this.fetchJsonAsync(url, signal, label),
                postJsonAsync: (url, data, requestOptions) => this.postJsonAsync(url, data, requestOptions),
                getConfiguration: () => this.getConfiguration(),
                getShouldIgnoreThePrefix: () => this.preferences.shouldIgnoreThePrefixForAlphaPicker === true
            });
            this.shuffleSource = null;
            // The expanded player presents the current track's album details. They are loaded only while the bar is expanded,
            // cached per album, and ignored when the descriptor has moved to another album before the request settled.
            this.nowPlayingAlbumKey = '';
            this.nowPlayingDetailsToken = 0;
            this.nowPlayingDetailsAbortController = null;
            this.nowPlayingDetailsRequestKey = '';
            this.nowPlayingDetails = null;
            this.nowPlayingDetailsCache = new Map();
            this.nowPlayingArtworkPath = '';
            // The generation of the queue, incremented whenever the queue is replaced, so a batch that was requested from the
            // previous queue can never be appended to the queue that replaced it.
            this.queueGeneration = 0;
            // The token of the play request that owns the player, incremented by every play, skip and stop, so a request that
            // was overtaken while it awaited its context or its engine cannot overwrite the newer one.
            this.playRequestToken = 0;
            // The build handles of the play context fetch, the background batch fetch (continuation or shuffle) and the
            // explicit play next fetch. They are separate so a skip or a background batch can never silently abort the item
            // the user chose to play next, and a play next never aborts a background batch.
            this.playContextBuildHandle = null;
            this.queueBatchBuildHandle = null;
            this.playNextBuildHandle = null;
            // True while a shuffle scope is being resolved, so a concurrent continuation does not abort its source fetch.
            this.isShufflePending = false;
            this.shuffleToken = 0;
            // The order the queue list was last rendered in, so a reorder that changes the queue without rendering the list
            // is detected instead of updating the wrong items in place.
            this.renderedQueueOrder = [];
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
                streamingQuality: 'Auto',
                shouldIgnoreThePrefixForAlphaPicker: false
            };
            this.capabilityReport = null;
            this.capabilitiesReady = null;
            this.engine = null;
            this.isAvailable = false;
            this.isInitialized = false;
            // True while the bar shows a loading state, which is either a seek waiting for the range of its target, or
            // the engine reporting that it is itself waiting for data. Boolean state keeps its conventional is/has
            // prefix, except where a public is*() accessor already owns that name: isBuffering() is that accessor here,
            // so the backing field keeps the participle instead of shadowing the method.
            this.buffering = false;
            // The position a pending seek is waiting for, or null when no seek is pending.
            this.pendingSeekTime = null;
            // The pending seek time the last loading state event carried, so a second seek that only moves the target
            // while the bar is already buffering still publishes the new target.
            this.announcedPendingSeekTime = null;
            // The position the pending seek started from, so a backward seek is only cleared once the playhead crosses
            // down into the target instead of by the pre-seek position the engine reports first.
            this.pendingSeekFromTime = null;
            // True while the engine reports that it is waiting for data on its own, which is independent of a seek and
            // is therefore never cleared by a playhead report.
            this.engineBuffering = false;
            // The ratio, between 0 and 1, the pointer currently scrubs the progress bar to, or null when no scrub is
            // in progress. While it is set the bar shows the scrub position instead of the reported playhead.
            this.scrubRatio = null;
            this.isScrubbing = false;
            // The pointer Id captured by the progress bar or the volume control, or null, so the capture is always
            // released with the Id it was taken with, whatever ends the drag.
            this.scrubPointerId = null;
            this.volumePointerId = null;
            this.isAdjustingVolume = false;
            // The bounds of a control, measured once when a drag starts, so a pointer move never has to read the layout
            // again, which would force a reflow per event.
            this.progressBounds = null;
            this.volumeBounds = null;
            // The latest ratio of a volume drag and whether a render of it is already scheduled, so a burst of pointer
            // moves is coalesced into one volume change per frame.
            this.pendingVolumeRatio = null;
            this.isVolumeApplyScheduled = false;
            // The last values written to the bar, so a render skips the DOM writes whose value did not move; the clock
            // and the accessible value only change once per second, while reports arrive far more often.
            this.renderedProgress = null;
            // True once the user changes the volume or the mute state, so a stored state that arrives after the change
            // is not applied over the user's choice.
            this.hasUserChangedVolume = false;
            this.hasUserChangedMute = false;
            // True once the stored playback state has been read successfully, i.e. once the player knows the values the
            // server holds. A write is refused until then, so a flush on unload can never overwrite the stored volume
            // and mute state with the local defaults of a player that has not loaded them yet.
            this.isStoredPlaybackStateLoaded = false;
            this.persistenceTimer = null;
            this.lastPersistedPlaybackState = null;
            this.keyboardHandler = null;
            this.unloadHandler = null;
            this.container = null;
            this.main = null;
            this.dom = {};
            // The raw theme sub templates of the dynamic content, read once with the bar elements and null when the theme does not provide them.
            this.subTemplates = { queueItem: null, playlistItem: null };
            this.descriptorClickHandler = null;
            this.controlHandlers = [];
            this.isProgressRenderScheduled = false;
            // The in flight batch extension of the queue, shared by every caller that needs the same next batch.
            this.queueExtensionPromise = null;
            // Explicit play next items that arrived while a play context was still resolving. They are inserted after that
            // context becomes current, so a slow context build cannot swallow the later user action.
            this.pendingPlayNext = [];
            // True while the end of a track is being resolved, so two end reports of the same timeline cannot both continue.
            this.isHandlingEnded = false;
        }

        /**
         * Gets the zero based position of the entry that is playing.
         * @returns {number} The queue position, or -1 when the queue has no current entry.
         */
        get currentIndex() {
            return this.queue.currentIndex;
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
            this.bindKeyboardShortcuts();
            this.bindUnloadFlush();
            this.bindDescriptorDelegation();
            this.bindQueueInteraction();
            // A page load starts with a hidden bar and an empty queue. Nothing is restored and nothing is auto played.
            this.suppressInitialTransitions();
            this.applyHiddenState();
            this.releaseInitialHiddenState();
            // The volume and mute controls are rendered from the state the player already holds, so the bar shows the
            // default volume even before the stored state is read.
            this.renderVolume();
            this.renderTransportState();

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
            this.dom.albumName = query('[data-audio-player-album-name]');
            this.dom.coverThumbnail = query('[data-audio-player-cover-thumbnail]');
            this.dom.coverImage = query('[data-audio-player-cover-image]');
            this.dom.currentTime = query('[data-audio-player-current-time]');
            this.dom.totalTime = query('[data-audio-player-total-time]');
            this.dom.progress = query('[data-audio-player-progress]');
            this.dom.progressPlayed = query('[data-audio-player-progress-played]');
            this.dom.progressDownloaded = query('[data-audio-player-progress-downloaded]');
            this.dom.progressUndownloaded = query('[data-audio-player-progress-undownloaded]');
            this.dom.playPause = query('[data-audio-player-play-pause]');
            this.dom.stop = query('[data-audio-player-stop]');
            this.dom.previous = query('[data-audio-player-previous]');
            this.dom.next = query('[data-audio-player-next]');
            this.dom.repeat = query('[data-audio-player-repeat]');
            this.dom.volume = query('[data-audio-player-volume]');
            this.dom.mute = query('[data-audio-player-mute]');
            this.dom.queue = query('[data-audio-player-queue]');
            this.dom.nowPlayingTitle = query('[data-audio-player-now-playing-title]');
            this.dom.nowPlayingReleaseType = query('[data-audio-player-now-playing-release-type]');
            this.dom.nowPlayingRelease = query('[data-audio-player-now-playing-release]');
            this.dom.nowPlayingYear = query('[data-audio-player-now-playing-year]');
            this.dom.nowPlayingArtist = query('[data-audio-player-now-playing-artist]');
            this.dom.artworkCarousel = query('[data-audio-player-artwork-carousel]');
            // The sub template sources are read once, with the bar elements, because the theme markup is part of the bar
            // the player was bound to, and the player is never recreated by a navigation.
            this.subTemplates = {
                queueItem: readSubTemplate('audio-player-template-queue-item'),
                playlistItem: readSubTemplate('audio-player-template-playlist-item')
            };
        }

        /**
         * Binds the transport controls the player owns: the play and pause toggle, the scrub of the progress bar and
         * of the volume control, and the mute toggle. The expand and close affordances are bound by the global site
         * script, which delegates them to this player, so that a single click never toggles them twice.
         */
        bindPlaybackControls() {
            if (this.dom.playPause)
                this.addControlHandler(this.dom.playPause, 'click', () => this.toggleAsync());
            if (this.dom.stop)
                this.addControlHandler(this.dom.stop, 'click', () => this.stopAsync());
            if (this.dom.previous)
                this.addControlHandler(this.dom.previous, 'click', () => this.previousAsync());
            if (this.dom.next)
                this.addControlHandler(this.dom.next, 'click', () => this.nextAsync());
            if (this.dom.repeat)
                this.addControlHandler(this.dom.repeat, 'click', () => this.cycleRepeatModeAsync());
            if (this.dom.progress) {
                // The progress bar is scrubbed with pointer events rather than clicks, so that a drag moves the
                // playhead continuously; the pointer is captured, so a drag that leaves the bar keeps scrubbing. A
                // released pointer commits the seek, while a cancelled one discards the preview.
                this.addControlHandler(this.dom.progress, 'pointerdown', (event) => this.handleProgressPointerDown(event));
                this.addControlHandler(this.dom.progress, 'pointermove', (event) => this.handleProgressPointerMove(event));
                this.addControlHandler(this.dom.progress, 'pointerup', () => this.handleProgressPointerUp());
                this.addControlHandler(this.dom.progress, 'pointercancel', () => this.handleProgressPointerCancel());
            }
            if (this.dom.volume) {
                // A released pointer commits the last queued volume, while a cancelled one drops it.
                this.addControlHandler(this.dom.volume, 'pointerdown', (event) => this.handleVolumePointerDown(event));
                this.addControlHandler(this.dom.volume, 'pointermove', (event) => this.handleVolumePointerMove(event));
                this.addControlHandler(this.dom.volume, 'pointerup', () => this.handleVolumePointerUp());
                this.addControlHandler(this.dom.volume, 'pointercancel', () => this.handleVolumePointerCancel());
            }
            if (this.dom.mute)
                this.addControlHandler(this.dom.mute, 'click', () => this.toggleMuteAsync());
        }

        /**
         * Binds the keyboard shortcuts of the transport, matching the reference web player: the left and right arrows
         * seek backward and forward on the track, and the up and down arrows change the volume. The listener is on the
         * document rather than on the bar, so a shortcut works without the bar being focused, and it is bound once
         * because the player is created once and never torn down by a navigation.
         */
        bindKeyboardShortcuts() {
            this.keyboardHandler = (event) => this.handleKeyboardShortcut(event);
            document.addEventListener('keydown', this.keyboardHandler);
        }

        /**
         * Binds the page unload flush, so a volume or mute change that is still inside its debounce window is written
         * before the page goes away. The write is sent with <c>keepalive</c>, so the browser finishes it after the page
         * is gone instead of cancelling it with the page's other requests.
         */
        bindUnloadFlush() {
            this.unloadHandler = () => this.flushPlaybackStateAsync({ keepalive: true });
            window.addEventListener('pagehide', this.unloadHandler);
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
                const playNextTrigger = event.target.closest(`[${LuminaAudioPlayer.PLAY_NEXT_ATTRIBUTE}]`);
                if (playNextTrigger) {
                    event.preventDefault();
                    const descriptor = readPlayDescriptorFromElement(playNextTrigger);
                    this.playNextAsync(descriptor).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED));
                    return;
                }
                const shuffledTrigger = event.target.closest(`[${LuminaAudioPlayer.PLAY_SHUFFLED_ATTRIBUTE}]`);
                if (shuffledTrigger) {
                    event.preventDefault();
                    const descriptor = readPlayDescriptorFromElement(shuffledTrigger);
                    const scope = shuffledTrigger.getAttribute(LuminaAudioPlayer.PLAY_SHUFFLE_SCOPE_ATTRIBUTE)
                        || LuminaAudioPlayer.QueueShuffleScope.RELEASE;
                    this.playShuffledAsync(descriptor, scope).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED));
                    return;
                }
                const libraryTrigger = event.target.closest(`[${LuminaAudioPlayer.PLAY_LIBRARY_ATTRIBUTE}]`);
                if (libraryTrigger) {
                    event.preventDefault();
                    const descriptor = readPlayDescriptorFromElement(libraryTrigger);
                    this.playAsync(descriptor).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR));
                    return;
                }
                const libraryShuffledTrigger = event.target.closest(`[${LuminaAudioPlayer.PLAY_LIBRARY_SHUFFLED_ATTRIBUTE}]`);
                if (libraryShuffledTrigger) {
                    event.preventDefault();
                    const libraryId = libraryShuffledTrigger.dataset.libraryId || '';
                    this.playLibraryShuffledAsync(libraryId).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED));
                    return;
                }
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
         * Starts playback of a track, an album or an artist, and shows the bar.
         *
         * A descriptor that already carries a queue is played as is. A descriptor that names only an album, an artist or a
         * library is resolved into the ordered tracks of that context through {@link LuminaAudioPlayer.QueueContextService},
         * which also documents the endpoints and the order it uses. Starting any new context discards the previous queue,
         * its history, its autoplay continuation and its shuffle, because a play now is an explicit choice.
         * @param {Object} descriptorInput - The play descriptor to play, per {@link LuminaAudioPlayer.createPlayDescriptor}.
         * @param {Object} [options] - The play options.
         * @param {boolean} [options.shouldStartRandomTrack] - True to start a random track of the resolved context instead of its first track. Used by the shuffled play actions, so the first track of a shuffle is random too.
         * @returns {Promise<boolean>} True when playback started, false when the request was rejected.
         */
        async playAsync(descriptorInput, options = {}) {
            // The player is only available when the layout rendered the bar, so a caller on a page without one is ignored instead of failing.
            if (!this.isAvailable)
                return false;
            const descriptor = createPlayDescriptor(descriptorInput);
            if (!descriptor) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor identifies no library, or no playable context.'));
                return false;
            }
            // The token invalidates this request if a later play, skip or stop takes the player while the context is loading.
            const requestToken = ++this.playRequestToken;
            // A new explicit play supersedes any play next that was waiting for an earlier context, so the latest play request owns the queue transition.
            this.pendingPlayNext = [];
            // A play now supersedes every queue fetch of the previous context, so nothing of the old queue keeps downloading.
            this.abortQueueActivity();
            // Capability detection runs in the background while the page loads, so the first play waits for its answer instead
            // of starting a track the browser may not be able to decode.
            const report = await this.capabilitiesReady;
            if (requestToken !== this.playRequestToken)
                return false;
            if (report && !report.isSupported) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }));
                return false;
            }
            // A new play context starts with fresh queue metadata, so a rescan that changed releases or artists is not
            // masked by a list cached for an earlier context of this page.
            if (typeof this.queueService.clearCaches === 'function')
                this.queueService.clearCaches();
            // A failed or empty context keeps the old queue, so it must also keep describing the old timeline instead of
            // leaving the bar on the title of the context that never loaded.
            const previousDescriptor = this.queue.getCurrentDescriptor() || this.descriptor;
            const previousTimeline = {
                descriptor: previousDescriptor,
                duration: previousDescriptor
                    && Number.isFinite(previousDescriptor.durationInSeconds)
                    && previousDescriptor.durationInSeconds > 0
                    ? previousDescriptor.durationInSeconds
                    : this.duration,
                currentTime: this.currentTime,
                downloadedFraction: this.downloadedFraction
            };

            // A new descriptor starts a new timeline, so the playhead and the progress of the previous track are dropped.
            this.descriptor = descriptor;
            this.duration = descriptor.durationInSeconds;
            this.currentTime = 0;
            this.downloadedFraction = 0;
            this.scrubRatio = null;
            this.endScrub();
            this.endVolumeAdjust();
            // clearBuffering also clears any pending seek of the previous timeline.
            this.clearBuffering();
            this.shuffleSource = null;

            // The bar is shown and the state moves to loading before the context is resolved, so a slow endpoint still gives
            // the user immediate feedback that the request was accepted.
            this.renderDescriptor();
            this.renderProgress();
            this.showBar();
            this.setState(LuminaAudioPlayer.PlayerState.LOADING);

            let built;
            const buildHandle = this.openPlayContextBuild();
            try {
                built = await this.queueService.buildQueueAsync(descriptor, buildHandle.signal);
            } catch (error) {
                // An aborted context is either superseded by a stop or a newer request, which owns the player, or it hit the
                // build timeout. Only the timeout of the request that still owns the player is reported as a failure.
                if (buildHandle.signal && buildHandle.signal.aborted) {
                    if (requestToken === this.playRequestToken
                        && buildHandle.timedOut
                        && this.state === LuminaAudioPlayer.PlayerState.LOADING) {
                        // The old timeline may still be playing while the new context never arrived. It is silenced before
                        // the error state makes the play control a recovery action instead of a pause control.
                        await this.pauseEngineQuietlyAsync(this.engine);
                        this.restoreTimelineState(previousTimeline);
                        this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The play context could not be loaded in time.'));
                        await this.flushPendingPlayNext(requestToken);
                    }
                    return false;
                }
                // A session that expired while the context was loaded already owns the page: it is navigating to the sign in
                // page, so the failure is not reported as a playback failure.
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return false;
                if (requestToken !== this.playRequestToken || this.state !== LuminaAudioPlayer.PlayerState.LOADING)
                    return false;
                await this.pauseEngineQuietlyAsync(this.engine);
                this.restoreTimelineState(previousTimeline);
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED);
                await this.flushPendingPlayNext(requestToken);
                return false;
            } finally {
                this.closePlayContextBuild(buildHandle);
            }
            // A stop or a newer play request while the context was loading owns the player and must not be overwritten by it.
            if (requestToken !== this.playRequestToken || this.state !== LuminaAudioPlayer.PlayerState.LOADING)
                return false;

            // An empty context is reported before the existing queue is replaced, so a failed play of an empty album or
            // artist does not wipe the queue that was already in memory.
            if (!built || !Array.isArray(built.descriptors) || built.descriptors.length === 0) {
                await this.pauseEngineQuietlyAsync(this.engine);
                this.restoreTimelineState(previousTimeline);
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The play context holds no playable track.'));
                await this.flushPendingPlayNext(requestToken);
                return false;
            }
            // A shuffled play starts a random track of the context, so the first entry of a shuffle is random as well. The ordered context still becomes the queue, which is what turning shuffle off restores.
            const startTrackId = options && options.shouldStartRandomTrack === true
                ? built.descriptors[Math.floor(Math.random() * built.descriptors.length)].trackId
                : descriptor.trackId;
            const current = this.queue.replaceContext(built.descriptors, {
                startTrackId,
                context: built.context,
                // A whole library is materialized lazily, one artist at a time, so its continuation is part of the explicit
                // play action rather than an optional autoplay. Album and artist contexts load in full before playback.
                continuationEnabled: built.continuationEnabled
                    && (built.context.kind === LuminaAudioPlayer.PlayContextKind.LIBRARY
                        || this.preferences.shouldAutoplay === true)
            });
            // A new context is explicit and ordered, so it starts unshuffled; the model owns that reset together with the
            // continuation flags. The repeat mode is a listening preference and is kept.
            this.queueGeneration += 1;
            this.shuffleSource = null;
            if (!current) {
                await this.pauseEngineQuietlyAsync(this.engine);
                this.restoreTimelineState(previousTimeline);
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The play context holds no playable track.'));
                await this.flushPendingPlayNext(requestToken);
                return false;
            }
            this.descriptor = current.descriptor;
            this.duration = current.descriptor.durationInSeconds;
            this.renderDescriptor();
            this.renderProgress();
            this.renderQueue();
            this.scrollQueueToCurrentEntry();
            this.renderQueueControls();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.DESCRIPTOR_CHANGED, current.descriptor);
            this.emitQueueStateChanged();
            // Play next items that arrived while this context was resolving become the successors of the new current entry.
            await this.flushPendingPlayNext(requestToken);

            const started = await this.startLoadedDescriptorAsync(current.descriptor, { keepBuffering: true, requestToken });
            if (started)
                this.maybeExtendQueueAsync();
            return started;
        }

        /**
         * Starts playback of a track, an album or an artist, then turns shuffle on for the requested scope.
         *
         * The context is resolved like {@link AudioPlayer#playAsync}, but playback starts on a random track of it, so the
         * first entry of the shuffle is random too. The ordered context remains the queue that turning shuffle off
         * restores. An unknown or missing scope falls back to the release scope, because a trigger without a scope is
         * asking to shuffle what it just started.
         * @param {Object} descriptorInput - The play descriptor to play, per {@link LuminaAudioPlayer.createPlayDescriptor}.
         * @param {string} [scope] - One of the {@link LuminaAudioPlayer.QueueShuffleScope} values.
         * @returns {Promise<boolean>} True when playback started and the shuffle was applied, false otherwise.
         */
        async playShuffledAsync(descriptorInput, scope) {
            const targetScope = Object.values(LuminaAudioPlayer.QueueShuffleScope).includes(scope)
                ? scope
                : LuminaAudioPlayer.QueueShuffleScope.RELEASE;
            const started = await this.playAsync(descriptorInput, { shouldStartRandomTrack: true });
            if (!started)
                return false;
            return this.setShuffleAsync(true, targetScope);
        }

        /**
         * Starts a shuffled playback of a whole media library.
         *
         * A library shuffle has no ordered track to start from, so the method draws one random track from the library shuffle
         * source, starts that track, then turns the shuffle on for the library scope. The first track is the one the random
         * draw picked, and every later track is drawn from the library as the queue approaches its end.
         * @param {string} libraryId - The Id of the media library to shuffle.
         * @returns {Promise<boolean>} True when playback started and the shuffle was applied, false otherwise.
         */
        async playLibraryShuffledAsync(libraryId) {
            if (!this.isAvailable || !LuminaAudioPlayer.isGuid(libraryId))
                return false;
            // A newer play, skip or stop supersedes this request while its first random batch is resolved.
            const requestToken = ++this.playRequestToken;
            const report = await this.capabilitiesReady;
            if (requestToken !== this.playRequestToken)
                return false;
            if (report && !report.isSupported) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }));
                return false;
            }
            const buildHandle = this.createQueueBuildHandle();
            let batch;
            try {
                const source = await this.queueService.createShuffleSourceAsync(
                    LuminaAudioPlayer.QueueShuffleScope.LIBRARY,
                    null,
                    { libraryId },
                    buildHandle.signal);
                batch = await source.nextBatchAsync(new Set(), 1, buildHandle.signal);
            } catch (error) {
                if (buildHandle.signal && buildHandle.signal.aborted)
                    return false;
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return false;
                this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED);
                return false;
            } finally {
                buildHandle.dispose();
            }
            if (requestToken !== this.playRequestToken)
                return false;
            if (!Array.isArray(batch) || batch.length === 0) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The library holds no playable track.'));
                return false;
            }
            const start = batch[0];
            const started = await this.playAsync({ ...start });
            if (!started)
                return false;
            return this.setShuffleAsync(true, LuminaAudioPlayer.QueueShuffleScope.LIBRARY);
        }

        /**
         * Drops a prefetched or already appended handoff of the engine when the queue was replaced under a playing track.
         * The engine is optional in this interface, so a player without that capability simply keeps its earlier behavior.
         * @returns {Promise<void>} A promise that completes when the handoff was released.
         */
        async cancelUpcomingTransitionAsync() {
            if (!this.engine || typeof this.engine.cancelUpcomingAsync !== 'function')
                return;
            try {
                await this.engine.cancelUpcomingAsync();
            } catch (error) {
                console.debug('Audio player: the upcoming track of the replaced queue could not be released.', error);
            }
        }

        /**
         * Creates a build handle for a queue fetch: an abort controller, a timeout and a disposal.
         *
         * A queue fetch is a whole context, a continuation or a shuffle source, which can take seconds across many pages. The
         * handle lets a stop or a newer action abort the request, and the timeout guarantees that a response that never
         * arrives cannot leave the player loading or the extension promise pending forever.
         * @returns {{ signal: AbortSignal|undefined, timedOut: boolean, abort: Function, resetTimeout: Function, dispose: Function }} The build handle.
         */
        createQueueBuildHandle() {
            if (typeof AbortController !== 'function') {
                return {
                    signal: undefined,
                    timedOut: false,
                    abort: () => {},
                    resetTimeout: () => {},
                    dispose: () => {}
                };
            }
            const controller = new AbortController();
            const handle = {
                signal: controller.signal,
                timedOut: false,
                timer: null,
                resetTimeout: () => {
                    if (handle.timer !== null)
                        clearTimeout(handle.timer);
                    handle.timedOut = false;
                    handle.timer = setTimeout(() => {
                        handle.timedOut = true;
                        handle.abort();
                    }, QUEUE_BUILD_TIMEOUT_MS);
                },
                abort: () => {
                    try {
                        if (!controller.signal.aborted)
                            controller.abort();
                    } catch (error) {
                        console.debug('Audio player: the queue request could not be aborted.', error);
                    }
                },
                dispose: () => {
                    if (handle.timer !== null) {
                        clearTimeout(handle.timer);
                        handle.timer = null;
                    }
                }
            };
            handle.resetTimeout();
            return handle;
        }

        /**
         * Opens the build handle of a play context fetch, aborting a previous play context fetch that is still running.
         * @returns {{ signal: AbortSignal|undefined, timedOut: boolean, abort: Function, dispose: Function }} The build handle.
         */
        openPlayContextBuild() {
            this.abortPlayContextBuild();
            const handle = this.createQueueBuildHandle();
            this.playContextBuildHandle = handle;
            return handle;
        }

        /**
         * Closes a play context build handle, when it is still the current one.
         * @param {Object} handle - The handle to close.
         */
        closePlayContextBuild(handle) {
            if (this.playContextBuildHandle === handle)
                this.playContextBuildHandle = null;
            if (handle)
                handle.dispose();
        }

        /**
         * Opens the build handle of a queue batch fetch, which is either an autoplay continuation or a shuffle source and
         * its first batch, aborting a previous batch fetch that is still running.
         * @returns {{ signal: AbortSignal|undefined, timedOut: boolean, abort: Function, dispose: Function }} The build handle.
         */
        openQueueBatchBuild() {
            const previous = this.queueBatchBuildHandle;
            this.queueBatchBuildHandle = null;
            if (previous) {
                previous.abort();
                previous.dispose();
            }
            const handle = this.createQueueBuildHandle();
            this.queueBatchBuildHandle = handle;
            return handle;
        }

        /**
         * Closes a queue batch build handle, when it is still the current one.
         * @param {Object} handle - The handle to close.
         */
        closeQueueBatchBuild(handle) {
            if (this.queueBatchBuildHandle === handle)
                this.queueBatchBuildHandle = null;
            if (handle)
                handle.dispose();
        }

        /**
         * Opens the build handle of an explicit play next fetch, aborting a previous play next fetch that is still running.
         * It never touches the background batch handle, so a continuation or shuffle batch cannot cancel the user's play
         * next item.
         * @returns {{ signal: AbortSignal|undefined, timedOut: boolean, abort: Function, dispose: Function }} The build handle.
         */
        openPlayNextBuild() {
            const previous = this.playNextBuildHandle;
            this.playNextBuildHandle = null;
            if (previous) {
                previous.abort();
                previous.dispose();
            }
            const handle = this.createQueueBuildHandle();
            this.playNextBuildHandle = handle;
            return handle;
        }

        /**
         * Closes a play next build handle, when it is still the current one.
         * @param {Object} handle - The handle to close.
         */
        closePlayNextBuild(handle) {
            if (this.playNextBuildHandle === handle)
                this.playNextBuildHandle = null;
            if (handle)
                handle.dispose();
        }

        /**
         * Aborts the play context fetch, when one is running.
         */
        abortPlayContextBuild() {
            const handle = this.playContextBuildHandle;
            this.playContextBuildHandle = null;
            if (!handle)
                return;
            handle.abort();
            handle.dispose();
        }

        /**
         * Aborts the continuation or shuffle batch fetch, when one is running.
         */
        abortQueueBatchBuild() {
            const handle = this.queueBatchBuildHandle;
            this.queueBatchBuildHandle = null;
            if (!handle)
                return;
            handle.abort();
            handle.dispose();
        }

        /**
         * Aborts the explicit play next fetch, when one is running.
         */
        abortPlayNextBuild() {
            const handle = this.playNextBuildHandle;
            this.playNextBuildHandle = null;
            if (!handle)
                return;
            handle.abort();
            handle.dispose();
        }

        /**
         * Aborts every queue fetch of the player, used by a stop, a teardown and a new play request.
         */
        abortQueueActivity() {
            this.abortPlayContextBuild();
            this.abortQueueBatchBuild();
            this.abortPlayNextBuild();
        }

        /**
         * Starts the engine load and playback of a descriptor the player has already made current and set to its loading
         * state. The player state is checked again after each await, so a stop that happens while a load is running keeps
         * its intent instead of being overwritten by the load or by the engine.
         * @param {Object} descriptor - The descriptor that is already current on the player.
         * @param {Object} [options] - The load options.
         * @param {boolean} [options.keepBuffering] - True to keep a loading state that was handed to the load, used by an
         * automatic continuation so the gap before the manifest and header probe shows a loading state.
         * @param {number} [options.requestToken] - The play request token that owns this load. A load whose token is no longer
         * the current one is a request that was overtaken by a later play, skip or stop, and it never moves the player to READY
         * or PLAYING, even when the newer request is still building its own queue.
         * @returns {Promise<boolean>} True when playback started, false when it was stopped or failed.
         */
        async startLoadedDescriptorAsync(descriptor, options = {}) {
            let engine = null;
            const requestToken = options.requestToken;
            const isRequestCurrent = () => requestToken === undefined || requestToken === this.playRequestToken;
            try {
                // The state follows the engine step by step, so a consumer can tell preparation apart from actual playback.
                engine = await this.ensureEngineAsync();
                if (!isRequestCurrent())
                    return false;
                await engine.loadAsync(descriptor, options);
                if (!isRequestCurrent())
                    return false;
                if (this.state !== LuminaAudioPlayer.PlayerState.LOADING) {
                    // A stop during the load already paused the engine; a newer request owns the player state and must not be touched by this stale continuation.
                    if (this.state === LuminaAudioPlayer.PlayerState.IDLE)
                        await this.pauseEngineQuietlyAsync(engine);
                    return false;
                }
                this.setState(LuminaAudioPlayer.PlayerState.READY);
                await engine.playAsync();
                if (!isRequestCurrent()) {
                    // Audio has started for a request that a newer one superseded. If the newer request has not started its own
                    // playback yet, the superseded audio must not keep playing while its context builds, or forever when that
                    // build fails. Playback of a newer request is left untouched.
                    if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                        await this.pauseEngineQuietlyAsync(engine);
                    return false;
                }
                if (this.state !== LuminaAudioPlayer.PlayerState.READY) {
                    if (this.state === LuminaAudioPlayer.PlayerState.IDLE)
                        await this.pauseEngineQuietlyAsync(engine);
                    return false;
                }
                this.setState(LuminaAudioPlayer.PlayerState.PLAYING);
                this.renderTransportState();
            } catch (error) {
                // A load that a newer play request overtook, and a session that expired and is already navigating to the
                // sign in page, are expected outcomes. They must not move the player to its error state, and they must not
                // let this stale continuation set READY or PLAYING over the request that now owns the player.
                if (!isRequestCurrent())
                    return false;
                const reason = error && error.details ? error.details.reason : null;
                if (reason === LuminaAudioPlayer.PlayerErrorReason.LOAD_SUPERSEDED
                    || reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return false;
                // A failure before playback starts is a load failure; later failures are reported by the engine through its host.
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED);
                return false;
            }
            return true;
        }

        /**
         * Pauses an engine after a load that the user stopped, without letting a pause failure disturb the player state.
         * @param {Object} engine - The engine to pause.
         * @returns {Promise<void>} A promise that completes when the pause was attempted.
         */
        async pauseEngineQuietlyAsync(engine) {
            if (!engine || typeof engine.pauseAsync !== 'function')
                return;
            try {
                await engine.pauseAsync();
            } catch (error) {
                console.debug('Audio player: the engine could not be paused after a stopped load.', error);
            }
        }

        /**
         * Requests the next queue entry as a new playback, with the player's own state machine owning the transition. It is
         * the seam the engine uses for the end of a track when a gapless handoff could not be prepared.
         * @param {string} [reason] - A developer facing reason for the request, for diagnostics.
         * @returns {Promise<boolean>} True when the player took ownership of the request, false when it declined it because the player is paused, stopped, or has no next entry.
         */
        async requestNextTrackAsync(reason) {
            // A paused or stopped player has made an explicit choice that the end of a track must not override.
            if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                return false;
            // The capability gate is part of the player's play path, so an auto advance never starts a track the browser
            // cannot decode.
            const report = await this.capabilitiesReady;
            if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                return false;
            if (report && !report.isSupported) {
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }), LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING);
                return true;
            }
            // The next entry is re-resolved after the capability await, so a queue that changed in the meantime is respected.
            const next = this.getNextDescriptor();
            if (!next)
                return false;
            // The player owns the outcome from here, including a load that fails: returning true keeps the engine from
            // reporting an end over a state the player has already set. The loading state is kept across the engine
            // teardown, so the end gap shows that the next track is being prepared.
            const hasStarted = await this.startQueuedEntryAsync(next.entryId, { keepBuffering: true });
            if (!hasStarted && this.state === LuminaAudioPlayer.PlayerState.IDLE
                && this.engine && typeof this.engine.discardCurrentAsync === 'function') {
                // A stop during the continuation load owns the outcome. The prepared track is released, so a stopped
                // player does not keep a track loaded that was started only to continue a queue.
                try {
                    await this.engine.discardCurrentAsync();
                } catch (error) {
                    console.debug('Audio player: the stopped continuation track could not be released.', error);
                }
            }
            return true;
        }

        /**
         * Starts a queue entry that is already part of the queue, through the player's own play path. It is the single
         * transition used by next, previous, the end of a track and the engine's gapless fallback, so the queue cursor, the
         * bar and the state machine move together.
         * @param {string} entryId - The identity of the queue entry to start.
         * @param {Object} [options] - The start options.
         * @param {boolean} [options.keepBuffering=false] - True to keep the loading state the caller already announced.
         * @param {boolean} [options.recordHistory=true] - True to remember the entry that was playing in the navigation history.
         * @param {string|null} [options.consumeHistoryId=null] - A history entry to consume once the commit succeeded.
         * @returns {Promise<boolean>} True when the entry started, false when it was rejected or stopped.
         */
        async startQueuedEntryAsync(entryId, options = {}) {
            if (!this.isAvailable)
                return false;
            // A skip is a new play request from the moment it starts, so an earlier play that is still waiting for its
            // capability answer can no longer take the player. The token is checked again after the wait, so a request that
            // arrives later still wins over this one.
            const requestToken = ++this.playRequestToken;
            // A skip also supersedes every queue fetch that was resolved for the track the skip is leaving: the play context
            // fetch of a play that is still resolving, and a continuation or shuffle batch. None of them may keep downloading
            // or apply a result to the new cursor. A play next waiting for that context belongs to it and is discarded.
            this.pendingPlayNext = [];
            this.abortPlayContextBuild();
            this.abortQueueBatchBuild();
            if (this.isShufflePending) {
                this.shuffleToken += 1;
                this.isShufflePending = false;
            }
            // Every way into a new track passes the same capability gate, so next, previous and the end of a track never
            // start a track the browser cannot decode either.
            const report = await this.capabilitiesReady;
            if (requestToken !== this.playRequestToken)
                return false;
            if (report && !report.isSupported) {
                this.fail(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING, 'The browser is missing a capability the player requires.',
                    { missing: report.missing }), LuminaAudioPlayer.PlayerErrorCode.CAPABILITY_MISSING);
                return false;
            }
            const target = this.queue.findEntry(entryId);
            if (target && !createPlayDescriptor(target.entry.descriptor)) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The queue entry does not describe a playable track.'));
                return false;
            }
            const committed = this.queue.commit(entryId, {
                recordHistory: options.recordHistory !== false,
                consumeHistoryId: options.consumeHistoryId || null
            });
            if (!committed)
                return false;
            // Restarting the entry that is already current must still reload its timeline, but it is not a queue move, so the
            // state and the bar are reset by the committed entry and no history entry is produced.
            this.showBar();
            const applied = this.applyCommittedEntry(committed.entry, committed.changed ? committed.previousIndex : null, committed.removedCount);
            if (!applied) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The queue entry is no longer the current playable entry.'));
                return false;
            }
            this.setState(LuminaAudioPlayer.PlayerState.LOADING);
            this.renderTransportState();
            const hasStarted = await this.startLoadedDescriptorAsync(committed.entry.descriptor, {
                keepBuffering: options.keepBuffering === true,
                requestToken
            });
            if (hasStarted)
                this.maybeExtendQueueAsync();
            return hasStarted;
        }

        /**
         * Starts the next entry of the queue. It is the next action of the transport, and wraps at the end only when repeat
         * all is on. When the autoplay continuation of an album or an artist still has a batch, that batch is loaded before
         * the end, so next also works at the boundary of the context.
         * @param {Object} [options] - The next options.
         * @param {boolean} [options.allowWrap] - Set to false to skip without wrapping, used by error recovery.
         * @returns {Promise<boolean>} True when an entry started.
         */
        async nextAsync(options = {}) {
            if (!this.isAvailable || this.queue.entries.length === 0)
                return false;
            const allowWrap = options.allowWrap === false ? false : this.canWrapToFirstEntry();
            const stateAtStart = this.state;
            const cursorEntryId = this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null;
            const queueGeneration = this.queueGeneration;
            const requestToken = this.playRequestToken;
            let next = this.queue.getNextEntry({ allowWrap });
            if (!next) {
                await this.maybeExtendQueueAsync({ force: true });
                // A slow continuation must not start an entry for a cursor, queue or play request that moved while it loaded.
                // A player that was already in its error state is still allowed to skip past the failed entry, while a new
                // error during the extension keeps the recovery from starting over the failure.
                if (this.queueGeneration !== queueGeneration
                    || (this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null) !== cursorEntryId
                    || this.playRequestToken !== requestToken
                    || (stateAtStart !== LuminaAudioPlayer.PlayerState.ERROR && this.state === LuminaAudioPlayer.PlayerState.ERROR))
                    return false;
                next = this.queue.getNextEntry({ allowWrap });
            }
            if (!next)
                return false;
            return this.startQueuedEntryAsync(next.entryId);
        }

        /**
         * Returns to the previous entry of the navigation history. When there is no history and the playhead is already at
         * the start of the current track, it moves to the previous queue entry, or wraps to the last entry with repeat all.
         * At the start of an entry with no earlier entry it restarts the current entry.
         * @returns {Promise<boolean>} True when an entry started.
         */
        async previousAsync() {
            if (!this.isAvailable || this.queue.entries.length === 0)
                return false;
            // Past the restart threshold the previous action restarts the track, like the reference web players, and only a
            // second press walks back. At the very start it walks back immediately. A finished track is already at a fresh
            // boundary, so previous walks back instead of restarting it. A failed entry is never restarted, because that
            // would only repeat the failure; the history or the previous entry is the recovery path.
            if (this.state !== LuminaAudioPlayer.PlayerState.ERROR
                && this.state !== LuminaAudioPlayer.PlayerState.ENDED
                && this.currentTime > PREVIOUS_RESTART_THRESHOLD_SECONDS)
                return this.restartCurrentEntryAsync();
            const previous = this.queue.getPreviousEntry();
            if (!previous) {
                if (this.state === LuminaAudioPlayer.PlayerState.ERROR)
                    return false;
                return this.restartCurrentEntryAsync();
            }
            return this.startQueuedEntryAsync(previous.entry.entryId, {
                recordHistory: false,
                consumeHistoryId: previous.fromHistory ? previous.entry.entryId : null
            });
        }

        /**
         * Checks whether the queue may continue past its current entries. The stored autoplay preference controls the
         * optional continuation of an album or an artist, while a whole library is materialized lazily, one artist at a
         * time, so its continuation is part of the explicit library play action and always runs.
         * @returns {boolean} True when the continuation may extend the queue.
         */
        isQueueContinuationAllowed() {
            if (!this.queue)
                return false;
            const contextKind = this.queue.context ? this.queue.context.kind : '';
            return this.preferences.shouldAutoplay === true || contextKind === LuminaAudioPlayer.PlayContextKind.LIBRARY;
        }

        /**
         * Checks whether the next action may wrap from the last entry to the first one. A repeat all wraps, unless an
         * autoplay continuation of an album, an artist or a library is still available: that continuation is the true
         * next content, and the wrap only happens once the continuation is exhausted.
         * @returns {boolean} True when the next action may wrap.
         */
        canWrapToFirstEntry() {
            if (this.queue.getRepeatMode() !== LuminaAudioPlayer.QueueRepeatMode.ALL)
                return false;
            const canContinue = this.queue.isContinuationEnabled()
                && this.isQueueContinuationAllowed()
                && this.queue.isContinuationExhausted() !== true;
            const canExtendShuffle = this.queue.isShuffleEnabled() && this.queue.isShuffleSourceExhausted() !== true;
            return !canContinue && !canExtendShuffle;
        }

        /**
         * Restarts the current entry from its beginning, without moving the queue cursor.
         * @returns {Promise<boolean>} True when the entry restarted.
         */
        async restartCurrentEntryAsync() {
            const current = this.queue.getCurrentEntry();
            if (!current)
                return false;
            return this.startQueuedEntryAsync(current.entryId, { recordHistory: false });
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
         * Pauses when playing, resumes when paused or ready, replays the current track when idle or ended, and moves to a playable neighbour when the current entry failed.
         * @returns {Promise<void>} A promise that completes when the toggle was applied.
         */
        async toggleAsync() {
            if (this.state === LuminaAudioPlayer.PlayerState.PLAYING)
                await this.pauseAsync();
            else if (this.state === LuminaAudioPlayer.PlayerState.PAUSED || this.state === LuminaAudioPlayer.PlayerState.READY)
                await this.resumeAsync();
            else if (this.state === LuminaAudioPlayer.PlayerState.ERROR) {
                // The current entry already failed, so the play control tries the next entry and falls back to the previous
                // one. It never wraps back to the same dead entry, even with repeat all and a single entry queue.
                const started = await this.nextAsync({ allowWrap: false });
                if (!started)
                    await this.previousAsync();
            }
            else if (this.state === LuminaAudioPlayer.PlayerState.IDLE || this.state === LuminaAudioPlayer.PlayerState.ENDED) {
                // A stopped or finished queue is replayed from its current entry, so it resumes without rebuilding from a
                // context descriptor that would discard the rest of the queue.
                const current = this.queue.getCurrentEntry();
                if (current)
                    await this.startQueuedEntryAsync(current.entryId, { recordHistory: false });
                else if (this.descriptor)
                    await this.playAsync(this.descriptor);
            }
        }

        /**
         * Stops playback. While the bar is condensed, it slides out of view; while the bar is expanded,
         * it stays on screen with the current track, and only playback stops.
         * @returns {Promise<void>} A promise that completes when playback stopped.
         */
        async stopAsync() {
            // A stop invalidates a play request that is still resolving its context, so the stopped request cannot start later.
            this.playRequestToken += 1;
            this.shuffleToken += 1;
            this.isShufflePending = false;
            // A later stop wins over a play next that was waiting for a context build.
            this.pendingPlayNext = [];
            this.abortQueueActivity();
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
            this.scrubRatio = null;
            this.endScrub();
            this.endVolumeAdjust();
            this.clearBuffering();
            this.renderProgress();
            this.setState(LuminaAudioPlayer.PlayerState.IDLE);
            this.renderTransportState();
            // A condensed bar slides out when playback stops, while an expanded one stays so the user keeps the track and its queue in view.
            if (!this.isExpanded())
                this.hideBar();
        }

        /**
         * Moves the playhead to an absolute position.
         *
         * When the engine reports the ranges it already downloaded and the target position is not inside one of them,
         * the seek needs the range of that position, so the bar shows a loading state until the engine reports the
         * range arrived. The engine's seekAsync is what triggers that range fetch.
         * @param {number} seconds - The target position, in seconds.
         * @returns {Promise<void>} A promise that completes when the seek was requested.
         */
        async seekAsync(seconds) {
            // Without a descriptor there is no timeline to seek in. A finished, failed or still loading track is not
            // seekable either: its pipeline is either released or not yet the one the seek would address. Seeking a
            // finished track is deliberately a no-op rather than a replay, and no resume is reported for it; a new play starts the next timeline.
            if (!this.descriptor
                || this.state === LuminaAudioPlayer.PlayerState.IDLE
                || this.state === LuminaAudioPlayer.PlayerState.ENDED
                || this.state === LuminaAudioPlayer.PlayerState.ERROR
                || this.state === LuminaAudioPlayer.PlayerState.LOADING) {
                this.clearSeekPending();
                return;
            }
            // A known duration bounds the target, while without one the position is only kept non negative, so a seek is never
            // silently snapped to zero just because the duration has not been reported yet.
            const request = Number(seconds);
            const position = this.duration > 0
                ? clamp(request, 0, this.duration)
                : Math.max(0, Number.isFinite(request) ? request : 0);
            // The position the seek starts from is captured before the playhead moves, so a backward seek can be told
            // apart from the pre-seek position the engine reports first.
            const fromPosition = this.currentTime;
            this.currentTime = position;
            // A track that is not loaded yet, or an engine that does not report its downloaded ranges, has no range to
            // wait for, so no loading state is shown; only a known-to-be-missing range produces one.
            const knowsBufferedRanges = this.engine !== null && typeof this.engine.getBufferedTimeRanges === 'function';
            if (knowsBufferedRanges && !this.isPositionBuffered(position))
                this.setSeekPending(true, position, fromPosition);
            else
                this.clearSeekPending();
            if (this.engine) {
                try {
                    await this.engine.seekAsync(position);
                } catch (error) {
                    this.clearSeekPending();
                    this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
                }
            }
            // The engine reports the new playhead itself, so the render is coalesced with that report instead of racing it.
            this.scheduleProgressRender();
        }

        /**
         * Seeks by a relative number of seconds, bounded by the duration of the current track.
         * @param {number} deltaSeconds - The number of seconds to seek by, negative to seek backward.
         */
        seekBy(deltaSeconds) {
            if (!this.descriptor || this.duration <= 0)
                return;
            this.seekAsync(this.currentTime + deltaSeconds);
        }

        /**
         * Changes the volume by a relative amount, unmuting first when the volume changes out of a muted state.
         * @param {number} delta - The amount to change the volume by, between -1 and 1.
         * @returns {Promise<void>} A promise that completes when the volume was applied.
         */
        async changeVolumeByAsync(delta) {
            const next = clamp(this.volume + delta, 0, 1);
            // Changing the volume out of a muted state restores audible output, exactly like the reference web player;
            // lowering the volume to zero still leaves the output audible at the new, lower volume.
            if (this.muted && next > 0)
                await this.setMutedAsync(false);
            await this.setVolumeAsync(next);
        }

        /**
         * Sets the output volume, as a user change, and writes it to the stored playback state of the user. Mute is
         * independent of the volume: a muted player remembers the volume.
         * @param {number} volume - The output volume, between 0 and 1.
         * @returns {Promise<number>} The applied volume.
         */
        async setVolumeAsync(volume) {
            this.hasUserChangedVolume = true;
            return this.applyVolumeValueAsync(volume, true);
        }

        /**
         * Sets the mute state, as a user change, and writes it to the stored playback state of the user.
         * @param {boolean} muted - True to mute the output, false to unmute it.
         * @returns {Promise<void>} A promise that completes when the mute was applied.
         */
        async setMutedAsync(muted) {
            this.hasUserChangedMute = true;
            return this.applyMuteValueAsync(muted, true);
        }

        /**
         * Applies a volume value to the output. It is the shared body of the user-facing setter and the stored-state
         * apply, so the stored state is applied without being marked as a user change or written back.
         * @param {number} volume - The output volume, between 0 and 1.
         * @param {boolean} shouldPersist - True to schedule a write of the new state.
         * @returns {Promise<number>} The applied volume.
         */
        async applyVolumeValueAsync(volume, shouldPersist) {
            const value = clamp(Number(volume), 0, 1);
            this.volume = value;
            await this.applyVolumeAsync();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.VOLUME_CHANGED, this.buildVolumePayload());
            // The volume is written to the stored playback state of the user, so it survives a reload and never has to
            // be a field of the user settings page.
            if (shouldPersist)
                this.schedulePlaybackStatePersist();
            return this.volume;
        }

        /**
         * Applies a mute state to the output. It is the shared body of the user-facing setter and the stored-state apply.
         * @param {boolean} muted - True to mute the output, false to unmute it.
         * @param {boolean} shouldPersist - True to schedule a write of the new state.
         * @returns {Promise<void>} A promise that completes when the mute was applied.
         */
        async applyMuteValueAsync(muted, shouldPersist) {
            this.muted = muted === true;
            await this.applyVolumeAsync();
            this.events.emit(LuminaAudioPlayer.PlayerEvent.MUTE_CHANGED, this.buildVolumePayload());
            if (shouldPersist)
                this.schedulePlaybackStatePersist();
        }

        /**
         * Flips the mute state.
         * @returns {Promise<void>} A promise that completes when the mute was applied.
         */
        async toggleMuteAsync() {
            await this.setMutedAsync(!this.muted);
        }

        /**
         * Expands the bar, or collapses it back to the condensed bar. The expanded state is layout-neutral: the theme decides what expansion changes and in which direction.
         * @returns {boolean} True when the bar is expanded after the toggle.
         */
        toggleExpanded() {
            if (!this.container)
                return false;
            const isExpanded = !this.container.classList.contains('expanded');
            this.container.classList.toggle('expanded', isExpanded);
            if (this.main)
                this.main.classList.toggle('audio-player-bar-expanded', isExpanded);
            // The queue is only rendered while the bar is expanded, so a fresh expansion draws the current queue and a collapse stops paying for a list that is not visible.
            if (isExpanded) {
                this.renderQueue();
                // An expansion can happen long after playback started, when the current entry is far below the top of a
                // long queue. Showing it first makes the expanded queue describe what is playing instead of where it ended.
                this.scrollQueueToCurrentEntry();
                // The album details are only needed by the expanded view, so a collapsed player does not request them.
                this.renderNowPlayingDetails();
            } else
                this.cancelNowPlayingDetailsRequest();
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
            // A shown bar is interactive again, so its controls return to the tab order and to assistive technology.
            this.setBarInert(false);
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
            // A collapsed bar is only hidden by height and overflow, so its focusable controls would otherwise stay in
            // the tab order while invisible; inert removes the whole bar from the tab order and from assistive technology.
            this.setBarInert(true);
            if (this.main)
                this.main.classList.add('audio-player-not-shown');
            this.events.emit(LuminaAudioPlayer.PlayerEvent.VISIBILITY_CHANGED, { isVisible: false });
        }

        /**
         * Sets whether the bar is inert, so that a collapsed bar keeps no focusable control in the tab order.
         * @param {boolean} isInert - True to remove the bar and its controls from the tab order and from assistive technology.
         */
        setBarInert(isInert) {
            if (!this.container)
                return;
            if (isInert)
                this.container.setAttribute('inert', '');
            else
                this.container.removeAttribute('inert');
        }

        /**
         * Checks whether the bar is in its expanded form.
         * @returns {boolean} True when the bar is expanded.
         */
        isExpanded() {
            return this.container !== null && this.container.classList.contains('expanded');
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
            return this.queue.entries.map((entry) => entry.descriptor);
        }

        /**
         * Gets the playable state of the queue: its entries, cursor, history, repeat mode, shuffle mode and play context.
         * @returns {Object} The queue state.
         */
        getQueueState() {
            return this.queue.getState();
        }

        /**
         * Gets the compact queue state published to observers, without every entry, history identity or dedupe set.
         * @returns {Object} The compact queue state.
         */
        getQueueStateSummary() {
            return this.queue.getStateSummary();
        }

        /**
         * Publishes the compact queue state, but only when somebody subscribes to it. Building a state payload for no listener is pure allocation on every cursor move.
         */
        emitQueueStateChanged() {
            const canCheckListeners = typeof this.events.hasListeners === 'function';
            // The deprecated event keeps its original descriptor array payload, but only pays for the mapping when a legacy consumer is actually subscribed.
            if (!canCheckListeners || this.events.hasListeners(LuminaAudioPlayer.PlayerEvent.QUEUE_CHANGED))
                this.events.emit(LuminaAudioPlayer.PlayerEvent.QUEUE_CHANGED, this.getQueue());
            if (canCheckListeners && !this.events.hasListeners(LuminaAudioPlayer.PlayerEvent.QUEUE_STATE_CHANGED))
                return;
            this.events.emit(LuminaAudioPlayer.PlayerEvent.QUEUE_STATE_CHANGED, this.queue.getStateSummary());
        }

        /**
         * Gets the current repeat mode.
         * @returns {string} One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         */
        getRepeatMode() {
            return this.queue.getRepeatMode();
        }

        /**
         * Applies a repeat mode and publishes it. It is the shared body of the synchronous and asynchronous repeat controls.
         * @param {string} mode - One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         * @returns {{ changed: boolean, shouldCancelUpcoming: boolean }} Whether the mode changed and whether the gapless
         * handoff must be cancelled because the mode crossed the repeat-one boundary.
         */
        applyRepeatMode(mode) {
            const previous = this.queue.getRepeatMode();
            if (!this.queue.setRepeatMode(mode))
                return { changed: false, shouldCancelUpcoming: false };
            this.renderQueueControls();
            this.emitQueueStateChanged();
            // Entering or leaving repeat one changes what the engine's already prepared handoff would do at the boundary,
            // so a handoff cached before the change must be released.
            return {
                changed: true,
                shouldCancelUpcoming: previous === LuminaAudioPlayer.QueueRepeatMode.ONE
                    || mode === LuminaAudioPlayer.QueueRepeatMode.ONE
            };
        }

        /**
         * Sets the repeat mode.
         * @param {string} mode - One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         * @returns {boolean} True when the mode changed.
         */
        setRepeatMode(mode) {
            const result = this.applyRepeatMode(mode);
            if (result.changed && result.shouldCancelUpcoming)
                this.cancelUpcomingTransitionAsync();
            return result.changed;
        }

        /**
         * Sets the repeat mode through the asynchronous face of the control API.
         * @param {string} mode - One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         * @returns {Promise<boolean>} True when the mode changed.
         */
        async setRepeatModeAsync(mode) {
            const result = this.applyRepeatMode(mode);
            if (result.changed && result.shouldCancelUpcoming)
                await this.cancelUpcomingTransitionAsync();
            return result.changed;
        }

        /**
         * Cycles the repeat mode off, all, one and back to off.
         * @returns {string} The new repeat mode.
         */
        cycleRepeatMode() {
            const modes = [
                LuminaAudioPlayer.QueueRepeatMode.OFF,
                LuminaAudioPlayer.QueueRepeatMode.ALL,
                LuminaAudioPlayer.QueueRepeatMode.ONE
            ];
            const index = modes.indexOf(this.queue.getRepeatMode());
            const mode = modes[(index + 1) % modes.length];
            const result = this.applyRepeatMode(mode);
            if (result.changed && result.shouldCancelUpcoming)
                this.cancelUpcomingTransitionAsync();
            return mode;
        }

        /**
         * Cycles the repeat mode from a control click.
         * @returns {Promise<void>} A promise that completes when the control is rendered.
         */
        async cycleRepeatModeAsync() {
            const modes = [
                LuminaAudioPlayer.QueueRepeatMode.OFF,
                LuminaAudioPlayer.QueueRepeatMode.ALL,
                LuminaAudioPlayer.QueueRepeatMode.ONE
            ];
            const index = modes.indexOf(this.queue.getRepeatMode());
            const mode = modes[(index + 1) % modes.length];
            const result = this.applyRepeatMode(mode);
            if (result.changed && result.shouldCancelUpcoming)
                await this.cancelUpcomingTransitionAsync();
        }

        /**
         * Gets the shuffle state.
         * @returns {{ isShuffled: boolean, scope: string }} The shuffle state.
         */
        getShuffleState() {
            return { isShuffled: this.queue.isShuffleEnabled(), scope: this.queue.getShuffleScope() };
        }

        /**
         * Cancels a shuffle whose scope is still resolving, without turning an already enabled shuffle off.
         * @returns {boolean} True when a pending shuffle was cancelled.
         */
        cancelPendingShuffle() {
            if (!this.isShufflePending)
                return false;
            this.shuffleToken += 1;
            this.isShufflePending = false;
            this.abortQueueBatchBuild();
            this.renderQueueControls();
            this.emitQueueStateChanged();
            return true;
        }

        /**
         * Turns shuffle on for a scope, or off again.
         *
         * Shuffle never materializes the whole category: it queues a batch of randomized tracks of the scope, keeps the
         * track that is playing first, and appends another batch while the queue approaches the end of the previous one.
         * A release draws from its own tracks, an artist from all releases of the artist, and a library from all releases of
         * all artists. Each batch is a uniform random sample selected by the server from every track of the scope, excluding
         * the tracks already queued, so artists and albums are interleaved instead of being played one after another.
         * Turning shuffle off restores the queue the shuffle replaced.
         * @param {boolean} enabled - True to enable shuffle, false to disable it.
         * @param {string} [scope] - One of the {@link LuminaAudioPlayer.QueueShuffleScope} values. Kept when omitted.
         * @returns {Promise<boolean>} True when the shuffle state was applied.
         */
        async setShuffleAsync(enabled, scope) {
            if (!this.isAvailable)
                return false;
            const targetScope = Object.values(LuminaAudioPlayer.QueueShuffleScope).includes(scope)
                ? scope
                : this.queue.getShuffleScope();
            if (!enabled) {
                // A scope that is still resolving is not visible through isShuffleEnabled yet. Selecting off must cancel
                // that in-flight shuffle instead of doing nothing and letting it apply when the request returns.
                const hadPendingShuffle = this.isShufflePending;
                if (hadPendingShuffle) {
                    this.shuffleToken += 1;
                    this.isShufflePending = false;
                    this.abortQueueBatchBuild();
                }
                if (!this.queue.isShuffleEnabled()) {
                    if (hadPendingShuffle) {
                        this.renderQueueControls();
                        this.emitQueueStateChanged();
                    }
                    return hadPendingShuffle;
                }
                // Turning shuffle off invalidates a scope that may still be resolving, and aborts its batch fetch.
                this.shuffleToken += 1;
                this.isShufflePending = false;
                this.abortQueueBatchBuild();
                this.queue.restoreUnshuffledQueue();
                this.queueGeneration += 1;
                this.shuffleSource = null;
                await this.cancelUpcomingTransitionAsync();
                this.renderQueue();
                this.scrollQueueToCurrentEntry();
                this.renderQueueControls();
                this.emitQueueStateChanged();
                if (this.queue.getCurrentEntry())
                    this.maybeExtendQueueAsync();
                return true;
            }
            // Shuffle needs something to draw from. A queue that has not started has no context and no playing descriptor yet.
            const descriptor = this.queue.getCurrentDescriptor() || this.descriptor;
            if (!descriptor)
                return false;
            const generation = this.queueGeneration;
            const cursorEntryId = this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null;
            const sourceContext = this.queue.context;
            const requestToken = this.playRequestToken;
            const shuffleCallToken = ++this.shuffleToken;
            this.isShufflePending = true;
            // A shuffle supersedes a continuation or shuffle batch that is still being fetched, so that fetch is aborted.
            this.abortQueueBatchBuild();
            const buildHandle = this.openQueueBatchBuild();
            try {
                const source = await this.queueService.createShuffleSourceAsync(targetScope, this.queue, descriptor, buildHandle.signal);
                if (generation !== this.queueGeneration
                    || (this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null) !== cursorEntryId
                    || this.queue.context !== sourceContext
                    || this.playRequestToken !== requestToken)
                    return false;
                // The first shuffle batch replaces the ordered queue of the context, so only the track that is playing must be
                // kept out of it: the tracks of the replaced queue are exactly the category a release or artist shuffle draws
                // from. The complete queued set is excluded only by the later batches of extendQueueAsync, which must not
                // repeat anything the shuffled queue already holds.
                const excluded = new Set();
                const identity = descriptor.trackId ? descriptor.trackId.toLowerCase() : '';
                if (identity)
                    excluded.add(identity);
                const batch = await source.nextBatchAsync(excluded, LuminaAudioPlayer.SHUFFLE_BATCH_SIZE, buildHandle.signal);
                // A queue item click, next or previous does not bump the generation, so the cursor and the context are compared
                // as well: a shuffle resolved for the track or context the user already left must not fill the new queue.
                if (generation !== this.queueGeneration
                    || (this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null) !== cursorEntryId
                    || this.queue.context !== sourceContext
                    || this.playRequestToken !== requestToken)
                    return false;
                this.shuffleSource = source;
                this.queue.applyShuffle(batch, targetScope, source.isExhausted);
                this.queueGeneration += 1;
                await this.cancelUpcomingTransitionAsync();
                const current = this.queue.getCurrentEntry();
                if (current) {
                    this.descriptor = current.descriptor;
                    this.renderDescriptor();
                }
                this.renderQueue();
                this.scrollQueueToCurrentEntry();
                this.renderQueueControls();
                this.emitQueueStateChanged();
                return true;
            } catch (error) {
                // A build that this player aborted was superseded by a stop, a new play or a newer shuffle. A timeout of the
                // request that still owns the player is reported, because the shuffle could not be resolved at all.
                if (buildHandle.signal && buildHandle.signal.aborted) {
                    if (buildHandle.timedOut
                        && generation === this.queueGeneration
                        && (this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null) === cursorEntryId)
                        this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The shuffle scope could not be loaded in time.'));
                    return false;
                }
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return false;
                this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED);
                return false;
            } finally {
                this.closeQueueBatchBuild(buildHandle);
                if (this.shuffleToken === shuffleCallToken)
                    this.isShufflePending = false;
            }
        }

        /**
         * Turns shuffle on with a scope, or off again when it is already on.
         * @param {string} [scope] - One of the {@link LuminaAudioPlayer.QueueShuffleScope} values.
         * @returns {Promise<boolean>} True when the shuffle state was applied.
         */
        async toggleShuffleAsync(scope) {
            // A pending shuffle is still off as far as the queue is concerned, so toggling it again cancels it.
            if (this.isShufflePending && !this.queue.isShuffleEnabled())
                return this.setShuffleAsync(false, scope);
            return this.setShuffleAsync(!this.queue.isShuffleEnabled(), scope);
        }

        /**
         * Queues a descriptor or the tracks of a context after the current entry, as an explicit play next action.
         *
         * The pending autoplay continuation is discarded, because an explicit play next is a manual queue: the user chose
         * what comes after the current track, so autopicking more of the album or artist would override that choice.
         * @param {Object} descriptorInput - The play descriptor to queue, per {@link LuminaAudioPlayer.createPlayDescriptor}.
         * @returns {Promise<boolean>} True when at least one entry was queued.
         */
        async playNextAsync(descriptorInput) {
            if (!this.isAvailable)
                return false;
            const descriptor = createPlayDescriptor(descriptorInput);
            if (!descriptor) {
                this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor identifies no library, or no playable context.'));
                return false;
            }
            // When nothing is current there is no "next" to queue after, so the request starts a new context instead and reports whether playback actually started.
            if (!this.queue.getCurrentEntry())
                return this.playAsync(descriptor);
            // An explicit play next is a later user action than a shuffle that is still resolving, so the shuffle is
            // cancelled and cannot replace the queue under the manual item.
            this.cancelPendingShuffle();
            // An end of track during this operation can move the player to ENDED after the cursor was captured. When that
            // happens, the queued item becomes the successor of the track that just ended, so it is started instead of being
            // left behind an ended player. A player that was already ended before the request keeps its ended state.
            const stateAtStart = this.state;
            const generation = this.queueGeneration;
            // A context build that still owns the player is the play request the manual item belongs to. If it is still
            // resolving when the item is ready, the item waits for it instead of being inserted into the queue it replaces.
            const pendingContextToken = this.state === LuminaAudioPlayer.PlayerState.LOADING && this.playContextBuildHandle !== null
                ? this.playRequestToken
                : null;
            const buildHandle = this.openPlayNextBuild();
            let descriptors;
            try {
                descriptors = (await this.queueService.buildQueueAsync(descriptor, buildHandle.signal)).descriptors;
            } catch (error) {
                if (buildHandle.signal && buildHandle.signal.aborted) {
                    if (buildHandle.timedOut)
                        this.raiseError(new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The play next item could not be loaded in time.'));
                    return false;
                }
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return false;
                this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED);
                return false;
            } finally {
                this.closePlayNextBuild(buildHandle);
            }
            if (descriptors.length === 0)
                return false;
            // The context is still resolving, so the item is held for the request that owns it. It is inserted as soon as
            // that context becomes current, either on success or on a preserved old queue after a failed context.
            if (pendingContextToken !== null
                && this.playRequestToken === pendingContextToken
                && this.playContextBuildHandle !== null) {
                this.pendingPlayNext.push({ requestToken: pendingContextToken, descriptors });
                return true;
            }
            // A new play request replaced the queue while the play next item was resolved, so the item belongs to a queue the
            // user already left and must not be inserted into the new one. A context that resolved successfully during the
            // fetch is owned by the same play request, so its new queue is exactly where the item belongs. A cursor that
            // moved because a track ended or the user skipped is not an abort either.
            const isOwnedContextResolution = pendingContextToken !== null && this.playRequestToken === pendingContextToken;
            if (generation !== this.queueGeneration && !isOwnedContextResolution)
                return false;
            // A handoff the engine already prepared from the continuation is dropped, so the explicit play next item becomes
            // the true successor and the discarded continuation is never heard.
            await this.cancelUpcomingTransitionAsync();
            // A queue replacement during the handoff teardown still aborts the insertion, unless it is the context this play
            // next was waiting for. A cursor that moved because the track ended is not an abort: insertNext anchors the item
            // to the entry that is current now.
            if (generation !== this.queueGeneration && !isOwnedContextResolution)
                return false;
            // The discard is mirrored only once the insertion is certain, so a superseded play next does not leave the
            // continuation disabled in a queue it never joined.
            this.queue.removePendingContinuation();
            const inserted = this.queue.insertNext(descriptors);
            if (inserted === 0)
                return false;
            this.renderQueue();
            this.renderQueueControls();
            this.emitQueueStateChanged();
            // The track that was playing ended while the item was queued, so the item is the next content and is started now.
            if (stateAtStart !== LuminaAudioPlayer.PlayerState.ENDED
                && this.state === LuminaAudioPlayer.PlayerState.ENDED) {
                const next = this.queue.getNextEntry({ allowWrap: false });
                if (next)
                    await this.startQueuedEntryAsync(next.entryId);
            }
            return true;
        }

        /**
         * Inserts the explicit play next items that waited for a play context to resolve. The items belong to one play
         * request, so a newer request discards them instead of applying an old user action to a different queue.
         * @param {number} requestToken - The play request token the pending items belong to.
         * @returns {Promise<void>} A promise that completes when the pending items were inserted.
         */
        async flushPendingPlayNext(requestToken) {
            if (!this.queue.getCurrentEntry())
                return;
            const pending = this.pendingPlayNext.filter((item) => item.requestToken === requestToken);
            if (pending.length === 0)
                return;
            await this.cancelUpcomingTransitionAsync();
            if (this.playRequestToken !== requestToken)
                return;
            this.pendingPlayNext = this.pendingPlayNext.filter((item) => item.requestToken !== requestToken);
            for (const item of pending) {
                this.queue.removePendingContinuation();
                this.queue.insertNext(item.descriptors);
            }
            this.renderQueue();
            this.renderQueueControls();
            this.emitQueueStateChanged();
        }

        /**
         * Restores the timeline that was visible before a play context attempt. It is used when the context fails or is
         * empty, so the bar keeps describing the queue that is still in memory.
         * @param {Object} snapshot - The descriptor, duration, playhead and downloaded fraction to restore.
         */
        restoreTimelineState(snapshot) {
            this.descriptor = snapshot.descriptor;
            this.duration = snapshot.duration;
            this.currentTime = snapshot.currentTime;
            this.downloadedFraction = snapshot.downloadedFraction;
            this.renderDescriptor();
            this.renderProgress();
        }

        /**
         * Gets the queue entry that follows the current one. The wrap of repeat all is withheld while the autoplay
         * continuation of an album or artist is still available, so the continuation is the true next content and the wrap
         * only happens once the continuation is exhausted.
         * @returns {{ entryId: string, descriptor: Object }|null} The identity and descriptor of the next entry, or null at the end of the queue.
         */
        getNextDescriptor() {
            // Repeat one replays the current entry at the end, so there is no next entry for a gapless handoff to prefetch:
            // prefetching the next queue entry would hand off to another track instead of repeating the current one.
            if (this.queue.getRepeatMode() === LuminaAudioPlayer.QueueRepeatMode.ONE)
                return null;
            const entry = this.queue.getNextEntry({ allowWrap: this.canWrapToFirstEntry() });
            return entry ? { entryId: entry.entryId, descriptor: entry.descriptor } : null;
        }

        /**
         * Advances the queue to the entry with the given identity, without starting a new playback. It is the single commit
         * of a queue transition: a gapless handoff passes the identity the engine already scheduled, and a non-gapless
         * continuation passes the identity the player selected. The entry is found by identity rather than by position, so
         * a linear advance, a wrap to the first entry and a reordered queue all share one path.
         * @param {string} entryId - The identity of the queue entry to make current.
         * @returns {Object|null} The descriptor the player advanced to, or null when the player is not playing or the entry is not part of the queue.
         */
        advanceQueueTo(entryId) {
            // A gapless boundary or a continuation only exists while the player is playing. In another state, for example
            // while a new play request is loading, the queue cursor belongs to that request and must not be moved by the old timeline.
            if (!entryId || this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                return null;
            // The entry is validated before the cursor moves, so a malformed entry can never leave the queue committed to a
            // descriptor the bar and the engine cannot play.
            const target = this.queue.findEntry(entryId);
            if (!target || !createPlayDescriptor(target.entry.descriptor))
                return null;
            const committed = this.queue.commit(entryId);
            // An entry that is no longer part of the queue is not a track this player can describe.
            if (!committed)
                return null;
            // Moving to the entry that is already current is not a transition. Applying its descriptor again would reset the
            // playhead, the streamed fraction and the scrubbing state while the audio keeps playing.
            if (!committed.changed)
                return committed.entry.descriptor;
            const applied = this.applyCommittedEntry(committed.entry, committed.previousIndex, committed.removedCount);
            // The commit may have trimmed the played part of the queue, which changes what the next control can do.
            this.renderQueueControls();
            this.maybeExtendQueueAsync();
            return applied;
        }

        /**
         * Applies a queue entry that a commit has already made current. It is an internal seam of the validated queue
         * advance, not a public way to start a track: the entry must be the current queue entry and it must still describe a
         * playable descriptor, so a caller cannot bypass the identity and descriptor checks of the play path.
         * @param {Object} entry - The committed queue entry to show as the playing track.
         * @param {number|null} [previousIndex] - The queue position that was current before the cursor moved, when the
         * commit knows it. It lets the queue update only the items whose current or played state changed.
         * @param {number} [removedCount=0] - The number of played entries the commit trimmed from the head of the queue.
         * @returns {Object|null} The applied descriptor, or null when the entry is not the current playable entry.
         */
        applyCommittedEntry(entry, previousIndex = null, removedCount = 0) {
            const found = entry ? this.queue.findEntry(entry.entryId) : null;
            const current = this.queue.getCurrentEntry();
            if (!found || found.entry !== entry || !current || current !== entry || !createPlayDescriptor(entry.descriptor))
                return null;
            const descriptor = entry.descriptor;
            this.descriptor = descriptor;
            this.currentTime = 0;
            this.downloadedFraction = 0;
            this.scrubRatio = null;
            this.endScrub();
            this.endVolumeAdjust();
            this.clearBuffering();
            // The duration of the next track is reported by its manifest as soon as the engine plays it, so the descriptor value is used here only until that report arrives.
            this.duration = Number.isFinite(descriptor.durationInSeconds) && descriptor.durationInSeconds > 0 ? descriptor.durationInSeconds : 0;
            this.renderDescriptor();
            this.renderProgress();
            // The queue list is only visible in the expanded bar, so its DOM is only updated while it is expanded. An expansion
            // renders it from the current queue, so a change made while it was collapsed is not lost.
            if (this.isExpanded()) {
                // A trim removes entries from the head of the queue, so the one based index and the last flag of every
                // remaining item move. Re-rendering the remaining list keeps the numbering correct; the fragment build makes
                // it one insertion instead of one per item.
                if (removedCount > 0)
                    this.renderQueue();
                else if (Number.isInteger(previousIndex))
                    this.updateQueueSelection(previousIndex);
                else
                    this.renderQueue();
                // A long queue can be scrolled far above the entry that just became current. Moving it to the top keeps the
                // queue anchored to playback, so a listener never has to search for the current track after an advance.
                this.scrollQueueToCurrentEntry();
            }
            this.events.emit(LuminaAudioPlayer.PlayerEvent.DESCRIPTOR_CHANGED, descriptor);
            this.emitQueueStateChanged();
            return descriptor;
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
         * Checks whether the bar is showing a loading state, which is the case both while a seek waits for the range of
         * its target position and while the engine reports that it is waiting for data on its own.
         * @returns {boolean} True when the player is buffering.
         */
        isBuffering() {
            return this.buffering;
        }

        /**
         * Gets the time ranges the engine has already downloaded, in seconds. An engine that does not report them
         * yields an empty list, and the player then never shows a loading state for a seek, because it cannot know
         * whether the target is buffered.
         * @returns {Array<{ start: number, end: number }>} The buffered time ranges, in ascending order.
         */
        getBufferedTimeRanges() {
            if (!this.engine || typeof this.engine.getBufferedTimeRanges !== 'function')
                return [];
            try {
                const ranges = this.engine.getBufferedTimeRanges();
                // A copy is returned, never the live array of the engine, so a consumer that reads or writes the list
                // cannot reach into the download state of the engine, exactly like the other getters copy.
                return Array.isArray(ranges)
                    ? ranges.map((range) => ({ start: range.start, end: range.end }))
                    : [];
            } catch (error) {
                console.debug('Audio player: the buffered ranges of the engine could not be read.', error);
                return [];
            }
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
            this.shuffleToken += 1;
            this.isShufflePending = false;
            this.abortQueueActivity();
            if (this.descriptorClickHandler) {
                document.removeEventListener('click', this.descriptorClickHandler);
                this.descriptorClickHandler = null;
            }
            if (this.keyboardHandler) {
                document.removeEventListener('keydown', this.keyboardHandler);
                this.keyboardHandler = null;
            }
            if (this.unloadHandler) {
                window.removeEventListener('pagehide', this.unloadHandler);
                this.unloadHandler = null;
            }
            // A change that is still inside its debounce window is flushed before the timer is dropped, so a teardown
            // never loses the last volume, mute or queue the user set. The flush clears the timer itself.
            await this.flushPlaybackStateAsync();
            this.endScrub();
            this.endVolumeAdjust();
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
            this.clearBuffering();
            this.setState(LuminaAudioPlayer.PlayerState.ERROR);
            // The error state still offers recovery: the play control retries the entry, next skips it and previous walks back
            // when there is somewhere to go. The controls are rendered now so that recovery is available immediately.
            this.renderTransportState();
            return typed;
        }

        /**
         * Registers an engine factory on this player, by name.
         *
         * This is the documented seam through which the download, decode and render engine is plugged into the player,
         * without reaching into its internals.
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
                // The engine shares the player's endpoint configuration, JSON transport and capability detector, so an
                // expired session redirect is followed by the same code that follows it for every other background
                // request, and the engine never keeps a second copy of the configuration.
                getConfiguration: () => this.getConfiguration(),
                fetchJsonAsync: (url, signal, label, options) => this.fetchJsonAsync(url, signal, label, options),
                getCapabilityDetector: () => this.capabilities,
                // The queue cursor is read from the player for the gapless handoff, so the engine never re-implements the
                // traversal and can never pick the wrong occurrence of a repeated track.
                getNextDescriptor: () => this.getNextDescriptor(),
                advanceQueueTo: (entryId) => this.advanceQueueTo(entryId),
                requestNextTrackAsync: (reason) => this.requestNextTrackAsync(reason),
                reportTime: (currentTime, duration) => this.handleEngineTime(currentTime, duration),
                reportDuration: (duration) => this.handleEngineDuration(duration),
                reportProgress: (fraction) => this.handleEngineProgress(fraction),
                reportBuffering: (isBuffering) => this.handleEngineBuffering(isBuffering),
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
            // The playhead crossing from where the seek started into the target means its range arrived, so the loading
            // state of the seek is cleared. The crossing is directional: a forward seek clears once the playhead
            // reaches the target, while a backward seek clears once it falls to the target, so the pre-seek position
            // the engine reports first never clears the state. Engine-originated buffering is not playhead driven and
            // is left untouched here.
            if (this.pendingSeekTime !== null && this.hasReachedPendingSeek())
                this.clearSeekPending();
            this.scheduleProgressRender();
        }

        /**
         * Checks whether the playhead has crossed from where a pending seek started into its target.
         * @returns {boolean} True when the playhead reached the target, within the settle tolerance.
         */
        hasReachedPendingSeek() {
            const target = this.pendingSeekTime;
            const from = this.pendingSeekFromTime;
            // Without a start position there is no direction to check, so any report counts as having arrived.
            if (from === null)
                return true;
            return target >= from
                ? this.currentTime >= target - SEEK_SETTLE_TOLERANCE_SECONDS
                : this.currentTime <= target + SEEK_SETTLE_TOLERANCE_SECONDS;
        }

        /**
         * Handles a buffering report from the engine, so an engine that fetches a range on its own can show the same
         * loading state the player shows for its own seeks. The state is kept on its own flag, so a playhead report
         * never clears a buffering state the engine reported.
         * @param {boolean} isBuffering - True when the engine is waiting for data, false when it has enough to play.
         */
        handleEngineBuffering(isBuffering) {
            this.setEngineBuffering(isBuffering === true);
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
            // The range a pending seek waits for may have just arrived, so the seek is cleared as soon as the buffered
            // ranges cover the target, whichever direction it was in. Engine-originated buffering is left untouched.
            if (this.pendingSeekTime !== null && this.isPositionBuffered(this.pendingSeekTime))
                this.clearSeekPending();
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
         * Handles the end of the current track reported by the engine: it repeats the track, continues with the next queue
         * entry, extends an exhausted queue from its autoplay continuation, or moves the player to its ended state.
         */
        handleEngineEnded() {
            // Only a playing player may continue its queue. A pause, a stop, a failure or a load in progress is an explicit
            // state that an end report of the old timeline must never override by starting the next entry.
            if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                return;
            this.handleEndedAsync().catch((error) => {
                this.fail(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR);
            });
        }

        /**
         * Resolves the end of the current track. It is asynchronous because an autoplay continuation may have to load the
         * next batch of the album, artist or shuffle before the next entry exists.
         * @returns {Promise<void>} A promise that completes when the end was resolved.
         */
        async handleEndedAsync() {
            if (this.isHandlingEnded)
                return;
            this.isHandlingEnded = true;
            try {
                const endedEntryId = this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null;
                const queueGeneration = this.queueGeneration;
                const requestToken = this.playRequestToken;
                // Repeat one replays the entry that just ended, through the player's own play path, so the queue cursor
                // stays where it is and the bar keeps describing the same track.
                if (this.queue.getRepeatMode() === LuminaAudioPlayer.QueueRepeatMode.ONE) {
                    await this.restartCurrentEntryAsync();
                    return;
                }
                let next = this.queue.getNextEntry({ allowWrap: this.canWrapToFirstEntry() });
                if (!next) {
                    await this.maybeExtendQueueAsync({ force: true });
                    // A slow continuation must not advance a track whose cursor, queue or play request moved while it loaded.
                    // A manual pick or skip during the fetch owns the transition now, and the end of the old track is stale.
                    if (this.queueGeneration !== queueGeneration
                        || (this.queue.getCurrentEntry() ? this.queue.getCurrentEntry().entryId : null) !== endedEntryId
                        || this.playRequestToken !== requestToken
                        || this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                        return;
                    next = this.queue.getNextEntry({ allowWrap: this.canWrapToFirstEntry() });
                }
                if (next) {
                    await this.startQueuedEntryAsync(next.entryId);
                    return;
                }
                if (this.state !== LuminaAudioPlayer.PlayerState.PLAYING)
                    return;
                this.setState(LuminaAudioPlayer.PlayerState.ENDED);
                this.renderTransportState();
                this.events.emit(LuminaAudioPlayer.PlayerEvent.ENDED);
            } finally {
                this.isHandlingEnded = false;
            }
        }

        /**
         * Loads the next batch of the queue when it approaches the end of what is already queued, so a long album, artist or shuffle scope never stalls at the end of a batch.
         *
         * A shuffled queue appends another batch of its scope when fewer than the shuffle lookahead of entries remain. An
         * album or artist queue appends the autoplay continuation only when the current entry is the last one, because the
         * continuation is a boundary of the whole context. A caller that is already at the end of the queue forces the
         * fetch, even when the lookahead would not have asked for it yet.
         * @param {Object} [options] - The extension options.
         * @param {boolean} [options.force=false] - True to fetch the next batch regardless of the lookahead.
         * @returns {Promise<void>} A promise that completes when no more data can be fetched immediately.
         */
        async maybeExtendQueueAsync(options = {}) {
            if (!this.isAvailable)
                return;
            // A shuffle is resolving its own source and first batch, so a continuation must not start a competing batch fetch.
            if (this.isShufflePending)
                return;
            const force = options.force === true;
            if (this.queue.isShuffleEnabled()) {
                if (this.queue.isShuffleSourceExhausted())
                    return;
                if (!force && this.queue.getRemainingCount() > LuminaAudioPlayer.SHUFFLE_LOOKAHEAD)
                    return;
            } else {
                if (this.queue.getRepeatMode() === LuminaAudioPlayer.QueueRepeatMode.ONE)
                    return;
                if (!this.queue.isContinuationEnabled() || !this.isQueueContinuationAllowed() || this.queue.isContinuationExhausted())
                    return;
                if (!force && this.queue.getRemainingCount() > 0)
                    return;
            }
            // A shuffle batch and an autoplay batch are both a single request per queue change, so a burst of advances shares
            // one in flight extension instead of starting the same request several times.
            if (this.queueExtensionPromise)
                return this.queueExtensionPromise;
            const promise = this.extendQueueAsync();
            this.queueExtensionPromise = promise;
            try {
                await promise;
            } finally {
                if (this.queueExtensionPromise === promise)
                    this.queueExtensionPromise = null;
                // A batch can change the queue length, the continuation state or the shuffle exhaustion, all of which decide
                // whether the next control is available, so the transport is rendered after every extension attempt.
                this.renderQueueControls();
            }
        }

        /**
         * Fetches and appends one batch of the queue: the next shuffle batch of the current scope, or the autoplay continuation of the current play context.
         * @returns {Promise<void>} A promise that completes when the batch was handled.
         */
        async extendQueueAsync() {
            const generation = this.queueGeneration;
            const buildHandle = this.openQueueBatchBuild();
            try {
                if (this.queue.isShuffleEnabled()) {
                    if (!this.shuffleSource) {
                        const descriptor = this.queue.getCurrentDescriptor() || this.descriptor;
                        if (!descriptor)
                            return;
                        this.shuffleSource = await this.queueService.createShuffleSourceAsync(this.queue.getShuffleScope(), this.queue, descriptor, buildHandle.signal);
                        if (generation !== this.queueGeneration || !this.queue.isShuffleEnabled())
                            return;
                    }
                    const excluded = this.queue.getQueuedTrackIdentities();
                    const batch = await this.shuffleSource.nextBatchAsync(excluded, LuminaAudioPlayer.SHUFFLE_BATCH_SIZE, buildHandle.signal);
                    // A queue that was replaced or turned unshuffled while the batch was requested must not receive it.
                    if (generation !== this.queueGeneration || !this.queue.isShuffleEnabled())
                        return;
                    if (batch.length === 0) {
                        this.queue.markShuffleSourceExhausted();
                        // The exhaustion is part of the queue state, so it is published like every other batch result.
                        this.emitQueueStateChanged();
                        return;
                    }
                    const insertIndex = this.queue.entries.length;
                    this.queue.appendShuffleBatch(batch, this.shuffleSource.isExhausted);
                    this.appendQueueTail(insertIndex);
                    this.emitQueueStateChanged();
                    return;
                }
                if (!this.queue.isContinuationEnabled() || !this.isQueueContinuationAllowed())
                    return;
                const batch = await this.queueService.buildContinuationBatchAsync(this.queue.context, this.queue, buildHandle.signal);
                // A new play context replaced the queue, or an explicit play next discarded the continuation, while the batch
                // was requested: the batch belongs to a queue that no longer exists and is dropped.
                if (generation !== this.queueGeneration || !this.queue.isContinuationEnabled())
                    return;
                if (batch.descriptors.length === 0) {
                    // Only the service declaring the continuation exhausted ends it. An empty, non-exhausted result is a transient gap and is retried by the next transition.
                    if (batch.isExhausted === true) {
                        this.queue.markContinuationExhausted();
                        this.emitQueueStateChanged();
                    }
                    return;
                }
                const insertIndex = this.queue.entries.length;
                this.queue.append(batch.descriptors, 'continuation');
                if (batch.isExhausted)
                    this.queue.markContinuationExhausted();
                this.appendQueueTail(insertIndex);
                this.emitQueueStateChanged();
            } catch (error) {
                // A build that this player aborted was superseded by a stop, a new play, a shuffle or a newer batch. A timeout
                // of a batch that still belongs to the current queue is treated as exhausted, so a hanging response marks the
                // batch done instead of leaving the extension promise pending and the queue unable to extend again.
                if (buildHandle.signal && buildHandle.signal.aborted) {
                    if (buildHandle.timedOut) {
                        if (this.queue.isShuffleEnabled())
                            this.queue.markShuffleSourceExhausted();
                        else
                            this.queue.markContinuationExhausted();
                        this.renderQueueControls();
                        this.emitQueueStateChanged();
                    }
                    return;
                }
                // A failed extension must never stop the current track. The session expiry is already navigating away, and
                // any other failure leaves the queue at its current end, so the player can still be stopped or restarted.
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return;
                this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED);
            } finally {
                this.closeQueueBatchBuild(buildHandle);
            }
        }

        /**
         * Binds the queue list, so a queue item starts the entry it describes. The entry identity is written on the rendered
         * item root whatever the theme markup is, so a theme item is clickable without knowing the identity contract.
         */
        bindQueueInteraction() {
            if (!this.dom.queue)
                return;
            // One activation rule for the pointer and the keyboard: any non-control part of an item starts it, while a button,
            // link, field or editable element a theme renders inside the item keeps its own activation. The item root itself
            // is allowed even when the theme renders it as a button or a link.
            const interactiveSelector = 'button, a, input, select, textarea, [role="button"], [role="link"], [contenteditable="true"]';
            const findActivationItem = (target) => {
                const item = target.closest('[data-audio-player-queue-entry-id]');
                if (!item)
                    return null;
                const control = target.closest(interactiveSelector);
                return control && control !== item ? null : item;
            };
            const startFromItem = (item) => {
                const entryId = item.getAttribute('data-audio-player-queue-entry-id');
                if (!entryId || !this.queue.findEntry(entryId))
                    return;
                this.startQueuedEntryAsync(entryId).catch((error) => this.raiseError(error, LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR));
            };
            this.addControlHandler(this.dom.queue, 'click', (event) => {
                if (!(event.target instanceof Element))
                    return;
                const item = findActivationItem(event.target);
                if (!item)
                    return;
                event.preventDefault();
                startFromItem(item);
            });
            // The same activation rule applies to a queue item that has keyboard focus. A focused control inside an item still
            // consumes its own Enter and Space, because the shared rule excludes it.
            this.addControlHandler(this.dom.queue, 'keydown', (event) => {
                if (!(event.target instanceof Element) || (event.key !== 'Enter' && event.key !== ' '))
                    return;
                const item = findActivationItem(event.target);
                if (!item)
                    return;
                event.preventDefault();
                startFromItem(item);
            });
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
                    streamingQuality: settings.streamingQuality || 'Auto',
                    shouldIgnoreThePrefixForAlphaPicker: settings.shouldIgnoreThePrefixForAlphaPicker === true
                };
                // The payload is a frozen copy, so a subscriber that mutates it cannot change the preferences the player uses.
                this.events.emit(LuminaAudioPlayer.PlayerEvent.PREFERENCES_CHANGED, Object.freeze({ ...this.preferences }));
                // The autoplay continuation is enabled by the preference, so a queue that was built before the preferences
                // arrived follows the stored value instead of the default of the player. A library context always keeps its
                // continuation, because it holds only the first artist until that continuation appends the next ones. A
                // shuffled queue extends itself with its next shuffle batch instead, so it is never given an autoplay continuation.
                const contextKind = this.queue.context ? this.queue.context.kind : '';
                if (this.queue.entries.length > 0) {
                    if (this.queue.isShuffleEnabled()) {
                        this.maybeExtendQueueAsync();
                    } else if (contextKind === LuminaAudioPlayer.PlayContextKind.ALBUM
                        || contextKind === LuminaAudioPlayer.PlayContextKind.ARTIST
                        || contextKind === LuminaAudioPlayer.PlayContextKind.LIBRARY) {
                        const isLibrary = contextKind === LuminaAudioPlayer.PlayContextKind.LIBRARY;
                        this.queue.setContinuationEnabled(isLibrary || this.preferences.shouldAutoplay === true);
                        if (isLibrary || this.preferences.shouldAutoplay === true)
                            this.maybeExtendQueueAsync();
                    }
                }
            }
            if (playbackState) {
                // The state the server holds is recorded before it is applied, so a change the user makes while this
                // request was in flight is still seen as a difference and written, instead of being recorded as already
                // persisted and dropped. The stored state is applied through the internal appliers, which neither mark
                // it as a user change nor write it back, and it is skipped for whichever control the user already
                // changed, so the late answer never reverts the user's own choice.
                this.lastPersistedPlaybackState = {
                    volume: Number.isFinite(Number(playbackState.volume)) ? clamp(Number(playbackState.volume), 0, 1) : this.volume,
                    isMuted: typeof playbackState.isMuted === 'boolean' ? playbackState.isMuted : this.muted
                };
                if (Number.isFinite(Number(playbackState.volume)) && !this.hasUserChangedVolume)
                    await this.applyVolumeValueAsync(clamp(Number(playbackState.volume), 0, 1), false);
                if (typeof playbackState.isMuted === 'boolean' && !this.hasUserChangedMute)
                    await this.applyMuteValueAsync(playbackState.isMuted, false);
                // The stored state is now known, so a write is allowed from this point on. A change the user made while
                // this request was in flight was refused a write until now, so it is scheduled here to be written with
                // the stored state of the control the user did not change.
                this.isStoredPlaybackStateLoaded = true;
                if (this.hasUserChangedVolume || this.hasUserChangedMute)
                    this.schedulePlaybackStatePersist();
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
         * @param {AbortSignal} [signal] - An optional signal that cancels the request, used by an engine's prefetch.
         * @param {string} [label] - The description used in the failure log, so a caller such as the engine reports its own request rather than a preferences request.
         * @param {Object} [options] - The request options.
         * @param {string} [options.redirect='follow'] - 'manual' to keep the browser from following a redirect before its target is validated.
         * @returns {Promise<*|null|undefined|Object>} The unwrapped data, null when the endpoint answered without data, undefined when the request failed, or the REDIRECTED marker when the session expired and the page is navigating away.
         */
        async fetchJsonAsync(url, signal, label = 'a preferences request', options = {}) {
            if (!url)
                return null;
            const requestUrl = resolveConfiguredUrl(url);
            if (!requestUrl) {
                console.error(`Audio player: ${label} has an endpoint that is not on this origin.`);
                return undefined;
            }
            try {
                const request = {
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    signal: signal
                };
                if (options.redirect === 'manual')
                    request.redirect = 'manual';
                const response = await fetch(requestUrl, request);
                if (options.redirect === 'manual' && isRedirectResponse(response)) {
                    // The browser did not follow the redirect, so the request cannot reach a target that was not validated.
                    // When the environment exposes the target, it goes through the same origin policy as a followed
                    // redirect; otherwise the request is reported as a session redirect without requesting the unknown target.
                    let location = null;
                    try {
                        location = response.headers && typeof response.headers.get === 'function'
                            ? response.headers.get('location')
                            : null;
                    } catch (error) {
                        console.debug('Audio player: the redirect target of the request could not be read.', error);
                    }
                    if (location) {
                        try {
                            navigateToRedirect(new URL(location, requestUrl).href);
                        } catch (error) {
                            console.debug('Audio player: the redirect target of the request could not be resolved.', error);
                        }
                    }
                    return REDIRECTED;
                }
                if (response.redirected) {
                    // The session expired and the server answered with the login page; follow it, and report the redirect, not a failure.
                    navigateToRedirect(response.url);
                    return REDIRECTED;
                }
                if (!response.ok)
                    return undefined;
                const payload = await response.json();
                return payload && payload.success === true ? payload.data : null;
            } catch (error) {
                // A request that a cancelled prefetch, a superseded load or a timeout aborted is an expected stop, not a
                // failed request, so it is reported with the same undefined result but without a console error.
                if (LuminaAudioPlayer.isAbortError(error))
                    return undefined;
                console.error(`Audio player: ${label} failed.`, error);
                return undefined;
            }
        }

        /**
         * Writes a JSON payload to a web endpoint, with the anti-forgery token the page rendered, without surfacing a
         * notification on failure, because the request is a background one that must never disturb the page.
         * @param {string} url - The endpoint URL. Nothing is requested when it is empty.
         * @param {Object} data - The payload to send as the JSON body.
         * @param {Object} [options] - The request options.
         * @param {boolean} [options.keepalive=false] - True to let the browser finish the request after the page is gone, used by the unload flush.
         * @param {AbortSignal} [options.signal] - An optional signal that cancels the request when the player supersedes or stops it.
         * @param {string} [options.label] - The description used in the failure log, so a caller such as the queue reports its own request.
         * @returns {Promise<*|null|undefined|Object>} The unwrapped data, null when the endpoint answered without data, undefined when the request failed, or the REDIRECTED marker when the session expired and the page is navigating away.
         */
        async postJsonAsync(url, data, options = {}) {
            if (!url)
                return null;
            const requestUrl = resolveConfiguredUrl(url);
            if (!requestUrl) {
                console.error('Audio player: the playback state endpoint is not on this origin.');
                return undefined;
            }
            try {
                const headers = {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                };
                // The state endpoint validates an anti-forgery token, which the global scripts partial renders into the
                // page, so the header is set whenever the token is present.
                const token = document.querySelector('input[name="__RequestVerificationToken"]');
                if (token && token.value)
                    headers['RequestVerificationToken'] = token.value;
                const response = await fetch(requestUrl, {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify(data ?? {}),
                    signal: options.signal,
                    // A keepalive request is not cancelled when the page unloads, so the last volume or mute set before leaving the page is not lost to the navigation.
                    keepalive: options.keepalive === true
                });
                if (response.redirected) {
                    // The session expired and the server answered with the login page; follow it, and report the redirect, not a failure.
                    navigateToRedirect(response.url);
                    return REDIRECTED;
                }
                if (!response.ok)
                    return undefined;
                const payload = await response.json();
                return payload && payload.success === true ? payload.data : null;
            } catch (error) {
                // A request a superseded play, stop or timeout aborted is an expected stop, not a failed request.
                if (LuminaAudioPlayer.isAbortError(error))
                    return undefined;
                console.error(`Audio player: ${options.label || 'a playback state request'} failed.`, error);
                return undefined;
            }
        }

        /**
         * Applies the effective volume to the engine.
         * @returns {Promise<void>} A promise that completes when the engine received the volume.
         */
        async applyVolumeAsync() {
            // The volume control is rendered before the engine check, so the bar reflects the volume even before the first
            // track creates an engine.
            this.renderVolume();
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
         * Handles a pointer press on the progress bar: it captures the pointer so a drag that leaves the bar keeps
         * scrubbing, and moves the preview to the pressed ratio.
         * @param {PointerEvent} event - The pointer event on the progress bar.
         */
        handleProgressPointerDown(event) {
            // Scrubbing only makes sense with a track and a known duration, because the ratio is mapped through the
            // duration; without the bar there is nothing to scrub either. A secondary mouse button is ignored.
            if (!this.descriptor || this.duration <= 0 || !this.dom.progress)
                return;
            if (event.button !== undefined && event.button !== 0)
                return;
            event.preventDefault();
            // The pointer press is prevented so it never starts a text selection, which suppresses the default focus on
            // a press, so the element is focused explicitly; a focusable slider is then reachable and operable by
            // keyboard, and the shortcut handler routes to it.
            if (typeof this.dom.progress.focus === 'function')
                this.dom.progress.focus({ preventScroll: true });
            // The bounds are measured once, at the start of the drag, so a pointer move never reads the layout again.
            this.progressBounds = this.dom.progress.getBoundingClientRect();
            this.isScrubbing = true;
            this.scrubPointerId = event.pointerId;
            this.dom.progress.classList.add('is-scrubbing');
            try {
                this.dom.progress.setPointerCapture(event.pointerId);
            } catch (error) {
                console.debug('Audio player: the progress bar could not capture the pointer.', error);
            }
            this.updateScrubFromPointer(event);
        }

        /**
         * Handles a pointer move while scrubbing the progress bar, updating the preview position.
         * @param {PointerEvent} event - The pointer event on the progress bar.
         */
        handleProgressPointerMove(event) {
            if (!this.isScrubbing)
                return;
            event.preventDefault();
            this.updateScrubFromPointer(event);
        }

        /**
         * Handles the release of the pointer that scrubbed the progress bar: the seek is committed once, at the
         * scrubbed position, so that dragging across the bar never issues one range fetch per pixel.
         */
        handleProgressPointerUp() {
            if (!this.isScrubbing)
                return;
            const ratio = this.scrubRatio;
            this.scrubRatio = null;
            this.endScrub();
            // A scrub that never moved still commits the pressed ratio, so a plain click keeps seeking where it clicked.
            if (ratio !== null)
                this.seekAsync(ratio * this.duration);
            else
                this.scheduleProgressRender();
        }

        /**
         * Handles the cancellation of the pointer that scrubbed the progress bar, which happens when the browser takes
         * the gesture over. The scrub is abandoned without seeking, and the bar returns to the reported playhead.
         */
        handleProgressPointerCancel() {
            if (!this.isScrubbing)
                return;
            this.scrubRatio = null;
            this.endScrub();
            this.scheduleProgressRender();
        }

        /**
         * Ends a scrub of the progress bar, whatever ends it: the pointer being released, or the track stopping or
         * another one starting while the drag is still in progress. It clears the scrubbing state, drops the dragging
         * cursor and releases a captured pointer, so a stopped player is never left with a control that still reacts
         * to pointer moves. It is idempotent, so calling it when no scrub is in progress is harmless.
         */
        endScrub() {
            const element = this.dom.progress;
            if (element) {
                element.classList.remove('is-scrubbing');
                if (this.scrubPointerId !== null) {
                    try {
                        element.releasePointerCapture(this.scrubPointerId);
                    } catch (error) {
                        console.debug('Audio player: the progress bar could not release the pointer.', error);
                    }
                }
            }
            this.scrubPointerId = null;
            this.progressBounds = null;
            this.isScrubbing = false;
        }

        /**
         * Updates the preview position of a scrub from the horizontal position of the pointer.
         * @param {PointerEvent} event - The pointer event on the progress bar.
         */
        updateScrubFromPointer(event) {
            const ratio = this.computeRatioFromPointer(event, this.dom.progress, this.progressBounds);
            if (ratio === null)
                return;
            this.scrubRatio = ratio;
            this.scheduleProgressRender();
        }

        /**
         * Handles a pointer press on the volume control: it captures the pointer and starts adjusting the volume.
         * @param {PointerEvent} event - The pointer event on the volume control.
         */
        handleVolumePointerDown(event) {
            if (!this.dom.volume)
                return;
            if (event.button !== undefined && event.button !== 0)
                return;
            event.preventDefault();
            // The pointer press is prevented so it never starts a text selection, which suppresses the default focus on
            // a press, so the element is focused explicitly; a focusable slider is then reachable and operable by
            // keyboard, and the shortcut handler routes to it.
            if (typeof this.dom.volume.focus === 'function')
                this.dom.volume.focus({ preventScroll: true });
            // The bounds are measured once, at the start of the drag, so a pointer move never reads the layout again.
            this.volumeBounds = this.dom.volume.getBoundingClientRect();
            this.isAdjustingVolume = true;
            this.volumePointerId = event.pointerId;
            this.dom.volume.classList.add('is-scrubbing');
            try {
                this.dom.volume.setPointerCapture(event.pointerId);
            } catch (error) {
                console.debug('Audio player: the volume control could not capture the pointer.', error);
            }
            this.updateVolumeFromPointer(event);
        }

        /**
         * Handles a pointer move while adjusting the volume, queuing the volume of the pointer position.
         * @param {PointerEvent} event - The pointer event on the volume control.
         */
        handleVolumePointerMove(event) {
            if (!this.isAdjustingVolume)
                return;
            event.preventDefault();
            this.updateVolumeFromPointer(event);
        }

        /**
         * Handles the release of the pointer that adjusted the volume. The last queued volume is applied, because the
         * frame it was queued for may not have run yet, so a fast click or the final delta of a drag is never lost.
         */
        handleVolumePointerUp() {
            if (!this.isAdjustingVolume)
                return;
            this.endVolumeAdjust(false);
        }

        /**
         * Handles the cancellation of the pointer that adjusted the volume, which happens when the browser takes the
         * gesture over. The queued volume is dropped, so a cancelled gesture never changes the volume.
         */
        handleVolumePointerCancel() {
            if (!this.isAdjustingVolume)
                return;
            this.endVolumeAdjust();
        }

        /**
         * Ends a volume adjustment, whatever ends it: the pointer being released, or the track stopping or another one
         * starting while the drag is still in progress. It clears the adjusting state, drops the dragging cursor and
         * releases a captured pointer, so a stopped player is never left with a volume control that still reacts to
         * pointer moves. It is idempotent, so calling it when no adjustment is in progress is harmless.
         * @param {boolean} [discardPending=true] - True to drop a volume change that is still queued for the next frame, which is what a stop or a track change wants; false to apply it, which is what a pointer release wants.
         */
        endVolumeAdjust(discardPending = true) {
            const element = this.dom.volume;
            if (element) {
                element.classList.remove('is-scrubbing');
                if (this.volumePointerId !== null) {
                    try {
                        element.releasePointerCapture(this.volumePointerId);
                    } catch (error) {
                        console.debug('Audio player: the volume control could not release the pointer.', error);
                    }
                }
            }
            this.volumePointerId = null;
            this.volumeBounds = null;
            const pendingRatio = this.pendingVolumeRatio;
            // The queued change is cleared either way, so the frame that was scheduled for it finds nothing to do; a
            // release applies it here instead, while a stop or a track change drops it, so the control never applies one
            // more volume after it was released.
            this.pendingVolumeRatio = null;
            this.isAdjustingVolume = false;
            if (!discardPending && pendingRatio !== null)
                this.applyVolumeRatioAsync(pendingRatio);
        }

        /**
         * Queues the volume of the horizontal position of the pointer, coalescing a burst of pointer moves into a
         * single volume change per frame.
         * @param {PointerEvent} event - The pointer event on the volume control.
         */
        updateVolumeFromPointer(event) {
            const ratio = this.computeRatioFromPointer(event, this.dom.volume, this.volumeBounds);
            if (ratio === null)
                return;
            this.pendingVolumeRatio = ratio;
            this.scheduleVolumeApply();
        }

        /**
         * Schedules the queued volume change on the next frame, so a burst of pointer moves does not turn into a burst
         * of volume changes and bar renders.
         */
        scheduleVolumeApply() {
            if (this.isVolumeApplyScheduled)
                return;
            this.isVolumeApplyScheduled = true;
            const apply = () => {
                this.isVolumeApplyScheduled = false;
                const ratio = this.pendingVolumeRatio;
                this.pendingVolumeRatio = null;
                if (ratio !== null)
                    this.applyVolumeRatioAsync(ratio);
            };
            if (typeof requestAnimationFrame === 'function')
                requestAnimationFrame(apply);
            else
                apply();
        }

        /**
         * Applies a volume ratio, unmuting a muted player first, exactly like the keyboard shortcut does.
         * @param {number} ratio - The volume ratio to apply, between 0 and 1.
         * @returns {Promise<void>} A promise that completes when the volume was applied.
         */
        async applyVolumeRatioAsync(ratio) {
            if (this.muted && ratio > 0)
                await this.setMutedAsync(false);
            await this.setVolumeAsync(ratio);
        }

        /**
         * Computes the ratio, between 0 and 1, of the horizontal position of the pointer within an element.
         * @param {PointerEvent} event - The pointer event whose position is measured.
         * @param {HTMLElement} element - The element the ratio is measured on.
         * @param {Object} [bounds] - The bounds of the element, measured when a drag started, so a move never reads the layout. They are measured here when omitted.
         * @returns {number|null} The ratio, or null when the element has no measurable width.
         */
        computeRatioFromPointer(event, element, bounds) {
            if (!element)
                return null;
            const measured = bounds || element.getBoundingClientRect();
            if (measured.width <= 0)
                return null;
            return clamp((event.clientX - measured.left) / measured.width, 0, 1);
        }

        /**
         * Handles a keyboard shortcut of the transport. The listener is on the document, so the shortcuts work without
         * the bar being focused, but they never run while a form control or an editable element has focus, and never
         * while the bar is hidden, so they never steal a keystroke from the page.
         * @param {KeyboardEvent} event - The keydown event.
         */
        handleKeyboardShortcut(event) {
            if (event.defaultPrevented || event.ctrlKey || event.metaKey || event.altKey)
                return;
            if (!this.isAvailable || !this.container || this.container.classList.contains('not-shown'))
                return;
            if (this.isTextEntryTarget(event.target))
                return;
            switch (event.key) {
                case 'ArrowRight':
                    this.seekBy(SEEK_STEP_SECONDS);
                    event.preventDefault();
                    break;
                case 'ArrowLeft':
                    this.seekBy(-SEEK_STEP_SECONDS);
                    event.preventDefault();
                    break;
                case 'ArrowUp':
                    this.changeVolumeByAsync(VOLUME_STEP);
                    event.preventDefault();
                    break;
                case 'ArrowDown':
                    this.changeVolumeByAsync(-VOLUME_STEP);
                    event.preventDefault();
                    break;
                default:
                    break;
            }
        }

        /**
         * Checks whether a key event targets a control that consumes the keystroke itself, such as a text field.
         * @param {EventTarget|null} target - The target of the key event.
         * @returns {boolean} True when the target is a form control or an editable element.
         */
        isTextEntryTarget(target) {
            if (!(target instanceof Element))
                return false;
            const tagName = target.tagName;
            if (tagName === 'INPUT' || tagName === 'TEXTAREA' || tagName === 'SELECT')
                return true;
            return target.isContentEditable === true;
        }

        /**
         * Marks whether a seek is waiting for the range of its target, and publishes the combined loading state.
         * @param {boolean} isPending - True to mark a seek as pending.
         * @param {number|null} targetTime - The position the pending seek waits for.
         * @param {number|null} fromTime - The position the pending seek started from, or null.
         */
        setSeekPending(isPending, targetTime, fromTime) {
            const nextTarget = isPending ? targetTime : null;
            const nextFrom = isPending ? fromTime : null;
            if (this.pendingSeekTime === nextTarget && this.pendingSeekFromTime === nextFrom)
                return;
            this.pendingSeekTime = nextTarget;
            this.pendingSeekFromTime = nextFrom;
            this.updateBufferingState();
        }

        /**
         * Clears a pending seek, when there is one, leaving an engine-originated buffering state in place.
         */
        clearSeekPending() {
            if (this.pendingSeekTime === null && this.pendingSeekFromTime === null)
                return;
            this.pendingSeekTime = null;
            this.pendingSeekFromTime = null;
            this.updateBufferingState();
        }

        /**
         * Marks whether the engine reports that it is waiting for data on its own, and publishes the combined state.
         * @param {boolean} isBuffering - True when the engine is waiting for data.
         */
        setEngineBuffering(isBuffering) {
            if (this.engineBuffering === isBuffering)
                return;
            this.engineBuffering = isBuffering;
            this.updateBufferingState();
        }

        /**
         * Recomputes the loading state from the pending seek and the engine buffering report, renders the bar, and
         * publishes the change when it moved. The state is shown while either input is active, so a buffering engine
         * stays visible even when no seek is pending, and a seek stays visible even when the engine reports no
         * buffering.
         */
        updateBufferingState() {
            const isBuffering = this.pendingSeekTime !== null || this.engineBuffering;
            // The event is published when either input changed, so a second seek that only moves the pending target
            // while the bar is already buffering still carries the new target, exactly as the event documents.
            const hasChanged = this.buffering !== isBuffering || this.announcedPendingSeekTime !== this.pendingSeekTime;
            this.buffering = isBuffering;
            this.scheduleProgressRender();
            if (!hasChanged)
                return;
            this.announcedPendingSeekTime = this.pendingSeekTime;
            this.events.emit(LuminaAudioPlayer.PlayerEvent.BUFFERING_CHANGED, {
                isBuffering: this.buffering,
                pendingSeekTime: this.pendingSeekTime
            });
        }

        /**
         * Clears the whole loading state: a pending seek and an engine-originated buffering report.
         */
        clearBuffering() {
            const hadSeek = this.pendingSeekTime !== null || this.pendingSeekFromTime !== null;
            const hadEngineBuffering = this.engineBuffering;
            if (!hadSeek && !hadEngineBuffering)
                return;
            this.pendingSeekTime = null;
            this.pendingSeekFromTime = null;
            this.engineBuffering = false;
            this.updateBufferingState();
        }

        /**
         * Checks whether a position, in seconds, falls inside one of the ranges the engine downloaded.
         * @param {number} seconds - The position to check.
         * @returns {boolean} True when the position is covered by a buffered range.
         */
        isPositionBuffered(seconds) {
            const ranges = this.getBufferedTimeRanges();
            if (ranges.length === 0)
                return false;
            return ranges.some((range) => Number.isFinite(range.start) && Number.isFinite(range.end) && seconds >= range.start && seconds <= range.end);
        }

        /**
         * Schedules a write of the volume and mute state to the stored playback state of the user.
         *
         * A drag of the volume control changes the volume on every pointer move, so the writes are coalesced into one
         * request. Only a user change schedules a write: the stored state is applied through the internal appliers,
         * which never call this.
         */
        schedulePlaybackStatePersist() {
            if (this.persistenceTimer !== null)
                clearTimeout(this.persistenceTimer);
            this.persistenceTimer = setTimeout(() => {
                this.persistenceTimer = null;
                this.persistPlaybackStateAsync();
            }, PLAYBACK_STATE_PERSIST_DELAY_MS);
        }

        /**
         * Writes the volume and mute state right away, cancelling a scheduled debounce, so a change that is still
         * waiting is not lost when the page goes away or the player is torn down.
         * @param {Object} [options] - The request options, forwarded to the write.
         * @returns {Promise<void>} A promise that completes when the write was attempted.
         */
        flushPlaybackStateAsync(options = {}) {
            if (this.persistenceTimer !== null) {
                clearTimeout(this.persistenceTimer);
                this.persistenceTimer = null;
            }
            return this.persistPlaybackStateAsync(options);
        }

        /**
         * Writes the volume and mute state to the stored playback state of the user, unless it is already the stored
         * state. The request is best effort: a failure is logged, and the player keeps working with the local values.
         *
         * The write is refused until the stored state has been read once. The endpoint takes the whole state, both the
         * volume and the mute flag, and rejects a missing one, so writing the local defaults of a player that has not
         * loaded the stored state yet, for example from the unload flush, would overwrite what the user had stored.
         * @param {Object} [options] - The request options, forwarded to the write.
         * @returns {Promise<void>} A promise that completes when the write was attempted.
         */
        async persistPlaybackStateAsync(options = {}) {
            if (!this.isStoredPlaybackStateLoaded)
                return;
            const configuration = this.getConfiguration();
            const url = configuration.updatePlaybackStateUrl;
            if (!url)
                return;
            const state = { volume: this.volume, isMuted: this.muted };
            const last = this.lastPersistedPlaybackState;
            if (last && last.volume === state.volume && last.isMuted === state.isMuted)
                return;
            // The state is remembered as persisted before the request settles, so a failed background write is not
            // retried on every later change of an unrelated value.
            this.lastPersistedPlaybackState = state;
            await this.postJsonAsync(url, state, options);
        }

        /**
         * Ensures the bar starts hidden, so that a page load never flashes an empty player.
         */
        applyHiddenState() {
            this.container.classList.add('not-shown');
            this.setBarInert(true);
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
            if (this.dom.albumName)
                this.dom.albumName.textContent = descriptor ? getDisplayAlbumTitle(descriptor.albumTitle) : '';
            const coverArtUrl = descriptor && descriptor.coverArtUrl
                ? descriptor.coverArtUrl
                : MUSIC_PLACEHOLDER_URL;
            if (this.dom.coverThumbnail)
                this.dom.coverThumbnail.src = coverArtUrl;
            if (this.dom.coverImage)
                this.dom.coverImage.src = coverArtUrl;
            // The artwork chosen from the carousel belongs to the album that was showing when it was chosen. A new track resets the selection to the cover of its own album.
            this.nowPlayingArtworkPath = descriptor ? (descriptor.coverArtPath || '') : '';
            this.renderNowPlayingDetails();
            if (this.dom.totalTime)
                this.dom.totalTime.textContent = formatDuration(this.duration);
            this.renderTransportState();
        }

        /**
         * Renders the now playing labels and the album artwork carousel of the expanded player. The track, release and
         * artist labels fall back to the play descriptor until the album details arrive, and the carousel is hidden while the album has no stored artwork.
         */
        renderNowPlayingDetails() {
            // An older custom theme can omit the now playing area entirely. In that case there is nothing to label and no reason to fetch the album details.
            if (!this.dom.nowPlayingTitle
                && !this.dom.nowPlayingReleaseType
                && !this.dom.nowPlayingRelease
                && !this.dom.nowPlayingYear
                && !this.dom.nowPlayingArtist
                && !this.dom.artworkCarousel)
                return;
            const descriptor = this.descriptor;
            if (this.dom.nowPlayingTitle)
                this.dom.nowPlayingTitle.textContent = descriptor ? descriptor.title : '';
            if (!descriptor) {
                this.setNowPlayingDetailsText('', '', '');
                this.renderNowPlayingArtwork([]);
                this.nowPlayingAlbumKey = '';
                this.nowPlayingDetails = null;
                this.nowPlayingDetailsToken += 1;
                this.cancelNowPlayingDetailsRequest();
                return;
            }
            const albumKey = this.getNowPlayingAlbumKey(descriptor);
            if (!albumKey) {
                this.setNowPlayingDetailsText(getDisplayAlbumTitle(descriptor.albumTitle), '', '');
                this.renderNowPlayingArtwork([]);
                this.nowPlayingAlbumKey = '';
                this.nowPlayingDetails = null;
                this.nowPlayingDetailsToken += 1;
                this.cancelNowPlayingDetailsRequest();
                return;
            }
            if (albumKey !== this.nowPlayingAlbumKey) {
                this.nowPlayingAlbumKey = albumKey;
                this.nowPlayingDetails = null;
                this.nowPlayingDetailsToken += 1;
                this.cancelNowPlayingDetailsRequest();
                this.setNowPlayingDetailsText(getDisplayAlbumTitle(descriptor.albumTitle), '', '');
                this.renderNowPlayingArtwork([]);
                const cached = this.readNowPlayingDetailsCache(albumKey);
                if (cached)
                    this.applyNowPlayingDetails(cached);
                else if (this.isExpanded())
                    this.loadNowPlayingDetailsAsync(albumKey, descriptor);
                return;
            }
            if (this.nowPlayingDetails)
                this.applyNowPlayingDetails(this.nowPlayingDetails);
            else if (this.isExpanded())
                this.loadNowPlayingDetailsAsync(albumKey, descriptor);
        }

        /**
         * Writes the now playing labels of the expanded player. Empty values are left to the stylesheet, which hides the label of an empty value instead of leaving a gap in the column.
         * @param {string} release - The release name.
         * @param {number|string} year - The release year.
         * @param {string} artist - The artist name.
         * @param {string} [releaseType] - The display label of the release type.
         */
        setNowPlayingDetailsText(release, year, artist, releaseType = '') {
            if (this.dom.nowPlayingReleaseType)
                this.dom.nowPlayingReleaseType.textContent = releaseType || '';
            if (this.dom.nowPlayingRelease)
                this.dom.nowPlayingRelease.textContent = release || '';
            if (this.dom.nowPlayingYear)
                this.dom.nowPlayingYear.textContent = year === null || year === undefined || year === '' ? '' : String(year);
            if (this.dom.nowPlayingArtist)
                this.dom.nowPlayingArtist.textContent = artist || '';
        }

        /**
         * Builds the album identity used to cache and cancel the now playing album details.
         * @param {Object} descriptor - The current play descriptor.
         * @returns {string} The album identity, or an empty string when the descriptor names no album.
         */
        getNowPlayingAlbumKey(descriptor) {
            if (!descriptor || !descriptor.libraryId || !descriptor.artistId || !descriptor.albumId)
                return '';
            return `${descriptor.libraryId}|${descriptor.artistId}|${descriptor.albumId}`.toLowerCase();
        }

        /**
         * Loads the details of the current album and applies its release labels and artwork.
         * @param {string} albumKey - The album identity the request belongs to.
         * @param {Object} descriptor - The descriptor the album identity was read from.
         * @returns {Promise<void>} A promise that completes when the request settled.
         */
        async loadNowPlayingDetailsAsync(albumKey, descriptor) {
            if (!this.isExpanded() || !descriptor)
                return;
            // A request for the same album is already in flight, so a track change inside that album does not repeat it.
            if (this.nowPlayingDetailsRequestKey === albumKey && this.nowPlayingDetailsAbortController)
                return;
            this.cancelNowPlayingDetailsRequest();
            const token = ++this.nowPlayingDetailsToken;
            const controller = typeof AbortController === 'function' ? new AbortController() : null;
            this.nowPlayingDetailsAbortController = controller;
            this.nowPlayingDetailsRequestKey = albumKey;
            try {
                const details = await this.queueService.fetchAlbumDetailsAsync(
                    descriptor.artistId, descriptor.albumId, descriptor.libraryId, controller ? controller.signal : undefined);
                if (token !== this.nowPlayingDetailsToken
                    || !this.isExpanded()
                    || this.getNowPlayingAlbumKey(this.descriptor) !== albumKey)
                    return;
                const summary = this.createNowPlayingDetails(details);
                this.writeNowPlayingDetailsCache(albumKey, summary);
                this.applyNowPlayingDetails(summary);
            } catch (error) {
                if (token !== this.nowPlayingDetailsToken)
                    return;
                if (error && error.details && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED)
                    return;
                if ((controller && controller.signal.aborted) || LuminaAudioPlayer.isAbortError(error))
                    return;
                // The album details are decorative. A failure leaves the descriptor fallback labels in place and does not interrupt playback with a notification.
                console.debug('Audio player: the now playing album details could not be loaded.', error);
            } finally {
                if (token === this.nowPlayingDetailsToken) {
                    this.nowPlayingDetailsAbortController = null;
                    this.nowPlayingDetailsRequestKey = '';
                }
            }
        }

        /**
         * Cancels a now playing album details request that a newer track, a collapse or a stop superseded.
         */
        cancelNowPlayingDetailsRequest() {
            if (this.nowPlayingDetailsAbortController) {
                this.nowPlayingDetailsAbortController.abort();
                this.nowPlayingDetailsAbortController = null;
            }
            this.nowPlayingDetailsRequestKey = '';
        }

        /**
         * Converts an album response to the small model the expanded player keeps, so the full album payload, with all of its tracks and ratings, is not retained per cached album.
         * @param {Object} details - The album response.
         * @returns {Object} The now playing album model.
         */
        createNowPlayingDetails(details) {
            const metadata = details && details.metadata && typeof details.metadata === 'object' ? details.metadata : {};
            const releaseInfo = metadata.releaseInfo && typeof metadata.releaseInfo === 'object' ? metadata.releaseInfo : {};
            const releaseYear = releaseInfo.reReleaseYear !== null && releaseInfo.reReleaseYear !== undefined && Number.isFinite(Number(releaseInfo.reReleaseYear))
                ? Number(releaseInfo.reReleaseYear)
                : (releaseInfo.originalReleaseYear !== null && releaseInfo.originalReleaseYear !== undefined && Number.isFinite(Number(releaseInfo.originalReleaseYear))
                    ? Number(releaseInfo.originalReleaseYear)
                    : '');
            const artwork = Array.isArray(details && details.artwork)
                ? details.artwork
                    .filter((item) => item && typeof item.path === 'string' && item.path.trim().length > 0)
                    .map((item, index) => ({
                        path: item.path.trim(),
                        type: item.type,
                        ordinal: Number.isFinite(Number(item.ordinal)) ? Number(item.ordinal) : index
                    }))
                : [];
            const releaseTypes = Array.isArray(metadata.releaseTypes)
                ? metadata.releaseTypes.map(formatReleaseType).filter((type) => type !== '')
                : [];
            return {
                artistName: details && typeof details.artistName === 'string' ? details.artistName.trim() : '',
                releaseName: typeof metadata.title === 'string' ? metadata.title.trim() : '',
                releaseYear,
                releaseTypes,
                artwork
            };
        }

        /**
         * Applies a now playing album model to the labels and artwork carousel.
         * @param {Object} details - The now playing album model.
         */
        applyNowPlayingDetails(details) {
            if (!details)
                return;
            this.nowPlayingDetails = details;
            const descriptor = this.descriptor;
            const fallbackRelease = descriptor ? getDisplayAlbumTitle(descriptor.albumTitle) : '';
            const releaseName = getDisplayAlbumTitle(details.releaseName) || fallbackRelease;
            const releaseType = Array.isArray(details.releaseTypes) ? details.releaseTypes.join(', ') : '';
            this.setNowPlayingDetailsText(releaseName, details.releaseYear, details.artistName, releaseType);
            this.renderNowPlayingArtwork(details.artwork);
            this.syncNowPlayingArtworkSelection();
        }

        /**
         * Renders the thumbnails of the stored album artwork, reusing the same carousel classes as the album editor.
         * @param {Array<Object>} artwork - The stored artwork of the album.
         */
        renderNowPlayingArtwork(artwork) {
            const carousel = this.dom.artworkCarousel;
            if (!carousel)
                return;
            carousel.innerHTML = '';
            const items = Array.isArray(artwork) ? artwork : [];
            const label = this.getStrings().albumArtwork || 'Album artwork';
            for (let index = 0; index < items.length; index++) {
                const path = items[index].path;
                const url = resolveCoverArtUrl(path);
                if (!url)
                    continue;
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'artwork-carousel-item';
                button.title = items.length > 1 ? `${label} ${index + 1}` : label;
                button.setAttribute('aria-label', button.title);
                button.setAttribute('aria-pressed', 'false');
                button.dataset.artworkPath = path;
                const image = document.createElement('img');
                image.src = url;
                image.alt = '';
                image.loading = 'lazy';
                button.appendChild(image);
                button.addEventListener('click', () => this.selectNowPlayingArtwork(path, url));
                carousel.appendChild(button);
            }
            carousel.hidden = carousel.children.length === 0;
            if (carousel.hidden)
                this.nowPlayingArtworkPath = '';
        }

        /**
         * Shows one of the album artwork pieces in the expanded cover and the condensed thumbnail.
         * @param {string} path - The stored path of the chosen artwork.
         * @param {string} [url] - The resolved URL of the artwork.
         */
        selectNowPlayingArtwork(path, url) {
            this.nowPlayingArtworkPath = path || '';
            const resolved = url || resolveCoverArtUrl(path) || MUSIC_PLACEHOLDER_URL;
            if (this.dom.coverImage)
                this.dom.coverImage.src = resolved;
            if (this.dom.coverThumbnail)
                this.dom.coverThumbnail.src = resolved;
            this.syncNowPlayingArtworkSelection();
        }

        /**
         * Marks the carousel item that matches the artwork currently shown.
         */
        syncNowPlayingArtworkSelection() {
            if (!this.dom.artworkCarousel)
                return;
            const selected = normalizeArtworkPath(this.nowPlayingArtworkPath);
            const items = this.dom.artworkCarousel.querySelectorAll('.artwork-carousel-item');
            for (const item of items) {
                const isSelected = selected !== '' && normalizeArtworkPath(item.dataset.artworkPath) === selected;
                item.classList.toggle('selected', isSelected);
                item.setAttribute('aria-pressed', isSelected ? 'true' : 'false');
            }
        }

        /**
         * Reads cached now playing album details and marks them as the most recently used.
         * @param {string} albumKey - The album identity.
         * @returns {Object|null} The cached model, or null when it is not cached.
         */
        readNowPlayingDetailsCache(albumKey) {
            if (!this.nowPlayingDetailsCache.has(albumKey))
                return null;
            const value = this.nowPlayingDetailsCache.get(albumKey);
            this.nowPlayingDetailsCache.delete(albumKey);
            this.nowPlayingDetailsCache.set(albumKey, value);
            return value;
        }

        /**
         * Writes now playing album details to the bounded cache.
         * @param {string} albumKey - The album identity.
         * @param {Object} details - The now playing album model.
         */
        writeNowPlayingDetailsCache(albumKey, details) {
            this.nowPlayingDetailsCache.set(albumKey, details);
            while (this.nowPlayingDetailsCache.size > 32)
                this.nowPlayingDetailsCache.delete(this.nowPlayingDetailsCache.keys().next().value);
        }

        /**
         * Renders the three state progress bar: the played part, the downloaded but not played part, and the not yet
         * downloaded part.
         *
         * The player publishes the geometry of each state, as a start and a width, through the CSS custom properties
         * '--audio-player-progress-<state>-start' and '--audio-player-progress-<state>-width' on the bar, where the
         * state is 'played', 'downloaded' or 'undownloaded'. It writes no inline geometry, so a theme consumes those
         * properties to style each state, and to give the bar any shape or orientation; the fallback stylesheet
         * consumes them too. The player never hardcodes a color or a shape here. While the bar is scrubbed the position
         * shown is the scrub position, and while a seek waits for its range the bar is marked with the 'is-buffering'
         * class and the 'buffering' state.
         */
        renderProgress() {
            const isScrubbing = this.scrubRatio !== null;
            const position = isScrubbing ? this.scrubRatio * this.duration : this.currentTime;
            const playedRatio = this.duration > 0 ? clamp(position / this.duration, 0, 1) : 0;
            const downloadedRatio = clamp(this.downloadedFraction, 0, 1);
            // The downloaded but not played state runs from the playhead to the downloaded frontier, and the not yet
            // downloaded state runs from that frontier to the end. Scrubbing past the frontier keeps the three states
            // contiguous by moving the frontier with the scrub position.
            const frontier = Math.max(playedRatio, downloadedRatio);
            const downloadedOnlyRatio = Math.max(downloadedRatio - playedRatio, 0);
            const undownloadedRatio = clamp(1 - frontier, 0, 1);
            const currentTimeText = formatTime(position);
            const totalTimeText = formatDuration(this.duration);
            // The bar is a slider: its value is the playhead in seconds, its maximum is the duration once it is known,
            // and its readable value is the position within the track, replaced by the loading label while it buffers.
            const maximum = this.duration > 0 ? Math.max(1, Math.round(this.duration)) : 1;
            const value = Math.round(clamp(playedRatio, 0, 1) * maximum);
            const strings = this.getStrings();
            const bufferingLabel = this.buffering ? (strings.buffering || '') : '';
            const valueText = bufferingLabel !== '' ? bufferingLabel : `${currentTimeText} / ${totalTimeText}`;
            const state = this.descriptor ? (this.buffering ? 'buffering' : 'ready') : 'idle';
            // The values are compared to the previous render, so a report that arrives between two seconds skips the
            // clock and the accessible value instead of rewriting them unchanged, while the geometry, which moves every
            // frame during playback, is still written.
            const next = {
                currentTimeText,
                totalTimeText,
                playedStart: '0%',
                playedWidth: `${playedRatio * 100}%`,
                downloadedStart: `${playedRatio * 100}%`,
                downloadedWidth: `${downloadedOnlyRatio * 100}%`,
                undownloadedStart: `${frontier * 100}%`,
                undownloadedWidth: `${undownloadedRatio * 100}%`,
                buffering: this.buffering,
                state,
                seekLabel: strings.seek || null,
                maximum: String(maximum),
                value: String(value),
                valueText,
                ariaBusy: this.buffering ? 'true' : 'false'
            };
            const previous = this.renderedProgress;
            this.renderedProgress = next;
            const changed = (key) => !previous || previous[key] !== next[key];
            if (this.dom.currentTime && changed('currentTimeText'))
                this.dom.currentTime.textContent = currentTimeText;
            if (this.dom.totalTime && changed('totalTimeText'))
                this.dom.totalTime.textContent = totalTimeText;
            if (this.dom.progress) {
                // Each state gets the start and the width it occupies, so the name of a property and the value it
                // carries always agree, and no theme has to subtract one state from another.
                const style = this.dom.progress.style;
                if (changed('playedStart')) style.setProperty('--audio-player-progress-played-start', next.playedStart);
                if (changed('playedWidth')) style.setProperty('--audio-player-progress-played-width', next.playedWidth);
                if (changed('downloadedStart')) style.setProperty('--audio-player-progress-downloaded-start', next.downloadedStart);
                if (changed('downloadedWidth')) style.setProperty('--audio-player-progress-downloaded-width', next.downloadedWidth);
                if (changed('undownloadedStart')) style.setProperty('--audio-player-progress-undownloaded-start', next.undownloadedStart);
                if (changed('undownloadedWidth')) style.setProperty('--audio-player-progress-undownloaded-width', next.undownloadedWidth);
                if (changed('buffering')) this.dom.progress.classList.toggle('is-buffering', next.buffering);
                if (changed('state')) this.dom.progress.dataset.audioPlayerProgressState = next.state;
                if (!previous) this.dom.progress.setAttribute('aria-valuemin', '0');
                if (changed('maximum')) this.dom.progress.setAttribute('aria-valuemax', next.maximum);
                if (changed('value')) this.dom.progress.setAttribute('aria-valuenow', next.value);
                if (changed('valueText')) this.dom.progress.setAttribute('aria-valuetext', next.valueText);
                if (changed('ariaBusy')) this.dom.progress.setAttribute('aria-busy', next.ariaBusy);
                // The seek label comes from the configuration, so a themed bar that renders no label of its own still
                // gets one; the fallback markup already carries it, and setting an identical value is skipped.
                if (next.seekLabel && changed('seekLabel'))
                    this.dom.progress.setAttribute('aria-label', next.seekLabel);
            }
        }

        /**
         * Renders the play or pause affordance of the transport control, and its accessible label.
         */
        renderTransportState() {
            const isPlaying = this.state === LuminaAudioPlayer.PlayerState.PLAYING;
            const strings = this.getStrings();
            if (this.dom.playPause) {
                this.dom.playPause.classList.toggle('is-playing', isPlaying);
                const label = isPlaying ? strings.pause : strings.play;
                if (label)
                    this.dom.playPause.setAttribute('aria-label', label);
            }
            // The repeat and skip controls share the transport state, so their labels and pressed state move with it.
            this.renderQueueControls();
        }

        /**
         * Renders the queue controls: the repeat toggle and the skip controls.
         *
         * The repeat control exposes 'aria-pressed', the 'is-active' and 'is-one' classes, and the current mode as
         * 'data-audio-player-repeat-mode' ('off', 'all' or 'one'). The labels come from the localized strings of the
         * configuration, so a theme that renders the controls with its own text still gets the correct accessible name.
         */
        renderQueueControls() {
            const strings = this.getStrings();
            // The previous action normally has a meaning while the queue has an entry: it either walks back or restarts the
            // current track. A failed entry is not restarted, so after an error the control is only enabled when there is a real previous entry to recover to.
            if (this.dom.previous) {
                const hasPrevious = this.queue.entries.length > 0
                    && (this.state !== LuminaAudioPlayer.PlayerState.ERROR || this.queue.getPreviousEntry() !== null);
                this.dom.previous.disabled = !hasPrevious;
                this.dom.previous.classList.toggle('is-disabled', this.dom.previous.disabled);
            }
            if (this.dom.next) {
                const repeatMode = this.queue.getRepeatMode();
                const isRepeatOne = repeatMode === LuminaAudioPlayer.QueueRepeatMode.ONE;
                // Repeat one makes the continuation a no-op, so a continuation at the end of the queue must not advertise a next action that cannot start.
                const canContinue = !isRepeatOne
                    && this.queue.isContinuationEnabled()
                    && this.isQueueContinuationAllowed()
                    && this.queue.isContinuationExhausted() !== true;
                const hasNext = this.queue.entries.length > 0
                    && (this.queue.currentIndex + 1 < this.queue.entries.length
                        || repeatMode === LuminaAudioPlayer.QueueRepeatMode.ALL
                        || canContinue
                        || (this.queue.isShuffleEnabled() && this.queue.isShuffleSourceExhausted() !== true));
                this.dom.next.disabled = !hasNext;
                this.dom.next.classList.toggle('is-disabled', this.dom.next.disabled);
            }
            if (this.dom.repeat) {
                const mode = this.queue.getRepeatMode();
                const labels = {
                    off: strings.repeatOff,
                    all: strings.repeatAll,
                    one: strings.repeatOne
                };
                this.dom.repeat.dataset.audioPlayerRepeatMode = mode;
                this.dom.repeat.setAttribute('aria-pressed', mode === LuminaAudioPlayer.QueueRepeatMode.OFF ? 'false' : 'true');
                this.dom.repeat.classList.toggle('is-active', mode !== LuminaAudioPlayer.QueueRepeatMode.OFF);
                this.dom.repeat.classList.toggle('is-one', mode === LuminaAudioPlayer.QueueRepeatMode.ONE);
                const label = labels[mode] || strings.repeat;
                if (label)
                    this.dom.repeat.setAttribute('aria-label', label);
            }
        }

        /**
         * Renders the volume control and the mute toggle: the fill of the volume bar, and the accessible value and
         * label of both controls. The volume shown is the volume the user set, even while the output is muted, and the
         * muted state is exposed both as the 'aria-pressed' state of the toggle and as the 'is-muted' class.
         *
         * The fill is published as the '--audio-player-volume-start' and '--audio-player-volume-width' custom properties
         * on the control, which the stylesheet consumes, so a theme restyles or reorients the bar the same way it does
         * the progress bar; no inline geometry is written.
         */
        renderVolume() {
            const strings = this.getStrings();
            const levelRatio = clamp(this.volume, 0, 1);
            if (this.dom.volume) {
                this.dom.volume.style.setProperty('--audio-player-volume-start', '0%');
                this.dom.volume.style.setProperty('--audio-player-volume-width', `${levelRatio * 100}%`);
                this.dom.volume.setAttribute('aria-valuemin', '0');
                this.dom.volume.setAttribute('aria-valuemax', '100');
                this.dom.volume.setAttribute('aria-valuenow', String(Math.round(levelRatio * 100)));
                if (strings.volume)
                    this.dom.volume.setAttribute('aria-label', strings.volume);
                this.dom.volume.classList.toggle('is-muted', this.muted);
            }
            if (this.dom.mute) {
                this.dom.mute.setAttribute('aria-pressed', this.muted ? 'true' : 'false');
                this.dom.mute.classList.toggle('is-muted', this.muted);
                const label = this.muted ? strings.unmute : strings.mute;
                if (label)
                    this.dom.mute.setAttribute('aria-label', label);
            }
        }

        /**
         * Gets the localized strings of the bar, emitted with the global scripts configuration.
         * @returns {Object} The strings, keyed by name, or an empty object.
         */
        getStrings() {
            const configuration = this.getConfiguration();
            return configuration && configuration.strings ? configuration.strings : {};
        }

        /**
         * Scrolls the expanded queue so the current entry is the first visible item. The queue can be vertical or
         * horizontal, depending on the theme layout, so the browser aligns it on both axes rather than this code assuming
         * a vertical list. A shuffle queue can hold many more tracks than the panel can show, so without this the current
         * track would drift off screen as playback advances.
         */
        scrollQueueToCurrentEntry() {
            if (!this.dom.queue || !this.isExpanded() || this.currentIndex < 0)
                return;
            const item = this.dom.queue.children[this.currentIndex];
            if (!item)
                return;
            if (typeof item.scrollIntoView === 'function') {
                item.scrollIntoView({ block: 'start', inline: 'start', behavior: 'auto' });
                return;
            }
            // A host without scrollIntoView keeps the vertical fallback of the application layout.
            if (typeof item.getBoundingClientRect !== 'function' || typeof this.dom.queue.getBoundingClientRect !== 'function')
                return;
            const queueBounds = this.dom.queue.getBoundingClientRect();
            const itemBounds = item.getBoundingClientRect();
            this.dom.queue.scrollTop += itemBounds.top - queueBounds.top;
        }

        /**
         * Renders the ordered queue in the expanded bar. The current queue is always shown, including a single track, so
         * the expanded bar always lists what is playing now and what will play next.
         *
         * Each item is rendered from the 'shared/audio-player/queue-item' sub template of the active theme when it
         * provides one, so that the theme owns the structure of a queue item, and the built-in markup is used otherwise.
         */
        renderQueue() {
            if (!this.dom.queue || !this.isExpanded())
                return;
            const items = this.queue.entries;
            // The items are built into a detached fragment first, so a large context produces one insertion and one layout
            // pass instead of one per item. A host without a fragment falls back to the direct append path.
            const fragment = typeof document.createDocumentFragment === 'function' ? document.createDocumentFragment() : null;
            const target = fragment || this.dom.queue;
            const previousScrollTop = this.dom.queue.scrollTop;
            if (!fragment)
                this.dom.queue.innerHTML = '';
            for (let index = 0; index < items.length; index++) {
                const entry = this.renderQueueItem(items[index], index, items.length);
                if (entry)
                    target.appendChild(entry);
            }
            if (fragment) {
                this.dom.queue.innerHTML = '';
                this.dom.queue.appendChild(fragment);
            }
            // Rebuilding the list must not throw the panel back to the top, especially when a trim renumbered it.
            this.dom.queue.scrollTop = previousScrollTop;
            this.renderedQueueOrder = items.map((entry) => entry.entryId);
        }

        /**
         * Appends the entries a batch added to the rendered queue, without rebuilding the items that were already rendered.
         *
         * It is used while the expanded bar is open and a continuation or a shuffle batch appends entries at the end. When the rendered list is not exactly the entries before the new tail, because the cursor was trimmed, the
         * bar was collapsed, or a theme changed the markup, it falls back to a full render.
         * @param {number} firstNewIndex - The index of the first entry the batch appended.
         */
        appendQueueTail(firstNewIndex) {
            if (!this.dom.queue || !this.isExpanded())
                return;
            const total = this.queue.entries.length;
            if (!Number.isInteger(firstNewIndex) || firstNewIndex < 0) {
                this.renderQueue();
                return;
            }
            const existingCount = this.renderedQueueOrder.length;
            const children = this.dom.queue.children;
            if (total <= 1) {
                this.dom.queue.innerHTML = '';
                this.renderedQueueOrder = [];
                return;
            }
            // The tail can only be appended when the rendered children are exactly the entries before it. A changed order,
            // a trimmed cursor or a list that was never rendered needs the full render.
            if (existingCount === 0 || existingCount !== firstNewIndex || !children || children.length !== existingCount) {
                this.renderQueue();
                return;
            }
            // The item that was last before the batch is no longer last, and a theme may style the last item differently, so it
            // is re-rendered once with the new total before the tail is appended.
            const previousLastIndex = existingCount - 1;
            if (previousLastIndex >= 0) {
                const updatedLast = this.renderQueueItem(this.queue.entries[previousLastIndex], previousLastIndex, total);
                if (updatedLast && children && children[previousLastIndex])
                    this.dom.queue.replaceChild(updatedLast, children[previousLastIndex]);
            }
            for (let index = firstNewIndex; index < total; index++) {
                const item = this.renderQueueItem(this.queue.entries[index], index, total);
                if (item)
                    this.dom.queue.appendChild(item);
            }
            this.renderedQueueOrder = this.queue.entries.map((entry) => entry.entryId);
        }

        /**
         * Updates only the queue items whose current or played state changed after the cursor moved, so a gapless handoff
         * of a large play context does not rebuild the whole list at the transition. It falls back to a full render when
         * the DOM does not match the queue, for example before the first render or in a host that cannot replace a child.
         * @param {number} previousIndex - The zero based queue position that was current before the advance.
         */
        updateQueueSelection(previousIndex) {
            if (!this.dom.queue)
                return;
            const total = this.queue.entries.length;
            const children = this.dom.queue.children;
            // A changed order means the rendered child at an index no longer describes the entry at that index, so the
            // list is rebuilt instead of updating the wrong items. The same fallback covers a list that was never rendered.
            const hasSameOrder = this.renderedQueueOrder.length === total
                && this.renderedQueueOrder.every((entryId, index) => entryId === this.queue.entries[index].entryId);
            if (total <= 1 || !children || children.length !== total || !hasSameOrder
                || typeof this.dom.queue.replaceChild !== 'function') {
                this.renderQueue();
                return;
            }
            const from = Math.min(previousIndex, this.currentIndex);
            const to = Math.max(previousIndex, this.currentIndex);
            if (!Number.isInteger(from) || from < 0 || to >= total) {
                this.renderQueue();
                return;
            }
            // isPlayed flips for every position between the old and the new cursor, inclusive, and isCurrent flips at
            // both ends. Only the two ends need a template render; the items in between only change their played class, so
            // a click far from the current entry does not parse every item template on the way.
            for (let index = from; index <= to; index++) {
                if (index !== from && index !== to) {
                    const child = children[index];
                    if (child) {
                        child.classList.toggle('is-played', index < this.currentIndex);
                        child.classList.remove('is-current');
                    }
                    continue;
                }
                const item = this.renderQueueItem(this.queue.entries[index], index, total);
                if (item)
                    this.dom.queue.replaceChild(item, children[index]);
            }
        }

        /**
         * Renders one queue item, from the theme sub template when the theme provides one and from the built-in markup
         * otherwise. The rendered root always carries the 'data-audio-player-queue-entry-id' attribute, whatever the theme
         * markup is, so the queue list can start the entry the user clicked without the theme repeating the identity.
         * @param {Object} entry - The queue entry, with an entryId, a descriptor and a source.
         * @param {number} index - The zero based position of the item in the queue.
         * @param {number} totalItems - The number of items in the queue.
         * @returns {HTMLElement|null} The rendered queue item, or null when the markup produced no element.
         */
        renderQueueItem(entry, index, totalItems) {
            const item = entry.descriptor;
            // The model exposes only what a queue item can show, so a theme template never reaches into the player internals.
            const viewModel = {
                entryId: entry.entryId,
                index: index + 1,
                trackId: item.trackId,
                albumId: item.albumId,
                artistId: item.artistId,
                libraryId: item.libraryId,
                title: item.title || '',
                duration: formatDuration(item.durationInSeconds),
                durationSeconds: item.durationInSeconds,
                coverArtUrl: item.coverArtUrl || MUSIC_PLACEHOLDER_URL,
                isCurrent: index === this.currentIndex,
                isPlayed: index < this.currentIndex,
                isLast: index === totalItems - 1,
                isContinuation: entry.source === 'continuation',
                isShuffled: entry.source === 'shuffle'
            };
            let rendered = null;
            if (this.subTemplates.queueItem) {
                try {
                    const container = document.createElement('div');
                    container.innerHTML = window.renderThemeTemplate(this.subTemplates.queueItem, viewModel);
                    rendered = container.firstElementChild;
                } catch (error) {
                    // A broken theme sub template falls through to the built-in markup, so the queue never disappears.
                    console.error('Audio player: the theme queue item template could not be rendered.', error);
                }
            }
            if (!rendered)
                rendered = this.buildDefaultQueueItem(viewModel);
            rendered.setAttribute('data-audio-player-queue-entry-id', entry.entryId);
            // The player writes the state classes on the item root itself, so a theme queue item template keeps the current and played distinction even when its markup omits the state classes.
            rendered.classList.toggle('is-current', viewModel.isCurrent);
            rendered.classList.toggle('is-played', viewModel.isPlayed);
            rendered.classList.toggle('is-continuation', viewModel.isContinuation);
            rendered.classList.toggle('is-shuffled', viewModel.isShuffled);
            // The item is reachable by keyboard, so a user who cannot point can still start an entry. A control the theme rendered inside the item keeps its own focus and keyboard behavior.
            if (!rendered.hasAttribute('tabindex'))
                rendered.setAttribute('tabindex', '0');
            return rendered;
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
            if (viewModel.isPlayed)
                entry.classList.add('is-played');
            if (viewModel.isContinuation)
                entry.classList.add('is-continuation');
            if (viewModel.isShuffled)
                entry.classList.add('is-shuffled');
            const cover = document.createElement('img');
            cover.className = 'audio-player-queue-item-cover';
            cover.src = viewModel.coverArtUrl;
            cover.alt = '';
            cover.loading = 'lazy';
            const title = document.createElement('span');
            title.className = 'audio-player-queue-item-title';
            title.textContent = viewModel.title;
            const duration = document.createElement('span');
            duration.className = 'audio-player-queue-item-duration';
            duration.textContent = viewModel.duration;
            entry.appendChild(cover);
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
