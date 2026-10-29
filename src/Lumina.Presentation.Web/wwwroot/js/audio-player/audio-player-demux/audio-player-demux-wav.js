/**
 * WAV (RIFF) demuxer of the audio player.
 *
 * A WAV file is a RIFF container: the four byte magic 'RIFF', a size, the four byte form 'WAVE', and then a sequence of
 * tagged chunks, of which the 'fmt ' chunk describes the audio and the 'data' chunk holds it. The audio is uncompressed,
 * so the payload of the data chunk is the samples themselves, interleaved by channel. There is no framing inside the
 * payload at all: this demuxer invents packets by cutting the payload into fixed size blocks of whole frames, which is
 * what makes the stream seekable to an exact byte and therefore to an exact sample.
 *
 * The codec private data and the codec string
 * -------------------------------------------
 * WebCodecs decodes uncompressed audio under the PCM codec strings, one per sample format ('pcm-u8', 'pcm-s16',
 * 'pcm-s24', 'pcm-s32', 'pcm-f32'), each packet being raw interleaved samples and no description. This demuxer maps the
 * sample format and the bits per sample of the fmt chunk to the matching codec string: a 8 bit PCM stream is unsigned
 * (as RIFF defines 8 bit samples), an integer stream of 16, 24 or 32 bits is signed, and a 32 bit IEEE float stream is
 * 'pcm-f32'. A format or a bit depth the player has no codec string for is refused as unsupported.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The number of frames a WAV packet holds. The payload is uncompressed, so the block only has to be small enough to
     * keep a packet light and large enough to keep the call overhead low.
     * @readonly
     * @type {number}
     */
    const FRAMES_PER_PACKET = 4096;

    /**
     * The format tag RIFF uses for integer PCM.
     * @readonly
     * @type {number}
     */
    const FORMAT_PCM = 1;

    /**
     * The format tag RIFF uses for IEEE float PCM.
     * @readonly
     * @type {number}
     */
    const FORMAT_FLOAT = 3;

    /**
     * The format tag RIFF uses for an extensible format, whose real format is in a subformat GUID.
     * @readonly
     * @type {number}
     */
    const FORMAT_EXTENSIBLE = 0xFFFE;

    /**
     * Resolves the WebCodecs PCM codec string of a WAV sample format.
     * @param {number} audioFormat - The format tag of the fmt chunk, after resolving an extensible format.
     * @param {number} bitsPerSample - The bits per sample of the fmt chunk.
     * @returns {string|null} The codec string, or null when the player has no codec string for the format.
     */
    function resolvePcmCodec(audioFormat, bitsPerSample) {
        if (audioFormat === FORMAT_FLOAT)
            return bitsPerSample === 32 ? 'pcm-f32' : null;
        if (audioFormat !== FORMAT_PCM)
            return null;
        if (bitsPerSample === 8)
            return 'pcm-u8';
        if (bitsPerSample === 16)
            return 'pcm-s16';
        if (bitsPerSample === 24)
            return 'pcm-s24';
        if (bitsPerSample === 32)
            return 'pcm-s32';
        return null;
    }

    /**
     * Demuxes a WAV (RIFF) stream.
     */
    class WavDemuxer extends LuminaAudioPlayer.Demuxer {
        /**
         * Creates an instance of WavDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            this.offset = 0;
            this.dataStart = 0;
            this.dataEnd = 0;
            this.timelineMicros = 0;
            /** @type {Object|null} */
            this.format = null;
            this.durationSeconds = null;
            this.reachedEnd = false;
        }

        //+=======================================================================================+
        //|                                     Initialization                                    |
        //+=======================================================================================+

        /**
         * Reads the RIFF header and walks the chunks to find the fmt chunk and the data chunk.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not a WAV stream the player decodes.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                const riff = await this.reader.readAsync(0, 12);
                if (!LuminaAudioPlayer.demuxBytesEqualAscii(riff, 0, 'RIFF') || !LuminaAudioPlayer.demuxBytesEqualAscii(riff, 8, 'WAVE'))
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The bytes are not a WAV stream.', { reason: 'no-riff-magic' });
                const result = await this.readChunksAsync();
                if (result.formatResult === null)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The WAV stream carries no format chunk.', { reason: 'no-format-chunk' });
                if (result.formatResult.status === 'invalid')
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The WAV format chunk is too short or malformed to describe the audio.', { reason: 'invalid-format-chunk' });
                if (result.formatResult.status === 'unsupported')
                    // The chunk is present and well formed, but it names a format or a bit depth the player decodes no PCM
                    // codec string for, which is a different outcome from a chunk that is missing, and is reported as such.
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The WAV stream holds an audio format the player does not decode.', {
                        reason: 'unsupported-format',
                        audioFormat: result.formatResult.audioFormat,
                        bitsPerSample: result.formatResult.bitsPerSample
                    });
                if (result.dataStart === null)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The WAV stream carries no data chunk.', { reason: 'no-data-chunk' });
                const format = result.formatResult.format;
                this.format = format;
                this.dataStart = result.dataStart;
                this.dataEnd = result.dataEnd;
                this.durationSeconds = format.byteRate > 0 ? (this.dataEnd - this.dataStart) / format.byteRate : null;
                this.offset = this.dataStart;
                this.timelineMicros = 0;
                this.reachedEnd = false;
                this.initialized = true;
            } catch (error) {
                throw this.recordFailure(error, 'initialize-failed');
            }
        }

        /**
         * Gets the decoder configuration of the stream.
         * @returns {Object|null} The configuration, or null before initialization.
         */
        getConfiguration() {
            if (!this.format)
                return null;
            return {
                codec: this.format.codec,
                sampleRate: this.format.sampleRate,
                numberOfChannels: this.format.channels
            };
        }

        /**
         * Gets the duration of the stream, computed from the size of the data chunk and the byte rate of the format.
         * @returns {number|null} The duration in seconds, or null when it cannot be derived.
         */
        getDuration() {
            return this.durationSeconds;
        }

        //+=======================================================================================+
        //|                                       Reading                                         |
        //+=======================================================================================+

        /**
         * Reads the next block of whole frames from the data chunk as an encoded packet.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            if (this.reachedEnd)
                return null;
            try {
                const format = this.format;
                const remaining = this.dataEnd - this.offset;
                if (remaining < format.blockAlign) {
                    this.reachedEnd = true;
                    return null;
                }
                const frames = Math.min(FRAMES_PER_PACKET, Math.floor(remaining / format.blockAlign));
                const length = frames * format.blockAlign;
                const data = await this.reader.copyAsync(this.offset, length);
                if (data.length < length) { // A short read means the stream was truncated below the data chunk size it declared.                   
                    this.reachedEnd = true;
                    return null;
                }
                // The duration is kept unrounded while the timeline accumulates it, so that rounding it per packet does not
                // make the timeline drift; only the packet the decoder receives carries a whole number of microseconds.
                const duration = (frames / format.sampleRate) * 1000000;
                const packet = LuminaAudioPlayer.createDemuxerPacket(data, this.timelineMicros, duration);
                this.timelineMicros += duration;
                this.offset += length;
                if (this.offset + format.blockAlign > this.dataEnd)
                    this.reachedEnd = true;
                return packet;
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Repositions the read cursor to the exact frame that covers a time. The payload is uncompressed and frame aligned, so the position is computed rather than scanned.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the stream cannot be seeked.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const format = this.format;
            if (!format || !Number.isFinite(seconds) || seconds < 0)
                return null;
            const frame = Math.floor(seconds * format.sampleRate);
            const offset = Math.min(this.dataEnd, this.dataStart + frame * format.blockAlign);
            const aligned = this.dataStart + Math.floor((offset - this.dataStart) / format.blockAlign) * format.blockAlign;
            this.offset = aligned;
            this.reachedEnd = this.offset >= this.dataEnd;
            // The exact time of the requested frame, kept unrounded so the timeline it seeds does not drift.
            this.timelineMicros = ((aligned - this.dataStart) / format.blockAlign / format.sampleRate) * 1000000;
            return aligned;
        }

        //+=======================================================================================+
        //|                                   Internal helpers                                    |
        //+=======================================================================================+

        /**
         * Walks the RIFF chunks, parsing the format chunk and locating the data chunk.
         * @returns {Promise<Object>} The result of the format chunk, and the offset and end of the data chunk. The format result distinguishes an absent chunk from an unsupported one.
         */
        async readChunksAsync() {
            const end = this.reader.getLength();
            let cursor = 12;
            /** @type {Object|null} */
            let formatResult = null;
            let dataStart = null;
            let dataEnd = null;
            // The chunk list is bounded so a malformed size field cannot make this walk forever.
            for (let count = 0; count < 4096; count++) {
                if (end !== null && cursor + 8 > end)
                    break;
                const header = await this.reader.readAsync(cursor, 8);
                if (header.length < 8)
                    break;
                const id = String.fromCharCode(header[0], header[1], header[2], header[3]);
                const size = LuminaAudioPlayer.demuxReadUintLE(header, 4, 4) || 0;
                const bodyStart = cursor + 8;
                if (id === 'fmt ')
                    formatResult = await this.parseFormatChunkAsync(bodyStart, size);
                else if (id === 'data') {
                    dataStart = bodyStart;
                    dataEnd = end === null ? bodyStart + size : Math.min(bodyStart + size, end);
                }
                // A RIFF chunk body is padded to an even length, and the pad byte is not counted in the size.
                cursor = bodyStart + size + (size % 2);
            }
            return { formatResult, dataStart, dataEnd };
        }

        /**
         * Parses the fmt chunk of a WAV stream.
         * @param {number} offset - The offset of the chunk body.
         * @param {number} size - The number of bytes of the chunk body.
         * @returns {Promise<Object>} A result whose status is 'ok' with the parsed format, 'unsupported' with the format tag and bit depth the player decodes no PCM codec string for, or 'invalid' when the chunk is too short or malformed. The three are distinguished so that a present but unsupported format is not reported as a missing chunk.
         */
        async parseFormatChunkAsync(offset, size) {
            if (size < 16)
                return { status: 'invalid' };
            const bytes = await this.reader.readAsync(offset, Math.min(size, 40));
            if (bytes.length < 16)
                return { status: 'invalid' };
            // The first sixteen bytes of a fmt chunk are the format tag, the channel count, the sample rate, the byte rate,
            // the block alignment and the bits per sample, in that order, every one little endian.
            let audioFormat = LuminaAudioPlayer.demuxReadUintLE(bytes, 0, 2);
            const channels = LuminaAudioPlayer.demuxReadUintLE(bytes, 2, 2);
            const sampleRate = LuminaAudioPlayer.demuxReadUintLE(bytes, 4, 4);
            const byteRate = LuminaAudioPlayer.demuxReadUintLE(bytes, 8, 4);
            let blockAlign = LuminaAudioPlayer.demuxReadUintLE(bytes, 12, 2);
            const bitsPerSample = LuminaAudioPlayer.demuxReadUintLE(bytes, 14, 2);
            // An extensible format names its real format in the first two bytes of the subformat GUID.
            if (audioFormat === FORMAT_EXTENSIBLE && size >= 40 && bytes.length >= 40)
                audioFormat = LuminaAudioPlayer.demuxReadUintLE(bytes, 24, 2);
            if (!channels || channels <= 0 || !sampleRate || sampleRate <= 0 || !bitsPerSample)
                return { status: 'invalid' };
            const codec = resolvePcmCodec(audioFormat, bitsPerSample);
            if (!codec)
                return { status: 'unsupported', audioFormat, bitsPerSample };
            const sampleBytes = bitsPerSample / 8;
            // A stream whose block alignment disagrees with its channels and bit depth cannot be sliced into frames, so the computed alignment is used and the declared one is only a fallback.
            if (!blockAlign || blockAlign <= 0 || blockAlign % sampleBytes !== 0 || blockAlign % channels !== 0)
                blockAlign = channels * sampleBytes;
            return { status: 'ok', format: Object.freeze({ codec, channels, sampleRate, byteRate: byteRate || sampleRate * blockAlign, blockAlign, bitsPerSample }) };
        }
    }

    LuminaAudioPlayer.WavDemuxer = WavDemuxer;

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.WAV,
        codec: LuminaAudioPlayer.DemuxerCodec.PCM,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.PCM,
        create: (options) => new WavDemuxer(options)
    });
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
