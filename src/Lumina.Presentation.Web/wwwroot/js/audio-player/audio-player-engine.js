/**
 * Engine interface of the audio player, and the registry of its implementations.
 *
 * The player controller owns the state machine, the play descriptor, the queue and the bar, but it
 * produces no audio itself. Audio is produced by an engine, a swappable implementation of the
 * interface documented below. This keeps the controller stable while the audio pipeline is built up,
 * and lets a test drive the controller with a scripted engine instead of a real sound card.
 *
 * The engine interface
 * --------------------
 * An engine is created by a factory that receives the engine host (see below) and returns an object
 * with these methods, all of them returning a Promise:
 *   - loadAsync(descriptor, options): prepares the resources of a track (opens its stream, configures a decoder).
 *     options.keepBuffering keeps a loading state the player handed to the load, used by an automatic continuation so the
 *     teardown of the previous track does not clear it during the end gap.
 *   - playAsync(): starts, or resumes, audible playback.
 *   - pauseAsync(): suspends audible playback, keeping the current position.
 *   - seekAsync(seconds): moves the playhead to the given position, in seconds, fetching the range of that position
 *     when it has not been downloaded yet.
 *   - setVolumeAsync(volume): sets the output volume, as a value between 0 and 1, with 0 meaning silence.
 *   - getBufferedTimeRanges(): the time ranges, in seconds, the engine already downloaded, as a list of objects with
 *     a 'start' and an 'end'. It is optional: an engine that does not implement it makes the player unable to tell
 *     whether a seek target is buffered, in which case the player never shows a loading state for a seek.
 *   - discardCurrentAsync() (optional): releases the currently prepared track without destroying the engine, so a stop
 *     during an automatic continuation does not leave the track it started loaded behind an idle player.
 *   - destroyAsync(): releases every resource the engine holds, including any audio context.
 *
 * The engine host
 * ---------------
 * The factory also receives a host object that the engine uses to talk back to the controller, so
 * that the controller stays the single source of truth:
 *   - host.events: the player event bus, for {@link LuminaAudioPlayer.PlayerEvent} values the engine emits itself. It is
 *     also the inbound channel: an engine and the renderer subscribe to PREFERENCES_CHANGED to follow a gapless or
 *     ReplayGain preference that changes while a track is playing.
 *   - host.getPreferences(): the playback preferences of the user the player currently holds, for example the
 *     'keep downloading while paused' setting an engine's downloader honors.
 *   - host.getConfiguration(): the endpoint configuration of the player, so an engine never keeps a second copy of it.
 *   - host.fetchJsonAsync(url, signal, label): the player's JSON transport. The optional signal cancels the request, the
 *     label describes it in the failure log, and an expired session redirect is followed to the sign in page and reported
 *     through the player's REDIRECTED marker, so an engine request behaves like the player's other background requests.
 *   - host.getCapabilityDetector(): the browser capability detector of the player, so an engine does not run a second
 *     capability detection.
 *   - host.getNextDescriptor(): { entryId, descriptor } of the queue entry that follows the current one, or null at the
 *     end of the queue. It is the player's own queue cursor, so an engine never re-implements queue traversal or picks
 *     the wrong repeated track.
 *   - host.advanceQueueTo(entryId): moves the player queue cursor to the entry with that identity without starting
 *     playback, which an engine uses for a gapless transition whose audio is already scheduled immediately after the current
 *     track. The advance is refused when the identity is no longer part of the queue, so the bar never describes a track
 *     that is not playing. A linear advance, a wrap and a reordered queue all use this same commit.
 *   - host.requestNextTrackAsync(reason): asks the player to start the next queue entry through its own play path, which
 *     owns the player state, the pause intent and the capability gate. An engine uses it when a gapless handoff could not
 *     be prepared, instead of starting a playback session itself. The returned Promise<boolean> resolves true when the
 *     player took ownership of the transition, including a load it will report as failed, and false when it declined the
 *     request because the player is paused, stopped, or has no next entry. An engine must not report an end for a
 *     transition the player owns, and keeps the end it just reported when the promise resolves false.
 *   - host.reportTime(currentTime, duration): reports the playhead and the known duration.
 *   - host.reportDuration(duration): reports a duration discovered after loading.
 *   - host.reportProgress(downloadedFraction): reports how much of the track has been downloaded, between 0 and 1.
 *   - host.reportBuffering(isBuffering): reports that the engine is, or is no longer, waiting for data, so that the
 *     bar shows the same loading state for an engine's own range fetch as it does for a seek. The state it sets
 *     stays until the engine reports it is no longer buffering, and a playhead report never clears it.
 *   - host.reportEnded(): reports that the current track reached its end.
 *   - host.reportError(error): reports an engine failure, wrapped into a {@link LuminaAudioPlayer.PlayerError}. Every sub
 *     component that reports through its own callback must be forwarded here, because such a component only reports to its
 *     own callback and never emits on the event bus, so a failure that is not forwarded stays unobserved. The download
 *     engine must forward its downloader's onError, and an engine that decodes must forward its decoder's onError as well.
 *
 * Registering and selecting an engine
 * -----------------------------------
 * Engines are registered on a registry by name, and the player runs the engine selected by name. The silent engine is
 * registered under NULL_ENGINE_NAME and is the default of a bare player, but the shipped composition installs the media
 * pipeline engine when that module loads and selects it, so the running application plays through the pipeline. The
 * singleton is created automatically when the core module loads, so the way to plug a different engine into the running
 * application is the instance seam:
 *   - window.LuminaAudioPlayer.instance.registerEngine(name, factory) registers a factory of the shape (host) => engine.
 *   - window.LuminaAudioPlayer.instance.useEngine(name) selects a registered engine; it takes effect on the next load,
 *     and it returns the player to idle when an engine instance is already running.
 * A player can also be created directly with a selected engine and a populated registry through the constructor
 * ({ engineName, engineRegistry }); LuminaAudioPlayer.createDefaultPlayer(options) forwards those options only for the
 * first creation of the singleton, for a composition that creates the player itself, such as a test.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The name under which the silent engine that ships with the core module is registered.
     * @readonly
     * @type {string}
     */
    LuminaAudioPlayer.NULL_ENGINE_NAME = 'null';

    /**
     * An engine that fulfills the engine interface without producing any audio.
     *
     * It is the default engine of a bare player, so that the control API, the state machine, the
     * lifecycle and the bar are fully callable before, or without, the media pipeline. The shipped
     * composition selects the pipeline when that module loads. This engine tracks a playhead so that
     * a seek, a pause and a resume report the position the player expects, but it never loads,
     * decodes or plays a byte.
     */
    class NullPlayerEngine {
        /**
         * Creates an instance of NullPlayerEngine.
         * @param {Object} host - The engine host provided by the player controller.
         */
        constructor(host) {
            this.host = host;
            this.descriptor = null;
            this.currentTime = 0;
        }

        /**
         * Accepts a play descriptor without preparing any resource, and resets the tracked playhead.
         * @param {Object} descriptor - The play descriptor of the requested track.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async loadAsync(descriptor) {
            this.descriptor = descriptor;
            this.currentTime = 0;
        }

        /**
         * Reports the tracked playhead without producing any audio, so that a resume keeps the position a pause left.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async playAsync() {
            this.host.reportTime(this.currentTime, this.getDuration());
        }

        /**
         * Reports the suspension of playback without producing any audio, keeping the tracked playhead.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async pauseAsync() {
        }

        /**
         * Records and reports the requested playhead without producing any audio.
         * @param {number} seconds - The requested position, in seconds.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async seekAsync(seconds) {
            const position = Number(seconds);
            this.currentTime = Number.isFinite(position) && position > 0 ? position : 0;
            this.host.reportTime(this.currentTime, this.getDuration());
        }

        /**
         * Accepts an output volume without applying it to any output.
         * @param {number} volume - The requested output volume, between 0 and 1.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async setVolumeAsync(volume) {
        }

        /**
         * Releases nothing, because nothing was acquired.
         * @returns {Promise<void>} A promise that always resolves.
         */
        async destroyAsync() {
            this.descriptor = null;
            this.currentTime = 0;
        }

        /**
         * Gets the duration of the loaded descriptor, or zero when nothing is loaded.
         * @returns {number} The known duration, in seconds.
         */
        getDuration() {
            return this.descriptor && Number.isFinite(this.descriptor.durationInSeconds)
                ? this.descriptor.durationInSeconds
                : 0;
        }
    }

    /**
     * A registry of engine factories, keyed by name, used to resolve the engine the player runs with.
     * The later stages of the player register the real download, decode and render engine here; the
     * core module only registers the silent engine.
     */
    class PlayerEngineRegistry {
        /**
         * Creates an instance of PlayerEngineRegistry.
         */
        constructor() {
            /** @type {Map<string, Function>} */
            this.factories = new Map();
        }

        /**
         * Registers an engine factory.
         * @param {string} name - The name the engine is resolved by.
         * @param {Function} factory - A factory that receives the engine host and returns an engine.
         * @returns {LuminaAudioPlayer.PlayerEngineRegistry} This registry, for chaining.
         */
        register(name, factory) {
            if (typeof factory !== 'function')
                throw new TypeError('A player engine factory must be a function.');
            this.factories.set(name, factory);
            return this;
        }

        /**
         * Checks whether an engine is registered under the given name.
         * @param {string} name - The name of the engine.
         * @returns {boolean} True when an engine is registered under that name, false otherwise.
         */
        hasEngine(name) {
            return this.factories.has(name);
        }

        /**
         * Creates an engine by name.
         * @param {string} name - The name of the engine.
         * @param {Object} host - The engine host provided by the player controller.
         * @returns {Object|null} The created engine, or null when no engine with that name is registered.
         */
        create(name, host) {
            const factory = this.factories.get(name);
            return factory ? factory(host) : null;
        }

        /**
         * Lists the names of the registered engines.
         * @returns {Array<string>} The registered engine names.
         */
        getNames() {
            return [...this.factories.keys()];
        }
    }

    /**
     * Creates a registry holding only the silent engine, the default of the core module.
     * @returns {LuminaAudioPlayer.PlayerEngineRegistry} The default engine registry.
     */
    LuminaAudioPlayer.createDefaultEngineRegistry = function () {
        return new PlayerEngineRegistry().register(LuminaAudioPlayer.NULL_ENGINE_NAME, (host) => new NullPlayerEngine(host));
    };

    LuminaAudioPlayer.NullPlayerEngine = NullPlayerEngine;
    LuminaAudioPlayer.PlayerEngineRegistry = PlayerEngineRegistry;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
