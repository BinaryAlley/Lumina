/**
 * Typed errors of the audio player.
 *
 * The player never throws raw strings: every failure is reported through the event bus as a
 * {@link LuminaAudioPlayer.PlayerError}, so that a consumer can branch on a stable code instead of
 * matching a message, and so that a UI can present the failure without the player leaking
 * implementation details, file paths, or secrets.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Enum-like object for the stable codes carried by a {@link LuminaAudioPlayer.PlayerError}.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerErrorCode = Object.freeze({
        // The browser is missing a capability the player requires (WebCodecs, AudioDecoder or AudioContext).
        CAPABILITY_MISSING: 'capability-missing',
        // The play descriptor handed to the player is missing a required value, or carries an invalid one.
        INVALID_DESCRIPTOR: 'invalid-descriptor',
        // The requested operation is not valid in the current player state.
        INVALID_STATE: 'invalid-state',
        // The engine could not be created, loaded or configured.
        ENGINE_LOAD_FAILED: 'engine-load-failed',
        // The engine reported a failure while decoding, buffering or rendering audio.
        ENGINE_ERROR: 'engine-error',
        // The stored playback preferences of the user could not be read.
        PREFERENCES_LOAD_FAILED: 'preferences-load-failed'
    });

    /**
     * An error raised by the player, carrying a stable code and optional structured details.
     */
    class PlayerError extends Error {
        /**
         * Creates an instance of PlayerError.
         * @param {string} code - One of the {@link LuminaAudioPlayer.PlayerErrorCode} values.
         * @param {string} message - A developer facing description of the failure. It must never contain a file path, a token, or any other value that must not reach a log.
         * @param {Object} [details] - Optional structured details about the failure, for example the missing capabilities.
         */
        constructor(code, message, details = null) {
            super(message);
            this.name = 'PlayerError';
            this.code = code;
            this.details = details;
        }

        /**
         * Wraps any value into a {@link LuminaAudioPlayer.PlayerError}, so that reported failures are always typed.
         * @param {*} error - The value to wrap, typically an Error, a rejection reason, or a string.
         * @param {string} [fallbackCode] - The code to use when the value is not already a player error.
         * @returns {LuminaAudioPlayer.PlayerError} The typed player error.
         */
        static from(error, fallbackCode = LuminaAudioPlayer.PlayerErrorCode.ENGINE_ERROR) {
            if (error instanceof PlayerError)
                return error;
            if (error instanceof Error)
                return new PlayerError(fallbackCode, error.message, { cause: error });
            return new PlayerError(fallbackCode, String(error));
        }
    }

    LuminaAudioPlayer.PlayerError = PlayerError;
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
