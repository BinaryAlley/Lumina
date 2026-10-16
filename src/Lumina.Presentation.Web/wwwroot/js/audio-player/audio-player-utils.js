/**
 * Shared helpers of the audio player.
 *
 * This module has no dependency on any other player module, and it is loaded before them, so it is the home of the small
 * numeric and error helpers several modules need. Keeping one implementation here avoids the copies that otherwise drift
 * apart, and because this file is a dedicated, dependency free util, no consumer depends on the load order of a feature
 * module such as the event bus or the download engine. The error helpers read {@link LuminaAudioPlayer.PlayerError} at
 * call time rather than at load time, so they do not depend on the error module being loaded first either.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * Clamps a number into an inclusive range, defaulting a non finite value to the lower bound.
     * @param {number} value - The value to clamp.
     * @param {number} minimum - The inclusive lower bound.
     * @param {number} maximum - The inclusive upper bound.
     * @returns {number} The clamped value.
     */
    LuminaAudioPlayer.clamp = function clamp(value, minimum, maximum) {
        if (!Number.isFinite(value))
            return minimum;
        return Math.min(maximum, Math.max(minimum, value));
    };

    /**
     * Checks whether a value is the AbortError a reset, a close or an aborted fetch rejects with. An AbortError is tested
     * by name rather than by type, because in a browser it is a DOMException, which is not an Error.
     * @param {*} error - The value to check.
     * @returns {boolean} True when the value is an AbortError.
     */
    LuminaAudioPlayer.isAbortError = function isAbortError(error) {
        return !!error && error.name === 'AbortError';
    };

    /**
     * Merges two error detail objects into a new one, so that a details object a component owns is never mutated in place.
     * @param {Object|null} base - The existing details.
     * @param {Object|null} extra - The details to add.
     * @returns {Object|null} The merged details, or null when both are absent.
     */
    LuminaAudioPlayer.mergeErrorDetails = function mergeErrorDetails(base, extra) {
        const hasBase = base !== null && base !== undefined;
        const hasExtra = extra !== null && extra !== undefined;
        if (!hasBase && !hasExtra)
            return null;
        if (!hasExtra)
            return Object.assign({}, base);
        if (!hasBase)
            return Object.assign({}, extra);
        return Object.assign({}, base, extra);
    };

    /**
     * Copies a failure into a {@link LuminaAudioPlayer.PlayerError} that the caller owns, so that a player error raised by
     * another component is never aliased and its details are never mutated. A failure that is already a player error keeps
     * its own code, which is more specific than the fallback the caller would give it.
     * @param {*} error - The failure to own.
     * @param {string} [fallbackCode] - The code to use when the failure is not already a player error.
     * @param {Object|null} [details] - Optional details to merge in.
     * @returns {LuminaAudioPlayer.PlayerError} The owned typed error.
     */
    LuminaAudioPlayer.toOwnedPlayerError = function toOwnedPlayerError(error, fallbackCode, details) {
        if (error instanceof LuminaAudioPlayer.PlayerError)
            return new LuminaAudioPlayer.PlayerError(error.code, error.message, LuminaAudioPlayer.mergeErrorDetails(error.details, details || null));
        const typed = LuminaAudioPlayer.PlayerError.from(error, fallbackCode);
        if (!details)
            return typed;
        return new LuminaAudioPlayer.PlayerError(typed.code, typed.message, LuminaAudioPlayer.mergeErrorDetails(typed.details, details));
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
