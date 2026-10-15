/**
 * Capability detection of the audio player.
 *
 * The player decodes with WebCodecs and outputs through the Web Audio API, so before it can play anything it must know which of those browser capabilities exist, and which audio codecs the
 * browser can decode. The report is produced once, is cached, and is published through the event bus, so that a consumer can present a clear failure instead of a silent, broken player.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Enum-like object for the browser capabilities the player requires.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerCapability = Object.freeze({
        // The WebCodecs API, the family of decoders the player feeds.
        WEB_CODECS: 'webCodecs',
        // WebCodecs.AudioDecoder, the audio decoder the player actually consumes.
        AUDIO_DECODER: 'audioDecoder',
        // The Web Audio API, used for output.
        AUDIO_CONTEXT: 'audioContext'
    });

    /**
     * The codec configurations probed with AudioDecoder.isConfigSupported, one per audio codec the demuxers of the player can recognize.
     *  Each entry is a representative configuration; the returned report says whether the browser can decode that codec at all.
     * @readonly
     * @type {ReadonlyArray<{ name: string, config: Object }>}
     */
    LuminaAudioPlayer.PROBED_AUDIO_CODECS = Object.freeze([
        Object.freeze({ name: 'mp3', config: Object.freeze({ codec: 'mp3', sampleRate: 44100, numberOfChannels: 2 }) }),
        Object.freeze({ name: 'flac', config: Object.freeze({ codec: 'flac', sampleRate: 44100, numberOfChannels: 2 }) }),
        Object.freeze({ name: 'opus', config: Object.freeze({ codec: 'opus', sampleRate: 48000, numberOfChannels: 2 }) }),
        Object.freeze({ name: 'aac', config: Object.freeze({ codec: 'mp4a.40.2', sampleRate: 44100, numberOfChannels: 2 }) }),
        Object.freeze({ name: 'vorbis', config: Object.freeze({ codec: 'vorbis', sampleRate: 44100, numberOfChannels: 2 }) }),
        Object.freeze({ name: 'pcm', config: Object.freeze({ codec: 'pcm-s16', sampleRate: 44100, numberOfChannels: 2 }) })
    ]);

    /**
     * Detects the browser capabilities the audio player relies on, and caches the result.
     */
    class PlayerCapabilities {
        /**
         * Creates an instance of PlayerCapabilities.
         * @param {Window} [targetWindow] - The window whose capabilities are inspected. Defaults to the current window.
         */
        constructor(targetWindow = window) {
            this.window = targetWindow;
            /** @type {Object|null} */
            this.report = null;
            /** @type {Promise<Object>|null} */
            this.pendingReport = null;
        }

        /**
         * Checks whether the WebCodecs API is present.
         * @returns {boolean} True when the browser exposes at least one WebCodecs decoder.
         */
        hasWebCodecs() {
            // The audio side of WebCodecs is what the player feeds, but any decoder of the family implies the API is exposed
            // at all; the precise check of the decoder the player consumes is hasAudioDecoder below.
            return typeof this.window.AudioDecoder === 'function' || typeof this.window.VideoDecoder === 'function';
        }

        /**
         * Checks whether WebCodecs.AudioDecoder is present.
         * @returns {boolean} True when the audio decoder the player consumes exists.
         */
        hasAudioDecoder() {
            return typeof this.window.AudioDecoder === 'function';
        }

        /**
         * Checks whether the Web Audio API is present.
         * @returns {boolean} True when an AudioContext constructor exists.
         */
        hasAudioContext() {
            return typeof this.window.AudioContext === 'function' || typeof this.window.webkitAudioContext === 'function';
        }

        /**
         * Asks the browser whether it can decode a single audio configuration.
         * @param {Object} config - An AudioDecoderConfig like value, with codec, sampleRate and numberOfChannels.
         * @returns {Promise<boolean>} True when the configuration is decodable, false otherwise or when it cannot be probed.
         */
        async supportsAudioConfigAsync(config) {
            if (!this.hasAudioDecoder())
                return false;
            try {
                const result = await this.window.AudioDecoder.isConfigSupported(config);
                // The isConfigSupported call resolves with a result object; a missing result and a false supported flag both
                // mean the browser cannot decode this configuration, so the answer is returned as a single boolean.
                return result !== null && result !== undefined && result.supported === true;
            } catch (error) {
                // An unsupported codec string can make isConfigSupported reject; that is a "no", not a failure of the player.
                console.debug('Audio player: the codec configuration was rejected by the browser.', config, error);
                return false;
            }
        }

        /**
         * Reports whether the browser can decode a codec, by probing it with AudioDecoder.isConfigSupported.
         * @param {Object} config - An AudioDecoderConfig like value, with codec, sampleRate and numberOfChannels.
         * @returns {Promise<boolean>} True when the configuration is decodable, false otherwise.
         */
        async supportsCodecAsync(config) {
            return await this.supportsAudioConfigAsync(config);
        }

        /**
         * Detects every capability the player needs, in parallel, and caches the report.
         *
         * The returned report has the shape:
         * <code>{ hasWebCodecs, hasAudioDecoder, hasAudioContext, codecs, missing, isSupported }</code>, where <code>codecs</code> maps each probed codec name to a boolean, <code>missing</code> lists the
         * {@link LuminaAudioPlayer.PlayerCapability} values that are absent, and <code>isSupported</code> is true only when every required capability is present.
         * @returns {Promise<Object>} The capability report.
         */
        async detectAsync() {
            // The report is immutable and expensive to build, so it is cached for the life of the player; while the first
            // detection is still running, later callers share its promise instead of starting a second round of probes.
            if (this.report)
                return this.report;
            if (this.pendingReport)
                return this.pendingReport;
            this.pendingReport = this.runDetectionAsync();
            try {
                this.report = await this.pendingReport;
            } finally {
                this.pendingReport = null;
            }
            return this.report;
        }

        /**
         * Runs the actual detection, once.
         * @returns {Promise<Object>} The capability report.
         */
        async runDetectionAsync() {
            const hasWebCodecs = this.hasWebCodecs();
            const hasAudioDecoder = this.hasAudioDecoder();
            const hasAudioContext = this.hasAudioContext();

            const codecs = {};
            if (hasAudioDecoder) {
                // Each isConfigSupported is an independent round trip to the browser, so they are probed in parallel; the
                // results are folded in afterwards, in probe order, so the report never depends on which probe finishes first.
                const probeResults = await Promise.all(LuminaAudioPlayer.PROBED_AUDIO_CODECS.map(async (probe) => {
                    return { name: probe.name, isSupported: await this.supportsAudioConfigAsync(probe.config) };
                }));
                for (const probeResult of probeResults)
                    codecs[probeResult.name] = probeResult.isSupported;
            } else {
                // Without a decoder nothing can be probed, but every codec key is still reported as false, so that a
                // consumer reads the same shape whether the decoder exists or not, instead of testing for a missing key.
                for (const probe of LuminaAudioPlayer.PROBED_AUDIO_CODECS)
                    codecs[probe.name] = false;
            }

            // Only the three hard capabilities decide whether the player can run at all; a codec the browser cannot decode is
            // not fatal here, because the server can normalize such a track, which the delivery decision handles at play time.
            const missing = [];
            if (!hasWebCodecs)
                missing.push(LuminaAudioPlayer.PlayerCapability.WEB_CODECS);
            if (!hasAudioDecoder)
                missing.push(LuminaAudioPlayer.PlayerCapability.AUDIO_DECODER);
            if (!hasAudioContext)
                missing.push(LuminaAudioPlayer.PlayerCapability.AUDIO_CONTEXT);

            // The report is shared and cached, so it is frozen to stop one consumer from mutating what every other consumer reads.
            return Object.freeze({
                hasWebCodecs,
                hasAudioDecoder,
                hasAudioContext,
                codecs: Object.freeze(codecs),
                missing: Object.freeze(missing),
                isSupported: missing.length === 0
            });
        }

        /**
         * Gets the cached capability report, or null when detection has not run yet.
         * @returns {Object|null} The capability report, or null.
         */
        getReport() {
            return this.report;
        }
    }

    LuminaAudioPlayer.PlayerCapabilities = PlayerCapabilities;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
