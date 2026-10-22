/**
 * Shared add-to-playlist actions for the music library, artist and album cards, and for the player queue.
 *
 * The module owns one modal playlist picker. A page opens it with the scope or the single track it wants to add. A scope
 * is resolved by the server into its ordered track Ids and appended in one request; a single track is appended through
 * the single entry route. The actual card and track menu items are small wrappers that give the menu item its scope and
 * call <c>LuminaPlaylists.showAddScopeDialogAsync</c>. A theme template or a page script can also render a declarative
 * action, which needs no script of its own: an element with the `data-playlist-add` marker and the scope attributes
 * documented in the theming guide opens the same picker through the delegated click listener of this module.
 *
 * The scope is resolved on the server so the browser never enumerates a whole library, and the server can add all of its
 * tracks to the playlist atomically. A single track is already identified, so it uses the cheaper single-entry route
 * instead. The picker is built here, not rendered by each page, so the cards, the album and artist pages and the player
 * all share one dialog, one focus behavior and one error path. After a successful add the module dispatches the
 * `lumina:playlist-updated` DOM event: an open playlist page can refresh itself, while the callers stay decoupled from it.
 */
(function () {
    'use strict';

    // The layout may render this script on a page that already ran it, and the in-place navigation can evaluate scripts
    // again. A second set of listeners would send every request twice, so the first load owns the document.
    if (window.LuminaPlaylists)
        return;

    // The declarative add action: an element that carries the marker and the scope attributes documented in the theming
    // guide. The listener reads them when the user clicks, so a page can render the action before its data is known.
    const ADD_TRIGGER_SELECTOR = '[data-playlist-add]';

    // The layout renders the playlist id of the route as an empty Guid because the id is not known until the user picks
    // a playlist here. Replacing this sentinel keeps the route construction in one place and avoids hand-built paths.
    const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
    // The picker ids must be unique per dialog, because the toggle and the title are linked by id. The sequence makes a
    // second dialog safe even if a previous one was removed abruptly.
    let pickerSequence = 0;
    // Only one picker may be open: two overlays would compete for Escape and focus, and the user could add to two
    // different playlists. This holds the close function and the overlay of the current one.
    let activePicker = null;

    /**
     * Gets the playlist configuration the layout renders for every page.
     *
     * The configuration is the only contract between the layout and this module: it carries the routes and the localized
     * labels. Reading it lazily, instead of capturing it when the script loads, lets the layout render the configuration
     * after this deferred script has been parsed.
     * @returns {Object|null} The configuration, or null when the layout did not render it.
     */
    function getConfiguration() {
        return window.playlistConfiguration || null;
    }

    /**
     * Gets the localized strings of the playlist actions, with empty values when the layout did not render them.
     * @returns {Object} The localized strings.
     */
    function getStrings() {
        const configuration = getConfiguration();
        return configuration && configuration.strings ? configuration.strings : {};
    }

    /**
     * Shows a success or error notification through the global notification service, with a console fallback.
     *
     * The notification service is provided by the layout, but the module must not assume it exists: it is also loaded
     * on minimal pages and in test hosts. Falling back to the console keeps a failure visible without throwing from a
     * UI action, which would leave the picker stuck in its busy state.
     * @param {string} message - The localized message to show.
     * @param {boolean} isError - True to show an error, false for a success.
     */
    function showNotification(message, isError) {
        if (!message)
            return;
        if (typeof notificationService !== 'undefined' && typeof NotificationType !== 'undefined') {
            notificationService.show(message, isError ? NotificationType.ERROR : NotificationType.SUCCESS);
            return;
        }
        if (isError)
            console.error(`Playlists: ${message}`);
        else
            console.info(`Playlists: ${message}`);
    }

    /**
     * Builds the standard application combo box used to pick a playlist.
     *
     * The application has one combobox contract: the markup below is styled by the global stylesheet, and the global
     * site script handles opening, closing and copying the clicked option into the selected text. The module must not
     * add its own selection logic on top of that, or the two would fight. It builds the same structure a Razor view
     * would render, so the picker looks and behaves like every other select in the application.
     * @param {string} id - The unique root Id of the combo box.
     * @param {string} placeholder - The placeholder shown before a playlist is selected.
     * @returns {Object} The combo box elements.
     */
    function createCombobox(id, placeholder) {
        const root = document.createElement('div');
        root.className = 'enlightenment-combobox w-100';
        root.id = id;

        // The shine elements are purely decorative. They are kept when the options are rebuilt, so the dropdown does not
        // flicker or lose its inset shadow while the playlist request refreshes the rows.
        const shine = document.createElement('div');
        shine.className = 'shine-effect';
        shine.style.top = '1px';
        root.appendChild(shine);

        const toggle = document.createElement('input');
        toggle.type = 'checkbox';
        toggle.className = 'enlightenment-toggle-checkbox';
        toggle.id = `${id}-toggle`;
        root.appendChild(toggle);

        const label = document.createElement('label');
        label.className = 'enlightenment-toggle';
        label.htmlFor = toggle.id;
        const selected = document.createElement('span');
        selected.className = 'enlightenment-selected-text';
        selected.setAttribute('data-value', '');
        selected.textContent = placeholder;
        label.appendChild(selected);
        root.appendChild(label);

        const dropdown = document.createElement('div');
        dropdown.className = 'enlightenment-dropdown';
        const dropdownShine = document.createElement('div');
        dropdownShine.className = 'shine-effect';
        dropdownShine.style.top = '-4px';
        dropdown.appendChild(dropdownShine);
        // The disabled first option is the "nothing selected" row the global combobox script expects. It also gives
        // the dropdown a stable first child when the user has no playlists yet.
        const firstOption = document.createElement('div');
        firstOption.className = 'enlightenment-option first-option disabled';
        firstOption.setAttribute('data-value', '');
        firstOption.textContent = placeholder;
        dropdown.appendChild(firstOption);
        root.appendChild(dropdown);

        return { root, selected, dropdown, toggle };
    }

    /**
     * Fills the playlist combo box with the playlists of the user.
     *
     * Only the option rows are replaced. The first option and the decorative elements are kept because the global site
     * script and the stylesheet rely on them, and rebuilding the whole dropdown would reset its open state and focus.
     * @param {Object} combobox - The combo box elements.
     * @param {Array<Object>} playlists - The playlists to list.
     * @param {string} placeholder - The placeholder shown before a playlist is selected.
     */
    function fillPlaylists(combobox, playlists, placeholder) {
        combobox.selected.textContent = placeholder;
        combobox.selected.setAttribute('data-value', '');
        combobox.dropdown.querySelectorAll('.enlightenment-option:not(.first-option)').forEach(option => option.remove());
        const firstOption = combobox.dropdown.querySelector('.first-option');
        if (firstOption) {
            firstOption.textContent = placeholder;
            firstOption.setAttribute('data-value', '');
        }
        for (const playlist of playlists) {
            // A malformed entry has no id to send to the server and no value to select. Skipping it keeps the rest of
            // the dialog usable instead of rendering an option that cannot be added to.
            if (!playlist || !playlist.id)
                continue;
            const option = document.createElement('div');
            option.className = 'enlightenment-option';
            option.setAttribute('data-value', String(playlist.id));
            option.textContent = playlist.name || '';
            combobox.dropdown.appendChild(option);
        }
        // The stylesheet rounds the last row; it must be recomputed whenever the list changes, because the previous
        // last row becomes an ordinary row.
        const options = combobox.dropdown.querySelectorAll('.enlightenment-option:not(.first-option)');
        options.forEach((option, index) => option.classList.toggle('last-option', index === options.length - 1));
    }

    /**
     * Builds the scope add route of a playlist by replacing the empty Id placeholder with the selected playlist Id.
     *
     * The layout renders the route with an empty Guid because the playlist is not known until the user picks one. The
     * id is encoded so it stays a single path segment even if the DOM value is malformed.
     * @param {string} playlistId - The Id of the selected playlist.
     * @returns {string} The route that appends the scope to the playlist.
     */
    function buildScopeAddUrl(playlistId) {
        const configuration = getConfiguration();
        const template = configuration ? configuration.addScopeEntriesUrl : '';
        if (typeof template !== 'string' || template.length === 0)
            return '';
        return template.replace(EMPTY_GUID, encodeURIComponent(playlistId));
    }

    /**
     * Builds the single entry route of a playlist by replacing the empty Id placeholder with the selected playlist Id.
     *
     * A single track is already identified, so it does not need the server to resolve a scope. It uses the entry route,
     * which reports the added entry back to the caller.
     * @param {string} playlistId - The Id of the selected playlist.
     * @returns {string} The route that appends one track to the playlist.
     */
    function buildTrackAddUrl(playlistId) {
        const configuration = getConfiguration();
        const template = configuration ? configuration.addPlaylistEntryUrl : '';
        if (typeof template !== 'string' || template.length === 0)
            return '';
        return template.replace(EMPTY_GUID, encodeURIComponent(playlistId));
    }

    /**
     * Opens the playlist picker for a library, artist or album scope, or for one track, and appends it to the selected playlist.
     *
     * One picker serves every caller. It is created on demand and removed on close, so no hidden dialog stays in the
     * layout, and the element that opened it gets the keyboard focus back. The playlist list is loaded only when the
     * dialog opens, so a page that never uses the action does not make a request.
     * @param {Object} scope - The scope to add. It carries libraryId, and optionally artistId, albumId, trackId and title.
     * @returns {Promise<void>} A promise that completes when the picker has opened.
     */
    async function showAddScopeDialogAsync(scope) {
        const configuration = getConfiguration();
        // Without the layout configuration there is no route to post to, and without a library there is no scope to
        // resolve. The player can also call this module, so the check must not assume a page with playlist menus.
        if (!configuration || !scope || !scope.libraryId || typeof document === 'undefined')
            return;
        // A second picker would compete for Escape and focus, and the user could add to two different playlists.
        if (activePicker)
            activePicker.close();

        const strings = getStrings();
        const placeholder = strings.selectPlaylist || 'Select a playlist';
        // The caller's label is used for the title and the button, so the dialog wording matches the page it was opened
        // from instead of always saying "Add to playlist".
        const addLabel = scope.addToPlaylistLabel || 'Add to playlist';
        const pickerId = `playlist-picker-${++pickerSequence}`;
        // Remember the trigger before the dialog takes focus, so closing it, including with Escape, returns the user to
        // the control they used. The caller may pass the element explicitly because a menu item can be re-rendered.
        const previousFocus = scope.returnFocus && scope.returnFocus.isConnected ? scope.returnFocus : document.activeElement;

        const overlay = document.createElement('div');
        overlay.className = 'playlist-picker-overlay';
        overlay.addEventListener('click', event => {
            // Only the backdrop closes the dialog. A click inside the content must not dismiss it while the user is
            // choosing a playlist or reading an error.
            if (event.target === overlay)
                close();
        });

        const dialog = document.createElement('div');
        dialog.className = 'dialog';
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-labelledby', `${pickerId}-title`);
        overlay.appendChild(dialog);

        const content = document.createElement('div');
        content.className = 'dialog-content';
        dialog.appendChild(content);

        const header = document.createElement('div');
        header.className = 'dialog-header';
        const title = document.createElement('h2');
        title.id = `${pickerId}-title`;
        title.textContent = addLabel;
        header.appendChild(title);
        content.appendChild(header);

        const body = document.createElement('div');
        body.className = 'playlist-picker-body';
        content.appendChild(body);

        if (scope.title) {
            // The same dialog is opened from a library, an artist, an album and a track menu. Showing the title tells the
            // user what the action will add before they choose the playlist.
            const scopeLabel = document.createElement('p');
            scopeLabel.className = 'playlist-picker-scope';
            scopeLabel.textContent = scope.title;
            scopeLabel.title = scope.title;
            body.appendChild(scopeLabel);
        }

        const combobox = createCombobox(`${pickerId}-combobox`, placeholder);
        body.appendChild(combobox.root);

        const emptyMessage = document.createElement('p');
        emptyMessage.className = 'playlist-picker-empty hidden';
        emptyMessage.textContent = strings.noPlaylists || '';
        if (configuration.createPlaylistUrl) {
            const createLink = document.createElement('a');
            createLink.className = 'nav-link';
            createLink.href = configuration.createPlaylistUrl;
            createLink.textContent = strings.createPlaylist || '';
            // The link navigates to the dedicated create page, so the dialog is closed first. Leaving the overlay in the
            // page during the in-place navigation would cover the new page.
            createLink.addEventListener('click', () => close());
            emptyMessage.appendChild(document.createTextNode(' '));
            emptyMessage.appendChild(createLink);
        }
        body.appendChild(emptyMessage);

        const buttons = document.createElement('div');
        buttons.className = 'dialog-buttons';
        content.appendChild(buttons);

        const cancelButton = document.createElement('button');
        cancelButton.type = 'button';
        cancelButton.className = 'dialog-button abort-button';
        cancelButton.textContent = (typeof jsLocalizedDialogs !== 'undefined' && jsLocalizedDialogs.cancel) || 'Cancel';
        cancelButton.addEventListener('click', () => close());
        buttons.appendChild(cancelButton);

        const addButton = document.createElement('button');
        addButton.type = 'button';
        addButton.className = 'dialog-button confirm-button';
        addButton.textContent = addLabel;
        // Nothing can be added until the playlist request has returned and the user has made a choice.
        addButton.disabled = true;
        buttons.appendChild(addButton);

        const keyHandler = event => {
            // Escape is the standard way to dismiss a modal. The listener is registered on the document, so close removes it.
            if (event.key === 'Escape')
                close();
        };

        /** Closes the picker, removes its global listener and returns the focus to the element that opened it. */
        const close = () => {
            // The dialog may already have been closed by a successful add, a backdrop click or another picker. Removing
            // an already removed overlay and restoring focus twice would be harmless but noisy, so stop here.
            if (!document.body.contains(overlay))
                return;
            // The key listener is global, so it must be removed with the dialog; otherwise Escape would keep invoking a
            // close function that no longer has a dialog to close.
            document.removeEventListener('keydown', keyHandler);
            overlay.remove();
            if (activePicker && activePicker.overlay === overlay)
                activePicker = null;
            if (previousFocus && typeof previousFocus.focus === 'function' && previousFocus.isConnected)
                previousFocus.focus();
        };

        activePicker = { close, overlay };
        document.addEventListener('keydown', keyHandler);
        document.body.appendChild(overlay);
        // Loading starts immediately, so the toggle stays disabled until the choices arrive; opening it earlier would
        // show the placeholder as if it were a selectable playlist.
        combobox.toggle.disabled = true;

        /** Fetches the playlists of the user and fills the picker, or shows the empty state and disables the action. */
        const loadPlaylistsAsync = async () => {
            const playlists = await callApiGetAsync(configuration.getPlaylistsUrl);
            // The dialog may have been closed while the request was in flight. Touching its removed DOM would throw or
            // resurrect a stale overlay.
            if (!document.body.contains(overlay))
                return;
            if (!Array.isArray(playlists)) {
                // The API helper returns null on a failed request. An empty, unusable dialog is worse than closing it
                // and reporting the failure.
                close();
                showNotification(strings.tracksAddFailed, true);
                return;
            }
            if (playlists.length === 0) {
                // There is nothing to choose. Disable the action and show the link that creates the first playlist,
                // instead of letting the user press Add and receive an error.
                combobox.selected.textContent = strings.noPlaylists || placeholder;
                combobox.selected.setAttribute('data-value', '');
                emptyMessage.classList.remove('hidden');
                combobox.toggle.disabled = true;
                addButton.disabled = true;
                return;
            }
            fillPlaylists(combobox, playlists, placeholder);
            combobox.toggle.disabled = false;
            addButton.disabled = false;
            // Move the keyboard focus into the dialog once it has content, so a keyboard user is not left on the trigger
            // behind the modal.
            addButton.focus();
        };

        /**
         * Adds the chosen scope or track to the selected playlist, then closes the picker and announces the update.
         * The two routes differ because a single track is already identified, while a scope is resolved on the server.
         */
        addButton.addEventListener('click', async () => {
            const playlistId = combobox.selected.getAttribute('data-value') || '';
            if (!playlistId) {
                // The placeholder row is present in the markup but carries an empty value, so it means no selection.
                showNotification(strings.selectPlaylist, true);
                return;
            }
            // The request may be slow. Disabling both controls prevents a second click from appending the same content
            // twice, and makes the busy state visible.
            addButton.disabled = true;
            combobox.toggle.disabled = true;
            try {
                // A single track is already identified and uses the cheaper entry route. Everything else is a scope
                // that the server resolves into its ordered tracks, so the browser never has to enumerate a library.
                const isTrack = typeof scope.trackId === 'string' && scope.trackId.length > 0;
                const addUrl = isTrack ? buildTrackAddUrl(playlistId) : buildScopeAddUrl(playlistId);
                if (!addUrl) {
                    // The layout may not have rendered the route. Fail visibly instead of posting to an empty URL.
                    showNotification(strings.tracksAddFailed, true);
                    return;
                }
                const result = await callApiPostAsync(addUrl, isTrack
                    ? { trackId: scope.trackId }
                    : {
                        libraryId: scope.libraryId,
                        // Empty artist and album values mean "the whole scope"; sending them as null keeps the request
                        // shape explicit and avoids sending empty strings that the server would have to normalize.
                        artistId: scope.artistId || null,
                        albumId: scope.albumId || null
                    });
                if (!result) {
                    // The API helper already reported the failure; do not dispatch an update event for an add that did
                    // not happen.
                    showNotification(strings.tracksAddFailed, true);
                    return;
                }
                if (isTrack && !result.entry) {
                    // The single-entry response must contain the entry it added. A missing entry means the server did
                    // not append it, so this is a failure even if the HTTP call succeeded.
                    showNotification(strings.tracksAddFailed, true);
                    return;
                }
                if (!isTrack && result.addedCount === 0) {
                    // The scope resolved to no tracks, for example an empty album or an artist without releases. That
                    // is not a success and should not close the dialog silently.
                    showNotification(strings.noTracksToAdd, true);
                    return;
                }
                showNotification(strings.tracksAdded, false);
                // Let an open playlist page refresh its window without coupling this module to that page.
                document.dispatchEvent(new CustomEvent('lumina:playlist-updated', { detail: result }));
                close();
            } catch (error) {
                // callApiPostAsync normally resolves to null on failure; this catch is for unexpected errors, such as a
                // rejected promise from a test double. Keep the dialog open so the user can retry.
                console.error('Playlists: adding to a playlist failed.', error);
                showNotification(strings.tracksAddFailed, true);
            } finally {
                // After a success the dialog is already removed, so only an open dialog is re-enabled.
                if (document.body.contains(overlay)) {
                    addButton.disabled = false;
                    combobox.toggle.disabled = false;
                }
            }
        });

        loadPlaylistsAsync().catch(error => {
            // A rejected request leaves the dialog unusable: there are no choices and no way to load them again. Close
            // it and surface the failure instead of leaving an empty picker on screen.
            console.error('Playlists: loading the playlists failed.', error);
            if (document.body.contains(overlay)) {
                close();
                showNotification(strings.tracksAddFailed, true);
            }
        });
    }

    /**
     * Reads the scope of a declarative add action from its data attributes.
     *
     * The attributes are read when the click happens, so a page or a theme may render the action before the scope is
     * known and write the values later, which is what the audio player does for the current track. A missing library Id
     * makes the action a no-op, because every scope is resolved inside one library.
     * @param {Element} trigger - The element that carries the `data-playlist-add` marker.
     * @returns {Object|null} The scope, or null when the action has no library to resolve the scope in.
     */
    function readScopeFromTrigger(trigger) {
        const dataset = trigger && trigger.dataset ? trigger.dataset : null;
        if (!dataset || !dataset.playlistLibraryId)
            return null;
        return {
            libraryId: dataset.playlistLibraryId,
            trackId: dataset.playlistTrackId || '',
            albumId: dataset.playlistAlbumId || '',
            artistId: dataset.playlistArtistId || '',
            title: dataset.playlistTitle || '',
            addToPlaylistLabel: getStrings().addToPlaylist || '',
            returnFocus: trigger
        };
    }

    /**
     * Opens the picker for a declarative add action, through event delegation.
     *
     * The module binds one listener on the document, so a control works anywhere in the swapped page content and inside
     * the player bar, including controls that a page or a theme renders later. The default action is prevented, so a link
     * does not navigate, and further bubbling is stopped once the event reaches the document. A handler on an ancestor
     * that already ran before the document listener is not undone; the player queue ignores controls inside its entries,
     * and a menu that hosts such an action closes itself in its own handler.
     * @param {MouseEvent} event - The click event.
     */
    function handleAddTriggerClick(event) {
        const target = event.target;
        if (!target || typeof target.closest !== 'function')
            return;
        const trigger = target.closest(ADD_TRIGGER_SELECTOR);
        if (!trigger)
            return;
        // A disabled control explains itself through its text; opening a picker from it would be a surprising action.
        if (trigger.disabled === true || trigger.getAttribute('aria-disabled') === 'true')
            return;
        const scope = readScopeFromTrigger(trigger);
        if (!scope)
            return;
        event.preventDefault();
        event.stopPropagation();
        showAddScopeDialogAsync(scope).catch(error => {
            console.error('Playlists: opening the add-to-playlist picker failed.', error);
            showNotification(getStrings().tracksAddFailed, true);
        });
    }

    document.addEventListener('click', handleAddTriggerClick);

    window.LuminaPlaylists = {
        showAddScopeDialogAsync
    };
})();
