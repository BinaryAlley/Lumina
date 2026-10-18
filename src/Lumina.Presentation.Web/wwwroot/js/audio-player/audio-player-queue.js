/**
 * Queue model, play context resolution and client storage of the audio player.
 *
 * The player owns exactly one queue. This module defines that queue as a value object ({@link LuminaAudioPlayer.AudioQueue}),
 * the service that builds a queue from a play context through the web endpoints of the music pages
 * ({@link LuminaAudioPlayer.QueueContextService}), and the sampler that draws randomized tracks for a shuffle scope
 * ({@link LuminaAudioPlayer.ShuffleSource}).
 *
 * Play contexts and the web endpoints used for each
 * --------------------------------------------------
 * The pages hand the player a play descriptor, per LuminaAudioPlayer.createPlayDescriptor. A descriptor that carries an
 * explicit queue is used as is: the page already knows the ordered tracks of its context. A descriptor that names only
 * a context is resolved here:
 *   - Album context (libraryId, artistId, albumId, without a trackId; optionally with a queue):
 *     WebRoutes.Music.GET_ALBUM_TRACKS ("api-get-tracks") returns the ordered tracks of the release. The endpoint orders
 *     them by disc number and then track number, which is the playback order of the release.
 *   - Artist context (libraryId, artistId, without an albumId or trackId; optionally with a queue):
 *     WebRoutes.Music.GET_ARTIST_ALBUMS ("api-get-albums") returns the releases of the artist, which this module orders like
 *     the artist page: primary release type group in releaseTypeOrder, then release year, title and Id. Then
 *     WebRoutes.Music.GET_ALBUM_TRACKS returns the ordered tracks of a release. The queue is every release in that order,
 *     with each release's tracks in disc and track number order. All releases are fetched before playback starts, with a
 *     small concurrency bound so a prolific artist does not turn into hundreds of sequential requests.
 *   - Track context (libraryId, trackId):
 *     The descriptor itself is the only item. A track page does not load the album or the discography around it.
 *
 * The autoplay continuation at the end of an album or an artist uses the same endpoints:
 *   - After an album, the remaining releases of the artist, in the release order this module applies (primary release
 *     type group, then release year, title and Id). When those are exhausted, the next artists of the library, in the
 *     artist order of GET_ARTISTS ("api-get-artists"), one artist at a time, each with all of its releases and tracks.
 *   - After an artist, the next artists of the library, one at a time.
 * A continuation batch is never built for a track context or for an explicit queue, and it is dropped as soon as the user
 * queues an explicit play next item.
 *
 * Shuffle scopes
 * --------------
 * Shuffle is offered per release, per artist and per library. It does not materialize a whole category: it appends a
 * batch of {@link SHUFFLE_BATCH_SIZE} randomized tracks and, while the queue approaches the end of that batch, appends
 * another batch of the same category. The release and artist scopes only load the lightweight release metadata of the
 * category and prefetch the track lists of a small, bounded number of releases in parallel, never the whole category.
 * The library scope only loads the lightweight artist metadata and the releases of one artist at a time, so choosing
 * shuffle on an entire library never puts every track of the library into the queue and never fetches every track of the
 * library at once.
 *
 * Lifetime
 * --------
 * The queue lives only in memory for the current page. A full page load creates a new player with an empty queue and the
 * bar hidden. An in-place navigation does not recreate the player, so the queue survives those swaps.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The repeat modes of the queue.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.QueueRepeatMode = Object.freeze({
        // Playback stops at the end of the queue.
        OFF: 'off',
        // Playback wraps to the first entry at the end of the queue.
        ALL: 'all',
        // The current entry is played again when it ends.
        ONE: 'one'
    });

    /**
     * The categories a shuffle can draw from.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.QueueShuffleScope = Object.freeze({
        // The tracks of the release that is playing.
        RELEASE: 'release',
        // All tracks of all releases of the artist that is playing.
        ARTIST: 'artist',
        // All tracks of all releases of all artists of the library.
        LIBRARY: 'library'
    });

    /**
     * The kinds of play context a queue can belong to.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayContextKind = Object.freeze({
        ALBUM: 'album',
        ARTIST: 'artist',
        TRACK: 'track',
        QUEUE: 'queue'
    });

    /**
     * The number of randomized tracks a shuffle appends at a time, so that a large category is never materialized as one
     * queue.
     * @readonly
     * @type {number}
     */
    const SHUFFLE_BATCH_SIZE = 100;

    /**
     * The number of entries that may remain after the current one before the queue requests the next shuffle batch. It is
     * the look ahead that hides the fetch behind the last ten tracks of the previous batch.
     * @readonly
     * @type {number}
     */
    const SHUFFLE_LOOKAHEAD = 10;

    /**
     * The number of release track lists a shuffle batch fetches at the same time. A batch of 100 tracks spans about nine
     * releases, so resolving their track lists concurrently removes most of the sequential wait before the batch is ready.
     * @readonly
     * @type {number}
     */
    const SHUFFLE_PREFETCH_CONCURRENCY = 4;

    /**
     * The number of history entries the queue keeps, so a very long session cannot grow the queue without bound.
     * @readonly
     * @type {number}
     */
    const MAX_HISTORY_ENTRIES = 500;

    /**
     * The highest entry sequence the identity format accepts. It stays one below the largest safe integer, so the next
     * generated identity remains a safe integer.
     * @readonly
     * @type {number}
     */
    const MAX_ENTRY_SEQUENCE = Number.MAX_SAFE_INTEGER - 1;

    /**
     * The number of pages of a paginated endpoint the queue builder follows. It is a safety bound for a server that reports
     * an inconsistent page count, not a product limit.
     * @readonly
     * @type {number}
     */
    const MAX_PAGES = 1000;

    /**
     * The number of release track lists the queue builder requests at the same time when it builds a whole artist, so a
     * prolific artist does not turn into hundreds of sequential requests, while the server is not flooded either.
     * @readonly
     * @type {number}
     */
    const ALBUM_FETCH_CONCURRENCY = 4;

    /**
     * The maximum number of artist release lists kept by the queue context service. The cache is an LRU, so a long
     * library shuffle keeps the most recently used artists instead of every artist it ever visited.
     * @readonly
     * @type {number}
     */
    const RELEASE_CACHE_MAX_ENTRIES = 64;

    /**
     * The maximum number of library artist lists kept by the queue context service. A browser session normally touches a
     * handful of libraries, so a small LRU is enough while it still bounds a long session.
     * @readonly
     * @type {number}
     */
    const ARTIST_CACHE_MAX_ENTRIES = 8;

    /**
     * The order of the release type groups of an artist page, from the most common to the least common. It mirrors the
     * releaseTypeOrder of the artist page, because the queue plays the discography in the order the page displays it.
     * @readonly
     * @type {ReadonlyArray<string>}
     */
    const RELEASE_TYPE_ORDER = Object.freeze([
        'Album', 'Compilation', 'Ep', 'Live', 'Remix', 'Single', 'Soundtrack', 'Broadcast', 'AudioDrama', 'AudioBook',
        'Demo', 'DjMix', 'FieldRecording', 'Interview', 'Mixtape', 'SpokenWord', 'Other'
    ]);

    /**
     * Gets the primary release type group of a release row, falling back to Other when the endpoint carries no usable type.
     * @param {Object} release - The release row.
     * @returns {string} The primary release type.
     */
    function getPrimaryReleaseType(release) {
        const releaseTypes = release && release.releaseTypes;
        const primary = Array.isArray(releaseTypes) && releaseTypes.length > 0
            ? releaseTypes[0]
            : (typeof releaseTypes === 'string' ? releaseTypes : '');
        return typeof primary === 'string' && primary !== '' ? primary : 'Other';
    }

    /**
     * Orders release rows like the artist page: primary release type group first, then release year, title and Id.
     *
     * The release endpoint orders by album title, which is not the order an artist's discography is presented or played in.
     * The artist page groups releases by their first release type in its releaseTypeOrder and sorts each group by release
     * year. The queue applies the same order here, so an artist context and its autoplay continuation follow the page. A
     * release without a year sorts first within its group, and the title and Id keep the order deterministic for a year
     * shared by several releases.
     * @param {Object} left - The first release row.
     * @param {Object} right - The second release row.
     * @returns {number} The comparator result.
     */
    function compareReleaseRows(left, right) {
        const leftType = getPrimaryReleaseType(left);
        const rightType = getPrimaryReleaseType(right);
        const leftTypeIndex = RELEASE_TYPE_ORDER.indexOf(leftType);
        const rightTypeIndex = RELEASE_TYPE_ORDER.indexOf(rightType);
        const leftGroup = leftTypeIndex >= 0 ? leftTypeIndex : RELEASE_TYPE_ORDER.length;
        const rightGroup = rightTypeIndex >= 0 ? rightTypeIndex : RELEASE_TYPE_ORDER.length;
        if (leftGroup !== rightGroup)
            return leftGroup - rightGroup;
        const leftYear = Number.isFinite(Number(left && left.releaseYear)) ? Number(left.releaseYear) : 0;
        const rightYear = Number.isFinite(Number(right && right.releaseYear)) ? Number(right.releaseYear) : 0;
        if (leftYear !== rightYear)
            return leftYear - rightYear;
        const leftTitle = String((left && left.title) || '');
        const rightTitle = String((right && right.title) || '');
        if (leftTitle !== rightTitle)
            return leftTitle < rightTitle ? -1 : 1;
        const leftId = String((left && left.id) || '');
        const rightId = String((right && right.id) || '');
        return leftId === rightId ? 0 : (leftId < rightId ? -1 : 1);
    }

    /**
     * Shuffles a copy of a list with the Fisher-Yates algorithm.
     * @template T
     * @param {Array<T>} items - The items to shuffle.
     * @returns {Array<T>} A new, shuffled list; the input is never mutated.
     */
    function shuffleCopy(items) {
        const result = Array.isArray(items) ? [...items] : [];
        for (let index = result.length - 1; index > 0; index--) {
            const swapIndex = Math.floor(Math.random() * (index + 1));
            const value = result[index];
            result[index] = result[swapIndex];
            result[swapIndex] = value;
        }
        return result;
    }

    /**
     * Reads a value from an insertion ordered cache and marks it as the most recently used.
     * @param {Map<string, *>} cache - The cache to read.
     * @param {string} key - The cache key.
     * @returns {*|null} The cached value, or null when the key is absent.
     */
    function readCacheEntry(cache, key) {
        if (!cache.has(key))
            return null;
        const value = cache.get(key);
        cache.delete(key);
        cache.set(key, value);
        return value;
    }

    /**
     * Writes a value to an insertion ordered cache and evicts the least recently used entries above its bound.
     * @param {Map<string, *>} cache - The cache to write.
     * @param {string} key - The cache key.
     * @param {*} value - The value to cache.
     * @param {number} maximum - The maximum number of entries the cache may hold.
     */
    function writeCacheEntry(cache, key, value, maximum) {
        if (cache.has(key))
            cache.delete(key);
        cache.set(key, value);
        while (cache.size > maximum) {
            const oldest = cache.keys().next().value;
            if (oldest === undefined)
                break;
            cache.delete(oldest);
        }
    }

    /**
     * Gets the identity a track is deduplicated by inside a shuffled queue.
     * @param {Object} descriptor - The play descriptor of the track.
     * @returns {string} The track identity, or an empty string when the descriptor carries no track.
     */
    function getTrackIdentity(descriptor) {
        return descriptor && descriptor.trackId ? descriptor.trackId.toLowerCase() : '';
    }

    /**
     * Limits a list of promises to a maximum number of concurrent operations.
     * @template T, R
     * @param {Array<T>} items - The items to process.
     * @param {number} limit - The maximum number of operations that may run at the same time.
     * @param {Function} operation - The operation, receiving an item and returning a Promise.
     * @returns {Promise<Array<R>>} The results, in the order of the input.
     */
    async function mapWithConcurrencyAsync(items, limit, operation) {
        const results = new Array(items.length);
        let nextIndex = 0;
        const workers = [];
        const workerCount = Math.max(1, Math.min(limit, items.length));
        for (let worker = 0; worker < workerCount; worker++) {
            workers.push((async () => {
                while (nextIndex < items.length) {
                    const index = nextIndex;
                    nextIndex += 1;
                    results[index] = await operation(items[index], index);
                }
            })());
        }
        await Promise.all(workers);
        return results;
    }

    /**
     * Gets the origin the queue endpoints are resolved against. The page origin is used in a browser; a fixed local
     * origin keeps the URL checks usable in a host without a DOM.
     * @returns {string} The base origin.
     */
    function getBaseOrigin() {
        return typeof window !== 'undefined' && window.location && window.location.origin
            ? window.location.origin
            : 'http://localhost';
    }

    /**
     * Throws the typed error that stops a queue build which the session redirect already owns.
     * @returns {never} Never returns; it always throws.
     */
    function throwSessionExpired() {
        throw new LuminaAudioPlayer.PlayerError(
            LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED,
            'The session expired while the play context was loaded.',
            { reason: LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED });
    }

    /**
     * The queue of the player: ordered entries with an identity per occurrence, the cursor, the navigation history, the
     * repeat and shuffle modes, the play context, and the continuation state.
     *
     * Every traversal is derived from the entries and the cursor, so a queue can never describe two different tracks at
     * the same time. A queue entry keeps its identity across an advance, which is what lets a gapless handoff and a normal
     * continuation share the same commit.
     */
    class AudioQueue {
        /**
         * Creates an instance of AudioQueue.
         */
        constructor() {
            /**
             * The ordered entries of the queue, each { entryId, descriptor, source }. The source is one of 'context',
             * 'manual', 'queue', 'shuffle' or 'continuation'.
             * @type {Array<Object>}
             */
            this.entries = [];
            // The zero based position of the current entry, or -1 when the queue has no current entry.
            this.currentIndex = -1;
            // The identities of the entries that were current before the current one, most recent last, used by previous.
            this.history = [];
            this.repeatMode = LuminaAudioPlayer.QueueRepeatMode.OFF;
            // True while the queue is a shuffled selection, so the queue extension adds another shuffle batch instead of
            // the autoplay continuation.
            this.shuffle = false;
            this.shuffleScope = LuminaAudioPlayer.QueueShuffleScope.RELEASE;
            // The play context the queue was built from: { kind, libraryId, albumId, artistId, title, coverArtPath }.
            this.context = null;
            // True while the autoplay continuation of an album or artist may still append entries.
            this.continuationEnabled = false;
            // True once the autoplay continuation of the context reported that it has nothing left.
            this.continuationExhausted = false;
            // The queue state a shuffle replaced: { entries, currentEntryId, continuationEnabled, context }. It is
            // restored when shuffle is turned off.
            this.unshuffledState = null;
            // The sequence used by entry identities, so a new occurrence never reuses the identity of another one.
            this.entrySequence = 0;
            // The album and artist Ids this context has ever queued. They survive the trimming of played entries, so a
            // continuation after trimming never re-appends a release or an artist that was already played. They are reset
            // when the queue is replaced by a new context.
            this.queuedAlbumIds = new Set();
            this.queuedArtistIds = new Set();
            // True once an explicit play next discarded the autoplay continuation of the current context. The flag keeps a
            // late preferences response from re-enabling the continuation the user already discarded, until a new context
            // replaces the queue.
            this.continuationDiscarded = false;
            // True once the shuffle source of the current selection reported that its category has no more tracks.
            this.shuffleSourceExhausted = false;
        }

        /**
         * Creates the identity of a new queue entry. Every occurrence in the queue gets its own identity, so two entries
         * that repeat the same track are never confused.
         * @returns {string} The new entry Id.
         * @throws {LuminaAudioPlayer.PlayerError} When the safe identity space is exhausted.
         */
        createEntryId() {
            if (this.entrySequence >= MAX_ENTRY_SEQUENCE)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The queue entry identity space is exhausted.');
            this.entrySequence += 1;
            return `queue-entry-${this.entrySequence}`;
        }

        /**
         * Creates a queue entry and its identity.
         * @param {Object} descriptor - The play descriptor of the entry.
         * @param {string} [source='context'] - Where the entry came from: 'context', 'manual', 'queue', 'shuffle' or
         * 'continuation'.
         * @returns {Object} The queue entry.
         */
        createEntry(descriptor, source = 'context') {
            const entry = {
                entryId: this.createEntryId(),
                descriptor,
                source
            };
            // A manual play next item is listener chosen content, not part of the album or artist context. Recording it
            // would make the artist fill and the autoplay continuation skip a whole release or artist because one of its
            // tracks was queued explicitly.
            if (source !== 'manual')
                this.recordDescriptor(descriptor);
            return entry;
        }

        /**
         * Records the album and artist of a descriptor as queued by this context, so the autoplay continuation can skip
         * them even after the played entries themselves were trimmed.
         * @param {Object} descriptor - The play descriptor to record.
         */
        recordDescriptor(descriptor) {
            if (!descriptor)
                return;
            if (descriptor.albumId)
                this.queuedAlbumIds.add(String(descriptor.albumId).toLowerCase());
            if (descriptor.artistId)
                this.queuedArtistIds.add(String(descriptor.artistId).toLowerCase());
        }

        /**
         * Checks whether an album was queued by this context at any point, including before its entries were trimmed.
         * @param {*} albumId - The album Id to check.
         * @returns {boolean} True when the album was queued.
         */
        hasQueuedAlbum(albumId) {
            return this.queuedAlbumIds.has(String(albumId || '').toLowerCase());
        }

        /**
         * Checks whether an artist was queued by this context at any point, including before its entries were trimmed.
         * @param {*} artistId - The artist Id to check.
         * @returns {boolean} True when the artist was queued.
         */
        hasQueuedArtist(artistId) {
            return this.queuedArtistIds.has(String(artistId || '').toLowerCase());
        }

        /**
         * Replaces the whole queue with a new ordered list of descriptors. Replacing the entries always leaves the shuffled
         * selection and drops its snapshot, so the queue can never remain shuffled without the state that turning shuffle
         * off restores.
         * @param {Array<Object>} descriptors - The ordered play descriptors of the queue.
         * @param {Object} [options] - The queue options.
         * @param {number} [options.startIndex=0] - The index to make current, clamped into the queue.
         * @param {string} [options.startTrackId] - The track to make current, used when the list is not ordered by the start descriptor.
         * @param {Object|null} [options.context=null] - The play context of the queue.
         * @param {boolean} [options.continuationEnabled=false] - True while the autoplay continuation of the context is available.
         * @param {string} [options.source='context'] - The source of the created entries.
         * @returns {Object|null} The new current entry, or null when the queue is empty.
         */
        setQueue(descriptors, options = {}) {
            this.queuedAlbumIds = new Set();
            this.queuedArtistIds = new Set();
            this.continuationDiscarded = false;
            this.entries = (Array.isArray(descriptors) ? descriptors : []).map((descriptor) => this.createEntry(descriptor, options.source || 'context'));
            this.history = [];
            this.context = options.context || null;
            this.continuationEnabled = options.continuationEnabled === true;
            this.continuationExhausted = false;
            // Replacing the entries always leaves the shuffled selection: a caller that only wants to change the tracks must
            // not keep shuffle enabled without the snapshot that turning it off restores.
            this.shuffle = false;
            this.shuffleSourceExhausted = false;
            this.unshuffledState = null;
            this.currentIndex = this.resolveStartIndex(options.startIndex || 0, options.startTrackId);
            return this.getCurrentEntry();
        }

        /**
         * Replaces the whole queue with a new play context, resetting the shuffle selection that belonged to the previous
         * context. It is the single entry point for a play now, so the controller never has to reset the queue flags itself.
         * @param {Array<Object>} descriptors - The ordered play descriptors of the queue.
         * @param {Object} [options] - The queue options, per {@link AudioQueue#setQueue}.
         * @returns {Object|null} The new current entry, or null when the queue is empty.
         */
        replaceContext(descriptors, options = {}) {
            return this.setQueue(descriptors, options);
        }

        /**
         * Sets whether the autoplay continuation of the context may append entries. Enabling it clears a previous exhaustion,
         * so a preference that arrives after the queue was built can still let the continuation run.
         * @param {boolean} enabled - True when the continuation is available.
         * @returns {boolean} True when the value changed.
         */
        setContinuationEnabled(enabled) {
            // An explicit play next discarded the continuation of this context. A preferences response that arrives later
            // must not reintroduce it; only a new context resets the discard.
            const next = enabled === true && this.continuationDiscarded !== true;
            if (this.continuationEnabled === next)
                return false;
            this.continuationEnabled = next;
            if (next)
                this.continuationExhausted = false;
            return true;
        }

        /**
         * Marks the autoplay continuation of the context as having no more content.
         */
        markContinuationExhausted() {
            this.continuationExhausted = true;
        }

        /**
         * Marks the shuffle source as having no more tracks.
         */
        markShuffleSourceExhausted() {
            this.shuffleSourceExhausted = true;
        }

        /**
         * Trims the entries that were played long ago, so a queue that the player keeps for the whole session cannot grow
         * without bound. The last {@link MAX_HISTORY_ENTRIES} played entries are kept, which is exactly the window the
         * navigation history can address, and the history is pruned to the entries that still exist.
         */
        trimPlayedEntries() {
            let removed = 0;
            if (this.currentIndex > MAX_HISTORY_ENTRIES) {
                removed = this.currentIndex - MAX_HISTORY_ENTRIES;
                this.entries.splice(0, removed);
                this.currentIndex -= removed;
            }
            // The set of remaining identities is only needed when entries were actually removed. A regular cursor move
            // must not allocate it.
            if (removed > 0)
                this.pruneHistory();
            return removed;
        }

        /**
         * Drops history entries that no longer address an entry of the queue.
         */
        pruneHistory() {
            if (this.history.length === 0)
                return;
            const identities = new Set(this.entries.map((entry) => entry.entryId));
            this.history = this.history.filter((entryId) => identities.has(entryId));
        }

        /**
         * Trims the played part of the queue state a shuffle replaced, so the snapshot does not keep the whole played history
         * of the original context beside the shuffled queue.
         */
        trimUnshuffledState() {
            const state = this.unshuffledState;
            if (!state || !Array.isArray(state.entries) || state.entries.length === 0)
                return;
            const index = state.currentEntryId
                ? state.entries.findIndex((entry) => entry.entryId === state.currentEntryId)
                : -1;
            if (index <= MAX_HISTORY_ENTRIES)
                return;
            state.entries.splice(0, index - MAX_HISTORY_ENTRIES);
        }

        /**
         * Resolves the index a queue starts at, favoring an explicit index and falling back to a track identity and then
         * to the first entry.
         * @param {number} startIndex - The preferred index.
         * @param {string} [startTrackId] - The track to start at.
         * @returns {number} The resolved index, or -1 when the queue is empty.
         */
        resolveStartIndex(startIndex, startTrackId) {
            if (this.entries.length === 0)
                return -1;
            const trackId = LuminaAudioPlayer.toIdString(startTrackId);
            if (trackId) {
                const trackIndex = this.entries.findIndex((entry) => entry.descriptor.trackId
                    && entry.descriptor.trackId.toLowerCase() === trackId.toLowerCase());
                if (trackIndex >= 0)
                    return trackIndex;
            }
            const index = Number.isInteger(startIndex) ? startIndex : 0;
            return Math.max(0, Math.min(index, this.entries.length - 1));
        }

        /**
         * Gets the current entry.
         * @returns {Object|null} The current entry, or null when the queue has none.
         */
        getCurrentEntry() {
            return this.currentIndex >= 0 && this.currentIndex < this.entries.length
                ? this.entries[this.currentIndex]
                : null;
        }

        /**
         * Gets the current descriptor.
         * @returns {Object|null} The current play descriptor, or null.
         */
        getCurrentDescriptor() {
            const entry = this.getCurrentEntry();
            return entry ? entry.descriptor : null;
        }

        /**
         * Finds an entry by its identity.
         * @param {string} entryId - The identity of the entry.
         * @returns {{ entry: Object, index: number }|null} The entry and its index, or null.
         */
        findEntry(entryId) {
            if (typeof entryId !== 'string')
                return null;
            const index = this.entries.findIndex((entry) => entry.entryId === entryId);
            return index >= 0 ? { entry: this.entries[index], index } : null;
        }

        /**
         * Gets the entry that follows the current one, without moving the cursor. Repeat one is not applied here: the
         * player owns the end of a track, and a gapless handoff of the same entry would be the same entry again. Repeat all
         * and shuffle both work through the same ordered entries.
         * @param {Object} [options] - The lookup options.
         * @param {boolean} [options.allowWrap=true] - True to wrap to the first entry at the end with repeat all.
         * @returns {Object|null} The following entry, or null at the end of the queue.
         */
        getNextEntry(options = {}) {
            if (this.entries.length === 0 || this.currentIndex < 0)
                return this.entries.length > 0 ? this.entries[0] : null;
            if (this.currentIndex + 1 < this.entries.length)
                return this.entries[this.currentIndex + 1];
            if (options.allowWrap !== false && this.repeatMode === LuminaAudioPlayer.QueueRepeatMode.ALL)
                return this.entries[0];
            return null;
        }

        /**
         * Gets the entry the previous action moves to, without consuming the navigation history. It first walks the
         * navigation history backwards, then the ordered entries, and finally wraps to the last entry with repeat all. The
         * player consumes the history entry only when the transition is committed, so a refused start does not lose it.
         * @returns {{ entry: Object, fromHistory: boolean }|null} The previous entry, or null when there is none.
         */
        getPreviousEntry() {
            if (this.entries.length === 0)
                return null;
            for (let historyIndex = this.history.length - 1; historyIndex >= 0; historyIndex--) {
                const found = this.findEntry(this.history[historyIndex]);
                if (found && found.index !== this.currentIndex)
                    return { entry: found.entry, fromHistory: true };
            }
            if (this.currentIndex > 0)
                return { entry: this.entries[this.currentIndex - 1], fromHistory: false };
            if (this.currentIndex < 0)
                return { entry: this.entries[0], fromHistory: false };
            if (this.repeatMode === LuminaAudioPlayer.QueueRepeatMode.ALL && this.entries.length > 1)
                return { entry: this.entries[this.entries.length - 1], fromHistory: false };
            return null;
        }

        /**
         * Removes one occurrence of an entry from the navigation history, used when a previous action committed to it.
         * @param {string} entryId - The identity to remove.
         */
        removeHistoryEntry(entryId) {
            const index = this.history.lastIndexOf(entryId);
            if (index >= 0)
                this.history.splice(index, 1);
        }

        /**
         * Moves the cursor to an entry, without starting playback. The entry is found by identity rather than by position,
         * so a linear advance, a wrap and a reordered queue share one commit.
         * @param {string} entryId - The identity of the entry to make current.
         * @param {Object} [options] - The commit options.
         * @param {boolean} [options.recordHistory=true] - True to remember the entry that was current as history.
         * @param {string|null} [options.consumeHistoryId=null] - A history entry to remove once the commit succeeded.
         * @returns {{ entry: Object, previousIndex: number, changed: boolean, removedCount: number }|null} The committed
         * entry, the removed played entries, or null when the identity is not part of the queue.
         */
        commit(entryId, options = {}) {
            const found = this.findEntry(entryId);
            if (!found)
                return null;
            const previousIndex = this.currentIndex;
            if (found.index === this.currentIndex)
                return { entry: found.entry, previousIndex, changed: false, removedCount: 0 };
            const previous = this.getCurrentEntry();
            if (previous && options.recordHistory !== false)
                this.pushHistory(previous.entryId);
            this.currentIndex = found.index;
            // The history entry is only consumed by a commit that actually moved the cursor, so a refused start never
            // loses the previous entry.
            if (options.consumeHistoryId)
                this.removeHistoryEntry(options.consumeHistoryId);
            const removed = this.trimPlayedEntries();
            // The controller updates the rendered list by index, so the previous index is shifted by the same amount as the
            // queue when old played entries were trimmed.
            return { entry: found.entry, previousIndex: removed > 0 ? Math.max(0, previousIndex - removed) : previousIndex, changed: true, removedCount: removed };
        }

        /**
         * Remember a played entry in the navigation history.
         * @param {string} entryId - The identity of the entry that was current.
         */
        pushHistory(entryId) {
            if (!entryId)
                return;
            if (this.history.length > 0 && this.history[this.history.length - 1] === entryId)
                return;
            this.history.push(entryId);
            if (this.history.length > MAX_HISTORY_ENTRIES)
                this.history.splice(0, this.history.length - MAX_HISTORY_ENTRIES);
        }

        /**
         * Inserts descriptors after the current entry, as an explicit play next action.
         * @param {Array<Object>} descriptors - The descriptors to insert, in order.
         * @returns {number} The number of entries inserted.
         */
        insertNext(descriptors) {
            const items = Array.isArray(descriptors) ? descriptors : [];
            if (items.length === 0)
                return 0;
            const insertAt = this.currentIndex >= 0 ? this.currentIndex + 1 : this.entries.length;
            const entries = items.map((descriptor) => this.createEntry(descriptor, 'manual'));
            this.entries.splice(insertAt, 0, ...entries);
            return entries.length;
        }

        /**
         * Appends descriptors at the end of the queue.
         * @param {Array<Object>} descriptors - The descriptors to append.
         * @param {string} [source='queue'] - The source of the appended entries.
         * @returns {number} The number of entries appended.
         */
        append(descriptors, source = 'queue') {
            const items = Array.isArray(descriptors) ? descriptors : [];
            if (items.length === 0)
                return 0;
            const entries = items.map((descriptor) => this.createEntry(descriptor, source));
            this.entries.push(...entries);
            return entries.length;
        }

        /**
         * Removes the continuation entries that have not played yet, used when the user queues an explicit play next item.
         * The current entry is kept even when it came from the continuation, because it is what is playing.
         * @returns {number} The number of entries removed.
         */
        removePendingContinuation() {
            let removed = 0;
            for (let index = this.entries.length - 1; index > this.currentIndex; index--) {
                if (this.entries[index].source === 'continuation') {
                    this.entries.splice(index, 1);
                    removed += 1;
                }
            }
            this.continuationEnabled = false;
            this.continuationExhausted = true;
            this.continuationDiscarded = true;
            // The queue a shuffle replaced must carry the same discard, otherwise turning shuffle off would restore the
            // autoplay continuation that the explicit play next already discarded.
            if (this.unshuffledState) {
                this.unshuffledState.continuationEnabled = false;
                this.unshuffledState.continuationExhausted = true;
                this.unshuffledState.entries = this.unshuffledState.entries.filter((entry) => entry.source !== 'continuation');
            }
            return removed;
        }

        /**
         * Replaces the entries a shuffle draws from with a shuffled selection, keeping the current entry first.
         * @param {Array<Object>} descriptors - The randomized descriptors to queue after the current entry.
         * @param {string} [scope] - The shuffle scope the selection was drawn from.
         * @param {boolean} [isSourceExhausted=false] - True when the shuffle source reported that its category has no more tracks.
         * @returns {Object|null} The new current entry.
         */
        applyShuffle(descriptors, scope, isSourceExhausted = false) {
            const current = this.getCurrentEntry();
            const selection = Array.isArray(descriptors) ? descriptors : [];
            if (!this.unshuffledState) {
                this.unshuffledState = {
                    entries: this.entries.map((entry) => ({ entryId: entry.entryId, descriptor: entry.descriptor, source: entry.source })),
                    currentEntryId: current ? current.entryId : null,
                    continuationEnabled: this.continuationEnabled,
                    continuationExhausted: this.continuationExhausted,
                    context: this.context,
                    queuedAlbumIds: [...this.queuedAlbumIds],
                    queuedArtistIds: [...this.queuedArtistIds]
                };
            }
            const entries = [];
            if (current) {
                entries.push({ entryId: this.createEntryId(), descriptor: current.descriptor, source: 'queue' });
            }
            for (const descriptor of selection) {
                entries.push(this.createEntry(descriptor, 'shuffle'));
            }
            this.entries = entries;
            this.currentIndex = entries.length > 0 ? 0 : -1;
            this.history = [];
            this.shuffle = true;
            this.shuffleScope = scope || this.shuffleScope;
            this.trimUnshuffledState();
            // Shuffle expansion is the continuation of a shuffled queue, so the autoplay continuation is suspended while it
            // is on and restored when it is turned off.
            this.continuationEnabled = false;
            this.continuationExhausted = true;
            this.shuffleSourceExhausted = isSourceExhausted === true;
            return this.getCurrentEntry();
        }

        /**
         * Appends another shuffle batch to a shuffled queue.
         * @param {Array<Object>} descriptors - The randomized descriptors to append.
         * @param {boolean} [isSourceExhausted=false] - True when the shuffle source reported that its category has no more tracks.
         * @returns {number} The number of entries appended.
         */
        appendShuffleBatch(descriptors, isSourceExhausted = false) {
            const appended = this.append(descriptors, 'shuffle');
            if (isSourceExhausted)
                this.shuffleSourceExhausted = true;
            return appended;
        }

        /**
         * Leaves the shuffled selection and restores the queue the shuffle replaced, keeping the track that was playing and
         * the explicit play next entries that were queued while it was shuffled.
         * @returns {Object|null} The new current entry.
         */
        restoreUnshuffledQueue() {
            if (!this.unshuffledState) {
                this.shuffle = false;
                return this.getCurrentEntry();
            }
            const currentEntry = this.getCurrentEntry();
            const currentDescriptor = currentEntry ? currentEntry.descriptor : null;
            // Explicit play next items live only in the shuffled entries. They are pending content the user chose, so they
            // are merged back after the restored current entry instead of being discarded with the shuffled selection.
            const pendingManualEntries = this.entries
                .slice(this.currentIndex + 1)
                .filter((entry) => entry.source === 'manual')
                .map((entry) => ({ ...entry }));
            const snapshot = this.unshuffledState;
            this.entries = snapshot.entries.map((entry) => ({ ...entry }));
            this.continuationEnabled = snapshot.continuationEnabled === true;
            this.continuationExhausted = snapshot.continuationExhausted === true;
            this.context = snapshot.context || this.context;
            this.shuffle = false;
            this.shuffleSourceExhausted = true;
            // The dedupe sets of the shuffled selection are discarded with it; the context sets are the ones the fill and the
            // continuation use after shuffle is turned off.
            this.queuedAlbumIds = new Set(Array.isArray(snapshot.queuedAlbumIds) ? snapshot.queuedAlbumIds : []);
            this.queuedArtistIds = new Set(Array.isArray(snapshot.queuedArtistIds) ? snapshot.queuedArtistIds : []);
            this.unshuffledState = null;
            this.history = [];
            // Manual entries belong to the listener, not to the album or artist context, so they never enter the dedupe sets.
            for (const entry of this.entries) {
                if (entry.source !== 'manual')
                    this.recordDescriptor(entry.descriptor);
            }
            // Keep the track that was playing even when the shuffle had moved into a track of another release or artist.
            const snapshotIndex = snapshot.currentEntryId
                ? this.entries.findIndex((entry) => entry.entryId === snapshot.currentEntryId)
                : -1;
            const trackIndex = currentDescriptor && currentDescriptor.trackId
                ? this.entries.findIndex((entry) => getTrackIdentity(entry.descriptor) === getTrackIdentity(currentDescriptor))
                : -1;
            if (trackIndex >= 0) {
                this.currentIndex = trackIndex;
            } else if (currentDescriptor) {
                // The track that is playing is not part of the original snapshot, so it is kept in front of it instead of
                // silently replacing the playhead with the entry the snapshot started from.
                this.entries.unshift(this.createEntry(currentDescriptor, currentEntry && currentEntry.source === 'manual' ? 'manual' : 'queue'));
                this.currentIndex = 0;
            } else if (snapshotIndex >= 0) {
                this.currentIndex = snapshotIndex;
            } else {
                this.currentIndex = this.entries.length > 0 ? 0 : -1;
            }
            if (pendingManualEntries.length > 0) {
                const insertAt = this.currentIndex >= 0 ? this.currentIndex + 1 : this.entries.length;
                this.entries.splice(insertAt, 0, ...pendingManualEntries);
            }
            this.trimPlayedEntries();
            return this.getCurrentEntry();
        }

        /**
         * Checks whether the queue is a shuffled selection.
         * @returns {boolean} True when shuffle is enabled.
         */
        isShuffleEnabled() {
            return this.shuffle;
        }

        /**
         * Gets the shuffle scope of the current selection.
         * @returns {string} One of the {@link LuminaAudioPlayer.QueueShuffleScope} values.
         */
        getShuffleScope() {
            return this.shuffleScope;
        }

        /**
         * Checks whether the autoplay continuation of the context may append entries.
         * @returns {boolean} True when the continuation is available.
         */
        isContinuationEnabled() {
            return this.continuationEnabled;
        }

        /**
         * Checks whether the autoplay continuation reported that it has no more content.
         * @returns {boolean} True when the continuation is exhausted.
         */
        isContinuationExhausted() {
            return this.continuationExhausted;
        }

        /**
         * Checks whether an explicit play next discarded the autoplay continuation of the current context.
         * @returns {boolean} True when the continuation may not be re-enabled before a new context.
         */
        isContinuationDiscarded() {
            return this.continuationDiscarded;
        }

        /**
         * Checks whether the shuffle source reported that its category has no more tracks.
         * @returns {boolean} True when the shuffle source is exhausted.
         */
        isShuffleSourceExhausted() {
            return this.shuffleSourceExhausted;
        }

        /**
         * Gets the repeat mode.
         * @returns {string} One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         */
        getRepeatMode() {
            return this.repeatMode;
        }

        /**
         * Sets the repeat mode.
         * @param {string} mode - One of the {@link LuminaAudioPlayer.QueueRepeatMode} values.
         * @returns {boolean} True when the mode changed.
         */
        setRepeatMode(mode) {
            const modes = Object.values(LuminaAudioPlayer.QueueRepeatMode);
            if (!modes.includes(mode))
                return false;
            const changed = this.repeatMode !== mode;
            this.repeatMode = mode;
            return changed;
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
            const index = modes.indexOf(this.repeatMode);
            this.repeatMode = modes[(index + 1) % modes.length];
            return this.repeatMode;
        }

        /**
         * Gets the number of entries that remain after the current one.
         * @returns {number} The number of entries after the current one, never below zero.
         */
        getRemainingCount() {
            if (this.currentIndex < 0)
                return this.entries.length;
            return Math.max(0, this.entries.length - this.currentIndex - 1);
        }

        /**
         * Gets the identities of the tracks the queue already holds, so a shuffle batch never repeats one of them.
         * @returns {Set<string>} The lower case track identities of the queue.
         */
        getQueuedTrackIdentities() {
            const identities = new Set();
            for (const entry of this.entries) {
                const identity = getTrackIdentity(entry.descriptor);
                if (identity)
                    identities.add(identity);
            }
            return identities;
        }

        /**
         * Gets the playable state of the queue for a consumer.
         * @returns {Object} The queue state.
         */
        getState() {
            const current = this.getCurrentEntry();
            return {
                entries: this.entries.map((entry) => ({ entryId: entry.entryId, descriptor: entry.descriptor, source: entry.source })),
                currentIndex: this.currentIndex,
                currentEntryId: current ? current.entryId : null,
                history: [...this.history],
                repeatMode: this.repeatMode,
                isShuffled: this.shuffle,
                shuffleScope: this.shuffleScope,
                context: this.context ? { ...this.context } : null,
                isContinuationEnabled: this.continuationEnabled,
                isContinuationExhausted: this.continuationExhausted,
                isContinuationDiscarded: this.continuationDiscarded === true,
                isShuffleSourceExhausted: this.shuffleSourceExhausted === true,
                queuedAlbumIds: [...this.queuedAlbumIds],
                queuedArtistIds: [...this.queuedArtistIds]
            };
        }

        /**
         * Gets the compact queue state published to observers. It carries the cursor, the current entry and the queue
         * flags, not every entry, history identity or dedupe set, so a frequent state event is cheap to build and a
         * server side sync does not receive the entire queue as its wire payload.
         * @returns {Object} The compact queue state.
         */
        getStateSummary() {
            const current = this.getCurrentEntry();
            return {
                entryCount: this.entries.length,
                currentIndex: this.currentIndex,
                currentEntry: current
                    ? { entryId: current.entryId, descriptor: current.descriptor, source: current.source }
                    : null,
                historyLength: this.history.length,
                repeatMode: this.repeatMode,
                isShuffled: this.shuffle,
                shuffleScope: this.shuffleScope,
                context: this.context ? { ...this.context } : null,
                isContinuationEnabled: this.continuationEnabled,
                isContinuationExhausted: this.continuationExhausted,
                isContinuationDiscarded: this.continuationDiscarded === true,
                isShuffleSourceExhausted: this.shuffleSourceExhausted === true
            };
        }

    }

    /**
     * Builds the queue of the player from the play contexts, through the web endpoints of the music pages.
     *
     * The service is deliberately separate from the queue model: the model is synchronous and fully testable, while this
     * service owns the endpoint templates, pagination, Guid validation and descriptor construction.
     */
    class QueueContextService {
        /**
         * Creates an instance of QueueContextService.
         * @param {Object} options - The service options.
         * @param {Function} options.fetchJsonAsync - The JSON transport, in the shape of the player's fetchJsonAsync.
         * @param {Function} options.getConfiguration - A function returning the endpoints configuration of the player.
         * @param {Function} [options.getShouldIgnoreThePrefix] - A function returning the user's alpha picker prefix preference,
         * so the artist order the queue uses matches the order the artist page shows.
         */
        constructor(options) {
            const settings = options || {};
            this.fetchJsonAsync = settings.fetchJsonAsync;
            this.getConfiguration = settings.getConfiguration;
            this.getShouldIgnoreThePrefix = settings.getShouldIgnoreThePrefix || (() => false);
            // The ordered release list of an artist is reused by the artist context and by the continuation, so a long
            // discography does not re-fetch the same ordered list. Both caches are bounded LRUs, because a library shuffle
            // can otherwise visit every artist of the library for the whole page lifetime.
            this.releaseCache = new Map();
            this.artistCache = new Map();
        }

        /**
         * Clears the release and artist caches. A new play context uses it so a rescan that changed releases or artists is
         * picked up instead of being served from an earlier context of the same page.
         */
        clearCaches() {
            this.releaseCache.clear();
            this.artistCache.clear();
        }

        /**
         * Builds the queue of a play descriptor.
         * @param {Object} descriptor - The normalized play descriptor.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<{ descriptors: Array<Object>, context: Object, continuationEnabled: boolean }>} The queue.
         * @throws {LuminaAudioPlayer.PlayerError} When the context is incomplete, the session expired, or an endpoint failed.
         */
        async buildQueueAsync(descriptor, signal) {
            if (!descriptor || !LuminaAudioPlayer.isGuid(descriptor.libraryId))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries no valid library Id.', { field: 'libraryId' });
            // A page that already knows the ordered tracks of its context hands them over: it is the only way a themed page
            // can provide an order this client does not know how to fetch. Only full track descriptors are accepted, because
            // an album or artist context entry has no track to load and would become a dead queue item.
            const queueDescriptors = Array.isArray(descriptor.queue)
                ? descriptor.queue.filter((entry) => entry
                    && LuminaAudioPlayer.isGuid(entry.trackId)
                    && LuminaAudioPlayer.isGuid(entry.albumId)
                    && LuminaAudioPlayer.isGuid(entry.artistId)
                    && LuminaAudioPlayer.isGuid(entry.libraryId))
                : [];
            if (queueDescriptors.length > 0) {
                return {
                    descriptors: queueDescriptors,
                    context: this.createContext(descriptor, LuminaAudioPlayer.PlayContextKind.QUEUE),
                    continuationEnabled: false
                };
            }
            if (descriptor.trackId) {
                if (!LuminaAudioPlayer.isGuid(descriptor.trackId))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries an invalid track Id.', { field: 'trackId' });
                return {
                    descriptors: [descriptor],
                    context: this.createContext(descriptor, LuminaAudioPlayer.PlayContextKind.TRACK),
                    continuationEnabled: false
                };
            }
            if (descriptor.albumId) {
                const context = this.createContext(descriptor, LuminaAudioPlayer.PlayContextKind.ALBUM);
                this.assertContextIds(context, true);
                const tracks = await this.fetchAlbumTracksAsync(context.artistId, context.albumId, context.libraryId, signal);
                const descriptors = this.createTrackDescriptors(tracks, {
                    libraryId: context.libraryId,
                    artistId: context.artistId,
                    albumId: context.albumId,
                    coverArtPath: context.coverArtPath
                });
                return { descriptors, context, continuationEnabled: descriptors.length > 0 };
            }
            if (descriptor.artistId) {
                const context = this.createContext(descriptor, LuminaAudioPlayer.PlayContextKind.ARTIST);
                this.assertContextIds(context, false);
                // An artist context is the whole discography in artist page order. The track lists are fetched with a small
                // concurrency bound, so a prolific artist does not turn into hundreds of sequential requests.
                const descriptors = await this.fetchArtistDescriptorsAsync(context.artistId, context.libraryId, context.coverArtPath, signal);
                return { descriptors, context, continuationEnabled: descriptors.length > 0 };
            }
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor identifies no track, album or artist.');
        }

        /**
         * Builds one autoplay continuation batch for the context of a queue.
         * @param {Object} context - The play context of the queue.
         * @param {AudioQueue} queue - The queue, used to skip what it already holds.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<{ descriptors: Array<Object>, isExhausted: boolean }>} The next batch.
         * @throws {LuminaAudioPlayer.PlayerError} When the session expired or an endpoint failed.
         */
        async buildContinuationBatchAsync(context, queue, signal) {
            if (!context)
                return { descriptors: [], isExhausted: true };
            if (context.kind === LuminaAudioPlayer.PlayContextKind.ALBUM) {
                if (!LuminaAudioPlayer.isGuid(context.artistId) || !LuminaAudioPlayer.isGuid(context.albumId))
                    return { descriptors: [], isExhausted: true };
                const albums = await this.fetchArtistAlbumsAsync(context.artistId, context.libraryId, signal);
                const currentIndex = albums.findIndex((album) => String(album.id).toLowerCase() === context.albumId.toLowerCase());
                const candidates = albums
                    .slice(currentIndex >= 0 ? currentIndex + 1 : albums.length)
                    .filter((album) => !queue.hasQueuedAlbum(album.id));
                if (candidates.length > 0) {
                    // Empty releases are skipped, never treated as the end of the discography: a partially scanned release must
                    // not truncate the releases and artists that follow it.
                    for (const candidate of candidates) {
                        const descriptors = await this.fetchAlbumDescriptorsAsync(candidate, context.artistId, context.libraryId, context.coverArtPath, signal);
                        if (descriptors.length > 0)
                            return { descriptors, isExhausted: false };
                    }
                }
                // The discography of the artist is over, so autoplay continues with the next artist of the library.
                return this.buildNextArtistBatchAsync(context, queue, signal);
            }
            if (context.kind === LuminaAudioPlayer.PlayContextKind.ARTIST)
                return this.buildNextArtistBatchAsync(context, queue, signal);
            return { descriptors: [], isExhausted: true };
        }

        /**
         * Builds the next artist of the library after the context artist, with all of its tracks.
         * @param {Object} context - The play context of the queue.
         * @param {AudioQueue} queue - The queue, used to skip the artists it already holds.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<{ descriptors: Array<Object>, isExhausted: boolean }>} The next artist batch.
         */
        async buildNextArtistBatchAsync(context, queue, signal) {
            if (!LuminaAudioPlayer.isGuid(context.libraryId))
                return { descriptors: [], isExhausted: true };
            const artists = await this.fetchArtistsAsync(context.libraryId, signal);
            const currentIndex = LuminaAudioPlayer.isGuid(context.artistId)
                ? artists.findIndex((artist) => String(artist.id).toLowerCase() === context.artistId.toLowerCase())
                : -1;
            const candidates = artists
                .slice(currentIndex >= 0 ? currentIndex + 1 : 0)
                .filter((artist) => !queue.hasQueuedArtist(artist.id));
            for (const artist of candidates) {
                const descriptors = await this.fetchArtistDescriptorsAsync(String(artist.id), context.libraryId, artist.coverPath || context.coverArtPath, signal);
                if (descriptors.length > 0)
                    return { descriptors, isExhausted: false };
            }
            return { descriptors: [], isExhausted: true };
        }

        /**
         * Creates the shuffle source of a scope.
         * @param {string} scope - One of the {@link LuminaAudioPlayer.QueueShuffleScope} values.
         * @param {AudioQueue} queue - The queue the shuffle replaces.
         * @param {Object|null} descriptor - The descriptor that is playing, used to resolve the category when the context is incomplete.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<LuminaAudioPlayer.ShuffleSource>} The shuffle source.
         * @throws {LuminaAudioPlayer.PlayerError} When the scope cannot be resolved for this queue.
         */
        async createShuffleSourceAsync(scope, queue, descriptor, signal) {
            const context = queue && queue.context ? queue.context : null;
            const libraryId = (context && context.libraryId) || (descriptor && descriptor.libraryId);
            const artistId = (descriptor && descriptor.artistId) || (context && context.artistId);
            const albumId = (descriptor && descriptor.albumId) || (context && context.albumId);
            if (!LuminaAudioPlayer.isGuid(libraryId))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The shuffle scope carries no valid library Id.', { field: 'libraryId' });
            if (scope === LuminaAudioPlayer.QueueShuffleScope.RELEASE) {
                if (!LuminaAudioPlayer.isGuid(artistId) || !LuminaAudioPlayer.isGuid(albumId))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The release shuffle scope needs an album and its artist.');
                const tracks = await this.fetchAlbumTracksAsync(artistId, albumId, libraryId, signal);
                return new LuminaAudioPlayer.ShuffleSource({
                    scope,
                    albums: [{ id: albumId, artistId, coverPath: (descriptor && descriptor.coverArtPath) || (context && context.coverArtPath) || '' }],
                    tracksByAlbum: { [albumId]: tracks },
                    fetchAlbumsAsync: null,
                    fetchTracksAsync: null,
                    libraryId
                });
            }
            if (scope === LuminaAudioPlayer.QueueShuffleScope.ARTIST) {
                if (!LuminaAudioPlayer.isGuid(artistId))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The artist shuffle scope needs an artist Id.');
                const albums = await this.fetchArtistAlbumsAsync(artistId, libraryId, signal);
                return new LuminaAudioPlayer.ShuffleSource({
                    scope,
                    albums: albums.map((album) => ({ id: String(album.id), artistId, coverPath: album.coverPath || '', title: album.title || '' })),
                    tracksByAlbum: null,
                    fetchAlbumsAsync: null,
                    fetchTracksAsync: (albumArtistId, albumId, albumLibraryId, batchSignal) => this.fetchAlbumTracksAsync(albumArtistId, albumId, albumLibraryId, batchSignal),
                    libraryId
                });
            }
            if (scope === LuminaAudioPlayer.QueueShuffleScope.LIBRARY)
                return new LuminaAudioPlayer.ShuffleSource({
                    scope,
                    albums: [],
                    tracksByAlbum: null,
                    fetchArtistsAsync: (shuffleLibraryId, batchSignal) => this.fetchArtistsAsync(shuffleLibraryId, batchSignal),
                    fetchAlbumsAsync: (albumArtistId, albumLibraryId, batchSignal) => this.fetchArtistAlbumsAsync(albumArtistId, albumLibraryId, batchSignal),
                    fetchTracksAsync: (albumArtistId, albumId, albumLibraryId, batchSignal) => this.fetchAlbumTracksAsync(albumArtistId, albumId, albumLibraryId, batchSignal),
                    libraryId
                });
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The shuffle scope is not known.');
        }

        /**
         * Creates the play context of a descriptor.
         * @param {Object} descriptor - The normalized play descriptor.
         * @param {string} kind - One of the {@link LuminaAudioPlayer.PlayContextKind} values.
         * @returns {Object} The play context.
         */
        createContext(descriptor, kind) {
            return {
                kind,
                libraryId: descriptor.libraryId || '',
                albumId: descriptor.albumId || '',
                artistId: descriptor.artistId || '',
                title: descriptor.title || '',
                coverArtPath: descriptor.coverArtPath || ''
            };
        }

        /**
         * Checks the Ids a context needs for its kind.
         * @param {Object} context - The context to check.
         * @param {boolean} needsAlbum - True when the context needs an album.
         * @throws {LuminaAudioPlayer.PlayerError} When an Id is missing or malformed.
         */
        assertContextIds(context, needsAlbum) {
            if (!LuminaAudioPlayer.isGuid(context.artistId))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries no valid artist Id.', { field: 'artistId' });
            if (needsAlbum && !LuminaAudioPlayer.isGuid(context.albumId))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries no valid album Id.', { field: 'albumId' });
        }

        /**
         * Fetches every track of an album, following the pagination of the endpoint.
         * @param {string} artistId - The Id of the artist the album belongs to.
         * @param {string} albumId - The Id of the album.
         * @param {string} libraryId - The Id of the library.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The track rows of the album, in the order the endpoint returns them.
         */
        async fetchAlbumTracksAsync(artistId, albumId, libraryId, signal) {
            const url = this.buildUrl('getAlbumTracksUrl', { artistId, albumId }, {
                libraryId,
                perPage: 500
            });
            return this.fetchAllPagesAsync(url, 'the tracks of an album', signal);
        }

        /**
         * Fetches the lightweight releases of an artist, ordered like the artist page: primary release type group, then
         * release year, title and Id.
         * @param {string} artistId - The Id of the artist.
         * @param {string} libraryId - The Id of the library.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The release rows, in artist page order.
         */
        async fetchArtistAlbumsAsync(artistId, libraryId, signal) {
            const cacheKey = `${String(libraryId).toLowerCase()}|${String(artistId).toLowerCase()}`;
            const cached = readCacheEntry(this.releaseCache, cacheKey);
            if (cached)
                return cached;
            const url = this.buildUrl('getArtistAlbumsUrl', { artistId }, {
                libraryId,
                perPage: 500
            });
            const albums = await this.fetchAllPagesAsync(url, 'the releases of an artist', signal);
            // The endpoint orders releases by title. The artist page groups them by their primary release type and sorts
            // each group by year, so the queue applies that same order once here for both the artist context and its
            // autoplay continuation.
            albums.sort(compareReleaseRows);
            writeCacheEntry(this.releaseCache, cacheKey, albums, RELEASE_CACHE_MAX_ENTRIES);
            return albums;
        }

        /**
         * Fetches the lightweight artists of a library, in the order the artist page shows them. The ordered list is
         * cached with a bounded LRU, so reaching the end of an artist context does not re-paginate the whole library before
         * the next artist can start.
         * @param {string} libraryId - The Id of the library.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The artist rows, in artist page order.
         */
        async fetchArtistsAsync(libraryId, signal) {
            const cacheKey = `${String(libraryId).toLowerCase()}|${this.getShouldIgnoreThePrefix() === true ? 'ignore' : 'keep'}`;
            const cached = readCacheEntry(this.artistCache, cacheKey);
            if (cached)
                return cached;
            const url = this.buildUrl('getArtistsUrl', {}, {
                libraryId,
                perPage: 500,
                shouldIgnoreThePrefixForAlphaPicker: this.getShouldIgnoreThePrefix() === true ? 'true' : 'false'
            });
            const artists = await this.fetchAllPagesAsync(url, 'the artists of a library', signal);
            writeCacheEntry(this.artistCache, cacheKey, artists, ARTIST_CACHE_MAX_ENTRIES);
            return artists;
        }

        /**
         * Fetches every track of an album and turns it into play descriptors.
         * @param {Object} album - The release row.
         * @param {string} artistId - The Id of the artist.
         * @param {string} libraryId - The Id of the library.
         * @param {string} fallbackCoverPath - The cover to use when the release has none.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The play descriptors of the release.
         */
        async fetchAlbumDescriptorsAsync(album, artistId, libraryId, fallbackCoverPath, signal) {
            const tracks = await this.fetchAlbumTracksAsync(artistId, String(album.id), libraryId, signal);
            return this.createTrackDescriptors(tracks, {
                libraryId,
                artistId,
                albumId: String(album.id),
                coverArtPath: album.coverPath || fallbackCoverPath || ''
            });
        }

        /**
         * Fetches every release of an artist and turns all of its tracks into play descriptors, in release order and then
         * in track order. The release order is the artist page order: primary release type group, then release year, title
         * and Id.
         * @param {string} artistId - The Id of the artist.
         * @param {string} libraryId - The Id of the library.
         * @param {string} fallbackCoverPath - The cover to use when a release has none.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The play descriptors of the artist.
         */
        async fetchArtistDescriptorsAsync(artistId, libraryId, fallbackCoverPath, signal) {
            const albums = await this.fetchArtistAlbumsAsync(artistId, libraryId, signal);
            // The releases are fetched with a small concurrency bound, so a prolific artist does not produce hundreds of
            // sequential requests, while the API is not flooded with one request per release either.
            const perAlbum = await mapWithConcurrencyAsync(albums, ALBUM_FETCH_CONCURRENCY, (album) =>
                this.fetchAlbumDescriptorsAsync(album, artistId, libraryId, fallbackCoverPath, signal));
            return perAlbum.flat();
        }

        /**
         * Turns track rows into play descriptors of one album.
         * @param {Array<Object>} tracks - The track rows.
         * @param {Object} album - The album the tracks belong to.
         * @returns {Array<Object>} The play descriptors, in the input order.
         */
        createTrackDescriptors(tracks, album) {
            const descriptors = [];
            for (const track of Array.isArray(tracks) ? tracks : []) {
                const trackId = LuminaAudioPlayer.toIdString(track && track.id);
                if (!LuminaAudioPlayer.isGuid(trackId))
                    continue;
                const descriptor = LuminaAudioPlayer.createPlayDescriptor({
                    trackId,
                    albumId: album.albumId,
                    artistId: album.artistId,
                    libraryId: album.libraryId,
                    title: track.title || '',
                    durationInSeconds: track.durationInSeconds,
                    coverArtPath: album.coverArtPath || ''
                });
                if (descriptor)
                    descriptors.push(descriptor);
            }
            return descriptors;
        }

        /**
         * Builds an endpoint URL from a configuration template, substituting only validated Guid values and appending the
         * query parameters through the URL API, so a value can never escape its segment.
         * @param {string} templateKey - The configuration key of the endpoint template.
         * @param {Object} replacements - The route values, keyed by placeholder name.
         * @param {Object} query - The query parameters.
         * @returns {string} The root relative endpoint path.
         * @throws {LuminaAudioPlayer.PlayerError} When the template is missing, off this origin, or a route value is not a Guid.
         */
        buildUrl(templateKey, replacements, query) {
            const configuration = typeof this.getConfiguration === 'function' ? this.getConfiguration() : null;
            const template = configuration ? configuration[templateKey] : null;
            if (typeof template !== 'string' || template.length === 0)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, `The endpoint '${templateKey}' is not configured.`);
            let path = template;
            for (const name of Object.keys(replacements || {})) {
                const value = replacements[name];
                if (!LuminaAudioPlayer.isGuid(value))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, `The play context carries no valid ${name}.`, { field: name });
                path = path.split(`{${name}}`).join(encodeURIComponent(value));
            }
            let base;
            try {
                base = new URL(path, getBaseOrigin());
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The endpoint is not a valid URL.', { cause: error });
            }
            if ((base.protocol !== 'http:' && base.protocol !== 'https:')
                || base.origin !== new URL(getBaseOrigin()).origin
                || base.username !== ''
                || base.password !== '')
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, 'The endpoint is not on this origin.');
            for (const name of Object.keys(query || {})) {
                const value = query[name];
                if (value !== undefined && value !== null && value !== '')
                    base.searchParams.set(name, String(value));
            }
            return base.pathname + base.search;
        }

        /**
         * Fetches every page of a paginated music endpoint.
         * @param {string} url - The endpoint URL of the first page.
         * @param {string} label - The description of the request in a failure log.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The rows of every page, in endpoint order.
         */
        async fetchAllPagesAsync(url, label, signal) {
            const first = await this.fetchPageAsync(url, label, signal);
            const rows = Array.isArray(first.data) ? [...first.data] : [];
            const numberOfPages = Math.min(Math.max(1, Number(first.numberOfPages) || 1), MAX_PAGES);
            for (let page = 2; page <= numberOfPages; page++) {
                const pageUrl = new URL(url, getBaseOrigin());
                pageUrl.searchParams.set('currentPage', String(page));
                const next = await this.fetchPageAsync(pageUrl.pathname + pageUrl.search, label, signal);
                if (Array.isArray(next.data))
                    rows.push(...next.data);
            }
            return rows;
        }

        /**
         * Fetches one page of a paginated music endpoint.
         * @param {string} url - The endpoint URL.
         * @param {string} label - The description of the request in a failure log.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Object>} The paginated payload.
         * @throws {LuminaAudioPlayer.PlayerError} When the session expired, the endpoint failed, or the request was aborted.
         */
        async fetchPageAsync(url, label, signal) {
            const data = await this.fetchJsonAsync(url, signal, label);
            // The JSON transport maps an abort to the same undefined result as a failed request, so the signal is checked
            // explicitly to keep an abort from being reported as an endpoint failure by the caller's error handling.
            if (signal && signal.aborted)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, `The request for ${label} was aborted.`);
            if (data === LuminaAudioPlayer.REDIRECTED)
                throwSessionExpired();
            if (data === undefined || data === null || typeof data !== 'object')
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.QUEUE_LOAD_FAILED, `The request for ${label} failed.`);
            return data;
        }
    }

    /**
     * Draws randomized tracks of one shuffle scope, one batch at a time.
     *
     * The source keeps the tracks of a release it has not queued yet in a pool, so a batch never repeats a queued track and
     * a release that straddles two batches is not fetched twice. The artist and library scopes only fetch the lightweight
     * release metadata up front, or discover the releases of one artist at a time, and prefetch the track lists of a bounded
     * number of releases in parallel, which is what keeps a library shuffle from loading every track of the library.
     */
    class ShuffleSource {
        /**
         * Creates an instance of ShuffleSource.
         * @param {Object} options - The source options.
         * @param {string} options.scope - One of the {@link LuminaAudioPlayer.QueueShuffleScope} values.
         * @param {Array<Object>} options.albums - The releases the scope draws from, already known when the scope is a release or an artist.
         * @param {Object|null} options.tracksByAlbum - The track rows of a release, keyed by album Id, when they are already known.
         * @param {Function|null} options.fetchArtistsAsync - The function that fetches the artists of the library, used by the library scope.
         * @param {Function|null} options.fetchAlbumsAsync - The function that fetches the releases of an artist, used by the library scope.
         * @param {Function|null} options.fetchTracksAsync - The function that fetches the tracks of a release.
         * @param {string} options.libraryId - The Id of the library.
         */
        constructor(options) {
            const settings = options || {};
            this.scope = settings.scope;
            this.libraryId = settings.libraryId;
            this.albumQueue = shuffleCopy(settings.albums || []);
            this.loadedAlbums = settings.tracksByAlbum ? { ...settings.tracksByAlbum } : {};
            this.trackPools = new Map();
            // The release pools that were already fetched and are not drawn empty yet, so a batch that stops mid-release keeps
            // the remaining tracks ready for the next batch.
            this.readyPools = [];
            this.fetchArtistsAsync = settings.fetchArtistsAsync || null;
            this.fetchAlbumsAsync = settings.fetchAlbumsAsync || null;
            this.fetchTracksAsync = settings.fetchTracksAsync || null;
            // The library scope discovers artists lazily, and never holds more than one artist's releases at a time.
            this.artistQueue = null;
            this.isExhausted = false;
        }

        /**
         * Gets the next batch of randomized tracks that the queue does not already hold.
         * @param {Set<string>} excludedTrackIds - The lower case track identities the queue already holds.
         * @param {number} [count] - The number of tracks to draw.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The descriptors of the batch, never longer than the count.
         */
        async nextBatchAsync(excludedTrackIds, count = SHUFFLE_BATCH_SIZE, signal) {
            const excluded = excludedTrackIds instanceof Set ? excludedTrackIds : new Set();
            const batch = [];
            if (this.isExhausted)
                return batch;
            while (batch.length < count) {
                await this.ensureReadyPoolsAsync(signal);
                if (this.readyPools.length === 0) {
                    this.isExhausted = true;
                    break;
                }
                const ready = this.readyPools.shift();
                const pool = ready.pool;
                while (pool.length > 0 && batch.length < count) {
                    const descriptor = pool.pop();
                    const identity = getTrackIdentity(descriptor);
                    if (!identity || excluded.has(identity))
                        continue;
                    excluded.add(identity);
                    batch.push(descriptor);
                }
                // A release that straddled the batch keeps its remaining tracks ready, so the next batch finishes it instead
                // of leaving those tracks unreachable.
                if (pool.length > 0) {
                    this.readyPools.push(ready);
                    // The other ready releases are still offered before the batch ends, but a release that is only half drawn
                    // must not starve them when the next batch starts.
                    if (this.readyPools.length > 1)
                        this.readyPools.push(this.readyPools.shift());
                } else {
                    // A drained release is never drawn again, so its pool does not stay in the per release map for the rest
                    // of the shuffle session.
                    this.trackPools.delete(String(ready.album.id));
                }
            }
            return batch;
        }

        /**
         * Fills the ready release pools, fetching the track lists of several releases at the same time. The batch filler then
         * draws from the ready pools, so filling 100 tracks does not wait for one release track list at a time.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<void>} A promise that completes when at least one pool is ready, or the category is exhausted.
         */
        async ensureReadyPoolsAsync(signal) {
            if (this.readyPools.length > 0 || this.isExhausted)
                return;
            const albums = [];
            while (albums.length < SHUFFLE_PREFETCH_CONCURRENCY) {
                let album = this.albumQueue.shift();
                if (!album) {
                    album = await this.loadNextAlbumAsync(signal);
                    if (!album)
                        break;
                }
                albums.push(album);
            }
            if (albums.length === 0) {
                this.isExhausted = true;
                return;
            }
            const results = await Promise.allSettled(albums.map((album) => this.getTrackPoolAsync(album, signal)));
            const readyPools = [];
            const failedAlbums = [];
            for (let index = 0; index < results.length; index++) {
                if (results[index].status === 'fulfilled')
                    readyPools.push({ album: albums[index], pool: results[index].value });
                else
                    failedAlbums.push(albums[index]);
            }
            const isAborted = signal && signal.aborted;
            const failure = results.find((result) => result.status === 'rejected');
            // A batch that was aborted while its prefetches were settling belongs to a shuffle that is already gone. It is
            // reported as the same abort the caller expects, so no partial pool of it is drawn.
            if (isAborted)
                throw failure ? failure.reason : new Error('The shuffle prefetch was aborted.');
            // A release whose track list failed is put back into the queue instead of being lost for the rest of the shuffle.
            if (failedAlbums.length > 0)
                this.albumQueue.push(...failedAlbums);
            if (readyPools.length === 0) {
                if (failure)
                    throw failure.reason;
                this.isExhausted = true;
                return;
            }
            this.readyPools = readyPools;
        }

        /**
         * Loads the next release of the category.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Object|null>} The release, or null when the category has no more releases.
         */
        async loadNextAlbumAsync(signal) {
            if (this.scope !== LuminaAudioPlayer.QueueShuffleScope.LIBRARY)
                return null;
            while (true) {
                // The releases of the current artist come first, and the next artist is only discovered when those are
                // exhausted, so one artist's releases are never mixed with a second fetch of the artist list.
                const album = this.albumQueue.shift();
                if (album)
                    return album;
                const artist = await this.loadNextArtistAsync(signal);
                if (!artist)
                    return null;
                const albums = typeof this.fetchAlbumsAsync === 'function'
                    ? await this.fetchAlbumsAsync(String(artist.id), this.libraryId, signal)
                    : [];
                this.albumQueue = shuffleCopy(albums.map((albumRow) => ({
                    id: String(albumRow.id),
                    artistId: String(artist.id),
                    coverPath: albumRow.coverPath || '',
                    title: albumRow.title || ''
                })));
            }
        }

        /**
         * Loads the next artist of the library, one at a time, so the library is never enumerated as a whole set of tracks.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Object|null>} The artist, or null when the library has no more artists.
         */
        async loadNextArtistAsync(signal) {
            if (this.scope !== LuminaAudioPlayer.QueueShuffleScope.LIBRARY || typeof this.fetchArtistsAsync !== 'function')
                return null;
            if (this.artistQueue === null) {
                const artists = await this.fetchArtistsAsync(this.libraryId, signal);
                this.artistQueue = shuffleCopy(artists.map((artist) => ({ id: String(artist.id), coverPath: artist.coverPath || '' })));
            }
            return this.artistQueue.length > 0 ? this.artistQueue.shift() : null;
        }

        /**
         * Gets the randomized pool of the tracks of a release that the queue has not drawn yet.
         * @param {Object} album - The release.
         * @param {AbortSignal} [signal] - The signal that cancels the request when the player supersedes or stops it.
         * @returns {Promise<Array<Object>>} The remaining descriptors of the release.
         */
        async getTrackPoolAsync(album, signal) {
            const albumId = String(album.id);
            if (this.trackPools.has(albumId))
                return this.trackPools.get(albumId);
            let tracks = this.loadedAlbums[albumId];
            // The raw rows are released as soon as they are turned into descriptors, so a long library shuffle keeps only the
            // remaining pool of the releases it has open, not every row it has already fetched.
            if (tracks)
                delete this.loadedAlbums[albumId];
            if (!tracks && typeof this.fetchTracksAsync === 'function')
                tracks = await this.fetchTracksAsync(album.artistId, albumId, this.libraryId, signal);
            const descriptors = Array.isArray(tracks)
                ? tracks.map((track) => LuminaAudioPlayer.createPlayDescriptor({
                    trackId: track.id,
                    albumId,
                    artistId: album.artistId,
                    libraryId: this.libraryId,
                    title: track.title || '',
                    durationInSeconds: track.durationInSeconds,
                    coverArtPath: album.coverPath || ''
                })).filter((descriptor) => descriptor !== null)
                : [];
            const pool = shuffleCopy(descriptors);
            this.trackPools.set(albumId, pool);
            return pool;
        }
    }

    LuminaAudioPlayer.AudioQueue = AudioQueue;
    LuminaAudioPlayer.QueueContextService = QueueContextService;
    LuminaAudioPlayer.ShuffleSource = ShuffleSource;
    LuminaAudioPlayer.SHUFFLE_BATCH_SIZE = SHUFFLE_BATCH_SIZE;
    LuminaAudioPlayer.SHUFFLE_LOOKAHEAD = SHUFFLE_LOOKAHEAD;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
