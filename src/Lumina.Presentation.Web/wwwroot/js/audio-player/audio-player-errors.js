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
        // The WebCodecs decode pipeline could not be configured, refused a chunk, or failed to decode one.
        DECODE_FAILED: 'decode-failed',
        // The decoder has no room for another chunk right now, so a caller that paces itself should wait. Unlike
        // DECODE_FAILED, which is fatal, this is a transient, recoverable pacing condition.
        DECODER_BUSY: 'decoder-busy',
        // The download of a track failed, or was rejected by the stream.
        DOWNLOAD_FAILED: 'download-failed',
        // The download of a track was intentionally stopped, by an abort or a destroy, so a reader can tell a teardown apart from a stream failure.
        DOWNLOAD_ABORTED: 'download-aborted',
        // The stored playback preferences of the user could not be read.
        PREFERENCES_LOAD_FAILED: 'preferences-load-failed'
    });

    /**
     * Enum-like object for the stable <c>details.reason</c> values that cross a module boundary of the player.
     *
     * A reason is a structured detail of a {@link LuminaAudioPlayer.PlayerError} that a consumer branches on, so its value
     * is defined once here and shared, never written as a literal by the producer and the consumer separately: two copies
     * of a literal drift apart silently, and a consumer that keys on a stale one reclassifies a recoverable condition as
     * a fatal failure without anything failing to compile.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.PlayerErrorReason = Object.freeze({
        // A scheduler refused a decoded frame because its own scheduled ahead ceiling is tighter than the caller's, so the
        // frame is backpressure rather than a failure. Published by the renderer's schedule and recognized by the decode
        // pipeline, which pauses feeding instead of stopping.
        SCHEDULED_AHEAD_LIMIT: 'scheduled-ahead-limit',
        // A scheduler's scheduled ahead ceiling is smaller than one frame plus its scheduling lead, so it can never accept a
        // frame at all. Unlike SCHEDULED_AHEAD_LIMIT, which is recoverable backpressure, this is terminal: the decode
        // pipeline stops because it can never make progress.
        SCHEDULED_AHEAD_UNSATISFIABLE: 'scheduled-ahead-unsatisfiable'
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
