/**
 * MP3 demuxer of the audio player.
 *
 * An MP3 file is a stream of MPEG audio frames, each one a self contained frame whose four byte header fully describes
 * it: the MPEG version, the layer, the bitrate and the sample rate of the frame, so its length can be computed without
 * reading it, and the number of samples it decodes to is fixed by the version and the layer. This demuxer walks those
 * frames, hands each one to the decoder as an encoded packet, and derives the sample rate, the channel count and the
 * duration from the headers themselves.
 *
 * The header, and what is read from it
 * ------------------------------------
 * The header of a frame gives the MPEG version (MPEG-1, MPEG-2 or MPEG-2.5), the layer (I, II or III), the bitrate, the
 * sample rate, whether the frame is padded, and the channel mode. From the version and the layer come the samples per
 * frame (1152 for MPEG-1 Layer III, 576 for MPEG-2 or MPEG-2.5 Layer III, and so on), and from those, the bitrate and the
 * sample rate comes the length of the frame, which is what makes the next frame directly addressable. Only Layer III is a
 * stream the WebCodecs 'mp3' codec decodes, so a file that names another layer is refused as unsupported rather than
 * handed to a decoder that cannot decode it, which routes it to server normalization.
 *
 * Encoder padding and seeking
 * ---------------------------
 * VBR MP3 files carry an extra, silent MPEG frame at the very start (the Xing/Info header) that holds the frame count,
 * the byte count and a 100 entry table of percentage positions, or a VBRI header at a fixed offset instead. The Xing
 * frame is not audio: the first real frame follows it, so this demuxer reads it for the duration and the seek table and
 * skips it. Without a Xing or VBRI table the stream is assumed to be CBR and the duration is estimated from the file size
 * and the bitrate of the first frame. Seeking without a table maps a time to a byte position through that same estimate,
 * which is exact for CBR and approximate for a VBR file that lost its table; either way the demuxer then scans forward to
 * the next real frame, so the byte it lands on is always a frame boundary.
 *
 * The codec private data
 * ----------------------
 * The WebCodecs 'mp3' codec takes no description: each packet is a whole MPEG frame, header included, and the decoder
 * reads the frame header itself. The configuration therefore carries only the codec string, the sample rate and the
 * channel count.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The number of header bytes every MPEG frame starts with.
     * @readonly
     * @type {number}
     */
    const MPEG_HEADER_BYTES = 4;

    /**
     * The bitrates, in kbps, of the MPEG-1 and MPEG-2/MPEG-2.5 layers, indexed by the four bit bitrate index of the frame
     * header. Index 0 is the free format, which this demuxer refuses, and index 15 is invalid and is never looked up.
     * @readonly
     * @type {Object<string, Array<Array<number>>>}
     */
    const BITRATES = Object.freeze({
        // MPEG-1, indexed by layer (1 = Layer III, 2 = Layer II, 3 = Layer I).
        mpeg1: Object.freeze({
            1: Object.freeze([0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320]),
            2: Object.freeze([0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384]),
            3: Object.freeze([0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448])
        }),
        // MPEG-2 and MPEG-2.5.
        mpeg2: Object.freeze({
            1: Object.freeze([0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160]),
            2: Object.freeze([0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160]),
            3: Object.freeze([0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256])
        })
    });

    /**
     * The sample rates, in Hz, indexed by the two bit sample rate index of the frame header, per MPEG version.
     * @readonly
     * @type {Object<string, Array<number>>}
     */
    const SAMPLE_RATES = Object.freeze({
        mpeg1: Object.freeze([44100, 48000, 32000]),
        mpeg2: Object.freeze([22050, 24000, 16000]),
        mpeg25: Object.freeze([11025, 12000, 8000])
    });

    /**
     * The eleven bit frame sync of an MPEG audio frame, as the two byte prefix a scan matches on. The mask covers the
     * three sync bits of the second byte, so a scan rejects a byte pattern that is not a sync by a comparison instead of
     * by an awaited header parse, which matters because a scan's prefix is what decides how many candidates it validates.
     * @readonly
     * @type {Array<number>}
     */
    const MP3_SYNC_PREFIX = Object.freeze([0xFF, 0xE0]);

    /**
     * The byte mask of {@link MP3_SYNC_PREFIX}, matching the eleven bit sync exactly.
     * @readonly
     * @type {Array<number>}
     */
    const MP3_SYNC_MASK = Object.freeze([0xFF, 0xE0]);

    /**
     * Parses the four byte header of an MPEG audio frame.
     * @param {Uint8Array} bytes - The bytes to read the header from.
     * @param {number} offset - The offset of the first header byte.
     * @returns {Object|null} The parsed header, or null when the bytes are not a valid frame header this demuxer supports.
     */
    function parseMpegHeader(bytes, offset) {
        if (!bytes || offset < 0 || offset + MPEG_HEADER_BYTES > bytes.length)
            return null;
        const first = bytes[offset];
        const second = bytes[offset + 1];
        const third = bytes[offset + 2];
        const fourth = bytes[offset + 3];
        // The header is a bit packed big endian structure: eleven sync bits (all ones), two version bits, two layer bits,
        // a protection bit that is zero when a CRC follows, the four bit bitrate index and the two bit sample rate index
        // in the third byte, a padding bit and a private bit, the two bit channel mode in the top of the fourth byte, and
        // then the mode extension, the copyright and original bits and the emphasis. The fields this demuxer needs are
        // extracted below, which is why the shifts and masks split the second, third and fourth bytes.
        // The eleven bit frame sync is all ones; the two following bits are the version and the next two the layer.
        if (first !== 0xFF || (second & 0xE0) !== 0xE0)
            return null;
        const versionBits = (second >> 3) & 0x03;
        const layerBits = (second >> 1) & 0x03;
        const bitrateIndex = (third >> 4) & 0x0F;
        const sampleRateIndex = (third >> 2) & 0x03;
        if (versionBits === 1 || layerBits === 0 || bitrateIndex === 0 || bitrateIndex === 15 || sampleRateIndex === 3)
            return null;
        const isMpeg1 = versionBits === 3;
        const versionKey = isMpeg1 ? 'mpeg1' : 'mpeg2';
        const sampleRateKey = isMpeg1 ? 'mpeg1' : versionBits === 2 ? 'mpeg2' : 'mpeg25';
        const bitrate = BITRATES[versionKey][layerBits][bitrateIndex];
        const sampleRate = SAMPLE_RATES[sampleRateKey][sampleRateIndex];
        if (!bitrate || !sampleRate)
            return null;
        // Layer III decodes to 1152 samples on MPEG-1 and 576 on MPEG-2 and MPEG-2.5; Layer II always to 1152 and Layer I to 384.
        let samplesPerFrame;
        if (layerBits === 1)
            samplesPerFrame = isMpeg1 ? 1152 : 576;
        else if (layerBits === 2)
            samplesPerFrame = 1152;
        else
            samplesPerFrame = 384;
        const padding = (third >> 1) & 0x01;
        // Layer I pads in four byte slots; the other layers pad in single byte slots.
        const frameLength = layerBits === 3
            ? Math.floor((12 * bitrate * 1000) / sampleRate) * 4 + padding * 4
            : Math.floor(((samplesPerFrame / 8) * bitrate * 1000) / sampleRate) + padding;
        if (!Number.isFinite(frameLength) || frameLength < MPEG_HEADER_BYTES)
            return null;
        const channelMode = (fourth >> 6) & 0x03;
        const channels = channelMode === 3 ? 1 : 2;
        return Object.freeze({
            version: versionBits,
            layer: layerBits,
            isMpeg1,
            samplesPerFrame,
            sampleRate,
            bitrate,
            channels,
            frameLength,
            crcProtected: (second & 0x01) === 0
        });
    }

    /**
     * Demuxes an MP3 (MPEG Layer III) stream.
     */
    class Mp3Demuxer extends LuminaAudioPlayer.Demuxer {
        /**
         * Creates an instance of Mp3Demuxer.
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
            /** @type {Uint8Array|null} */
            this.xingToc = null;
        }

        //+=======================================================================================+
        //|                                     Initialization                                    |
        //+=======================================================================================+

        /**
         * Reads the ID3v2 tag, the first MPEG frame with its Xing or VBRI metadata, and the duration they state, and
         * positions the read cursor on the first real audio frame.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not an MP3 stream.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                const dataStart = await this.skipId3v2Async(0);
                const end = this.reader.getLength();
                const firstFrame = await LuminaAudioPlayer.scanForFrameAsync(this.reader, dataStart, end, MP3_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: MP3_SYNC_MASK });
                if (firstFrame < 0)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The bytes do not contain an MPEG audio frame.', { reason: 'no-mpeg-frame' });
                const header = await this.parseHeaderAtAsync(firstFrame);
                if (!header)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The MPEG frame header could not be parsed.', { reason: 'no-mpeg-frame' });
                if (header.layer !== 1)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The MPEG stream uses a layer the player does not decode.', { reason: 'unsupported-layer' });
                const metadata = await this.readVariableRateMetadataAsync(firstFrame, header);
                this.dataStart = metadata ? firstFrame + header.frameLength : firstFrame;
                this.offset = this.dataStart;
                this.header = header;
                this.durationSeconds = this.resolveDuration(header, metadata, end);
                this.xingToc = metadata && metadata.toc ? metadata.toc : null;
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
                codec: 'mp3',
                sampleRate: this.header.sampleRate,
                numberOfChannels: this.header.channels
            };
        }

        /**
         * Gets the duration of the stream, from the Xing or VBRI frame count when there is one and from the file size
         * otherwise.
         * @returns {number|null} The duration in seconds, or null when it cannot be derived.
         */
        getDuration() {
            return this.durationSeconds;
        }

        //+=======================================================================================+
        //|                                       Reading                                         |
        //+=======================================================================================+

        /**
         * Reads the next MPEG frame as an encoded packet. A byte that is not the start of a valid frame is skipped by
         * rescanning for the next frame sync, so a corrupted or partially downloaded region does not stop the stream.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            if (this.reachedEnd)
                return null;
            try {
                const header = await this.parseHeaderAtAsync(this.offset);
                if (!header) {
                    return await this.resyncAsync();
                }
                const data = await this.reader.readAsync(this.offset, header.frameLength);
                if (data.length < header.frameLength) {
                    // The last frame is incomplete, which is the end of a partially downloaded or truncated stream.
                    this.reachedEnd = true;
                    return null;
                }
                // The duration is kept unrounded while the timeline accumulates it, so that rounding it per packet does not
                // make the timeline drift; only the packet the decoder receives carries a whole number of microseconds.
                const duration = (header.samplesPerFrame / header.sampleRate) * 1000000;
                const packet = LuminaAudioPlayer.createDemuxerPacket(data.slice(), this.timelineMicros, duration);
                this.timelineMicros += duration;
                this.offset += header.frameLength;
                return packet;
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Rescans for the next valid frame after a byte that is not the start of one.
         * @returns {Promise<Object|null>} The next packet, or null when no further frame is found.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async resyncAsync() {
            const end = this.reader.getLength();
            const next = await LuminaAudioPlayer.scanForFrameAsync(this.reader, this.offset + 1, end, MP3_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: MP3_SYNC_MASK });
            if (next < 0) {
                this.reachedEnd = true;
                return null;
            }
            this.offset = next;
            return await this.readPacketAsync();
        }

        /**
         * Repositions the read cursor to the frame that covers a time, using the Xing table when the stream has one and
         * the constant bitrate estimate otherwise, and then scanning to the next real frame.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the stream cannot be seeked.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan for a frame gives up on a corrupt or crafted region.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const end = this.reader.getLength();
            if (end === null || !Number.isFinite(seconds) || this.durationSeconds === null || this.durationSeconds <= 0)
                return null;
            const fraction = LuminaAudioPlayer.clamp(seconds / this.durationSeconds, 0, 1);
            let estimate;
            if (this.xingToc) {
                const index = Math.min(99, Math.floor(fraction * 100));
                const byteFraction = this.xingToc[index] / 256;
                estimate = this.dataStart + Math.floor(byteFraction * (end - this.dataStart));
            } else {
                const bitrate = this.header ? this.header.bitrate * 1000 : 128000;
                estimate = this.dataStart + Math.floor((seconds * bitrate) / 8);
            }
            const start = Math.max(this.dataStart, Math.min(end - 1, estimate));
            const frame = await LuminaAudioPlayer.scanForFrameAsync(this.reader, start, end, MP3_SYNC_PREFIX, (offset) => this.isValidFrameAtAsync(offset), { prefixMask: MP3_SYNC_MASK });
            if (frame < 0)
                return null;
            this.offset = frame;
            this.reachedEnd = false;
            // The exact sample position of the frame is not known without walking the stream from the start, so the
            // timeline is seeded with the requested time, kept unrounded so the timeline it starts does not drift, and the
            // seek stage uses it as the target to trim the first packet to.
            this.timelineMicros = Math.max(0, seconds) * 1000000;
            return frame;
        }

        //+=======================================================================================+
        //|                                   Internal helpers                                    |
        //+=======================================================================================+

        /**
         * Reads the header of a frame at an offset.
         * @param {number} offset - The offset of the frame.
         * @returns {Promise<Object|null>} The parsed header, or null when the bytes are not a valid frame header.
         */
        async parseHeaderAtAsync(offset) {
            const bytes = await this.reader.readAsync(offset, MPEG_HEADER_BYTES);
            if (bytes.length < MPEG_HEADER_BYTES)
                return null;
            return parseMpegHeader(bytes, 0);
        }

        /**
         * Checks whether a candidate offset is the start of a real frame, by parsing its header and, unless it is the
         * last frame of a known length stream, by requiring the frame that follows it to parse as well with the same
         * version, layer and sample rate. That second frame is what rejects a byte pattern inside frame data.
         * @param {number} offset - The candidate offset.
         * @returns {Promise<boolean>} True when the offset is the start of a real frame.
         */
        async isValidFrameAtAsync(offset) {
            const header = await this.parseHeaderAtAsync(offset);
            if (!header || header.layer !== 1)
                return false;
            const end = this.reader.getLength();
            const next = offset + header.frameLength;
            if (end !== null && next + MPEG_HEADER_BYTES > end)
                return true;
            const nextHeader = await this.parseHeaderAtAsync(next);
            if (!nextHeader)
                return false;
            return nextHeader.version === header.version
                && nextHeader.layer === header.layer
                && nextHeader.sampleRate === header.sampleRate;
        }

        /**
         * Reads the Xing, Info or VBRI metadata of the first frame, when that frame is a metadata frame rather than audio.
         * @param {number} frameStart - The offset of the first frame.
         * @param {Object} header - The parsed header of the first frame.
         * @returns {Promise<Object|null>} The metadata, with the frame count and the seek table, or null when the first frame is real audio.
         */
        async readVariableRateMetadataAsync(frameStart, header) {
            const crcBytes = header.crcProtected ? 2 : 0;
            const sideInfoBytes = header.isMpeg1 ? (header.channels === 1 ? 17 : 32) : (header.channels === 1 ? 9 : 17);
            // The Xing or Info tag sits right after the header and the side info; the VBRI tag sits at a fixed offset from
            // the frame start. Both are read from one window so neither field is fetched twice.
            const window = await this.reader.readAsync(frameStart, header.frameLength);
            if (window.length < MPEG_HEADER_BYTES)
                return null;
            const xingOffset = MPEG_HEADER_BYTES + crcBytes + sideInfoBytes;
            const isXing = LuminaAudioPlayer.demuxBytesEqualAscii(window, xingOffset, 'Xing')
                || LuminaAudioPlayer.demuxBytesEqualAscii(window, xingOffset, 'Info');
            let cursor = -1;
            if (isXing)
                cursor = xingOffset + 4;
            else if (LuminaAudioPlayer.demuxBytesEqualAscii(window, MPEG_HEADER_BYTES + 32, 'VBRI'))
                cursor = MPEG_HEADER_BYTES + 32 + 4 + 2 + 2 + 2;
            if (cursor < 0)
                return null;
            const isVbri = !isXing;
            let frameCount = null;
            let toc = null;
            if (isVbri)  // VBRI: bytes (4), frames (4), then a two entry-per-percent table this demuxer does not use.               
                frameCount = LuminaAudioPlayer.demuxReadUintBE(window, cursor + 4, 4);
            else {
                const flags = LuminaAudioPlayer.demuxReadUintBE(window, cursor, 4) || 0;
                cursor += 4;
                if ((flags & 0x01) !== 0) {
                    frameCount = LuminaAudioPlayer.demuxReadUintBE(window, cursor, 4);
                    cursor += 4;
                }
                if ((flags & 0x02) !== 0) // The byte count the tag may carry is not needed, but its four bytes still shift the seek table.                   
                    cursor += 4;
                if ((flags & 0x04) !== 0) {
                    const raw = await this.reader.copyAsync(frameStart + cursor, 100);
                    if (raw.length === 100)
                        toc = raw;
                }
            }
            return { frameCount, toc };
        }

        /**
         * Resolves the duration of the stream from the metadata of its first frame, or from the file size and the first frame's bitrate when the stream carries no metadata.
         * @param {Object} header - The parsed header of the first real frame.
         * @param {Object|null} metadata - The Xing or VBRI metadata, or null.
         * @param {number|null} end - The length of the stream, or null when it is unknown.
         * @returns {number|null} The duration in seconds, or null when it cannot be derived.
         */
        resolveDuration(header, metadata, end) {
            if (metadata && Number.isFinite(metadata.frameCount) && metadata.frameCount > 0)
                return (metadata.frameCount * header.samplesPerFrame) / header.sampleRate;
            if (end === null)
                return null;
            const bytes = end - this.dataStart;
            const bitrate = header.bitrate * 1000;
            if (bitrate <= 0 || bytes <= 0)
                return null;
            return (bytes * 8) / bitrate;
        }
    }

    LuminaAudioPlayer.Mp3Demuxer = Mp3Demuxer;

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.MP3,
        codec: LuminaAudioPlayer.DemuxerCodec.MP3,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.MP3,
        create: (options) => new Mp3Demuxer(options)
    });
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
