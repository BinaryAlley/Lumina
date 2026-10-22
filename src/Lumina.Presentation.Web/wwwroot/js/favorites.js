/**
 * Favorite toggles: the hearts of the media cards (books, artists and albums, and the cards of the favorites page), of the
 * track rows of the album page, of the rows of a saved playlist, and of the current track and the queue items of the audio
 * player bar. The detail pages of an item have no toggle.
 *
 * A toggle is any focusable element, normally a button, that carries the `data-favorite-toggle` marker together with the
 * `data-favorite-media-kind` (`MusicArtist`, `MusicAlbum`, `MusicTrack` or `Book`) and `data-favorite-media-item-id`
 * attributes. The module owns the whole behavior of such an element, so a page or a theme template only renders the markup:
 * it finds the toggles, asks the server which of their items the current user favorited, writes the state on them, and
 * adds or removes the favorite when one is clicked. The state is exposed as `aria-pressed`, `data-favorite-state`
 * (`favorited`, `not-favorited`, `loading` or `failed`) and the `is-favorite` class, so a theme owns the look. The module
 * writes the localized text of the next action ("Add to favorites" or "Remove from favorites") as the `aria-label` and as
 * the `title`, which is the hover text of a heart that has no visible text, and also as the text of the optional
 * `.favorite-toggle-label` element. The module never renders markup: it only sets those attributes, the class and the text.
 *
 * Everything is attached once, to the document. The click listener uses event delegation and a MutationObserver hydrates
 * the toggles that are added later, by a page script or by the in-place navigation that swaps the page content, so no page
 * has to bind or unbind anything and loading this script a second time changes nothing. The state lives only on the
 * elements and in the requests in flight: nothing is cached or stored in the browser, so the favorites of one user can
 * never be shown to another user who signs in without a page reload.
 *
 * After every successful change the module dispatches the `lumina:favorite-changed` DOM event on the document, with
 * `detail = { mediaKind, mediaItemId, isFavorite }`, so a page that lists favorites can react while it stays decoupled
 * from the toggle that caused the change.
 */
