/**
 * Demux capability module of the audio player.
 *
 * The server decides, before any audio is fetched, whether a track is a direct play or has to be normalized (remuxed or
 * transcoded), from the container and the codec the player advertises. This module produces that advertisement from the
 * demuxers that loaded and the codecs the browser can decode, and it resolves the two halves of the client side of the
 * same decision: which demuxer handles the container and codec of a track, and whether the browser can decode the codec
 * it produces.
 *
 * Matching a manifest to a demuxer
 * --------------------------------
 * The manifest carries the container and the codec of the stored audio as free form descriptions taken from the probe
 * result, so they are canonicalized here exactly as the server canonicalizes them, and matched against the demuxer
 * registry. A pair no registered demuxer handles is a track the bundled player cannot play directly.
 *
 * Verifying with the browser
 * --------------------------
 * A demuxer existing for a format is not proof that the browser can decode it: the codec may be missing from this
 * browser, or the codec might be refused for the sample rate or channel count of the track. Two checks are therefore
 * offered, in increasing cost and precision:
 *   - a cheap check against a representative configuration of the codec, which answers whether the browser can decode
 *     that codec at all, using the same probes the capability detector already runs;
 *   - a full check against the real configuration the demuxer extracted from the file, which is exact but requires the
 *     header of the track to have been read already.
 * The convenience factories below use the full check when they initialize the demuxer, and fall back to a clear
 * 'needs normalization' event whenever a demuxer is missing, the bytes cannot be parsed, or the browser refuses the
 * configuration, so a consumer can ask the server to normalize the track instead of failing.
 *
 * Advertising capabilities
 * ------------------------
 * The 'capabilities' query parameter of the manifest request is documented as the container and codec pairs the player can
 * demux and decode, so the advertisement is filtered by the cheap browser check, and a codec this browser cannot decode is
 * never advertised. Advertising it would make the server mark such a track a direct play, and the client would discover
 * the gap only after fetching the manifest and would have to normalize in a second round trip. The unfiltered, demux only
 * set is available for a caller with no detector, and it is what the server already assumes when it applies its default.
 *
 * The needs normalization event
 * -----------------------------
 * It is published on the player event bus as {@link LuminaAudioPlayer.PlayerEvent.NEEDS_NORMALIZATION}, with the payload
 * { container, codec, reason }, plus directPlay when the caller supplied it, where the reason is one of the
 * {@link LuminaAudioPlayer.PlayerErrorReason} values NO_DEMUXER, CODEC_UNSUPPORTED or DEMUXER_FAILED. A consumer that
 * asked for a normalized delivery uses it to request one. A normalized manifest, whose directPlay flag is false, is
 * refused by {@link LuminaAudioPlayer.createDemuxerForManifest} without an event, because the server already normalized
 * such a track on purpose and its stream is not in the stored container.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Resolves the demuxer descriptors, the browser capabilities and the event bus a capability module works with. Only
     * the registry has a process wide default, because it is the one collaborator whose contents are the registry itself;
     * the capability detector and the event bus are injected by the caller, like every other collaborator of the player,
     * so that this module never reaches into a running player whose instance comes and goes.
     * @param {Object} [options] - The options to resolve from.
     * @returns {Object} The resolved registry, capabilities and events.
     */
    function resolveDependencies(options = {}) {
        const settings = options || {};
        return {
            registry: settings.registry || LuminaAudioPlayer.demuxerRegistry,
            capabilities: settings.capabilities || null,
            events: settings.events || null
        };
    }

    /**
     * Requires an injected collaborator, failing loudly rather than silently degrading when it is missing.
     * @param {*} value - The collaborator to check.
     * @param {string} name - The name of the collaborator, for the message.
     * @returns {*} The collaborator.
     * @throws {TypeError} When the collaborator was not injected.
     */
    function requireInjected(value, name) {
        if (!value)
            throw new TypeError(`The demux capability resolver requires an injected ${name}.`);
        return value;
    }

    /**
     * Publishes the need for a normalized delivery on the event bus.
     * @param {Object} events - The event bus.
     * @param {string} container - The canonical container of the track.
     * @param {string} codec - The canonical codec of the track.
     * @param {string} reason - One of the {@link LuminaAudioPlayer.PlayerErrorReason} values.
     * @param {boolean} [directPlay] - The direct play flag of the manifest, when the caller has one.
     */
    function publishNeedsNormalization(events, container, codec, reason, directPlay) {
        if (!events || !LuminaAudioPlayer.PlayerEvent || typeof events.emit !== 'function')
            return;
        const payload = {
            container: LuminaAudioPlayer.canonicalizeDemuxerContainer(container),
            codec: LuminaAudioPlayer.canonicalizeDemuxerCodec(codec),
            reason
        };
        // The direct play flag is carried only when the caller supplied it: an omitted flag means the caller does not
        // know, which is not the same as a false flag, the documented signal that the server already normalized the
        // track. Coercing an omitted flag to false would make every caller that has no manifest look like one whose
        // track the server chose to normalize.
        if (directPlay !== undefined)
            payload.directPlay = directPlay === true;
        events.emit(LuminaAudioPlayer.PlayerEvent.NEEDS_NORMALIZATION, Object.freeze(payload));
    }

    /**
     * Detects the demuxers the bundled player loaded and the browser configurations it can decode, and answers whether a track can be played directly.
     */
    class DemuxerCapabilities {
        /**
         * Creates an instance of DemuxerCapabilities.
         * @param {Object} [options] - The options.
         * @param {LuminaAudioPlayer.DemuxerRegistry} [options.registry] - The demuxer registry. Defaults to the process wide registry, the one collaborator that is not injected.
         * @param {LuminaAudioPlayer.PlayerCapabilities} [options.capabilities] - The browser capability detector, injected by the caller. A method that probes without it fails loudly.
         * @param {Object} [options.events] - The player event bus, injected by the caller.
         */
        constructor(options = {}) {
            const resolved = resolveDependencies(options);
            this.registry = resolved.registry;
            this.capabilities = resolved.capabilities;
            this.events = resolved.events;
        }

        /**
         * Finds the demuxer descriptor of a container and codec pair.
         * @param {string} container - The container of the track, in any casing or as a free form description.
         * @param {string} codec - The codec of the track, in any casing or as a free form description.
         * @returns {Object|null} The descriptor, or null when no demuxer handles the pair.
         */
        match(container, codec) {
            return this.registry ? this.registry.find(container, codec) : null;
        }

        /**
         * Finds the demuxer descriptor of the stored format of a playback manifest.
         * @param {Object} manifest - The playback manifest.
         * @returns {Object|null} The descriptor, or null when no demuxer handles the stored format.
         */
        matchManifest(manifest) {
            if (!manifest)
                return null;
            return this.match(manifest.sourceContainer, manifest.sourceCodec);
        }

        /**
         * Asks the browser whether it can decode a codec at all, through the representative probe the descriptor carries.
         * It is the cheap check, usable before any byte of the track is read. The probe is read from the descriptor rather
         * than looked up by name, so the check never depends on a demuxer being named in the registry.
         * @param {Object} descriptor - A demuxer descriptor.
         * @returns {Promise<boolean>} True when the browser can decode the codec.
         * @throws {TypeError} When no capability detector was injected.
         */
        async isCodecSupportedAsync(descriptor) {
            if (!descriptor)
                return false;
            const capabilities = requireInjected(this.capabilities, 'capability detector');
            return await capabilities.supportsAudioConfigAsync(descriptor.probe);
        }

        /**
         * Asks the browser whether it can decode the exact configuration a demuxer extracted from a track. It is the
         * exact check, usable once the demuxer was initialized.
         * @param {Object} configuration - A configuration, per {@link LuminaAudioPlayer.Demuxer#getConfiguration}.
         * @returns {Promise<boolean>} True when the browser can decode the configuration.
         * @throws {TypeError} When no capability detector was injected.
         */
        async isConfigurationSupportedAsync(configuration) {
            if (!configuration)
                return false;
            const capabilities = requireInjected(this.capabilities, 'capability detector');
            return await capabilities.supportsAudioConfigAsync(configuration);
        }

        /**
         * Resolves whether a track can be played directly, using the cheap representative check. It publishes the needs
         * normalization event and resolves false when no demuxer handles the format or the browser cannot decode it. The
         * event carries the reason the caller acts on, so the event bus is required rather than optional: a missing bus
         * would silently drop the only signal that says which delivery to request.
         * @param {string} container - The container of the track.
         * @param {string} codec - The codec of the track.
         * @param {boolean} [directPlay] - The direct play flag of the manifest, carried into the event.
         * @returns {Promise<boolean>} True when a demuxer exists and the browser can decode the codec.
         * @throws {TypeError} When no capability detector or no event bus was injected. It is a composition error, not a
         *     property of the track, so it rejects rather than classifying the track.
         */
        async canPlayDirectlyAsync(container, codec, directPlay) {
            const events = requireInjected(this.events, 'event bus');
            const descriptor = this.match(container, codec);
            if (!descriptor) {
                publishNeedsNormalization(events, container, codec, LuminaAudioPlayer.PlayerErrorReason.NO_DEMUXER, directPlay);
                return false;
            }
            if (!await this.isCodecSupportedAsync(descriptor)) {
                publishNeedsNormalization(events, container, codec, LuminaAudioPlayer.PlayerErrorReason.CODEC_UNSUPPORTED, directPlay);
                return false;
            }
            return true;
        }

        /**
         * Resolves whether the stored format of a manifest can be played directly.
         * @param {Object} manifest - The playback manifest.
         * @returns {Promise<boolean>} True when a demuxer exists and the browser can decode the codec.
         * @throws {TypeError} When no capability detector or no event bus was injected.
         */
        async canPlayManifestDirectlyAsync(manifest) {
            if (!manifest)
                return false;
            return await this.canPlayDirectlyAsync(manifest.sourceContainer, manifest.sourceCodec, manifest.directPlay);
        }

        /**
         * Builds the value of the 'capabilities' query parameter of the manifest request from the demuxers that loaded,
         * without consulting the browser. It is the set the player can demux, which is what the server already assumes
         * when it applies its own default; a caller that needs the set the player can also decode uses
         * {@link LuminaAudioPlayer.DemuxerCapabilities#describeFormatsAsync}.
         * @returns {string} A comma separated list of container/codec pairs.
         */
        describeFormats() {
            if (!this.registry)
                return '';
            return this.registry.getDescriptors()
                .map((descriptor) => `${descriptor.container.toLowerCase()}/${descriptor.codec.toLowerCase()}`)
                .join(',');
        }

        /**
         * Builds the value of the 'capabilities' query parameter of the manifest request from the demuxers whose codec the
         * browser can actually decode. The parameter is documented as the pairs the player can demux and decode, so a
         * codec the browser cannot decode must not be promised to the server: otherwise the server marks such a track a
         * direct play, and the client only discovers the gap after fetching the manifest and has to normalize in a second
         * round trip. With the memoizing capability detector, this is one probe per codec, once.
         * @returns {Promise<string>} A promise resolving to a comma separated list of container/codec pairs.
         * @throws {TypeError} When no capability detector was injected.
         */
        async describeFormatsAsync() {
            const capabilities = requireInjected(this.capabilities, 'capability detector');
            if (!this.registry)
                return '';
            const pairs = [];
            for (const descriptor of this.registry.getDescriptors()) {
                if (await capabilities.supportsAudioConfigAsync(descriptor.probe))
                    pairs.push(`${descriptor.container.toLowerCase()}/${descriptor.codec.toLowerCase()}`);
            }
            return pairs.join(',');
        }
    }

    LuminaAudioPlayer.DemuxerCapabilities = DemuxerCapabilities;

    /**
     * Creates a demux capability resolver. See {@link LuminaAudioPlayer.DemuxerCapabilities} for the options.
     * @param {Object} [options] - The options.
     * @returns {LuminaAudioPlayer.DemuxerCapabilities} The resolver.
     */
    LuminaAudioPlayer.createDemuxerCapabilities = function (options) {
        return new DemuxerCapabilities(options);
    };

    /**
     * Creates and verifies the demuxer of a container and codec pair.
     *
     * It resolves the demuxer, initializes it, and asks the browser whether it can decode the exact configuration the
     * demuxer extracted. A failure to parse the bytes or to decode the codec is an expected negotiation outcome, not a
     * playback failure, so the demuxer is created as a probe with the caller's error callback detached and that callback
     * is attached only once the demuxer proved usable. Anything that makes the pair unplayable (no demuxer, unparseable
     * bytes, or a codec the browser refuses) resolves to null and publishes a needs normalization event, so a caller can
     * request a normalized delivery instead of failing, and never receives both a player failure and a normalization
     * request for the same unplayable file.
     *
     * The one failure that resolves to null without publishing is an intentional download stop: a teardown that crossed
     * the header probe is not a track the server has to normalize. A caller that must tell the two nulls apart therefore
     * checks whether it received a needs normalization event, or its own teardown state, rather than the null itself.
     * @param {string} container - The container of the track.
     * @param {string} codec - The codec of the track.
     * @param {Object} [options] - The demuxer options, plus the injected capabilities and events, per {@link LuminaAudioPlayer.DemuxerCapabilities}.
     * @returns {Promise<LuminaAudioPlayer.Demuxer|null>} The ready demuxer, or null when the pair must be normalized.
     * @throws {TypeError} When no event bus was injected. It is a composition error, not a property of the track, so it
     *     rejects rather than resolving to a null the caller would have to classify without an event to classify it by.
     */
    LuminaAudioPlayer.createDemuxerFor = async function (container, codec, options) {
        const settings = options || {};
        const dependencies = resolveDependencies(settings);
        // The event bus is required rather than optional, because a caller tells a track that must be normalized from an
        // intentional teardown by whether a needs normalization event arrived, and a missing bus would silently drop that
        // only signal and make the two nulls indistinguishable.
        const events = requireInjected(dependencies.events, 'event bus');
        const descriptor = dependencies.registry ? dependencies.registry.find(container, codec) : null;
        if (!descriptor) {
            publishNeedsNormalization(events, container, codec, LuminaAudioPlayer.PlayerErrorReason.NO_DEMUXER, settings.directPlay);
            return null;
        }
        let demuxer = null;
        try {
            demuxer = descriptor.create(Object.assign({}, settings, { onError: null }));
            await demuxer.initializeAsync();
        } catch (error) {
            console.debug('Audio player: the demuxer of the track failed to initialize.', error);
            if (demuxer && typeof demuxer.destroyAsync === 'function')
                await demuxer.destroyAsync();
            // An intentional download stop, a teardown that crossed the header probe, is not a track the server has to
            // normalize, so it is not published as one; the demuxer already reports it as a normal stop everywhere else.
            if (!LuminaAudioPlayer.isDownloadAbort(error))
                publishNeedsNormalization(events, container, codec, LuminaAudioPlayer.PlayerErrorReason.DEMUXER_FAILED, settings.directPlay);
            return null;
        }
        const configuration = demuxer.getConfiguration();
        let isConfigurationSupported = false;
        try {
            const resolver = new DemuxerCapabilities(dependencies);
            isConfigurationSupported = await resolver.isConfigurationSupportedAsync(configuration);
        } catch (error) {
            // A wiring failure (a missing injected capability detector) is not a property of the track, so the just built
            // demuxer is released and the failure is propagated, never swallowed as a normalization request.
            await demuxer.destroyAsync();
            throw error;
        }
        if (!isConfigurationSupported) {
            await demuxer.destroyAsync();
            publishNeedsNormalization(events, container, codec, LuminaAudioPlayer.PlayerErrorReason.CODEC_UNSUPPORTED, settings.directPlay);
            return null;
        }
        // The demuxer is usable, so the caller's error callback is attached from here on: a failure while reading is a
        // playback failure and reaches it, while a failure during the probe above never did.
        demuxer.onError = typeof settings.onError === 'function' ? settings.onError : null;
        return demuxer;
    };

    /**
     * Creates and verifies the demuxer of the stored format of a direct play manifest. It uses the source container and
     * codec, because that is the format the capability negotiation is about. Only a direct play manifest has its stored
     * format as its stream: a manifest whose direct play flag is false describes a delivery in a different container, so
     * resolving its stored format would build a demuxer for the wrong bytes, and the failure to initialize it would
     * publish a normalization request for a track the server already normalized, a normalization loop. Such a manifest
     * therefore resolves to null here, silently, and a caller that has one runs the normalized pipeline instead.
     * @param {Object} manifest - The playback manifest.
     * @param {Object} [options] - The demuxer options, per {@link LuminaAudioPlayer.createDemuxerFor}.
     * @returns {Promise<LuminaAudioPlayer.Demuxer|null>} The ready demuxer, or null for a manifest that is not a direct play, or one that must be normalized.
     */
    LuminaAudioPlayer.createDemuxerForManifest = function (manifest, options) {
        if (!manifest || manifest.directPlay !== true)
            return Promise.resolve(null);
        return LuminaAudioPlayer.createDemuxerFor(manifest.sourceContainer, manifest.sourceCodec, Object.assign({}, options, { directPlay: true }));
    };

    /**
     * Builds the value of the 'capabilities' query parameter of the manifest request from the process wide registry,
     * without consulting the browser. See {@link LuminaAudioPlayer.describeDemuxerFormatsAsync} for the decode aware set.
     * @returns {string} A comma separated list of container/codec pairs.
     */
    LuminaAudioPlayer.describeDemuxerFormats = function () {
        return new DemuxerCapabilities().describeFormats();
    };

    /**
     * Builds the value of the 'capabilities' query parameter of the manifest request from the process wide registry,
     * keeping only the pairs whose codec the browser can decode, which is the set the parameter is documented to carry.
     * @param {Object} [options] - The options, with the injected capability detector and event bus, per {@link LuminaAudioPlayer.DemuxerCapabilities}.
     * @returns {Promise<string>} A promise resolving to a comma separated list of container/codec pairs.
     * @throws {TypeError} When no capability detector was injected.
     */
    LuminaAudioPlayer.describeDemuxerFormatsAsync = function (options) {
        return new DemuxerCapabilities(options).describeFormatsAsync();
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
