/**
 * AAC (ADTS) demuxer of the audio player.
 *
 * A raw AAC file is a stream of ADTS frames, each one a short header followed by one or more raw AAC data blocks. Unlike
 * an MP3 frame the header does not state the bitrate: it states the length of the whole ADTS frame in bytes, which is
 * what makes the frame directly addressable, together with the Audio Object Type, the sample rate index, the channel
 * configuration and the number of raw data blocks the frame carries. This demuxer walks those frames and hands each one
 * to the decoder as an encoded packet.
 *
 * Framing and the codec private data
 * ----------------------------------
 * The WebCodecs AAC registration accepts either an ADTS streamed form, where each EncodedAudioChunk is a whole ADTS frame
 * and no description is given, or a raw form, where each chunk is a raw AAC frame and the description is the
 * AudioSpecificConfig. This demuxer uses the first, because the ADTS header is already the per frame configuration and
 * carrying it in each frame means a frame can be decoded even after a seek that lands in the middle of the stream without
 * re-deriving the AudioSpecificConfig. The configuration therefore carries the codec string, the sample rate and the
 * channel count, and no description.
 *
 * Duration
 * --------
 * An AAC frame decodes to 1024 samples per raw data block, and the number of raw data blocks is stated by the two bit
 * field of the header, so the duration of a packet is 1024 times that count, divided by the sample rate. The total
 * duration of the stream is not stated anywhere in an ADTS header, so it is estimated from the average byte rate of the
 * frames read so far once the stream is past its beginning, and only once the end is reached is it exact.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The number of bytes of an ADTS header without the optional CRC.
     * @readonly
     * @type {number}
     */
    const ADTS_HEADER_BYTES = 7;

    /**
     * The number of bytes of an ADTS header with the optional CRC.
     * @readonly
     * @type {number}
     */
    const ADTS_HEADER_CRC_BYTES = 9;

    /**
     * The sample rates, in Hz, indexed by the four bit sample rate index of an AAC header.
     * @readonly
     * @type {Array<number>}
     */
    const SAMPLE_RATES = Object.freeze([96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11025, 8000, 7350]);

    /**
     * The number of samples one raw AAC data block decodes to.
     * @readonly
     * @type {number}
     */
    const SAMPLES_PER_BLOCK = 1024;

    /**
     * The twelve bit ADTS frame sync, as the two byte prefix a scan matches on. The mask covers the top nibble of the
     * second byte, so a scan rejects a byte pattern that is not a sync by a comparison instead of by an awaited header
     * parse.
     * @readonly
     * @type {Array<number>}
     */
    const ADTS_SYNC_PREFIX = Object.freeze([0xFF, 0xF0]);

    /**
     * The byte mask of {@link ADTS_SYNC_PREFIX}, matching the twelve bit sync exactly.
     * @readonly
     * @type {Array<number>}
     */
    const ADTS_SYNC_MASK = Object.freeze([0xFF, 0xF0]);

    /**
     * The number of channels each channel configuration of an ADTS header names, indexed by its three bit value. Index 0
     * means the channels are described by a program configuration element, which this demuxer refuses, and index 7 is eight
     * channels (7.1), not seven, so the raw value cannot be used as a channel count.
     * @readonly
     * @type {Array<number>}
     */
    const CHANNEL_CONFIGURATIONS = Object.freeze([0, 1, 2, 3, 4, 5, 6, 8]);

    /**
     * Parses the header of an ADTS frame.
     * @param {Uint8Array} bytes - The bytes to read the header from.
     * @param {number} offset - The offset of the first header byte.
     * @returns {Object|null} The parsed header, or null when the bytes are not a valid ADTS header.
     */
    function parseAdtsHeader(bytes, offset) {
        if (!bytes || offset < 0 || offset + ADTS_HEADER_BYTES > bytes.length)
            return null;
        // An ADTS header is a bit packed big endian structure. The fields this demuxer reads are laid out as:
        //   bytes 0-1: the twelve bit sync, then the MPEG version, two layer bits that are always zero, and the
        //     protection-absent flag that says whether a two byte CRC follows the header
        //   byte 2:   the two bit Audio Object Type, the four bit sample rate index, and the top bit of the channel
        //     configuration
        //   byte 3:   the low two bits of the channel configuration, then the top two bits of the frame length
        //   byte 4:   the middle eight bits of the frame length
        //   byte 5:   the low three bits of the frame length, then the buffer fullness
        //   byte 6:   the two bit count of raw data blocks
        // Every field below is extracted from that layout, which is why the shifts and masks cross byte boundaries.
        if (bytes[offset] !== 0xFF || (bytes[offset + 1] & 0xF0) !== 0xF0)
            return null;
        // The layer field of an ADTS header is always zero; a non zero layer is a false sync.
        if (((bytes[offset + 1] >> 1) & 0x03) !== 0)
            return null;
        const protectionAbsent = (bytes[offset + 1] & 0x01) === 1;
        // The Audio Object Type is one more than the two bit field, and the raw data block count is one more than its
        // two bit field, because both are stored biased by one.
        const objectType = ((bytes[offset + 2] >> 6) & 0x03) + 1;
        const sampleRateIndex = (bytes[offset + 2] >> 2) & 0x0F;
        if (sampleRateIndex >= SAMPLE_RATES.length)
            return null;
        const channelConfiguration = ((bytes[offset + 2] & 0x01) << 2) | ((bytes[offset + 3] >> 6) & 0x03);
        const frameLength = ((bytes[offset + 3] & 0x03) * 2048)
            + (bytes[offset + 4] * 8)
            + ((bytes[offset + 5] >> 5) & 0x07);
        const headerBytes = protectionAbsent ? ADTS_HEADER_BYTES : ADTS_HEADER_CRC_BYTES;
        if (frameLength < headerBytes)
            return null;
        const rawDataBlocks = (bytes[offset + 6] & 0x03) + 1;
        return Object.freeze({
            objectType,
            sampleRate: SAMPLE_RATES[sampleRateIndex],
            sampleRateIndex,
            channelConfiguration,
            frameLength,
            headerBytes,
            rawDataBlocks
        });
    }

    /**
     * Demuxes an AAC stream that is framed as ADTS.
     */
    class AacDemuxer extends LuminaAudioPlayer.Demuxer {
        /**
         * Creates an instance of AacDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            this.offset = 0;
            this.dataStart = 0;
            this.timelineMicros = 0;
            /** @type {Object|null} */
            this.header = null;
            this.durationSeconds = null;
            this.reachedEnd = false;
            // The first audio frame, the anchor the average byte rate is estimated from while the end of the stream is unknown.
            this.firstFrameOffset = 0;
        }

        //+======================================================================================+
        //|                                     Initialization                                    |
        //+======================================================================================+

        /**
         * Skips an ID3v2 tag if the file carries one and positions the read cursor on the first ADTS frame.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not an ADTS stream.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                const dataStart = await this.skipId3v2Async(0);
                const end = this.reader.getLength();
                const firstFrame = await LuminaAudioPlayer.scanForFrameAsync(this.reader, dataStart, end, ADTS_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: ADTS_SYNC_MASK });
                if (firstFrame < 0)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The bytes do not contain an ADTS frame.', { reason: 'no-adts-frame' });
                const header = await this.parseHeaderAtAsync(firstFrame);
                if (!header)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The ADTS frame header could not be parsed.', { reason: 'no-adts-frame' });
                if (header.channelConfiguration === 0)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The AAC stream describes its channels in a program configuration this demuxer does not read.', { reason: 'unsupported-channel-config' });
                this.dataStart = firstFrame;
                this.firstFrameOffset = firstFrame;
                this.offset = firstFrame;
                this.header = header;
                this.durationSeconds = null;
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
            if (!this.header)
                return null;
            return {
                codec: `mp4a.40.${this.header.objectType}`,
                sampleRate: this.header.sampleRate,
                numberOfChannels: CHANNEL_CONFIGURATIONS[this.header.channelConfiguration]
            };
        }

        /**
         * Gets the best known duration of the stream, which is exact only once the last frame was read.
         * @returns {number|null} The duration in seconds, or null when it is not known yet.
         */
        getDuration() {
            return this.durationSeconds;
        }

        //+======================================================================================+
        //|                                       Reading                                         |
        //+======================================================================================+

        /**
         * Reads the next ADTS frame as an encoded packet.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            if (this.reachedEnd)
                return null;
            try {
                const header = await this.parseHeaderAtAsync(this.offset);
                if (!header)
                    return await this.resyncAsync();
                const data = await this.reader.readAsync(this.offset, header.frameLength);
                if (data.length < header.frameLength) {
                    this.reachedEnd = true;
                    this.updateEstimatedDuration();
                    return null;
                }
                // The duration is kept unrounded while the timeline accumulates it, so that rounding it per packet does not
                // make the timeline drift; only the packet the decoder receives carries a whole number of microseconds.
                const duration = ((SAMPLES_PER_BLOCK * header.rawDataBlocks) / header.sampleRate) * 1000000;
                const packet = LuminaAudioPlayer.createDemuxerPacket(data.slice(), this.timelineMicros, duration);
                this.timelineMicros += duration;
                this.offset += header.frameLength;
                this.updateEstimatedDuration();
                return packet;
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Rescans for the next ADTS frame after a byte that is not the start of one.
         * @returns {Promise<Object|null>} The next packet, or null when no further frame is found.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async resyncAsync() {
            const end = this.reader.getLength();
            const next = await LuminaAudioPlayer.scanForFrameAsync(this.reader, this.offset + 1, end, ADTS_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: ADTS_SYNC_MASK });
            if (next < 0) {
                this.reachedEnd = true;
                this.updateEstimatedDuration();
                return null;
            }
            this.offset = next;
            return await this.readPacketAsync();
        }

        /**
         * Repositions the read cursor to the ADTS frame that covers a time. An ADTS stream has no seek table, so the
         * byte position is estimated from the average byte rate of the frames read so far, and then scanned forward to
         * the next real frame. A seek to the start is answered exactly even on a fresh demuxer, whose frames have not been
         * walked yet, because the first frame is already known from initialization.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the stream cannot be seeked.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan for a frame gives up on a corrupt or crafted region.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const end = this.reader.getLength();
            if (end === null || !Number.isFinite(seconds) || seconds < 0)
                return null;
            if (seconds <= 0) {
                this.offset = this.firstFrameOffset;
                this.reachedEnd = false;
                this.timelineMicros = 0;
                return this.firstFrameOffset;
            }
            const elapsedBytes = this.offset - this.firstFrameOffset;
            const elapsedMicros = this.timelineMicros;
            if (elapsedBytes <= 0 || elapsedMicros <= 0)
                return null;
            const bytesPerSecond = elapsedBytes / (elapsedMicros / 1000000);
            const estimate = this.firstFrameOffset + Math.floor(seconds * bytesPerSecond);
            const start = Math.max(this.firstFrameOffset, Math.min(end - 1, estimate));
            const frame = await LuminaAudioPlayer.scanForFrameAsync(this.reader, start, end, ADTS_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: ADTS_SYNC_MASK });
            if (frame < 0)
                return null;
            this.offset = frame;
            this.reachedEnd = false;
            // The exact sample position of the frame is not known without walking the stream from its start, so the
            // timeline is set to the requested time, which the seek stage uses as the target to trim the first packet to.
            this.timelineMicros = Math.max(0, seconds) * 1000000;
            return frame;
        }

        //+======================================================================================+
        //|                                   Internal helpers                                    |
        //+======================================================================================+

        /**
         * Reads the header of an ADTS frame at an offset.
         * @param {number} offset - The offset of the frame.
         * @returns {Promise<Object|null>} The parsed header, or null when the bytes are not a valid ADTS header.
         */
        async parseHeaderAtAsync(offset) {
            const bytes = await this.reader.readAsync(offset, ADTS_HEADER_BYTES);
            if (bytes.length < ADTS_HEADER_BYTES)
                return null;
            return parseAdtsHeader(bytes, 0);
        }

        /**
         * Checks whether a candidate offset is the start of a real ADTS frame, by parsing its header and, when the next
         * frame fits in the stream, by requiring it to parse with the same sample rate and channel configuration too.
         * @param {number} offset - The candidate offset.
         * @returns {Promise<boolean>} True when the offset is the start of a real frame.
         */
        async isValidFrameAtAsync(offset) {
            const header = await this.parseHeaderAtAsync(offset);
            if (!header)
                return false;
            const end = this.reader.getLength();
            const next = offset + header.frameLength;
            if (end !== null && next + ADTS_HEADER_BYTES > end)
                return true;
            const nextHeader = await this.parseHeaderAtAsync(next);
            if (!nextHeader)
                return false;
            return nextHeader.sampleRate === header.sampleRate
                && nextHeader.channelConfiguration === header.channelConfiguration;
        }

        /**
         * Refreshes the estimated duration from the average byte rate of the frames read so far. It is exact only once
         * the last frame was read, because an ADTS stream states no duration.
         */
        updateEstimatedDuration() {
            const bytes = this.offset - this.firstFrameOffset;
            const micros = this.timelineMicros;
            if (bytes <= 0 || micros <= 0)
                return;
            const end = this.reader.getLength();
            if (end === null)
                return;
            const remaining = end - this.firstFrameOffset;
            const totalMicros = (remaining / bytes) * micros;
            this.durationSeconds = totalMicros / 1000000;
        }
    }

    LuminaAudioPlayer.AacDemuxer = AacDemuxer;

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.AAC,
        codec: LuminaAudioPlayer.DemuxerCodec.AAC,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.AAC,
        create: (options) => new AacDemuxer(options)
    });
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