(function () {
    'use strict';

    // The layout may render this script on a page that already ran it, and the in-place navigation can evaluate scripts
    // again. A second set of listeners would send every request twice, so the first load owns the document.
    if (window.LuminaFavorites)
        return;

    // The layout renders the route templates with empty Guids because the kind and the item are not known until a toggle
    // is clicked. Replacing this sentinel keeps the route construction in one place and avoids hand-built paths.
    const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
    // The favorite kinds the server supports today. Anything else is rejected here, so a typo in a template shows up as a
    // failed toggle instead of as a request that the server would reject.
    const MEDIA_KINDS = ['MusicArtist', 'MusicAlbum', 'MusicTrack', 'Book'];
    // The server accepts up to 100 ids per status request, because the ids travel in the query string: 200 Guids are about
    // 10 KB, which is more than the 8 KB request line the web server accepts by default, while 100 ids stay under 6 KB.
    const MAX_STATUS_IDS_PER_REQUEST = 100;
    const TOGGLE_SELECTOR = '[data-favorite-toggle]';
    const FAVORITE_CHANGED_EVENT = 'lumina:favorite-changed';
    const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/;

    // The status checks asked for in the current tick, grouped by kind and then by item Id. They are sent together on the
    // next macrotask, so the toggles a page renders in a loop cost one request per kind instead of one per toggle.
    let pendingStatus = new Map();
    let statusFlushHandle = null;
    // The add or remove request of every item that is being changed. It makes a second click on the same item a no-op
    // while the first request is in flight, whichever toggle of the item it comes from.
    const mutationsInFlight = new Map();
    // A counter per item that moves every time the item changes. A status answer that was asked for before a change
    // describes an old state, so it is dropped when the counter moved while the request was in flight.
    const revisions = new Map();

    /**
     * Gets the favorites configuration the layout renders for every page.
     *
     * The configuration is the only contract between the layout and this module: it carries the routes and the localized
     * labels. Reading it lazily, instead of capturing it when the script loads, lets the layout render the configuration
     * after this deferred script has been parsed.
     * @returns {Object|null} The configuration, or null when the layout did not render it.
     */
    function getConfiguration() {
        return window.favoritesConfiguration || null;
    }

    /**
     * Gets the localized strings of the favorites, with empty values when the layout did not render them.
     * @returns {Object} The localized strings.
     */
    function getStrings() {
        const configuration = getConfiguration();
        return configuration && configuration.strings ? configuration.strings : {};
    }

    /**
     * Normalizes a media kind to its canonical name.
     *
     * The comparison ignores the case, so a template that renders `musicalbum` still works, and the canonical name is what the routes and the request bodies carry.
     * @param {*} mediaKind - The kind to normalize.
     * @returns {string} The canonical kind name, or an empty string when the kind is not supported.
     */
    function normalizeMediaKind(mediaKind) {
        if (typeof mediaKind !== 'string')
            return '';
        const lowered = mediaKind.trim().toLowerCase();
        return MEDIA_KINDS.find(kind => kind.toLowerCase() === lowered) || '';
    }

    /**
     * Normalizes the Id of a media item to its lowercase Guid text.
     *
     * Ids are compared as text across the DOM and the server answers, and the casing of a Guid is not significant, so the
     * lowercase form is the one key. An empty Guid is rejected because it never identifies an item.
     * @param {*} mediaItemId - The Id to normalize.
     * @returns {string} The lowercase Guid, or an empty string when the value is not a valid, non empty Guid.
     */
    function normalizeMediaItemId(mediaItemId) {
        if (typeof mediaItemId !== 'string')
            return '';
        const lowered = mediaItemId.trim().toLowerCase();
        return GUID_PATTERN.test(lowered) && lowered !== EMPTY_GUID ? lowered : '';
    }

    /**
     * Describes a media item by its normalized kind and Id.
     * @param {*} mediaKind - The kind of the item.
     * @param {*} mediaItemId - The Id of the item.
     * @returns {Object|null} The item, with its kind, its Id and the key that identifies it, or null when either is invalid.
     */
    function describeItem(mediaKind, mediaItemId) {
        const kind = normalizeMediaKind(mediaKind);
        const id = normalizeMediaItemId(mediaItemId);
        if (!kind || !id)
            return null;
        return { kind, id, key: `${kind}:${id}` };
    }

    /**
     * Reads the item a toggle element refers to from its data attributes.
     * @param {Element} toggle - The toggle element.
     * @returns {Object|null} The item, or null when the attributes are missing or invalid.
     */
    function readToggleItem(toggle) {
        return describeItem(toggle.getAttribute('data-favorite-media-kind'), toggle.getAttribute('data-favorite-media-item-id'));
    }

    /**
     * Collects the toggles inside an element, including the element itself.
     * @param {Element|Document|DocumentFragment} root - The element to search.
     * @returns {Array<Element>} The toggles that were found.
     */
    function collectToggles(root) {
        const toggles = [];
        if (root.nodeType === Node.ELEMENT_NODE && root.matches(TOGGLE_SELECTOR))
            toggles.push(root);
        if (typeof root.querySelectorAll === 'function')
            root.querySelectorAll(TOGGLE_SELECTOR).forEach(toggle => toggles.push(toggle));
        return toggles;
    }

    /**
     * Builds the index of every toggle of the document, grouped by item key.
     *
     * A status flush resolves many items at once, and every resolved item has to write its state on all of its toggles.
     * Reading the document once and grouping the toggles by key keeps that write linear in the number of toggles,
     * instead of rescanning the whole document for every item. The toggles a caller passes in are added as well, so an
     * element that is not attached to the document yet is still updated.
     * @param {Array<Element>} [additionalToggles] - The toggles to include besides the ones of the document.
     * @returns {Map<string, Array<Element>>} The toggles of each item key.
     */
    function buildToggleIndex(additionalToggles) {
        const index = new Map();
        const seenToggles = new Set();
        const addToggle = toggle => {
            // The document pass and the list a caller passed in can hold the same element, so it is added only once.
            if (seenToggles.has(toggle))
                return;
            seenToggles.add(toggle);
            const item = readToggleItem(toggle);
            if (!item)
                return;
            const toggles = index.get(item.key);
            if (toggles)
                toggles.push(toggle);
            else
                index.set(item.key, [toggle]);
        };
        collectToggles(document).forEach(addToggle);
        if (Array.isArray(additionalToggles))
            additionalToggles.forEach(addToggle);
        return index;
    }

    /**
     * Gets the toggles of an item from a built index.
     * @param {Map<string, Array<Element>>} index - The index built by {@link buildToggleIndex}.
     * @param {string} key - The key of the item.
     * @returns {Array<Element>} The toggles of the item, or an empty array when the item has none.
     */
    function getIndexedToggles(index, key) {
        return index.get(key) || [];
    }

    /**
     * Gets the revision counter of an item.
     * @param {string} key - The key of the item.
     * @returns {number} The current revision.
     */
    function getRevision(key) {
        return revisions.get(key) || 0;
    }

    /**
     * Moves the revision counter of an item, which invalidates the status answers that are still in flight for it.
     * @param {string} key - The key of the item.
     */
    function bumpRevision(key) {
        revisions.set(key, getRevision(key) + 1);
    }

    /**
     * Writes a state on a toggle: the pressed state, the state attribute, the class, and the localized label and hover text.
     *
     * The loading and failed states keep the pressed state the toggle already had, so the toggle does not flicker to
     * "not favorite" while a request is in flight, and the label always names the action the next click performs.
     * @param {Element} toggle - The toggle element.
     * @param {string} state - The state to write: favorited, not-favorited, loading or failed.
     * @param {boolean} isFavorite - Whether the toggle must show the item as a favorite.
     */
    function renderToggle(toggle, state, isFavorite) {
        toggle.setAttribute('data-favorite-state', state);
        toggle.setAttribute('aria-pressed', isFavorite ? 'true' : 'false');
        toggle.classList.toggle('is-favorite', isFavorite);
        if (state === 'loading')
            toggle.setAttribute('aria-busy', 'true');
        else
            toggle.removeAttribute('aria-busy');
        const label = isFavorite ? getStrings().remove : getStrings().add;
        if (!label)
            return;
        toggle.setAttribute('aria-label', label);
        toggle.setAttribute('title', label);
        const labelElement = toggle.querySelector('.favorite-toggle-label');
        // The text is written only when it changes, so the observer of the document does not see a change per state write.
        if (labelElement && labelElement.textContent !== label)
            labelElement.textContent = label;
    }

    /**
     * Gets whether a toggle currently shows its item as a favorite.
     * @param {Element} toggle - The toggle element.
     * @returns {boolean} True when the toggle is pressed.
     */
    function isPressed(toggle) {
        return toggle.getAttribute('aria-pressed') === 'true';
    }

    /**
     * Writes the settled state of an item on every toggle that shows it.
     * @param {string} key - The key of the item.
     * @param {boolean} isFavorite - Whether the item is a favorite of the current user.
     * @param {Map<string, Array<Element>>} [index] - An already built toggle index, so a batch of items does not scan the document once per item.
     */
    function applyFavoriteState(key, isFavorite, index) {
        const toggleIndex = index || buildToggleIndex();
        getIndexedToggles(toggleIndex, key)
            .forEach(toggle => renderToggle(toggle, isFavorite ? 'favorited' : 'not-favorited', isFavorite));
    }

    /**
     * Builds the status request route of a batch of items of one kind.
     * @param {string} kind - The kind of the items.
     * @param {Array<string>} ids - The Ids of the items to check.
     * @returns {string} The route with its query string, or an empty string when the layout did not render the route.
     */
    function buildStatusUrl(kind, ids) {
        const configuration = getConfiguration();
        const url = configuration ? configuration.getFavoriteStatusUrl : '';
        if (typeof url !== 'string' || url.length === 0)
            return '';
        const parameters = new URLSearchParams();
        parameters.set('mediaKind', kind);
        ids.forEach(id => parameters.append('mediaItemIds', id));
        return `${url}${url.includes('?') ? '&' : '?'}${parameters.toString()}`;
    }

    /**
     * Builds the route that removes a favorite by replacing its named mediaKind and mediaItemId route placeholders.
     *
     * The layout renders the route template with the placeholders of the route intact, so each value is filled in by name and a change
     * in the segment order cannot silently swap them. Both values are encoded so they stay single path segments even if the DOM value is malformed.
     * @param {Object} item - The item to remove from the favorites.
     * @returns {string} The route, or an empty string when the layout did not render the template.
     */
    function buildRemoveUrl(item) {
        const configuration = getConfiguration();
        const template = configuration ? configuration.removeFavoriteUrlTemplate : '';
        if (typeof template !== 'string' || template.length === 0)
            return '';
        return template
            .replace('{mediaKind}', encodeURIComponent(item.kind))
            .replace('{mediaItemId}', encodeURIComponent(item.id));
    }

    /**
     * Asks the server which items of one kind are favorites of the current user, in one request.
     * @param {string} kind - The kind of the items.
     * @param {Array<string>} ids - The Ids of the items to check.
     * @returns {Promise<Set<string>>} The lowercase Ids, among the checked ones, that are favorites.
     * @throws {Error} When the route is not available or the request failed.
     */
    async function requestStatusAsync(kind, ids) {
        const url = buildStatusUrl(kind, ids);
        if (!url)
            throw new Error('Favorites: the status route was not rendered by the layout.');
        if (typeof callApiGetAsync !== 'function')
            throw new Error('Favorites: the global API helper is not available.');
        const data = await callApiGetAsync(url);
        // The API helper resolves to undefined after it reported a failed request, and a malformed answer would let a toggle show a wrong state, so both are failures.
        if (!data || !Array.isArray(data.favoritedMediaItemIds))
            throw new Error('Favorites: the status request failed.');
        return new Set(data.favoritedMediaItemIds.map(normalizeMediaItemId).filter(id => id !== ''));
    }

    /**
     * Resolves the waiters of one chunk of a status batch from a single request.
     * @param {string} kind - The kind of the items of the chunk.
     * @param {Array<string>} ids - The Ids of the chunk.
     * @param {Map<string, Array<Object>>} waiters - The waiters of the whole batch, by item Id.
     * @returns {Promise<void>} A promise that completes when every waiter of the chunk was settled.
     */
    async function resolveStatusChunkAsync(kind, ids, waiters) {
        try {
            const favorited = await requestStatusAsync(kind, ids);
            ids.forEach(id => waiters.get(id).forEach(waiter => waiter.resolve(favorited.has(id))));
        } catch (error) {
            console.error('Favorites: reading the favorite state failed.', error);
            ids.forEach(id => waiters.get(id).forEach(waiter => waiter.reject(error)));
        }
    }

    /**
     * Sends the status checks queued in the current tick, one request per kind and per chunk of ids.
     *
     * The ids of a kind are deduplicated by the queue, and split into chunks so the query string stays below the request line limit of the server.
     */
    function flushStatusQueue() {
        statusFlushHandle = null;
        const batches = pendingStatus;
        pendingStatus = new Map();
        batches.forEach((waiters, kind) => {
            const ids = Array.from(waiters.keys());
            for (let start = 0; start < ids.length; start += MAX_STATUS_IDS_PER_REQUEST)
                resolveStatusChunkAsync(kind, ids.slice(start, start + MAX_STATUS_IDS_PER_REQUEST), waiters);
        });
    }

    /**
     * Queues the status check of an item, so it is sent in one request together with the items asked for in the same tick.
     * @param {Object} item - The item to check.
     * @returns {Promise<boolean>} A promise that resolves to whether the item is a favorite, and rejects when the check failed.
     */
    function queueStatusCheck(item) {
        return new Promise((resolve, reject) => {
            if (!pendingStatus.has(item.kind))
                pendingStatus.set(item.kind, new Map());
            const waiters = pendingStatus.get(item.kind);
            if (!waiters.has(item.id))
                waiters.set(item.id, []);
            waiters.get(item.id).push({ resolve, reject });
            if (statusFlushHandle === null)
                statusFlushHandle = setTimeout(flushStatusQueue, 0);
        });
    }

    /**
     * Reads the favorite state of the toggles from the server and writes it on them.
     *
     * A toggle with a settled state keeps showing it while the state is checked again, which is what happens when the
     * in-place navigation renders a page again, so nothing flickers. A toggle without a settled state shows the loading state. An item that is being
     * changed is skipped, because its change writes the state when it completes, and an answer that was overtaken by a change is dropped.
     * A failed check marks the toggles that have no settled state as failed, and a click on such a toggle checks it again.
     * @param {Array<Element>} toggles - The toggles to hydrate.
     * @returns {Promise<void>} A promise that completes when every check has finished. It never rejects.
     */
    async function hydrateToggles(toggles) {
        const checks = new Map();
        for (const toggle of toggles) {
            const item = readToggleItem(toggle);
            if (!item) {
                console.warn('Favorites: a toggle has an invalid media kind or media item Id.', toggle);
                renderToggle(toggle, 'failed', false);
                continue;
            }
            if (mutationsInFlight.has(item.key))
                continue;
            const state = toggle.getAttribute('data-favorite-state');
            if (state !== 'favorited' && state !== 'not-favorited')
                renderToggle(toggle, 'loading', isPressed(toggle));
            if (!checks.has(item.key))
                checks.set(item.key, { item, revision: getRevision(item.key) });
        }
        // Nothing to ask and nothing to write, so the document is not scanned for an empty batch.
        if (checks.size === 0)
            return;
        // One document pass serves every item of the batch, so hydrating a queue of N tracks with one toggle per item
        // costs one scan of the document instead of N scans, which is what made the previous lookup quadratic. A toggle
        // added while the requests are in flight is picked up by the observer of the document and hydrated on its own.
        const index = buildToggleIndex(toggles);
        await Promise.all(Array.from(checks.values(), check => queueStatusCheck(check.item).then(
            isFavorite => {
                if (getRevision(check.item.key) === check.revision && !mutationsInFlight.has(check.item.key))
                    applyFavoriteState(check.item.key, isFavorite, index);
            },
            () => {
                getIndexedToggles(index, check.item.key)
                    .filter(toggle => toggle.getAttribute('data-favorite-state') === 'loading')
                    .forEach(toggle => renderToggle(toggle, 'failed', isPressed(toggle)));
            })));
    }

    /**
     * Adds or removes the favorite on the server, and writes the result on every toggle of the item.
     *
     * Every toggle of the item shows the loading state meanwhile. On success they show the new state and the change event
     * is dispatched. On failure they get back the state they had, because a failed request changed nothing, and the failure is reported by the global API helper.
     * @param {Object} item - The item to change.
     * @param {boolean} shouldFavorite - True to add the favorite, false to remove it.
     * @returns {Promise<boolean>} A promise that resolves to whether the change was made.
     */
    async function changeFavoriteAsync(item, shouldFavorite) {
        const previousStates = new Map();
        const initialIndex = buildToggleIndex();
        getIndexedToggles(initialIndex, item.key).forEach(toggle => {
            previousStates.set(toggle, { state: toggle.getAttribute('data-favorite-state'), isFavorite: isPressed(toggle) });
            renderToggle(toggle, 'loading', isPressed(toggle));
        });
        bumpRevision(item.key);

        let isChanged = false;
        try {
            const configuration = getConfiguration();
            if (shouldFavorite) {
                const addUrl = configuration ? configuration.addFavoriteUrl : '';
                if (addUrl) {
                    const result = await callApiPostAsync(addUrl, { mediaKind: item.kind, mediaItemId: item.id });
                    isChanged = Boolean(result);
                }
            } else {
                const removeUrl = buildRemoveUrl(item);
                if (removeUrl)
                    isChanged = Boolean(await callApiDeleteAsync(removeUrl));
            }
        } catch (error) {
            // The API helpers normally resolve to a falsy value on failure; this catch is for unexpected errors, such as a
            // rejected promise from a test double. The toggles must leave the loading state either way.
            console.error('Favorites: changing the favorite failed.', error);
        }

        bumpRevision(item.key);
        if (isChanged) {
            applyFavoriteState(item.key, shouldFavorite);
            document.dispatchEvent(new CustomEvent(FAVORITE_CHANGED_EVENT, {
                detail: { mediaKind: item.kind, mediaItemId: item.id, isFavorite: shouldFavorite }
            }));
            return true;
        }

        // A toggle added while the request was in flight has no remembered state, so it gets the state that was true before the change.
        const restoreIndex = buildToggleIndex();
        getIndexedToggles(restoreIndex, item.key).forEach(toggle => {
            const previous = previousStates.get(toggle) || { state: shouldFavorite ? 'not-favorited' : 'favorited', isFavorite: !shouldFavorite };
            const state = previous.state === 'favorited' || previous.state === 'not-favorited' ? previous.state : 'failed';
            renderToggle(toggle, state, previous.isFavorite);
        });
        return false;
    }

    /**
     * Adds a media item to the favorites of the current user, or removes it.
     *
     * A second call for an item that is already being changed is ignored: it resolves to the result of the running change
     * when it asks for the same state, and to false when it asks for the opposite one, so a double click can never send two requests.
     * @param {string} mediaKind - The kind of the item: MusicArtist, MusicAlbum, MusicTrack or Book.
     * @param {string} mediaItemId - The Id of the item.
     * @param {boolean} shouldFavorite - True to add the favorite, false to remove it.
     * @returns {Promise<boolean>} A promise that resolves to true when the favorite state of the item is now the requested
     * one, and to false when the change was not made. A failure is reported to the user, so the caller only needs to react to the result.
     */
    function setFavoriteAsync(mediaKind, mediaItemId, shouldFavorite) {
        const item = describeItem(mediaKind, mediaItemId);
        if (!item) {
            console.error('Favorites: the media kind or the media item Id is invalid.', mediaKind, mediaItemId);
            return Promise.resolve(false);
        }
        const wanted = Boolean(shouldFavorite);
        const running = mutationsInFlight.get(item.key);
        if (running)
            return running.shouldFavorite === wanted ? running.promise : Promise.resolve(false);
        const promise = changeFavoriteAsync(item, wanted).finally(() => mutationsInFlight.delete(item.key));
        mutationsInFlight.set(item.key, { shouldFavorite: wanted, promise });
        return promise;
    }

    /**
     * Gets whether a media item is a favorite of the current user.
     *
     * The answer always comes from the server, never from a cache, and the check is batched with the other checks asked for in the same tick.
     * @param {string} mediaKind - The kind of the item: MusicArtist, MusicAlbum, MusicTrack or Book.
     * @param {string} mediaItemId - The Id of the item.
     * @returns {Promise<boolean>} A promise that resolves to whether the item is a favorite. It rejects when the kind or the
     * Id is invalid or the request failed; the failure of a request is also reported to the user.
     */
    function isFavorite(mediaKind, mediaItemId) {
        const item = describeItem(mediaKind, mediaItemId);
        if (!item)
            return Promise.reject(new Error('Favorites: the media kind or the media item Id is invalid.'));
        return queueStatusCheck(item);
    }

    /**
     * Hydrates the favorite toggles inside an element: reads which of their items are favorites and writes the state.
     *
     * The module already hydrates the toggles of the page and the ones that are added to it, so a page only needs this
     * for toggles that live in an element that is not attached to the document yet.
     * @param {Element|Document} [rootElement] - The element to search for toggles. Defaults to the whole document.
     * @returns {Promise<void>} A promise that completes when the state of the toggles was written. It never rejects.
     */
    function hydrate(rootElement) {
        const root = rootElement || document;
        if (!root || typeof root.querySelectorAll !== 'function')
            return Promise.resolve();
        return hydrateToggles(collectToggles(root));
    }

    /**
     * Handles a click on the document and toggles the favorite when the click happened on a favorite toggle.
     *
     * The click is handled by delegation, so a toggle works wherever it is rendered, even inside a link or a card, and a
     * toggle that is added later needs no listener. The default action is prevented, so a toggle inside a link does not also navigate.
     * @param {MouseEvent} event - The click event.
     */
    function handleClick(event) {
        const target = event.target;
        if (!target || typeof target.closest !== 'function')
            return;
        const toggle = target.closest(TOGGLE_SELECTOR);
        if (!toggle)
            return;
        event.preventDefault();
        const item = readToggleItem(toggle);
        if (!item)
            return;
        const state = toggle.getAttribute('data-favorite-state');
        if (state === 'loading' || mutationsInFlight.has(item.key))
            return;
        // The real state of the item is not known, because the toggle was not hydrated yet or its hydration failed, so a
        // click would guess the direction. The click reads the state again instead, and the next click acts on it.
        if (state !== 'favorited' && state !== 'not-favorited') {
            hydrateToggles([toggle]);
            return;
        }
        setFavoriteAsync(item.kind, item.id, !isPressed(toggle));
    }

    /**
     * Hydrates the toggles that were added to the document, and the toggles whose item attributes changed.
     *
     * This is how the toggles that a page script renders, or that the in-place navigation swaps in, get their state
     * without any call from the page. The writes of this module only touch attributes that are not observed and text nodes, so the observer cannot trigger itself.
     * @param {Array<MutationRecord>} mutations - The mutations of the document.
     */
    function handleMutations(mutations) {
        const toggles = new Set();
        for (const mutation of mutations) {
            if (mutation.type === 'attributes') {
                if (mutation.target.nodeType === Node.ELEMENT_NODE && mutation.target.matches(TOGGLE_SELECTOR)) {
                    // The element now refers to another item, so the state it shows belongs to the old one.
                    mutation.target.removeAttribute('data-favorite-state');
                    toggles.add(mutation.target);
                }
                continue;
            }
            mutation.addedNodes.forEach(node => {
                if (node.nodeType !== Node.ELEMENT_NODE)
                    return;
                collectToggles(node).forEach(toggle => {
                    // A re-appended row, such as the rows of a playlist after a drag reorder, keeps the state it already
                    // shows. Re-checking it would send a status request for a heart that is already settled on screen.
                    const state = toggle.getAttribute('data-favorite-state');
                    if (state !== 'favorited' && state !== 'not-favorited')
                        toggles.add(toggle);
                });
            });
        }
        if (toggles.size > 0)
            hydrateToggles(Array.from(toggles));
    }

    document.addEventListener('click', handleClick);
    new MutationObserver(handleMutations).observe(document.documentElement, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['data-favorite-toggle', 'data-favorite-media-kind', 'data-favorite-media-item-id']
    });

    window.LuminaFavorites = {
        hydrate,
        isFavorite,
        setFavoriteAsync
    };

    // The script is deferred, so the toggles that the server rendered are already in the document.
    hydrate(document);
})();
