/**
 * Event bus of the audio player.
 *
 * The player is decoupled from its consumers (the player bar, the transport UI, the queue panel, themes) through a single observable. 
 * Every module of the player publishes to this bus, and every consumer subscribes to it, so that no module needs a direct reference to another one.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Enum-like object for the events published by the player.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerEvent = Object.freeze({
        // The state machine moved to another state. Payload: { previousState, state }.
        STATE_CHANGED: 'statechange',
        // The currently playing track changed. Payload: the play descriptor.
        DESCRIPTOR_CHANGED: 'descriptorchange',
        // The queue changed. Payload: the ordered list of play descriptors.
        QUEUE_CHANGED: 'queuechange',
        // The playhead moved. Payload: { currentTime, duration }.
        TIME_UPDATE: 'timeupdate',
        // The duration of the current track became known or changed. Payload: { duration }.
        DURATION_CHANGED: 'durationchange',
        // The output volume changed. Payload: { volume, muted, effectiveVolume }.
        VOLUME_CHANGED: 'volumechange',
        // The mute state changed. Payload: { volume, muted, effectiveVolume }.
        MUTE_CHANGED: 'mutechange',
        // The downloaded fraction of the current track changed. Payload: { downloadedFraction }.
        PROGRESS: 'progress',
        // A seek started waiting for the range of its target position, or such a wait ended. Payload: { isBuffering, pendingSeekTime }.
        BUFFERING_CHANGED: 'bufferingchange',
        // The current track reached its end.
        ENDED: 'ended',
        // A {@link LuminaAudioPlayer.PlayerError} was raised. Payload: the player error.
        ERROR: 'error',
        // Capability detection completed. Payload: the capability report.
        CAPABILITIES_CHANGED: 'capabilitieschange',
        // The stored playback preferences of the user were loaded. Payload: the preferences object.
        PREFERENCES_CHANGED: 'preferenceschange',
        // A track cannot be demuxed and decoded directly by the bundled player, so the server has to normalize it (remux or
        // transcode) before it can play. Payload: { container, codec, reason }, where reason is one of the
        // {@link LuminaAudioPlayer.PlayerErrorReason} values NO_DEMUXER, CODEC_UNSUPPORTED or DEMUXER_FAILED, and, when the
        // caller supplied one, isDirectPlay, the direct play flag of the manifest it asked about, so a consumer can tell a
        // client-side capability gap from a track the server already decided to normalize. The flag is absent, not false,
        // when the caller had no manifest.
        NEEDS_NORMALIZATION: 'needsnormalization',
        // The player bar became visible or hidden. Payload: { isVisible }. The value is deliberately not 'visibilitychange',
        // which is the name of the DOM page visibility event, so that the two can never be confused.
        VISIBILITY_CHANGED: 'barvisibilitychange'
    });

    /**
     * A minimal synchronous event bus with subscribe, unsubscribe and one-shot subscribe.
     */
    class PlayerEventBus {
        /**
         * Creates an instance of PlayerEventBus.
         */
        constructor() {
            /** @type {Map<string, Set<Function>>} */
            this.listeners = new Map();
        }

        /**
         * Subscribes a handler to an event.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler invoked with the event payload.
         * @returns {Function} A function that unsubscribes the handler.
         */
        on(event, handler) {
            if (typeof handler !== 'function')
                throw new TypeError('The player event handler must be a function.');
            let handlers = this.listeners.get(event);
            if (!handlers) {
                handlers = new Set();
                this.listeners.set(event, handlers);
            }
            handlers.add(handler);
            return () => this.off(event, handler);
        }

        /**
         * Subscribes a handler to an event, removing it after the first emission.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler invoked once with the event payload.
         * @returns {Function} A function that unsubscribes the handler.
         */
        once(event, handler) {
            const unsubscribe = this.on(event, (payload) => {
                unsubscribe();
                handler(payload);
            });
            return unsubscribe;
        }

        /**
         * Unsubscribes a handler from an event.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {Function} handler - The handler to remove.
         */
        off(event, handler) {
            const handlers = this.listeners.get(event);
            if (handlers)
                handlers.delete(handler);
        }

        /**
         * Publishes an event to every subscribed handler.
         *
         * A handler that throws does not prevent the remaining handlers from running: the failure is
         * logged and swallowed, because a misbehaving consumer must never break the player.
         * @param {string} event - One of the {@link LuminaAudioPlayer.PlayerEvent} values.
         * @param {*} [payload] - The payload passed to each handler.
         */
        emit(event, payload) {
            const handlers = this.listeners.get(event);
            if (!handlers || handlers.size === 0)
                return;
            // Copy the set, so that a handler that subscribes or unsubscribes during the emission does not affect this run.
            for (const handler of [...handlers]) {
                try {
                    handler(payload);
                } catch (error) {
                    console.error(`Audio player: a handler of '${event}' threw.`, error);
                }
            }
        }

        /**
         * Removes every handler of an event, or of every event when none is provided.
         * @param {string} [event] - One of the {@link LuminaAudioPlayer.PlayerEvent} values, or nothing to clear all.
         */
        clear(event) {
            if (event === undefined)
                this.listeners.clear();
            else
                this.listeners.delete(event);
        }
    }

    LuminaAudioPlayer.PlayerEventBus = PlayerEventBus;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
