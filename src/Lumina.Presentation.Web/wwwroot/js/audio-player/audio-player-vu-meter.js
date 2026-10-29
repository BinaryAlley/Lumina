/**
 * VU meter of the audio player.
 *
 * The player bar shows a segmented level meter beside the track labels, so a listener can see how loud the track is at
 * any moment. The meter follows the level of the track itself, not the level of the player output: the renderer taps the
 * signal after the per-track ReplayGain gain and before the master gain that carries the volume and the mute state, so
 * changing the volume, or muting the player, never changes the height of the meter for the same moment of audio. This is
 * what makes the meter comparable across a listening session, and it is why it does not move when the volume slider is dragged.
 *
 * Words used in this file
 * -----------------------
 * Audio processing has its own vocabulary. These are the terms the comments below rely on:
 *   - sample: one number between -1 and 1 that describes the position of the speaker membrane at one instant. A track
 *     at 48 kHz has 48000 samples per second per channel.
 *   - time domain: the samples in the order they are played, which is what the AnalyserNode hands out. They tell how
 *     the signal moves over time, but not which frequencies (bass, voice, cymbals) it contains.
 *   - FFT (Fast Fourier Transform): the algorithm that turns a block of time domain samples into the frequency domain,
 *     that is, into how strong every frequency is inside that block.
 *   - bin: one output slot of the FFT. A transform of N samples produces N / 2 useful bins, and bin k describes the
 *     frequency k * sampleRate / N. With N = 2048 at 48 kHz each bin is about 23 Hz wide.
 *   - band: a group of neighbouring bins that the meter shows as one channel (one column of segments).
 *   - decibel (dB): a logarithmic unit for loudness. Every -6 dB halves the amplitude, and 0 dBFS ("full scale") is the
 *     loudest level a digital signal can hold, so real music levels are negative numbers.
 *   - RMS (root mean square): the square root of the average of the squared values. It measures the energy of a signal,
 *     which matches perceived loudness better than its highest value does.
 *   - window: a curve the samples are multiplied by before the FFT. See buildWindow for why it is needed.
 *
 * What it measures
 * ----------------
 * The meter is a spectrum meter rather than a two channel volume unit. A spectrum analyzer feeds it a set of bands, one
 * band per channel of the meter, and each channel is drawn as a column (or a row) of segments.
 * The analyzer is implemented here, so the meter has 24 logarithmically spaced bands between 40 Hz and 16 kHz, a -70 dB
 * noise floor and a +10 dB display offset. Any other band count can be requested, but the shipped composition uses 24.
 *
 * The analyzer reads the time domain samples of an AnalyserNode of the Web Audio graph and computes the bands itself: the
 * samples are windowed with a Hann window and transformed with an iterative radix-2 FFT. The browser is used only to
 * deliver the samples, so the meter has a sharp transient response and a fast attack rather than the smoothed response
 * of the built-in frequency data. (The AnalyserNode can compute a spectrum itself, through getFloatFrequencyData, but it
 * always blends every new spectrum with the previous ones, which makes a drum hit swell up instead of jumping, and its
 * smoothing is not the same in every browser.) A transform is taken once per block of samples, so a beat produces one
 * sharp jump and the display falls between blocks instead of being refreshed by a sliding, overlapping window. Each band
 * combines the power of the bins that fall into its frequency range, and the result is normalized to a level between 0
 * and 1. The transform allocates nothing per frame, because its scratch arrays are created once with the analyzer.
 *
 * Level dynamics
 * --------------
 * A level rises immediately and falls gradually, which is what makes a meter readable: the attack is the peak of the
 * new level and the release is a linear fall at releasePerSecond. Each channel also holds a peak marker. A new peak
 * captures the level, the marker is held for peakHoldDurationInMilliseconds, and it then falls at peakReleasePerSecond. Both rates are expressed
 * in level units per second, so a rate of 1 clears a full meter in one second, and both are user settings, together with the orientation,
 * the segment count, the segment gap, the two thresholds and whether the peak marker is shown at all.
 *
 * Pausing and stopping
 * --------------------
 * The meter follows the state machine of the player through start and stop. start begins to read the analyzer, while
 * stop stops reading it and lets the displayed levels and peaks fall to zero, so a paused or stopped track leaves an
 * empty meter instead of a frozen one. A suspended AudioContext would otherwise keep returning the last analyzed frame,
 * which would look like sound that is no longer playing. The frame loop ends by itself once every channel reached zero,
 * so a stopped player costs no animation frames.
 *
 * The DOM contract
 * ----------------
 * The meter is built from elements rather than a canvas, so a theme can restyle every part of it and give it its own
 * shape, orientation, colors and imagery, exactly like the progress bar. The player renders:
 *   - the meter root, with the 'audio-player-vu-meter' class and the 'data-audio-player-vu-meter' hook. It carries
 *     'data-audio-player-vu-meter-orientation' ('vertical' or 'horizontal'), 'data-audio-player-vu-meter-segments' (the
 *     segment count) and 'data-audio-player-vu-meter-show-peak' ('true' or 'false'), and it publishes the configured
 *     segment gap as the CSS custom property '--audio-player-vu-segment-gap', the segment count as
 *     '--audio-player-vu-segment-count' and the number of channels as '--audio-player-vu-band-count';
 *   - one channel per band, with the 'audio-player-vu-channel' class and 'data-audio-player-vu-channel' holding the zero based band index;
 *   - one segment per channel, with the 'audio-player-vu-segment' class and the
 *     'data-audio-player-vu-segment-state' attribute ('active' or 'inactive'). Every segment, active or not, also carries
 *     'data-audio-player-vu-segment-zone' ('normal', 'medium' or 'high'), which is derived from the segment position and
 *     the two thresholds, so a theme styles the three zones independently, and can even tint the unlit segments of a zone;
 *   - one peak marker per channel, with the 'audio-player-vu-peak' class, the 'data-audio-player-vu-peak' hook and the
 *     'data-audio-player-vu-peak-state' attribute ('active' or 'inactive'). The marker publishes its position on the
 *     channel axis as the CSS custom property '--audio-player-vu-peak-position', between 0% and 100%.
 * The segments are rendered in ascending level order, so the first segment of a channel is the quietest one. The
 * fallback stylesheet lays the channels out as columns whose segments grow from the bottom for the vertical orientation,
 * and as rows whose segments grow from the left for the horizontal one, but that is only its default: the meter writes
 * no inline geometry other than the custom properties above, so a theme may lay the segments out in any direction.
 *
 * Rendering cost
 * --------------
 * The elements are created once per configuration, and every frame only writes the attributes whose value changed. A
 * burst of identical frames therefore does no DOM work, and the animation frame loop stops entirely while the meter is
 * empty. The meter never reads the layout of its elements, so it does not force a reflow per frame. (A reflow is the
 * browser recomputing the size and position of elements. Reading a size, such as offsetHeight, right after writing a
 * style forces one immediately, and doing that every frame is a common cause of stuttering animations.)
 *
 * Using it
 * --------
 *   const meter = LuminaAudioPlayer.createVuMeter({
 *       container: barElement.querySelector('[data-audio-player-vu-meter]'),
 *       settings: player.getPreferences()
 *   });
 *   meter.setAnalyser(renderer.getOutputAnalyser());
 *   meter.start();                 // when playback starts
 *   meter.configure(preferences);  // whenever the user settings change
 *   meter.stop();                  // when playback pauses or stops
 *   meter.destroy();               // when the player is torn down
 * The meter tolerates a missing analyzer: it still renders, and its levels simply stay at zero. A player engine that
 * cannot expose an analyzer therefore degrades to an empty meter instead of failing.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The number of bands of the shipped meter. 24 bands give two to three bands per octave over the range below, which
     * is enough to tell bass, voice and cymbals apart while keeping the meter narrow enough for the player bar.
     * @readonly
     * @type {number}
     */
    const BAND_COUNT = 24;

    /**
     * The lowest frequency, in Hz, that the first band covers. Below about 40 Hz music carries little more than rumble,
     * and the bins there are so few that a band would flicker between one bin and none.
     * @readonly
     * @type {number}
     */
    const MINIMUM_FREQUENCY = 40;

    /**
     * The highest frequency, in Hz, that the last band covers. Most music has very little energy above 16 kHz, and many
     * lossy encoders (MP3, AAC at common bitrates) cut most of what lies above it, so bands up there would almost always be empty.
     * @readonly
     * @type {number}
     */
    const MAXIMUM_FREQUENCY = 16000;

    /**
     * The noise floor, in decibels, below which a band is reported as silent. It is the bottom of the meter: a level at
     * or under it lights no segment, so quiet hiss and dithering noise do not keep the lowest segments permanently lit.
     * @readonly
     * @type {number}
     */
    const NOISE_FLOOR_DB = -70;

    /**
     * The display offset, in decibels, added before a band is normalized, so a signal around -10 dBFS fills the meter.
     * Without it a band would only reach the top at 0 dBFS, which a single band of real music almost never does, because
     * the energy of a track is spread over all the bands. The offset makes ordinary loud passages use the top segments.
     * @readonly
     * @type {number}
     */
    const DISPLAY_GAIN_DB = 10;

    /**
     * The transform size of the analyzer. A transform of this size at a typical context rate resolves the lowest band, which starts at 40 Hz, while covering a window short enough to follow the beat.
     * The FFT can only take a power of two, and the choice is a trade-off: doubling it halves the width of a bin, which
     * gives the bass bands more bins, but also doubles the time a block covers, which makes the meter react more slowly.
     * 2048 samples is about 43 ms at 48 kHz, so the meter takes about 23 fresh measurements per second.
     * @readonly
     * @type {number}
     */
    const FFT_SIZE = 2048;

    /**
     * The longest frame delta, in seconds, that the release calculation accepts. A tab that was hidden for a long time
     * must not empty the meter in one visible frame and skip its release, so a long delta is clamped to a normal frame.
     * @readonly
     * @type {number}
     */
    const MAXIMUM_FRAME_SECONDS = 0.1;

    /**
     * The shortest and longest raw frame delta, in milliseconds, that is trusted as the duration of a real frame. The range
     * covers displays from about 30 to 240 Hz; anything outside it is a rounded timestamp or a stall.
     * See advanceClock for why the timestamps can be rounded at all.
     * @readonly
     * @type {number}
     */
    const MINIMUM_REAL_FRAME_MS = 4;
    const MAXIMUM_REAL_FRAME_MS = 40;

    /**
     * The fallback frame interval, in milliseconds, used when the page has no requestAnimationFrame.
     * @readonly
     * @type {number}
     */
    const FRAME_FALLBACK_INTERVAL_MS = 16;

    // The ranges below repeat the limits the server validates the user settings against (UpdateUserSettingsCommandValidator
    // and the UserSettings aggregate). They are repeated here because the meter also runs with settings that never went
    // through the server, such as the defaults or a theme building its own meter, and it must never be driven outside of
    // what the settings page can describe. When a limit changes on the server, it has to change here as well.

    /**
     * The minimum and maximum accepted number of segments of a channel.
     * @readonly
     * @type {{ minimum: number, maximum: number }}
     */
    const SEGMENT_COUNT_RANGE = Object.freeze({ minimum: 1, maximum: 128 });

    /**
     * The minimum and maximum accepted number of bands of the analyzer.
     * @readonly
     * @type {{ minimum: number, maximum: number }}
     */
    const BAND_COUNT_RANGE = Object.freeze({ minimum: 1, maximum: 128 });

    /**
     * The minimum and maximum accepted segment gap, in pixels.
     * @readonly
     * @type {{ minimum: number, maximum: number }}
     */
    const SEGMENT_GAP_RANGE = Object.freeze({ minimum: 0, maximum: 32 });

    /**
     * The minimum and maximum accepted peak hold duration, in milliseconds.
     * @readonly
     * @type {{ minimum: number, maximum: number }}
     */
    const PEAK_HOLD_RANGE = Object.freeze({ minimum: 0, maximum: 10000 });

    /**
     * The minimum and maximum accepted release rate, in level units per second.
     * @readonly
     * @type {{ minimum: number, maximum: number }}
     */
    const RELEASE_RANGE = Object.freeze({ minimum: 0, maximum: 100 });

    /**
     * The default settings of the meter, mirroring the defaults of the user settings aggregate.
     * They are used when a setting is absent, so a player that starts before the stored preferences arrive, or a theme that builds its own meter, still renders a usable meter.
     * When a default changes in the UserSettings aggregate, it has to change here as well, otherwise a new user would see one meter before the settings load and a different one after.
     * @readonly
     * @type {Object}
     */
    LuminaAudioPlayer.VU_METER_DEFAULTS = Object.freeze({
        orientation: 'vertical',
        segmentCount: 12,
        segmentGap: 1,
        mediumThreshold: 0.6,
        highThreshold: 0.8,
        showPeak: true,
        peakHoldDurationInMilliseconds: 650,
        releasePerSecond: 3,
        peakReleasePerSecond: 0.65
    });

    /**
     * The number of bands of the shipped meter, exposed for consumers that build their own meter markup.
     * @readonly
     * @type {number}
     */
    LuminaAudioPlayer.VU_METER_BAND_COUNT = BAND_COUNT;

    /**
     * Clamps a number into an inclusive range, through the shared helper on the namespace. The reference is resolved on
     * every call rather than captured at load time, so this module does not depend on the load order of the util.
     * @param {number} value - The value to clamp.
     * @param {number} minimum - The inclusive lower bound.
     * @param {number} maximum - The inclusive upper bound.
     * @returns {number} The clamped value.
     */
    function clamp(value, minimum, maximum) {
        return LuminaAudioPlayer.clamp(value, minimum, maximum);
    }

    /**
     * Coerces a value to a finite number, through the shared helper on the namespace.
     * It is used on every setting because settings arrive as JSON or form values, where a number can be a string, null, or missing, and a single NaN would silently break every calculation it touches.
     * @param {*} value - The value to coerce.
     * @returns {number|null} The finite number, or null.
     */
    function toFiniteNumber(value) {
        return LuminaAudioPlayer.toFiniteNumber(value);
    }

    /**
     * Normalizes the orientation of the meter into the value the DOM contract uses.
     * The comparison ignores case because the server serializes its enum as 'Vertical' and 'Horizontal', while the DOM
     * attributes and the stylesheet use lower case. Anything unknown falls back to vertical, the default orientation.
     * @param {*} value - The orientation the user settings carry, either the server's 'Vertical' or 'Horizontal', or the DOM form 'vertical' or 'horizontal' a consumer may pass.
     * @returns {string} 'vertical' or 'horizontal'.
     */
    function normalizeOrientation(value) {
        return typeof value === 'string' && value.toLowerCase() === 'horizontal' ? 'horizontal' : 'vertical';
    }

    /**
     * Normalizes a settings object into the values the meter renders and animates with.
     *
     * Every value is clamped into the range the server validates, so a stale or hand written settings object can never
     * put the meter into a state the user settings page cannot describe. A high threshold below the medium threshold is
     * raised to it instead of leaving the two zones inverted, which keeps a segment in a defined zone.
     * @param {Object} [settings] - The user settings the player holds.
     * @returns {Object} The normalized settings.
     */
    function normalizeSettings(settings) {
        const values = settings || {};
        // The counts, the gap and the hold time are whole numbers on the server, so they are rounded here too: half a
        // segment cannot be drawn, and a fractional gap would make the CSS cell sizing produce uneven rows.
        const segmentCount = Math.round(toFiniteNumber(values.vuMeterSegmentCount) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.segmentCount);
        const segmentGap = Math.round(toFiniteNumber(values.vuMeterSegmentGap) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.segmentGap);
        const mediumThreshold = clamp(toFiniteNumber(values.vuMeterMediumThreshold) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.mediumThreshold, 0, 1);
        const highThreshold = clamp(toFiniteNumber(values.vuMeterHighThreshold) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.highThreshold, 0, 1);
        const peakHoldDurationInMilliseconds = Math.round(toFiniteNumber(values.vuMeterPeakHoldDurationInMilliseconds) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.peakHoldDurationInMilliseconds);
        const releasePerSecond = toFiniteNumber(values.vuMeterReleasePerSecond) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.releasePerSecond;
        const peakReleasePerSecond = toFiniteNumber(values.vuMeterPeakReleasePerSecond) ?? LuminaAudioPlayer.VU_METER_DEFAULTS.peakReleasePerSecond;
        return {
            orientation: normalizeOrientation(values.vuMeterOrientation),
            segmentCount: clamp(segmentCount, SEGMENT_COUNT_RANGE.minimum, SEGMENT_COUNT_RANGE.maximum),
            segmentGap: clamp(segmentGap, SEGMENT_GAP_RANGE.minimum, SEGMENT_GAP_RANGE.maximum),
            mediumThreshold: mediumThreshold,
            highThreshold: Math.max(mediumThreshold, highThreshold),
            // Compared with false on purpose: a missing value means "not configured", and the default is to show the peak.
            showPeak: values.vuMeterShowPeak !== false,
            peakHoldDurationInMilliseconds: clamp(peakHoldDurationInMilliseconds, PEAK_HOLD_RANGE.minimum, PEAK_HOLD_RANGE.maximum),
            releasePerSecond: clamp(releasePerSecond, RELEASE_RANGE.minimum, RELEASE_RANGE.maximum),
            peakReleasePerSecond: clamp(peakReleasePerSecond, RELEASE_RANGE.minimum, RELEASE_RANGE.maximum)
        };
    }

    /**
     * Analyzes the time domain samples of an analyzer into logarithmically spaced bands.
     *
     * The time domain samples are windowed with a Hann window, transformed with an iterative radix-2 FFT, and each band combines the power of the bins that fall
     * into its frequency range. The browser is used only to deliver the samples through the AnalyserNode of the audio graph, not to compute the transform, so the meter has a sharp
     * transient response, like an analyzer that transforms whole blocks of samples.
     *
     * It is a class of its own, separate from VuMeter, so the signal processing can be tested with plain arrays of samples,
     * without a DOM, and so the drawing code never has to know anything about frequencies.
     */
    class VuMeterSpectrumAnalyzer {
        /**
         * Creates an instance of VuMeterSpectrumAnalyzer.
         * @param {Object} options - The analyzer options.
         * @param {AnalyserNode} options.analyser - The analyzer of the audio graph whose time domain data is read.
         * @param {number} [options.bandCount] - The number of bands to produce. Defaults to 24.
         * @param {number} [options.minimumFrequency] - The lowest frequency of the first band, in Hz.
         * @param {number} [options.maximumFrequency] - The highest frequency of the last band, in Hz.
         * @param {number} [options.sampleRate] - The sample rate of the audio context, when the analyzer does not expose it.
         * @param {number} [options.fftSize] - The transform size, when the analyzer does not expose it.
         */
        constructor(options = {}) {
            this.analyser = options.analyser || null;
            this.bandCount = clamp(Math.round(toFiniteNumber(options.bandCount) ?? BAND_COUNT), BAND_COUNT_RANGE.minimum, BAND_COUNT_RANGE.maximum);
            this.minimumFrequency = toFiniteNumber(options.minimumFrequency) ?? MINIMUM_FREQUENCY;
            this.maximumFrequency = toFiniteNumber(options.maximumFrequency) ?? MAXIMUM_FREQUENCY;
            this.sampleRate = toFiniteNumber(options.sampleRate) ?? this.readSampleRate();
            this.fftSize = Math.round(toFiniteNumber(options.fftSize) ?? this.readFftSize());
            // Every buffer is allocated once, here, and reused by every analysis. Allocating them per frame would create
            // garbage about 23 times a second, and the pauses of the garbage collector show up as visible stutters.
            // 'samples' is a Float32Array because that is the type getFloatTimeDomainData fills. The other three are
            // Float64Array because the FFT adds up thousands of products, and the extra precision keeps the rounding
            // errors of those sums from showing up as noise in the quiet bands.
            this.samples = new Float32Array(this.fftSize);
            this.window = new Float64Array(this.fftSize);
            this.real = new Float64Array(this.fftSize);
            this.imaginary = new Float64Array(this.fftSize);
            this.bandRanges = [];
            this.levels = new Array(this.bandCount).fill(0);
            // The time, in seconds, of the last transform. The analyzer transforms a fresh block of samples only,
            // not every animation frame, so the level of a band jumps once per block and falls in between. Keeping the same
            // cadence is what makes the meter kick on every beat instead of smearing every attack across a sliding window.
            this.lastAnalysisTime = null;
            // The window and the band ranges only depend on the transform size, the sample rate and the band layout,
            // none of which change after construction, so they are computed once instead of on every analysis.
            this.buildWindow();
            this.buildBandRanges();
        }

        /**
         * Reads the sample rate of the audio context the analyzer belongs to.
         * The rate is needed to know which frequency each bin describes. 48 kHz is the fallback because it is the rate most audio hardware, and therefore most audio contexts, run at.
         * @returns {number} The sample rate in Hz, or the default rate when it cannot be read.
         */
        readSampleRate() {
            const context = this.analyser && this.analyser.context;
            const sampleRate = context ? toFiniteNumber(context.sampleRate) : null;
            return sampleRate !== null && sampleRate > 0 ? sampleRate : 48000;
        }

        /**
         * Reads the transform size of the analyzer, which the renderer sets to the default size.
         * @returns {number} The transform size, or the default size when it cannot be read.
         */
        readFftSize() {
            const fftSize = this.analyser ? toFiniteNumber(this.analyser.fftSize) : null;
            // A transform size must be a power of two, which is what the FFT requires. An analyzer that
            // reports anything else is treated as the default size, so the FFT never receives an invalid length.
            // 32 to 32768 is the range the Web Audio specification allows for AnalyserNode.fftSize, and
            // (n & (n - 1)) === 0 is the usual bit trick to test for a power of two: a power of two has exactly one bit set.
            if (fftSize !== null && fftSize >= 32 && fftSize <= 32768 && (fftSize & (fftSize - 1)) === 0)
                return fftSize;
            return FFT_SIZE;
        }

        /**
         * Reads the clock the analysis cadence is measured against.
         * The audio context clock is preferred because it advances with the audio itself and stands still while the
         * context is suspended. VuMeter normally passes its own frame clock to readLevels, so this is only the fallback for a caller, such as a test, that does not.
         * @returns {number} The current audio context time in seconds, or the wall clock when there is no context.
         */
        readClock() {
            const context = this.analyser && this.analyser.context;
            const contextTime = context ? toFiniteNumber(context.currentTime) : null;
            if (contextTime !== null)
                return contextTime;
            return typeof performance !== 'undefined' ? performance.now() / 1000 : Date.now() / 1000;
        }

        /**
         * Builds the Hann window the analyzer applies before the transform.
         *
         * Why a window is needed: the FFT assumes the block it receives repeats forever, end to start. A block cut out of
         * real music almost never ends where it started, so that assumed repetition has a sudden jump at the seam, and
         * the FFT reports the jump as energy spread over every frequency ("spectral leakage"). On the meter, a single
         * bass note would then light the treble bands too. The Hann window is a smooth bump that is 0 at both ends and 1
         * in the middle; multiplying the samples by it fades the block in and out, which removes the jump.
         */
        buildWindow() {
            for (let index = 0; index < this.fftSize; index++)
                this.window[index] = 0.5 - (0.5 * Math.cos(2 * Math.PI * index / (this.fftSize - 1)));
        }

        /**
         * Builds the first and the last bin of every band, so a frame only visits the bins it needs.
         *
         * The bands are spaced logarithmically, not evenly, because hearing works in ratios: every octave doubles the
         * frequency, and 40 to 80 Hz sounds like as wide a step as 4000 to 8000 Hz. Evenly spaced bands would spend most
         * of the meter on the treble, where the bins are, and squeeze all the bass into the first one or two channels.
         * The formula gives every band the same frequency ratio: band b covers
         * minimum * (maximum / minimum) ^ (b / count) up to minimum * (maximum / minimum) ^ ((b + 1) / count).
         */
        buildBandRanges() {
            const binCount = this.fftSize / 2;
            this.bandRanges = [];
            for (let band = 0; band < this.bandCount; band++) {
                const lowFrequency = this.minimumFrequency * Math.pow(this.maximumFrequency / this.minimumFrequency, band / this.bandCount);
                const highFrequency = this.minimumFrequency * Math.pow(this.maximumFrequency / this.minimumFrequency, (band + 1) / this.bandCount);
                // The first bin is skipped because it carries the DC offset of the signal, which is not audible and would
                // only make the lowest band depend on the recording's offset instead of its sound.
                // A frequency f falls into bin f * fftSize / sampleRate. The low edge is rounded down and the high edge up,
                // so a narrow bass band that falls between two bins still gets at least one bin instead of none.
                const firstBin = Math.max(1, Math.floor(lowFrequency * this.fftSize / this.sampleRate));
                const lastBin = Math.min(binCount, Math.ceil(highFrequency * this.fftSize / this.sampleRate));
                this.bandRanges.push({ firstBin: firstBin, lastBin: lastBin });
            }
        }

        /**
         * Reads the current level of every band when a new transform is due.
         *
         * The samples of the analyzer are windowed, transformed, and combined per band, and each band is normalized between the noise floor and the display ceiling. A transform is only taken
         * after a whole block of new samples has arrived, which analyzes a block of 'fftSize' samples at a time: the frames in between return null, so the meter lets the displayed levels fall
         * instead of refreshing them from a sliding, overlapping window.
         * Why not analyze on every frame: the AnalyserNode always returns the latest fftSize samples, so two frames 16 ms
         * apart would see mostly the same samples. A drum hit would then show up in three or four consecutive analyses and
         * hold the bar up, instead of producing one jump followed by a fall, and the meter would look sluggish.
         * @param {number} [nowSeconds] - The current time, in seconds. Defaults to the audio context clock.
         * @returns {Array<number>|null} One level between 0 and 1 per band, or null when no analyzer is available or no
         * new block is due yet.
         */
        readLevels(nowSeconds) {
            if (!this.analyser || typeof this.analyser.getFloatTimeDomainData !== 'function')
                return null;
            const now = toFiniteNumber(nowSeconds) ?? this.readClock();
            // How long one block of samples lasts: 2048 samples at 48000 per second is about 0.043 seconds.
            const blockSeconds = this.fftSize / this.sampleRate;
            // The due time advances by one block from the previous due time rather than from the frame that happened to
            // observe it, so the analysis cadence stays close to one block per transform even though the meter renders at the
            // display refresh rate. A late frame, such as a resumed tab, resets the due time instead of catching up with a burst of transforms.
            const dueTime = this.lastAnalysisTime === null ? now : this.lastAnalysisTime + blockSeconds;
            if (this.lastAnalysisTime !== null && now < dueTime)
                return null;
            // Normally this stores dueTime, which keeps the cadence steady. After a long gap, now - blockSeconds is the
            // larger value, so the next analysis is due one block from now rather than immediately again.
            this.lastAnalysisTime = Math.max(now - blockSeconds, dueTime);
            try {
                this.analyser.getFloatTimeDomainData(this.samples);
            } catch (error) {
                console.debug('Audio player: the VU meter could not read the analyzer samples.', error);
                return null;
            }
            // Step 1: apply the window (see buildWindow). The FFT works on complex numbers, and audio samples are real
            // numbers, so the imaginary part starts at zero. It is reset on every analysis because the previous transform left its own results in the array.
            for (let index = 0; index < this.fftSize; index++) {
                this.real[index] = this.samples[index] * this.window[index];
                this.imaginary[index] = 0;
            }
            // Step 2: turn the block into frequencies. Afterwards real[k] and imaginary[k] describe bin k.
            this.transform();
            // Step 3: turn the bins of every band into one level between 0 and 1.
            for (let band = 0; band < this.bandCount; band++) {
                const range = this.bandRanges[band];
                let power = 0;
                let binCount = 0;
                // The power of a bin is its squared magnitude, real^2 + imaginary^2. Power is added up rather than
                // magnitude because the energy of independent frequencies adds up, while their magnitudes do not.
                for (let bin = range.firstBin; bin < range.lastBin; bin++) {
                    power += this.real[bin] * this.real[bin] + this.imaginary[bin] * this.imaginary[bin];
                    binCount++;
                }
                // The power is the squared magnitude of the bins, scaled by 2 / fftSize each, so the scale folds into one factor.
                // Averaging over the bins (rather than summing) keeps a wide treble band, which has many bins, from always
                // reading louder than a narrow bass band with only one or two. The raw FFT output grows with the number of
                // samples transformed, and the 2 / fftSize factor scales it back down, so the levels do not change when the transform size does.
                const rms = binCount === 0 ? 0 : Math.sqrt(power / binCount) * 2 / this.fftSize;
                // Amplitude to decibels. The tiny lower bound avoids Math.log10(0), which is -Infinity, during silence.
                const decibels = 20 * Math.log10(Math.max(rms, 1e-9));
                // Map the decibel scale onto 0 to 1: the noise floor (-70 dB) becomes 0, and -10 dB (0 dB minus the display
                // gain) becomes 1. For example, a band at -38 dB gives (-38 + 10 + 70) / 70 = 0.6, which is 60% of the meter.
                this.levels[band] = clamp((decibels + DISPLAY_GAIN_DB - NOISE_FLOOR_DB) / -NOISE_FLOOR_DB, 0, 1);
            }
            return this.levels;
        }

        /**
         * Transforms the windowed samples in place with an iterative radix-2 Cooley-Tukey FFT, the same transform the
         * analyzer uses. The transformation is applied to the real and imaginary arrays, so a frame allocates nothing.
         *
         * The input is the windowed block, the output is one complex number per bin, and the rest of the file only reads the result.
         * The algorithm is the textbook "iterative radix-2 Cooley-Tukey FFT", and it works in two stages:
         *   1. Reorder the samples by bit reversal. Sample 1 (binary 00000000001) swaps with sample 1024 (binary
         *      10000000000), and so on. This puts every pair of samples the next stage combines side by side.
         *   2. Combine the samples in rounds ("butterflies"): first pairs, then groups of 4, 8, and so on, up to the whole
         *      block. Each round merges two half size transforms into one of double size.
         * It computes the same result as the direct formula, but in about N * log2(N) steps instead of N * N, which for
         * 2048 samples is roughly 22 thousand steps instead of 4 million.
         */
        transform() {
            const length = this.fftSize;
            const real = this.real;
            const imaginary = this.imaginary;
            // Stage 1: bit reversal. 'reversed' is the bit reversed counterpart of 'index', updated incrementally (adding 1
            // to a bit reversed number means carrying from the top bit downwards) instead of being recomputed every time.
            // Each pair is swapped only once, when index < reversed, otherwise the second visit would swap it back.
            for (let index = 1, reversed = 0; index < length; index++) {
                let bit = length >> 1;
                while ((reversed & bit) !== 0) {
                    reversed ^= bit;
                    bit >>= 1;
                }
                reversed ^= bit;
                if (index < reversed) {
                    const realValue = real[index];
                    const imaginaryValue = imaginary[index];
                    real[index] = real[reversed];
                    imaginary[index] = imaginary[reversed];
                    real[reversed] = realValue;
                    imaginary[reversed] = imaginaryValue;
                }
            }
            // Stage 2: the butterflies, with blocks of 2, 4, 8, ... samples.
            for (let blockSize = 2; blockSize <= length; blockSize <<= 1) {
                // Every element of a block is rotated by a slightly larger angle than the previous one. Instead of calling
                // Math.cos and Math.sin for every element, the rotation of one step is computed once per round and then
                // applied repeatedly to (offsetReal, offsetImaginary), which is much cheaper.
                const angle = -2 * Math.PI / blockSize;
                const rotationReal = Math.cos(angle);
                const rotationImaginary = Math.sin(angle);
                const halfSize = blockSize / 2;
                for (let block = 0; block < length; block += blockSize) {
                    let offsetReal = 1;
                    let offsetImaginary = 0;
                    for (let offset = 0; offset < halfSize; offset++) {
                        // One butterfly: combine an element of the first half of the block with its partner in the second
                        // half. The partner is rotated first (a complex multiplication), then the pair becomes its sum and its difference.
                        const evenIndex = block + offset;
                        const oddIndex = evenIndex + halfSize;
                        const oddReal = real[oddIndex] * offsetReal - imaginary[oddIndex] * offsetImaginary;
                        const oddImaginary = real[oddIndex] * offsetImaginary + imaginary[oddIndex] * offsetReal;
                        real[oddIndex] = real[evenIndex] - oddReal;
                        imaginary[oddIndex] = imaginary[evenIndex] - oddImaginary;
                        real[evenIndex] += oddReal;
                        imaginary[evenIndex] += oddImaginary;
                        // Advance the rotation by one step for the next element (another complex multiplication).
                        const nextOffsetReal = offsetReal * rotationReal - offsetImaginary * rotationImaginary;
                        offsetImaginary = offsetReal * rotationImaginary + offsetImaginary * rotationReal;
                        offsetReal = nextOffsetReal;
                    }
                }
            }
        }
    }

    /**
     * A segmented VU meter bound to the analyzer of the player output.
     *
     * It owns three things: the DOM elements of the meter, the per channel state (displayed level, peak and peak hold), and the animation loop that moves that state forward once per display frame.
     */
    class VuMeter {
        /**
         * Creates an instance of VuMeter.
         * @param {Object} options - The meter options.
         * @param {HTMLElement} options.container - The meter root of the bar, carrying the 'data-audio-player-vu-meter' hook.
         * @param {Object} [options.settings] - The user settings of the meter, in the shape the user settings endpoint returns.
         * @param {Document} [options.document] - The document the elements are created in. Defaults to the current document.
         * @param {Function} [options.requestFrame] - The frame scheduler, for a composition or a test. Defaults to requestAnimationFrame.
         * @param {Function} [options.cancelFrame] - The frame canceller. Defaults to cancelAnimationFrame.
         */
        constructor(options = {}) {
            this.container = options.container || null;
            this.document = options.document || (typeof document !== 'undefined' ? document : null);
            // The scheduler can be injected so a test can step through frames one by one with fake timestamps, instead of
            // waiting for real animation frames. requestAnimationFrame is the default because the browser calls it right
            // before it paints, once per display refresh, and pauses it in hidden tabs, so the meter never draws frames nobody can see.
            this.requestFrame = typeof options.requestFrame === 'function'
                ? options.requestFrame
                : (typeof requestAnimationFrame === 'function' ? (callback) => requestAnimationFrame(callback) : null);
            this.cancelFrame = typeof options.cancelFrame === 'function'
                ? options.cancelFrame
                : (typeof cancelAnimationFrame === 'function' ? (handle) => cancelAnimationFrame(handle) : null);
            this.settings = normalizeSettings(options.settings);
            this.analyser = null;
            this.spectrum = null;
            this.channels = [];
            // Per channel state, one entry per band, all between 0 and 1 except the hold times:
            //   displayLevels - the height the bar is drawn at; it jumps up with the input and then falls at the release rate.
            //   peakLevels    - the height of the peak marker; it only moves up with a new peak, then waits and falls slowly.
            //   peakHoldUntil - the time, on the meter clock in milliseconds, until which the peak marker stays still.
            this.displayLevels = [];
            this.peakLevels = [];
            this.peakHoldUntil = [];
            // True between start and stop. While it is false, the meter ignores the analyzer and only lets the bars fall.
            this.isInputActive = false;
            this.isDestroyed = false;
            /** @type {{ id: *, isFrame: boolean }|null} */
            this.frameHandle = null;
            // The state of the frame clock, see advanceClock: the raw timestamp of the previous frame, when the current
            // run of frames started and how many frames it had, and the meter's own time in milliseconds.
            this.lastFrameTime = null;
            this.runStartTime = 0;
            this.runFrameCount = 0;
            this.clockMilliseconds = 0;
            this.build();
        }

        /**
         * Gets the normalized settings the meter currently renders with.
         * @returns {Object} A frozen copy of the settings.
         */
        getSettings() {
            // A frozen copy, so a caller can inspect the settings but never change them behind the meter's back. Settings must go through configure, which also updates the DOM.
            return Object.freeze({ ...this.settings });
        }

        /**
         * Gets a copy of the levels that were rendered last.
         * @returns {Array<number>} The displayed level of every channel, between 0 and 1.
         */
        getLevels() {
            return [...this.displayLevels];
        }

        /**
         * Checks whether the meter is reading the analyzer.
         * @returns {boolean} True while the frame loop reads the analyzer.
         */
        isActive() {
            return this.isInputActive;
        }

        /**
         * Applies a new settings object. A change of the segment count rebuilds the channels, because the number of
         * segments is part of the markup; every other setting is applied in place, so a change never drops the levels.
         * @param {Object} settings - The user settings of the meter, in the shape the user settings endpoint returns.
         * @returns {LuminaAudioPlayer.VuMeter} This meter, for chaining.
         */
        configure(settings) {
            if (this.isDestroyed)
                return this;
            const next = normalizeSettings(settings);
            const previousSegmentCount = this.settings.segmentCount;
            this.settings = next;
            if (!this.container)
                return this;
            // The channel count check covers a meter whose markup was never fully built, for example because it had no document at construction time; such a meter gets its elements now.
            if (next.segmentCount !== previousSegmentCount || this.channels.length !== BAND_COUNT) {
                this.build();
                return this;
            }
            this.container.dataset.audioPlayerVuMeterOrientation = next.orientation;
            this.container.dataset.audioPlayerVuMeterSegments = String(next.segmentCount);
            this.container.dataset.audioPlayerVuMeterShowPeak = next.showPeak ? 'true' : 'false';
            this.container.style.setProperty('--audio-player-vu-segment-gap', `${next.segmentGap}px`);
            this.container.style.setProperty('--audio-player-vu-segment-count', String(next.segmentCount));
            for (const channel of this.channels) {
                if (channel.peak)
                    channel.peak.hidden = !next.showPeak;
            }
            // Redraw once so a changed threshold or peak option shows at once, even while the frame loop is idle.
            this.ensureLevelArrays();
            for (let band = 0; band < this.channels.length; band++)
                this.renderChannel(band, this.displayLevels[band], this.peakLevels[band]);
            return this;
        }

        /**
         * Binds the analyzer the meter reads from. Passing a different analyzer replaces the previous one and resets the
         * displayed levels, because the levels of another graph do not describe this one.
         * @param {AnalyserNode|null} analyser - The analyzer of the player output, or null to detach.
         * @returns {LuminaAudioPlayer.VuMeter} This meter, for chaining.
         */
        setAnalyser(analyser) {
            if (this.isDestroyed)
                return this;
            // Binding the same analyzer again is a no-op, so the player can call this freely (for example on every track change) without the meter flashing back to empty each time.
            if (this.analyser === analyser)
                return this;
            this.analyser = analyser || null;
            // A new VuMeterSpectrumAnalyzer is created because its buffers and band ranges depend on the sample rate and
            // the transform size of this particular analyzer.
            this.spectrum = this.analyser ? new VuMeterSpectrumAnalyzer({ analyser: this.analyser, bandCount: BAND_COUNT }) : null;
            this.reset();
            if (this.isInputActive)
                this.startFrameLoop();
            return this;
        }

        /**
         * Starts reading the analyzer. The frame loop runs while the meter is active, and then for as long as a channel
         * still shows a level, so stopping the player never leaves a frozen meter behind.
         */
        start() {
            if (this.isDestroyed)
                return;
            this.isInputActive = true;
            this.startFrameLoop();
        }

        /**
         * Stops reading the analyzer and lets the displayed levels and peaks fall to zero.
         */
        stop() {
            if (this.isDestroyed || !this.isInputActive)
                return;
            this.isInputActive = false;
            // Stopping still needs frames: the bars have to fall to zero smoothly rather than vanish. The loop is started
            // (it may already be running), and renderFrame ends it by itself once every channel is empty.
            this.startFrameLoop();
        }

        /**
         * Resets every displayed level and peak to zero, and draws the empty meter once.
         */
        reset() {
            const bandCount = this.channels.length;
            this.displayLevels = new Array(bandCount).fill(0);
            this.peakLevels = new Array(bandCount).fill(0);
            this.peakHoldUntil = new Array(bandCount).fill(0);
            for (let channel = 0; channel < bandCount; channel++)
                this.renderChannel(channel, 0, 0);
        }

        /**
         * Releases the frame loop and the element references. The generated channels are removed from the root, so a later meter bound to the same root starts from a clean shell.
         */
        destroy() {
            // The flag is set first, because a frame that was already queued may still fire once; every public method and
            // renderFrame check it and do nothing on a destroyed meter.
            this.isDestroyed = true;
            this.isInputActive = false;
            this.cancelFrameLoop();
            this.clear();
            this.channels = [];
            this.displayLevels = [];
            this.peakLevels = [];
            this.peakHoldUntil = [];
            this.analyser = null;
            this.spectrum = null;
        }

        //+======================================================================================+
        //|                                     Internal helpers                                 |
        //+======================================================================================+

        /**
         * Creates the channels, segments and peak markers from the current settings.
         */
        build() {
            // Start from an empty root, so building twice (for example after a segment count change) never leaves the channels of the previous configuration behind.
            this.clear();
            if (!this.container || !this.document)
                return;
            const next = this.settings;
            this.container.style.setProperty('--audio-player-vu-band-count', String(BAND_COUNT));
            this.container.dataset.audioPlayerVuMeterOrientation = next.orientation;
            this.container.dataset.audioPlayerVuMeterSegments = String(next.segmentCount);
            this.container.dataset.audioPlayerVuMeterShowPeak = next.showPeak ? 'true' : 'false';
            this.container.style.setProperty('--audio-player-vu-segment-gap', `${next.segmentGap}px`);
            this.container.style.setProperty('--audio-player-vu-segment-count', String(next.segmentCount));
            this.channels = [];
            for (let band = 0; band < BAND_COUNT; band++) {
                const channelElement = this.document.createElement('div');
                channelElement.className = 'audio-player-vu-channel';
                channelElement.dataset.audioPlayerVuChannel = String(band);
                const segments = [];
                for (let index = 0; index < next.segmentCount; index++) {
                    const segmentElement = this.document.createElement('div');
                    segmentElement.className = 'audio-player-vu-segment';
                    segmentElement.dataset.audioPlayerVuSegment = '';
                    segmentElement.dataset.audioPlayerVuSegmentState = 'inactive';
                    // The zone is decided by the level at the top of the segment: with 12 segments, segment 0 covers
                    // the levels up to 1/12, so it is checked at 1/12, and the last segment is checked at 1.
                    const zone = this.getZone((index + 1) / next.segmentCount);
                    segmentElement.dataset.audioPlayerVuSegmentZone = zone;
                    channelElement.appendChild(segmentElement);
                    // The values last written to the DOM are remembered next to the element, so renderChannel can compare
                    // against them instead of reading the attribute back from the DOM, which is slower.
                    segments.push({ element: segmentElement, renderedState: 'inactive', renderedZone: zone });
                }
                const peakElement = this.document.createElement('div');
                peakElement.className = 'audio-player-vu-peak';
                peakElement.dataset.audioPlayerVuPeak = '';
                peakElement.dataset.audioPlayerVuPeakState = 'inactive';
                peakElement.hidden = !next.showPeak;
                channelElement.appendChild(peakElement);
                this.container.appendChild(channelElement);
                this.channels.push({ element: channelElement, segments: segments, peak: peakElement, renderedPeak: null });
            }
            this.reset();
        }

        /**
         * Removes every generated element from the meter root.
         */
        clear() {
            if (!this.container)
                return;
            // replaceChildren with no arguments removes every child in one call.
            this.container.replaceChildren();
        }

        /**
         * Resolves the style zone of a segment from its position on the channel.
         * The comparisons are strict (greater than, not greater or equal), so a segment whose top sits exactly on a
         * threshold still belongs to the lower zone: with thresholds of 0.6 and 0.8 and ten segments, the sixth segment (top at 0.6) is the last normal one.
         * @param {number} position - The level of the segment, between 0 and 1, at which it becomes active.
         * @returns {string} 'normal', 'medium' or 'high'.
         */
        getZone(position) {
            if (position > this.settings.highThreshold)
                return 'high';
            return position > this.settings.mediumThreshold ? 'medium' : 'normal';
        }

        /**
         * Starts the frame loop when it is not already running.
         */
        startFrameLoop() {
            // A pending frame means the loop is already running. Starting a second one would make the meter advance twice per display frame, so the bars would fall at double speed.
            if (this.frameHandle !== null || this.isDestroyed)
                return;
            // Forget the previous frame, so the first frame of this run does not see the whole idle time as its duration.
            this.lastFrameTime = null;
            if (!this.requestFrame) {
                // Without an animation frame the meter still renders the first frame, so its state is correct; only the
                // animation is missing, which is a reduced motion fallback rather than a broken meter.
                this.renderFrame(0);
                return;
            }
            this.scheduleFrame();
        }

        /**
         * Schedules the next frame.
         */
        scheduleFrame() {
            if (!this.requestFrame)
                return;
            const tick = (timestamp) => {
                // The handle is cleared before rendering, because renderFrame decides whether to schedule the next frame, and scheduling only works while no frame is pending.
                this.frameHandle = null;
                this.renderFrame(timestamp);
            };
            try {
                const id = this.requestFrame(tick);
                // An injected scheduler may not return an id. Without an id the frame cannot be cancelled, so canCancel records whether cancelFrameLoop may try.
                this.frameHandle = { id: id, isFrame: true, canCancel: id !== undefined && id !== null };
            } catch (error) {
                console.debug('Audio player: the VU meter could not schedule its frame.', error);
                this.frameHandle = null;
            }
        }

        /**
         * Cancels the pending frame, when there is one.
         */
        cancelFrameLoop() {
            if (this.frameHandle === null)
                return;
            const handle = this.frameHandle;
            this.frameHandle = null;
            if (!this.cancelFrame || handle.canCancel !== true)
                return;
            try {
                if (handle.isFrame === true)
                    this.cancelFrame(handle.id);
                else
                    clearTimeout(handle.id);
            } catch (error) {
                console.debug('Audio player: the VU meter frame could not be cancelled.', error);
            }
        }

        /**
         * Advances the frame clock of the meter and returns the time the frame lasted.
         *
         * Every duration of the meter, the falloff, the peak hold and the analysis cadence, is measured on this clock
         * instead of on the raw frame timestamp, because a browser that resists fingerprinting, such as LibreWolf or
         * Firefox with that protection on, rounds the timestamps to steps of 100 milliseconds or more. A frame would then
         * see a delta of zero, followed by a delta of a hundred milliseconds, so the bars would fall in lumps and the
         * analysis would run about ten times a second. A delta that looks like a real frame is used as it is. A delta that
         * cannot be one, because it is zero or far longer than a frame, is replaced by the average frame duration measured
         * over the whole run, which converges even when every single timestamp is coarse.
         * (Browsers round timestamps on purpose: precise timers let a page measure tiny differences in how fast the
         * machine runs, which can identify a user or leak data between pages.)
         *
         * The meter works with elapsed time rather than a fixed step per frame so it moves at the same speed on a 60 Hz
         * and on a 144 Hz display: a release of 3 per second must take a full bar down in a third of a second on both.
         * @param {number} [timestamp] - The frame timestamp, in milliseconds. Defaults to the current time.
         * @returns {number} The duration of the frame, in seconds.
         */
        advanceClock(timestamp) {
            const raw = toFiniteNumber(timestamp) ?? (typeof performance !== 'undefined' ? performance.now() : Date.now());
            let elapsed = 1 / 60;
            if (this.lastFrameTime === null) {
                // The first frame of a run has nothing to measure against, so it lasts zero seconds and starts the run.
                this.runStartTime = raw;
                this.runFrameCount = 0;
                elapsed = 0;
            } else {
                this.runFrameCount++;
                const delta = raw - this.lastFrameTime;
                const runSeconds = (raw - this.runStartTime) / 1000;
                // The average only becomes trustworthy after enough frames to smooth out the rounding, so it waits for 30
                // frames (about half a second) and assumes a 60 Hz display until then.
                const average = this.runFrameCount >= 30 && runSeconds > 0 ? runSeconds / this.runFrameCount : 1 / 60;
                const isPlausible = delta >= MINIMUM_REAL_FRAME_MS && delta <= MAXIMUM_REAL_FRAME_MS;
                // The lower bound (a 240 Hz frame) keeps a bogus tiny delta from stalling the animation, and the upper bound is MAXIMUM_FRAME_SECONDS (see its comment).
                elapsed = clamp(isPlausible ? delta / 1000 : average, 1 / 240, MAXIMUM_FRAME_SECONDS);
            }
            this.lastFrameTime = raw;
            this.clockMilliseconds += elapsed * 1000;
            return elapsed;
        }

        /**
         * Advances the meter by one frame: reads the analyzer, applies the attack, the release and the peak hold, and renders the result.
         * @param {number} [timestamp] - The frame timestamp, in milliseconds. Defaults to the current time.
         */
        renderFrame(timestamp) {
            if (this.isDestroyed)
                return;
            const elapsed = this.advanceClock(timestamp);
            const now = this.clockMilliseconds;
            // levels is null on most frames: either the meter is stopped, or no new block of samples is due yet (see
            // readLevels). A null means "no new measurement", not "silence", so the code below only lets the bars fall.
            const levels = this.isInputActive && this.spectrum ? this.spectrum.readLevels(now / 1000) : null;
            this.ensureLevelArrays();
            // How far a bar and a peak marker fall during this frame. A rate is in level units per second, so multiplying it
            // by the frame duration gives the fall for this frame: 3 per second over a 1/60 second frame is 0.05.
            const levelStep = this.settings.releasePerSecond * elapsed;
            const peakStep = this.settings.peakReleasePerSecond * elapsed;
            const holdMilliseconds = this.settings.peakHoldDurationInMilliseconds;
            let hasVisibleLevel = false;
            for (let band = 0; band < this.channels.length; band++) {
                const input = levels && band < levels.length ? clamp(levels[band], 0, 1) : 0;
                // Attack: a louder measurement moves the bar up at once. A quieter one is ignored here, because the bar
                // must come down at the release rate rather than jump down.
                if (levels && input > this.displayLevels[band])
                    this.displayLevels[band] = input;
                // Release: every frame, the bar falls by one step. It also runs on the frame a new level arrived, so a bar
                // starts falling straight away; this is what makes a beat read as a short, sharp kick.
                this.displayLevels[band] = Math.max(0, this.displayLevels[band] - levelStep);
                // A new peak, or a level that reaches the current peak again, moves the marker there and restarts its hold.
                // Greater or equal (not just greater) keeps the marker parked while a sustained note stays at the same level.
                if (levels && input >= this.peakLevels[band]) {
                    this.peakLevels[band] = input;
                    this.peakHoldUntil[band] = now + holdMilliseconds;
                }
                // Once the hold time is over, the marker falls at its own, usually slower, rate.
                if (now >= this.peakHoldUntil[band])
                    this.peakLevels[band] = Math.max(0, this.peakLevels[band] - peakStep);
                this.renderChannel(band, this.displayLevels[band], this.peakLevels[band]);
                if (this.displayLevels[band] > 0 || this.peakLevels[band] > 0)
                    hasVisibleLevel = true;
            }
            // A meter that is not reading the analyzer draws its last falling frame, then releases the loop. An active
            // meter keeps its loop while it has an analyzer to read, because new levels can arrive at any frame.
            if ((this.isInputActive && this.spectrum) || hasVisibleLevel)
                this.scheduleFrame();
            else
                this.cancelFrameLoop();
        }

        /**
         * Makes sure the per channel state arrays match the rendered channels.
         * They can fall out of step when the channels are rebuilt. The displayed levels are carried over, so a rebuild
         * does not make the bars flash to empty, while the peaks and their hold times simply start over.
         */
        ensureLevelArrays() {
            const count = this.channels.length;
            if (this.displayLevels.length !== count) {
                const previous = this.displayLevels;
                this.displayLevels = new Array(count).fill(0);
                for (let index = 0; index < Math.min(previous.length, count); index++)
                    this.displayLevels[index] = previous[index];
            }
            if (this.peakLevels.length !== count)
                this.peakLevels = new Array(count).fill(0);
            if (this.peakHoldUntil.length !== count)
                this.peakHoldUntil = new Array(count).fill(0);
        }

        /**
         * Renders one channel, writing only the attributes that changed since the last frame.
         * Writing an attribute, even with the value it already has, makes the browser re-check the styles of that element.
         * With 24 channels of 12 segments that is 288 elements per frame, so skipping unchanged writes is what keeps the meter cheap.
         * @param {number} band - The zero based channel index.
         * @param {number} level - The displayed level of the channel, between 0 and 1.
         * @param {number} peak - The held peak level of the channel, between 0 and 1.
         */
        renderChannel(band, level, peak) {
            const channel = this.channels[band];
            if (!channel)
                return;
            // Rounded up, so any level above zero lights at least one segment and quiet sound is still visible.
            const activeSegments = Math.ceil(clamp(level, 0, 1) * this.settings.segmentCount);
            for (let index = 0; index < channel.segments.length; index++) {
                const segment = channel.segments[index];
                const state = index < activeSegments ? 'active' : 'inactive';
                if (segment.renderedState !== state) {
                    segment.element.dataset.audioPlayerVuSegmentState = state;
                    segment.renderedState = state;
                }
                // The zone is checked here as well, and not only in build, because configure changes the thresholds without rebuilding the segments.
                const zone = this.getZone((index + 1) / this.settings.segmentCount);
                if (segment.renderedZone !== zone) {
                    segment.element.dataset.audioPlayerVuSegmentZone = zone;
                    segment.renderedZone = zone;
                }
            }
            if (!channel.peak)
                return;
            if (!this.settings.showPeak) {
                if (channel.renderedPeak !== null) {
                    channel.peak.dataset.audioPlayerVuPeakState = 'inactive';
                    channel.renderedPeak = null;
                }
                return;
            }
            const position = clamp(peak, 0, 1);
            const state = position > 0 ? 'active' : 'inactive';
            // The position is written as a percentage of the channel axis, so the theme maps it to whichever axis its own
            // layout uses. A small tolerance keeps sub pixel movement from rewriting the property every frame.
            // 0.005 is half a percent of the channel, which on a 47 pixel meter is less than a quarter of a pixel. An
            // inactive marker is always written, so a marker that falls to zero lands exactly at 0%.
            if (channel.renderedPeak === null || Math.abs(channel.renderedPeak - position) >= 0.005 || state === 'inactive') {
                channel.peak.style.setProperty('--audio-player-vu-peak-position', `${position * 100}%`);
                channel.renderedPeak = position;
            }
            if (channel.peak.dataset.audioPlayerVuPeakState !== state)
                channel.peak.dataset.audioPlayerVuPeakState = state;
        }
    }

    LuminaAudioPlayer.VuMeterSpectrumAnalyzer = VuMeterSpectrumAnalyzer;
    LuminaAudioPlayer.VuMeter = VuMeter;

    /**
     * Creates a VU meter. See {@link LuminaAudioPlayer.VuMeter} for the options.
     * @param {Object} options - The meter options.
     * @returns {LuminaAudioPlayer.VuMeter} The created meter.
     */
    LuminaAudioPlayer.createVuMeter = function (options) {
        return new VuMeter(options);
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
