/**
 * Shared helpers of the audio player.
 *
 * This module has no dependency on any other player module, and it is loaded before them, so it is the home of the small
 * numeric helpers several modules need. Keeping one implementation here avoids the copies that otherwise drift apart, and
 * because this file is a dedicated, dependency free util, no consumer depends on the load order of a feature module such
 * as the event bus or the download engine.
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
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
