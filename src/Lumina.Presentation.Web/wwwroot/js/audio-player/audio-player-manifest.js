/**
 * Playback manifest client of the audio player.
 *
 * It owns the transport concerns of a playback manifest: expanding the manifest route template with the Ids of a play
 * descriptor, appending the capability query, applying the request timeout, following the player's redirect policy, and
 * validating the stream path before a request that sends the session cookie is made. The playback engine and the later
 * normalized delivery pipeline both consume it, so the route, capability and same-origin rules are defined once.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The timeout, in milliseconds, of a playback manifest request.
     * @readonly
     * @type {number}
     */
    const MANIFEST_REQUEST_TIMEOUT_MS = 20000;

    /**
     * Gets the origin this module resolves relative URLs against. The page origin is used in a browser; a fixed local
     * origin keeps the route and origin checks testable in a host without a DOM.
     * @returns {string} The base origin.
     */
    function getBaseOrigin() {
        return typeof window !== 'undefined' && window.location && window.location.origin
            ? window.location.origin
            : 'http://localhost';
    }

    /**
     * Checks whether the module can compare a URL against the page origin.
     * @returns {boolean} True when a page origin is available.
     */
    function hasWindowOrigin() {
        return typeof window !== 'undefined' && !!window.location && !!window.location.origin;
    }

    /**
     * Resolves and validates a path that a request carrying the session cookie will fetch.
     *
     * The value is built by the server, but it is validated again here as defense in depth: it must be a root relative
     * path on this origin, with no backslash, control character, fragment, userinfo, dot segment, or encoded path
     * separator that a browser could normalize into another location. The resolved absolute URL is returned, pinned to
     * this origin by construction, so a base element can never redirect the fetch to another origin.
     * @param {*} value - The path to validate.
     * @returns {{ href: string, url: URL }} The validated resolved path and its parsed URL.
     * @throws {LuminaAudioPlayer.PlayerError} When the path is not a safe same-origin root relative path.
     */
    function resolveSameOriginUrl(value) {
        if (typeof value !== 'string' || value.length === 0)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The request carries no path.');
        if (!value.startsWith('/') || value.startsWith('//'))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path is not a root relative path.');
        if (/[\u0000-\u001f\u007f\\#]/.test(value))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path carries a forbidden character.');
        const queryIndex = value.indexOf('?');
        const path = queryIndex >= 0 ? value.slice(0, queryIndex) : value;
        // Decode every segment before judging it. The browser normalizes %2e%2e, %2f and %5c after the string is accepted,
        // so checking only the raw text would let an encoded separator or dot segment through.
        for (const segment of path.split('/')) {
            if (segment === '')
                continue;
            let decoded;
            try {
                decoded = decodeURIComponent(segment);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path carries an invalid escape.', { cause: error });
            }
            if (decoded === '.' || decoded === '..' || decoded.includes('/') || decoded.includes('\\')
                || /[\u0000-\u001f\u007f]/.test(decoded))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path carries a traversal segment.');
        }
        // Resolve last, so the returned href is absolute and a base element cannot change where the fetch goes. A host
        // without a DOM uses the synthetic base only for parsing and keeps the raw path.
        let resolved;
        try {
            resolved = new URL(value, getBaseOrigin());
        } catch (error) {
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path is not a valid URL.', { cause: error });
        }
        // A request with credentials or a non-http scheme is not one this player is allowed to make, and an origin other
        // than the page origin would send the session cookie somewhere it must never go.
        if ((resolved.protocol !== 'http:' && resolved.protocol !== 'https:')
            || (hasWindowOrigin() && resolved.origin !== window.location.origin)
            || resolved.username !== ''
            || resolved.password !== '')
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The path is not on this origin.');
        return { href: hasWindowOrigin() ? resolved.href : value, url: resolved };
    }

    /**
     * Checks that a resolved stream path matches the stream route template this application emitted and belongs to the
     * expected track and library. The template is the single source of truth for the route's static segments, their order
     * and its placeholders, so a server route rename or an inserted segment does not need a matching client edit. The
     * same-origin check alone is not enough: the session cookie and the Range header are sent before the response is
     * inspected, so a state changing GET route on this origin must never be reachable as a stream.
     * @param {URL} url - The resolved stream URL.
     * @param {Object} descriptor - The play descriptor the manifest belongs to.
     * @param {Object} configuration - The endpoints configuration of the player.
     * @throws {LuminaAudioPlayer.PlayerError} When the route template is missing, is on another origin, or the path does not match the route or the expected track and library Ids.
     */
    function validateStreamRoute(url, descriptor, configuration) {
        const templateValue = configuration && configuration.streamTrackRouteTemplate;
        if (typeof templateValue !== 'string' || templateValue.length === 0)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream route template is not configured.');
        let template;
        try {
            template = new URL(templateValue, getBaseOrigin());
        } catch (error) {
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream route template is not a valid URL.', { cause: error });
        }
        if ((template.protocol !== 'http:' && template.protocol !== 'https:')
            || (hasWindowOrigin() && template.origin !== window.location.origin)
            || template.origin !== url.origin
            || template.username !== ''
            || template.password !== '')
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream route template is not on this origin.');
        const expectedIds = {
            artistId: descriptor && descriptor.artistId,
            albumId: descriptor && descriptor.albumId,
            trackId: descriptor && descriptor.trackId
        };
        const templateSegments = template.pathname.split('/');
        const streamSegments = url.pathname.split('/');
        if (templateSegments.length !== streamSegments.length)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream path does not match the stream route.');
        for (let index = 0; index < templateSegments.length; index++) {
            let templateSegment;
            try {
                // The URL parser percent-encodes braces, so a template segment has to be decoded before it can be recognized as a placeholder.
                templateSegment = decodeURIComponent(templateSegments[index]);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream route template carries an invalid escape.', { cause: error });
            }
            const placeholder = /^\{([A-Za-z0-9_]+)\}$/.exec(templateSegment);
            if (placeholder) {
                const name = placeholder[1];
                if (!Object.prototype.hasOwnProperty.call(expectedIds, name))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream route template carries an unknown placeholder.');
                const expected = expectedIds[name];
                if (!LuminaAudioPlayer.isGuid(expected))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, `The play descriptor carries no valid ${name}.`, { field: name });
                const actual = streamSegments[index];
                // The path must describe the track of this descriptor, not merely another track of the same library.
                if (!LuminaAudioPlayer.isGuid(actual) || actual.toLowerCase() !== expected.toLowerCase())
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream path does not belong to the expected track.');
                continue;
            }
            let actualSegment;
            try {
                // Decode the actual segment too, so a path base or a static segment that contains an encoded character still compares equal to the template it came from.
                actualSegment = decodeURIComponent(streamSegments[index]);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream path carries an invalid escape.', { cause: error });
            }
            if (templateSegment !== actualSegment)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream path does not match the stream route.');
        }
        const libraryId = descriptor && descriptor.libraryId;
        if (!LuminaAudioPlayer.isGuid(libraryId))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries no valid library Id.', { field: 'libraryId' });
        const requestedLibraryIds = url.searchParams.getAll('libraryId');
        if (requestedLibraryIds.length !== 1 || requestedLibraryIds[0].toLowerCase() !== libraryId.toLowerCase())
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The stream path does not belong to the expected library.');
    }

    /**
     * Resolves the stream path of a playback manifest into the URL the player fetches, after checking that it points at
     * the stream route of the expected track and library.
     * @param {Object} manifest - The playback manifest.
     * @param {Object} descriptor - The play descriptor the manifest belongs to.
     * @param {Object} configuration - The endpoints configuration of the player.
     * @returns {string} The validated resolved stream URL.
     * @throws {LuminaAudioPlayer.PlayerError} When the manifest carries no safe stream path or the path is not the
     * stream route of the expected track and library.
     */
    function resolveStreamUrl(manifest, descriptor, configuration) {
        const resolved = resolveSameOriginUrl(manifest && manifest.streamPath);
        validateStreamRoute(resolved.url, descriptor, configuration);
        return resolved.href;
    }

    /**
     * Checks that a resolved segment path matches the segment route template this application emitted and belongs to the expected track, library and segment index.
     * @param {URL} url - The resolved segment URL.
     * @param {Object} descriptor - The play descriptor the manifest belongs to.
     * @param {Object} configuration - The endpoints configuration of the player.
     * @param {number} segmentIndex - The zero based index of the segment.
     * @param {string} deliveryId - The opaque delivery handle the manifest reported.
     * @throws {LuminaAudioPlayer.PlayerError} When the route template is missing, is on another origin, or the path does not
     * match the route, the expected track and library, the expected segment index, or the expected delivery handle.
     */
    function validateSegmentStreamRoute(url, descriptor, configuration, segmentIndex, deliveryId) {
        const templateValue = configuration && configuration.streamTrackSegmentRouteTemplate;
        if (typeof templateValue !== 'string' || templateValue.length === 0)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment stream route template is not configured.');
        let template;
        try {
            template = new URL(templateValue, getBaseOrigin());
        } catch (error) {
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment stream route template is not a valid URL.', { cause: error });
        }
        if ((template.protocol !== 'http:' && template.protocol !== 'https:')
            || (hasWindowOrigin() && template.origin !== window.location.origin)
            || template.origin !== url.origin
            || template.username !== ''
            || template.password !== '')
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment stream route template is not on this origin.');
        const expectedIds = {
            artistId: descriptor && descriptor.artistId,
            albumId: descriptor && descriptor.albumId,
            trackId: descriptor && descriptor.trackId
        };
        const templateSegments = template.pathname.split('/');
        const streamSegments = url.pathname.split('/');
        if (templateSegments.length !== streamSegments.length)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not match the segment route.');
        for (let index = 0; index < templateSegments.length; index++) {
            let templateSegment;
            try {
                templateSegment = decodeURIComponent(templateSegments[index]);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment route template carries an invalid escape.', { cause: error });
            }
            const placeholder = /^\{([A-Za-z0-9_]+)\}$/.exec(templateSegment);
            let actualSegment;
            try {
                // Decode the actual segment too, so a path base or a static segment that contains an encoded character still compares equal to the template it came from.
                actualSegment = decodeURIComponent(streamSegments[index]);
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path carries an invalid escape.', { cause: error });
            }
            if (placeholder) {
                const name = placeholder[1];
                if (name === 'segmentIndex') {
                    if (!Number.isInteger(segmentIndex) || segmentIndex < 0 || actualSegment !== String(segmentIndex))
                        throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not belong to the expected segment index.');
                    continue;
                }
                if (name === 'deliveryId') {
                    // The path must name the delivery handle the manifest reported, not another delivery of the same track.
                    if (typeof deliveryId !== 'string' || actualSegment !== deliveryId)
                        throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not belong to the expected delivery.');
                    continue;
                }
                if (!Object.prototype.hasOwnProperty.call(expectedIds, name))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment route template carries an unknown placeholder.');
                const expected = expectedIds[name];
                if (!LuminaAudioPlayer.isGuid(expected))
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, `The play descriptor carries no valid ${name}.`, { field: name });
                // The path must describe the track of this descriptor, not merely another track of the same library.
                if (!LuminaAudioPlayer.isGuid(actualSegment) || actualSegment.toLowerCase() !== expected.toLowerCase())
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not belong to the expected track.');
                continue;
            }
            if (templateSegment !== actualSegment)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not match the segment route.');
        }
        const libraryId = descriptor && descriptor.libraryId;
        if (!LuminaAudioPlayer.isGuid(libraryId))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, 'The play descriptor carries no valid library Id.', { field: 'libraryId' });
        const requestedLibraryIds = url.searchParams.getAll('libraryId');
        if (requestedLibraryIds.length !== 1 || requestedLibraryIds[0].toLowerCase() !== libraryId.toLowerCase())
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment path does not belong to the expected library.');
    }

    /**
     * Resolves the segment path template of a normalized playback manifest into the URL of one segment.
     * @param {Object} manifest - The playback manifest.
     * @param {Object} descriptor - The play descriptor the manifest belongs to.
     * @param {Object} configuration - The endpoints configuration of the player.
     * @param {number} segmentIndex - The zero based index of the segment to fetch.
     * @returns {string} The validated resolved segment URL.
     * @throws {LuminaAudioPlayer.PlayerError} When the manifest carries no safe segment path, the index is invalid, or the
     * path is not the segment route of the expected track, library and index.
     */
    function resolveSegmentStreamUrl(manifest, descriptor, configuration, segmentIndex) {
        if (!Number.isInteger(segmentIndex) || segmentIndex < 0)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The segment index is not valid.');
        const deliveryId = manifest && manifest.deliveryId;
        // The delivery handle is an opaque server issued token, so it is confined to the characters a path segment can carry and never contains a
        // separator, a query, a fragment or a control character that could change the route it is substituted into.
        if (typeof deliveryId !== 'string' || !/^[0-9a-f]{32}$/.test(deliveryId))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The manifest carries no valid delivery handle.');
        const template = manifest && manifest.segmentStreamPathTemplate;
        if (typeof template !== 'string' || !template.includes('{deliveryId}') || !template.includes('{segmentIndex}'))
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The manifest carries no segment stream path template.');
        const value = template
            .split('{deliveryId}').join(deliveryId)
            .split('{segmentIndex}').join(String(segmentIndex));
        const resolved = resolveSameOriginUrl(value);
        validateSegmentStreamRoute(resolved.url, descriptor, configuration, segmentIndex, deliveryId);
        return resolved.href;
    }

    /**
     * Builds the URL of the playback manifest endpoint for a descriptor, including the capability list.
     * @param {Object} configuration - The endpoints configuration of the player.
     * @param {Object} descriptor - The play descriptor of the track.
     * @param {string} capabilities - The comma separated container and codec capability list, or an empty string.
     * @returns {string} The absolute manifest URL, pinned to this origin.
     * @throws {LuminaAudioPlayer.PlayerError} When the descriptor or the configuration is incomplete, or the endpoint is not on this origin.
     */
    function buildManifestUrl(configuration, descriptor, capabilities) {
        const template = configuration && configuration.getPlaybackManifestUrl;
        if (!template)
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest endpoint is not configured.');
        const ids = {
            artistId: descriptor.artistId,
            albumId: descriptor.albumId,
            trackId: descriptor.trackId,
            libraryId: descriptor.libraryId
        };
        for (const name of Object.keys(ids)) {
            if (!ids[name])
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, `The play descriptor carries no ${name}.`, { field: name });
            // The manifest route only accepts Guids, and a value that is not a Guid must never be interpolated into a path the browser resolves against this origin.
            if (!LuminaAudioPlayer.isGuid(ids[name]))
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.INVALID_DESCRIPTOR, `The play descriptor carries an invalid ${name}.`, { field: name });
        }
        let url = String(template)
            .split('{artistId}').join(encodeURIComponent(ids.artistId))
            .split('{albumId}').join(encodeURIComponent(ids.albumId))
            .split('{trackId}').join(encodeURIComponent(ids.trackId));
        const separator = url.includes('?') ? '&' : '?';
        url = `${url}${separator}libraryId=${encodeURIComponent(ids.libraryId)}`;
        if (capabilities)
            url = `${url}&capabilities=${encodeURIComponent(capabilities)}`;
        // The manifest template is request derived, and the manifest is the response that controls the next fetches, so
        // it is resolved against this origin and a different origin is refused instead of being requested.
        let resolved;
        try {
            resolved = new URL(url, getBaseOrigin());
        } catch (error) {
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest endpoint is not a valid URL.', { cause: error });
        }
        if ((resolved.protocol !== 'http:' && resolved.protocol !== 'https:')
            || (hasWindowOrigin() && resolved.origin !== window.location.origin)
            || resolved.username !== ''
            || resolved.password !== '')
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest endpoint is not on this origin.');
        return resolved.href;
    }

    /**
     * Creates an AbortSignal for a manifest request that aborts on the external signal or after the manifest timeout.
     * @param {AbortSignal} [externalSignal] - The signal of the caller, for example the load or prefetch signal.
     * @returns {{ signal: AbortSignal|undefined, dispose: Function }} The signal to use, and the cleanup that clears the timeout and the external listener.
     */
    function createTimedRequestSignal(externalSignal) {
        // A host without AbortController cannot combine a timeout with the caller's signal, so it passes that signal through. Cancellation still belongs to the caller in that case.
        if (typeof AbortController !== 'function')
            return { signal: externalSignal, dispose: () => {} };
        const controller = new AbortController();
        const abort = () => {
            try {
                controller.abort();
            } catch (error) {
                console.debug('Audio player: the manifest request could not be aborted.', error);
            }
        };
        const onExternalAbort = () => abort();
        if (externalSignal) {
            if (externalSignal.aborted)
                abort();
            else
                // A cancelled load or prefetch must cancel the manifest request too, otherwise a superseded prefetch keeps
                // a connection and its timeout alive.
                externalSignal.addEventListener('abort', onExternalAbort, { once: true });
        }
        // A manifest that never answers must not hold a load forever. The deadline is independent of the caller's signal, so a stalled server cannot leave the player loading indefinitely.
        const timer = setTimeout(abort, MANIFEST_REQUEST_TIMEOUT_MS);
        return {
            signal: controller.signal,
            dispose: () => {
                clearTimeout(timer);
                // Remove the listener on both success and failure, so a long lived external signal does not retain this request after it settled.
                if (externalSignal)
                    externalSignal.removeEventListener('abort', onExternalAbort);
            }
        };
    }

    /**
     * Fetches the playback manifest of a track from the web application.
     * @param {Object} options - The request options.
     * @param {Object} options.configuration - The endpoints configuration of the player.
     * @param {Object} options.descriptor - The play descriptor of the track.
     * @param {string} options.capabilities - The comma separated capability list, or an empty string.
     * @param {Function} options.transport - The JSON transport, in the shape of the player's fetchJsonAsync. It receives the
     * URL, an optional signal, a label and a request options object with <c>redirect: 'manual'</c>, so a redirect target is validated before the browser follows it.
     * @param {AbortSignal} [options.signal] - An optional signal that cancels the request, used by a cancelled prefetch.
     * @returns {Promise<Object>} The playback manifest.
     * @throws {LuminaAudioPlayer.PlayerError} When the descriptor is incomplete, the session expired, or the manifest could not be read.
     */
    async function fetchPlaybackManifestAsync(options) {
        const settings = options || {};
        const transport = settings.transport;
        if (typeof transport !== 'function')
            throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest transport is not configured.');
        const url = buildManifestUrl(settings.configuration, settings.descriptor, settings.capabilities || '');
        const request = createTimedRequestSignal(settings.signal);
        try {
            let data;
            try {
                data = await transport(url, request.signal, 'a playback manifest request', { redirect: 'manual' });
            } catch (error) {
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest of the track could not be requested.', { cause: error });
            }
            if (data === LuminaAudioPlayer.REDIRECTED)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The session expired while the playback manifest was requested.', { reason: LuminaAudioPlayer.PlayerErrorReason.SESSION_EXPIRED });
            if (data === undefined)
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest of the track could not be requested.');
            if (!data || typeof data !== 'object')
                throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.ENGINE_LOAD_FAILED, 'The playback manifest of the track carried no data.');
            return data;
        } finally {
            request.dispose();
        }
    }

    LuminaAudioPlayer.resolveStreamUrl = resolveStreamUrl;
    LuminaAudioPlayer.resolveSegmentStreamUrl = resolveSegmentStreamUrl;
    LuminaAudioPlayer.fetchPlaybackManifestAsync = fetchPlaybackManifestAsync;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
