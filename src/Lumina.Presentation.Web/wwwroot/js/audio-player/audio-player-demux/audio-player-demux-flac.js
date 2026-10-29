/**
 * FLAC demuxer of the audio player.
 *
 * A FLAC file is the four byte magic 'fLaC', a sequence of metadata blocks whose first is the mandatory STREAMINFO
 * block, and then a stream of audio frames. STREAMINFO states the sample rate, the channel count, the bit depth and the
 * total number of samples of the whole stream, so the duration is known up front, which no other container here gives.
 * Each frame header states its own block size, sample rate, channel assignment and sample size, plus a CRC-8 over the
 * header, and the frame number or sample number it starts at.
 *
 * Finding the exact frame boundary
 * --------------------------------
 * A FLAC frame does not state its own compressed length. This demuxer computes it the way the format defines it, by
 * walking the frame's bitstream: the frame header, then one subframe per channel, then zero padding to the next byte
 * boundary, then the 16 bit frame CRC. Each subframe is parsed to its exact end without decoding it, whatever it holds:
 *   - a constant subframe is one tail of bits-per-sample bits;
 *   - a verbatim subframe is a whole block of bits-per-sample bits;
 *   - a fixed or an LPC subframe is a run of warm-up samples, the LPC parameters when it is LPC, and then a coded
 *     residual;
 *   - a coded residual is one or more Rice partitions, each either Rice coded with a per-partition parameter, or, under
 *     the escape code, a run of raw bits. Walking a partition means walking every coded value of it, because a Rice code
 *     is not a fixed width, so the parser reads the unary prefix and the remainder of each value.
 * The 16 bit frame CRC closes the frame, and it is verified against the bytes that were walked, so a frame that was
 * framed wrongly, or that is corrupt, is reported rather than handed to the decoder. The frame header CRC-8 is verified
 * too, so a byte pattern that merely looks like a sync cannot start a frame.
 *
 * The codec private data
 * ----------------------
 * The WebCodecs FLAC registration requires a description that is the bytes of a FLAC bitstream up to the first frame,
 * at least the 'fLaC' magic followed by the STREAMINFO metadata block. This demuxer builds exactly that: the magic and
 * the STREAMINFO block, header included, without the other metadata blocks of the file, so the description stays small
 * whatever a file carries.
 *
 * Seeking and resynchronization
 * -----------------------------
 * A frame boundary is located from a distance by scanning for a frame header whose CRC-8 and stream parameters match,
 * which is what a seek uses to land on a frame before parsing it exactly, and what a resynchronization uses after a
 * corrupted region. A FLAC stream may carry a SEEKTABLE, but this demuxer does not need one: it estimates the byte
 * position of a time from its proportion of the total samples and then scans to the next frame header, whose coded frame
 * or sample number gives the position back exactly.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The four byte magic every FLAC stream starts with.
     * @readonly
     * @type {string}
     */
    const FLAC_MAGIC = 'fLaC';

    /**
     * The number of bytes of the STREAMINFO metadata block body.
     * @readonly
     * @type {number}
     */
    const STREAMINFO_BODY_BYTES = 34;

    /**
     * The number of header bytes read to parse a frame header. A FLAC frame header is at most seventeen bytes long.
     * @readonly
     * @type {number}
     */
    const FRAME_HEADER_WINDOW_BYTES = 20;

    /**
     * The number of bytes held in the read ahead window of the frame bitstream reader. It is at least a whole FLAC
     * subframe for the common configurations, so the parser issues one read per window rather than one per bit.
     * @readonly
     * @type {number}
     */
    const FRAME_WINDOW_BYTES = 65536;

    /**
     * The byte budget a frame is allowed to consume when neither the maximum frame size of STREAMINFO nor the length of
     * the stream is known. It is above the largest frame the format can produce, so a parser never stops a valid frame,
     * while a corrupt one cannot make it read without bound.
     * @readonly
     * @type {number}
     */
    const DEFAULT_MAX_FRAME_BYTES = 16 * 1024 * 1024;

    /**
     * The sample rates, in Hz, named directly by the sample rate codes of a FLAC frame header.
     * @readonly
     * @type {Object<number, number>}
     */
    const SAMPLE_RATE_CODES = Object.freeze({
        1: 88200,
        2: 176400,
        3: 192000,
        4: 8000,
        5: 16000,
        6: 22050,
        7: 24000,
        8: 32000,
        9: 44100,
        10: 48000,
        11: 96000
    });

    /**
     * The bits-per-sample each sample size code names, with zero meaning 'from STREAMINFO'.
     * @readonly
     * @type {Array<number>}
     */
    const SAMPLE_SIZE_CODES = Object.freeze([0, 8, 12, 0, 16, 20, 24, 32]);

    /**
     * The fourteen bit frame sync of a FLAC frame, as the two byte prefix a scan matches on. The mask covers the top six
     * bits of the second byte, so a scan rejects a byte pattern that is not a sync by a comparison instead of by an
     * awaited header parse.
     * @readonly
     * @type {Array<number>}
     */
    const FLAC_SYNC_PREFIX = Object.freeze([0xFF, 0xF8]);

    /**
     * The byte mask of {@link FLAC_SYNC_PREFIX}, matching the fourteen bit sync exactly.
     * @readonly
     * @type {Array<number>}
     */
    const FLAC_SYNC_MASK = Object.freeze([0xFF, 0xFC]);

    /**
     * The number of metadata blocks the demuxer will walk past STREAMINFO before it refuses the stream. Real files carry a
     * handful, and the walk is bounded by this cap rather than by the length of the stream.
     * @readonly
     * @type {number}
     */
    const MAX_METADATA_BLOCKS = 4096;

    /**
     * The lookup table of the FLAC frame checksum, built once. It turns the checksum from eight bit iterations to one byte
     * lookup per byte of a frame.
     * @readonly
     * @type {Uint32Array}
     */
    const CRC16_TABLE = LuminaAudioPlayer.demuxBuildCrcTable(0x8005, 16);

    /**
     * The lookup table of the FLAC frame header checksum, built once.
     * @readonly
     * @type {Uint32Array}
     */
    const CRC8_TABLE = LuminaAudioPlayer.demuxBuildCrcTable(0x07, 8);

    /**
     * Computes the CRC-16 of a run of bytes, with the polynomial 0x8005 and an initial value of zero, the checksum a
     * FLAC frame ends with.
     * @param {Uint8Array} bytes - The bytes to checksum.
     * @returns {number} The checksum.
     */
    function computeCrc16(bytes) {
        let crc = 0;
        for (let index = 0; index < bytes.length; index++)
            crc = ((crc << 8) ^ CRC16_TABLE[((crc >> 8) ^ bytes[index]) & 0xFF]) & 0xFFFF;
        return crc & 0xFFFF;
    }

    /**
     * Computes the CRC-8 of a run of bytes, with the polynomial 0x07 and an initial value of zero, the checksum a FLAC
     * frame header ends with.
     * @param {Uint8Array} bytes - The bytes to checksum.
     * @returns {number} The checksum.
     */
    function computeCrc8(bytes) {
        let crc = 0;
        for (let index = 0; index < bytes.length; index++)
            crc = CRC8_TABLE[(crc ^ bytes[index]) & 0xFF];
        return crc;
    }

    /**
     * Reads the UTF-8 style coded number a FLAC frame header uses for the frame number or the sample number.
     * @param {LuminaAudioPlayer.DemuxerBitReader} reader - The bit reader positioned on the first byte of the number.
     * @returns {number} The decoded number.
     * @throws {RangeError} When the coded number is malformed.
     */
    function readCodedNumber(reader) {
        const first = reader.readBits(8);
        if ((first & 0x80) === 0)
            return first;
        // The number is coded the way UTF-8 codes a code point: the run of leading one bits in the first byte states how
        // many bytes the number spans, and each following byte starts with a '10' prefix and contributes six more bits.
        let leadingOnes = 0;
        let mask = 0x80;
        while (mask !== 0 && (first & mask) !== 0) {
            leadingOnes++;
            mask >>= 1;
        }
        if (leadingOnes < 2 || leadingOnes > 7)
            throw new RangeError('The FLAC coded number is malformed.');
        let value = first & (0x7F >> leadingOnes);
        for (let index = 1; index < leadingOnes; index++) {
            const continuation = reader.readBits(8);
            if ((continuation & 0xC0) !== 0x80)
                throw new RangeError('The FLAC coded number is malformed.');
            value = value * 64 + (continuation & 0x3F);
        }
        return value;
    }

    /**
     * Resolves the block size a FLAC frame header names.
     * @param {number} code - The four bit block size code.
     * @param {LuminaAudioPlayer.DemuxerBitReader} reader - The bit reader, positioned after the coded number, from which the variable codes read their extra bytes.
     * @returns {number|null} The block size in samples, or null when the code is invalid.
     */
    function resolveBlockSize(code, reader) {
        // Codes 2 to 5 are powers of two starting at 576 samples, codes 6 and 7 store the size minus one in the one or
        // two bytes that follow, and codes 8 to 15 are powers of two starting at 256.
        if (code === 0)
            return null;
        if (code === 1)
            return 192;
        if (code >= 2 && code <= 5)
            return 576 * Math.pow(2, code - 2);
        if (code === 6)
            return reader.readBits(8) + 1;
        if (code === 7)
            return reader.readBits(16) + 1;
        return 256 * Math.pow(2, code - 8);
    }

    /**
     * Resolves the sample rate a FLAC frame header names. It distinguishes the code that means "take it from STREAMINFO"
     * from the reserved code, which a frame may never use: collapsing the two into null would accept a frame header that
     * names a reserved sample rate code, leaving only the header checksum to reject a false sync.
     * @param {number} code - The four bit sample rate code.
     * @param {LuminaAudioPlayer.DemuxerBitReader} reader - The bit reader, positioned after the block size, from which the variable codes read their extra bytes.
     * @returns {{ rate: number|null, isValid: boolean }} The sample rate in Hz, or null to mean 'from STREAMINFO', together with whether the code is one the format defines.
     */
    function resolveFrameSampleRate(code, reader) {
        if (code === 0)
            return { rate: null, isValid: true };
        if (Object.prototype.hasOwnProperty.call(SAMPLE_RATE_CODES, code))
            return { rate: SAMPLE_RATE_CODES[code], isValid: true };
        if (code === 12)
            return { rate: reader.readBits(8) * 1000, isValid: true };
        if (code === 13)
            return { rate: reader.readBits(16), isValid: true };
        if (code === 14)
            return { rate: reader.readBits(16) * 10, isValid: true };
        return { rate: null, isValid: false };
    }

    /**
     * Parses the header of a FLAC frame.
     * @param {Uint8Array} bytes - The bytes holding the header.
     * @param {number} offset - The offset of the frame's first byte.
     * @param {Object} streamInfo - The STREAMINFO fields of the stream.
     * @returns {Object|null} The parsed header, or null when the bytes are not a valid frame header.
     */
    function parseFrameHeader(bytes, offset, streamInfo) {
        if (!bytes || offset < 0 || offset + 4 > bytes.length)
            return null;
        try {
            const reader = new LuminaAudioPlayer.DemuxerBitReader(bytes.subarray(offset));
            // A frame header is bit packed: a fourteen bit sync (0x3FFE), a reserved bit that must be zero, a blocking
            // strategy bit, a four bit block size code, a four bit sample rate code, a four bit channel assignment, a
            // three bit sample size code, a second reserved bit, the coded frame or sample number, and finally the eight
            // bit header checksum. The fields are validated against STREAMINFO so a byte pattern inside frame data is
            // rejected as a header.
            if (reader.readBits(14) !== 0x3FFE)
                return null;
            if (reader.readBits(1) !== 0)
                return null;
            const blockingStrategy = reader.readBits(1);
            const blockSizeCode = reader.readBits(4);
            const sampleRateCode = reader.readBits(4);
            const channelAssignment = reader.readBits(4);
            const sampleSizeCode = reader.readBits(3);
            if (reader.readBits(1) !== 0)
                return null;
            const codedNumber = readCodedNumber(reader);
            const blockSize = resolveBlockSize(blockSizeCode, reader);
            if (blockSize === null)
                return null;
            const frameSampleRate = resolveFrameSampleRate(sampleRateCode, reader);
            if (!frameSampleRate.isValid)
                return null;
            const sampleRate = frameSampleRate.rate === null ? streamInfo.sampleRate : frameSampleRate.rate;
            if (sampleRate !== streamInfo.sampleRate)
                return null;
            const channels = channelAssignment <= 7 ? channelAssignment + 1 : channelAssignment <= 10 ? 2 : null;
            if (channels === null || channels !== streamInfo.channels)
                return null;
            // Assignments 0 to 7 mean that many independent channels minus one, and 8, 9 and 10 are the three stereo
            // decorrelation modes (left/side, right/side and mid/side), which still carry two channels.
            if (sampleSizeCode === 3)
                return null;
            const bitsPerSample = sampleSizeCode === 0 ? streamInfo.bitsPerSample : SAMPLE_SIZE_CODES[sampleSizeCode];
            if (bitsPerSample !== streamInfo.bitsPerSample)
                return null;
            const crcPosition = reader.getBytePosition();
            const expectedCrc = reader.readBits(8);
            const computedCrc = computeCrc8(bytes.subarray(offset, offset + crcPosition));
            if (expectedCrc !== computedCrc)
                return null;
            return Object.freeze({
                blockSize,
                sampleRate,
                channels,
                channelAssignment,
                bitsPerSample,
                blockingStrategy,
                codedNumber,
                headerBytes: reader.getBytePosition()
            });
        } catch (error) {
            // A header that runs past the bytes read so far, or a malformed coded number, is not a frame header.
            return null;
        }
    }

    /**
     * A bit reader over the byte source, used to walk a whole FLAC frame without holding it in memory. It reads through a
     * window that it reloads as the walk crosses it, and it enforces a bit budget, so a corrupt bitstream cannot make it
     * read without bound.
     *
     * The reader has an awaited surface (readBitsAsync, readUnaryAsync, skipBitsAsync) and a synchronous one
     * (tryReadBits, walkRiceSamples) that runs over the resident window without a promise. The synchronous surface is the
     * hot path of a lossless frame: a coded residual holds thousands of values, and awaiting one promise per value, each
     * resolving from bytes already in the window, is pure overhead on a realtime path. A caller therefore reloads the
     * window with refillAsync and then walks as many values as fit, which costs one reload per window instead of one
     * promise per value; the awaited surface remains for the fields that span a window, which are rare and tiny.
     */
    class FlacBitstreamReader {
        /**
         * Creates an instance of FlacBitstreamReader.
         * @param {LuminaAudioPlayer.DemuxerReader} reader - The windowing reader over the track.
         * @param {Object} options - The options.
         * @param {number} options.startOffset - The absolute byte offset the bitstream starts at.
         * @param {number} options.maxBitOffset - The number of bits the walk may consume before the frame is refused.
         */
        constructor(reader, options) {
            this.reader = reader;
            this.base = options.startOffset;
            this.maxBitOffset = options.maxBitOffset;
            this.bitOffset = 0;
            /** @type {Uint8Array} */
            this.windowBytes = new Uint8Array(0);
            this.windowStart = 0;
            this.windowLength = 0;
        }

        /**
         * Gets the absolute byte offset the walk reached. It is meaningful while the walk is byte aligned.
         * @returns {number} The absolute byte offset.
         */
        getBytePosition() {
            return this.base + (this.bitOffset >> 3);
        }

        /**
         * Gets the number of bits the walk consumed.
         * @returns {number} The bit offset relative to the start of the bitstream.
         */
        getBitOffset() {
            return this.bitOffset;
        }

        /**
         * Makes sure a run of bytes is available in the window, reloading it when it is not. It throws when the run
         * reaches past the end of the stream, which means the frame is truncated, and when the walk exceeded its budget.
         * @param {number} relativeStart - The first byte of the run, relative to the start offset.
         * @param {number} byteCount - The number of bytes of the run.
         * @returns {Promise<void>} A promise resolving when the run is available.
         * @throws {RangeError} With an <c>isEof</c> or an <c>isBound</c> flag when the walk cannot continue.
         */
        async ensureRangeAsync(relativeStart, byteCount) {
            const absoluteStart = this.base + relativeStart;
            const absoluteEnd = absoluteStart + byteCount - 1;
            if (absoluteStart >= this.windowStart && absoluteEnd < this.windowStart + this.windowLength)
                return;
            const bytes = await this.reader.readAsync(absoluteStart, Math.max(FRAME_WINDOW_BYTES, byteCount));
            this.windowStart = absoluteStart;
            this.windowBytes = bytes;
            this.windowLength = bytes ? bytes.length : 0;
            if (this.windowLength === 0 || absoluteEnd >= this.windowStart + this.windowLength) {
                const error = new RangeError('The FLAC frame bitstream ended before the frame did.');
                error.isEof = true;
                throw error;
            }
        }

        /**
         * Refuses to read past the frame's bit budget.
         * @param {number} extraBits - The number of bits about to be consumed.
         * @throws {RangeError} With an <c>isBound</c> flag when the budget is exceeded.
         */
        checkBudget(extraBits) {
            if (this.maxBitOffset !== null && this.bitOffset + extraBits > this.maxBitOffset) {
                const error = new RangeError('The FLAC frame exceeded the size it may occupy.');
                error.isBound = true;
                throw error;
            }
        }

        /**
         * Reads an unsigned integer of up to 32 bits.
         * @param {number} count - The number of bits to read.
         * @returns {Promise<number>} The value.
         * @throws {RangeError} When the walk cannot continue.
         */
        async readBitsAsync(count) {
            if (count <= 0)
                return 0;
            this.checkBudget(count);
            const firstByte = this.bitOffset >> 3;
            const lastByte = (this.bitOffset + count - 1) >> 3;
            await this.ensureRangeAsync(firstByte, lastByte - firstByte + 1);
            let value = 0;
            for (let index = 0; index < count; index++) {
                const local = this.base + (this.bitOffset >> 3) - this.windowStart;
                const bit = 7 - (this.bitOffset & 7);
                value = value * 2 + ((this.windowBytes[local] >> bit) & 1);
                this.bitOffset++;
            }
            return value;
        }

        /**
         * Reads a unary coded value: the number of zero bits before the terminating one.
         * @returns {Promise<number>} The number of zero bits read.
         * @throws {RangeError} When the walk cannot continue.
         */
        async readUnaryAsync() {
            let zeros = 0;
            for (;;) {
                this.checkBudget(1);
                const byteIndex = this.bitOffset >> 3;
                await this.ensureRangeAsync(byteIndex, 1);
                const local = this.base + byteIndex - this.windowStart;
                const startBit = 7 - (this.bitOffset & 7);
                const byte = this.windowBytes[local];
                for (let bit = startBit; bit >= 0; bit--) {
                    if (((byte >> bit) & 1) !== 0) {
                        zeros += startBit - bit;
                        this.bitOffset += startBit - bit + 1;
                        return zeros;
                    }
                }
                // The rest of the byte is zeros, so it is consumed and the scan continues on the next byte.
                zeros += startBit + 1;
                this.bitOffset += startBit + 1;
            }
        }

        /**
         * Skips a number of bits, verifying that the last skipped byte exists so that a truncated frame is detected.
         * @param {number} count - The number of bits to skip.
         * @returns {Promise<void>} A promise resolving when the bits were skipped.
         * @throws {RangeError} When the walk cannot continue.
         */
        async skipBitsAsync(count) {
            if (count <= 0)
                return;
            this.checkBudget(count);
            const newOffset = this.bitOffset + count;
            await this.ensureRangeAsync((newOffset - 1) >> 3, 1);
            this.bitOffset = newOffset;
        }

        //+=======================================================================================+
        //|                              Synchronous fast path                                    |
        //+=======================================================================================+

        /**
         * Checks whether the byte the walk is on is already resident in the window, so that a caller can tell whether a
         * read would need to await a reload before it awaits one.
         * @returns {boolean} True when the current byte is in the window.
         */
        isResident() {
            const absolute = this.base + (this.bitOffset >> 3);
            return absolute >= this.windowStart && absolute < this.windowStart + this.windowLength;
        }

        /**
         * Loads the window so that it starts at the current byte, when it does not already cover it, or always when it is
         * forced. It is the single awaited step of the residual fast path: the walk below it runs synchronously until the
         * window runs out, so a frame costs one reload per window rather than one promise per coded value.
         * @param {boolean} [force] - True to reload even when the current byte is already resident.
         * @returns {Promise<void>} A promise resolving when the window covers the current byte.
         * @throws {RangeError} When the bitstream ended.
         */
        async refillAsync(force) {
            const absolute = this.base + (this.bitOffset >> 3);
            if (!force && absolute >= this.windowStart && absolute < this.windowStart + this.windowLength)
                return;
            const bytes = await this.reader.readAsync(absolute, FRAME_WINDOW_BYTES);
            this.windowStart = absolute;
            this.windowBytes = bytes;
            this.windowLength = bytes ? bytes.length : 0;
            if (this.windowLength === 0) {
                const error = new RangeError('The FLAC frame bitstream ended before the frame did.');
                error.isEof = true;
                throw error;
            }
            this.checkBudget(0);
        }

        /**
         * Reads an unsigned integer of up to 32 bits without awaiting, when the bits are already resident. It never
         * advances the walk when it returns null, so a caller can reload the window and try again.
         * @param {number} count - The number of bits to read.
         * @returns {number|null} The value, or null when the bits are not all resident yet.
         */
        tryReadBits(count) {
            if (count <= 0)
                return 0;
            if (this.base + ((this.bitOffset + count - 1) >> 3) >= this.windowStart + this.windowLength)
                return null;
            let value = 0;
            for (let index = 0; index < count; index++) {
                const local = this.base + (this.bitOffset >> 3) - this.windowStart;
                const bit = 7 - (this.bitOffset & 7);
                value = value * 2 + ((this.windowBytes[local] >> bit) & 1);
                this.bitOffset++;
            }
            // The synchronous path advances the walk itself, so it enforces the budget itself, exactly, rather than
            // leaving the overshoot to the next awaited read.
            this.checkBudget(0);
            return value;
        }

        /**
         * Walks a run of Rice coded residual values synchronously over the resident window. This is the hot path of a
         * lossless frame: the unary prefix is scanned byte by byte and the remainder is stepped over in one move, with no
         * promise per value. It returns the number of values it completed, and leaves a value whose bits reach past the
         * window untouched, so the walk resumes exactly on that value's first bit once the window is reloaded.
         * @param {number} parameter - The Rice parameter of the partition.
         * @param {number} maxSamples - The number of values still to walk in the partition.
         * @returns {number} The number of values completed, between zero and maxSamples.
         */
        walkRiceSamples(parameter, maxSamples) {
            this.checkBudget(0);
            const windowEndBit = (this.windowStart + this.windowLength - this.base) * 8;
            let completed = 0;
            // One iteration per residual value of the partition, until the partition is walked or a value turns out not to fit the resident window.
            while (completed < maxSamples) {
                const valueStart = this.bitOffset;
                let found = false;
                // Walk the unary prefix of this value: every zero bit is skipped and the first one bit ends the prefix.
                // The window bytes are scanned directly, so the prefix costs no promise per bit.
                while (this.bitOffset < windowEndBit) {
                    const local = this.base + (this.bitOffset >> 3) - this.windowStart;
                    const startBit = 7 - (this.bitOffset & 7);
                    const byte = this.windowBytes[local];
                    let hit = false;
                    // Within this byte, the bits are examined from the current bit down to bit 0, looking for the one bit. that ends the unary prefix.
                    for (let bit = startBit; bit >= 0; bit--) {
                        if (((byte >> bit) & 1) !== 0) {
                            this.bitOffset += startBit - bit + 1;
                            hit = true;
                            break;
                        }
                    }
                    if (hit) {
                        found = true;
                        break;
                    }
                    this.bitOffset += startBit + 1;
                    // The synchronous walk enforces the budget after every advance, so a corrupt value cannot overshoot it.
                    this.checkBudget(0);
                }
                if (!found || (parameter > 0 && this.bitOffset + parameter > windowEndBit)) {
                    // The value does not fit the window; it is left untouched for a reload or the spanning reader.
                    this.bitOffset = valueStart;
                    break;
                }
                this.bitOffset += parameter;
                completed++;
                this.checkBudget(0);
            }
            return completed;
        }
    }

    /**
     * Demuxes a FLAC stream.
     */
    class FlacDemuxer extends LuminaAudioPlayer.Demuxer {
        /**
         * Creates an instance of FlacDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            this.offset = 0;
            this.dataStart = 0;
            this.timelineMicros = 0;
            /** @type {Object|null} */
            this.streamInfo = null;
            /** @type {Uint8Array|null} */
            this.description = null;
            this.totalSamples = 0;
            this.maxFrameSize = 0;
            this.durationSeconds = null;
            this.reachedEnd = false;
        }

        //+=======================================================================================+
        //|                                     Initialization                                    |
        //+=======================================================================================+

        /**
         * Reads the STREAMINFO block and the metadata blocks that follow it, builds the codec private data, and positions the read cursor on the first audio frame.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not a FLAC stream.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                const magic = await this.reader.readAsync(0, 4);
                if (!LuminaAudioPlayer.demuxBytesEqualAscii(magic, 0, FLAC_MAGIC))
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The bytes are not a native FLAC stream.', { reason: 'no-flac-magic' });
                const streamInfo = await this.readStreamInfoAsync();
                if (!streamInfo)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The FLAC stream carries no STREAMINFO block.', { reason: 'no-streaminfo' });
                this.streamInfo = streamInfo;
                this.dataStart = streamInfo.dataStart;
                this.description = streamInfo.description;
                this.totalSamples = streamInfo.totalSamples;
                this.maxFrameSize = streamInfo.maxFrameSize;
                this.durationSeconds = streamInfo.sampleRate > 0 && streamInfo.totalSamples > 0
                    ? streamInfo.totalSamples / streamInfo.sampleRate
                    : null;
                this.offset = this.dataStart;
                this.timelineMicros = 0;
                this.reachedEnd = false;
                this.initialized = true;
            } catch (error) {
                throw this.recordFailure(error, 'initialize-failed');
            }
        }

        /**
         * Gets the decoder configuration of the stream, including the required FLAC description.
         * @returns {Object|null} The configuration, or null before initialization.
         */
        getConfiguration() {
            if (!this.streamInfo)
                return null;
            return {
                codec: 'flac',
                sampleRate: this.streamInfo.sampleRate,
                numberOfChannels: this.streamInfo.channels,
                description: this.description
            };
        }

        /**
         * Gets the duration of the stream, from the total sample count STREAMINFO states.
         * @returns {number|null} The duration in seconds, or null when it is not stated.
         */
        getDuration() {
            return this.durationSeconds;
        }

        //+=======================================================================================+
        //|                                       Reading                                         |
        //+=======================================================================================+

        /**
         * Reads the next FLAC frame as an encoded packet, by walking its bitstream to its exact end.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            if (this.reachedEnd)
                return null;
            try {
                let header = await this.parseHeaderAtAsync(this.offset);
                if (!header) {
                    const resynced = await this.resyncAsync();
                    if (resynced < 0) {
                        this.reachedEnd = true;
                        return null;
                    }
                    header = await this.parseHeaderAtAsync(this.offset);
                    if (!header) {
                        this.reachedEnd = true;
                        return null;
                    }
                }
                const frameEnd = await this.measureFrameEndAsync(this.offset, header);
                const length = frameEnd - this.offset;
                const data = await this.reader.copyAsync(this.offset, length);
                if (data.length < length) {
                    // The bytes of the frame are not all there, which is the end of a truncated stream.
                    this.reachedEnd = true;
                    return null;
                }
                this.verifyFrameCrc16(data);
                // The duration is kept unrounded while the timeline accumulates it, so that rounding it per packet does not
                // make the timeline drift; only the packet the decoder receives carries a whole number of microseconds.
                const duration = (header.blockSize / header.sampleRate) * 1000000;
                const packet = LuminaAudioPlayer.createDemuxerPacket(data, this.timelineMicros, duration);
                this.timelineMicros += duration;
                this.offset = frameEnd;
                return packet;
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Repositions the read cursor to the frame that covers a time, using the sample proportion as a byte estimate and
         * then scanning to the next frame header, whose coded number states the exact sample it starts at.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the stream cannot be seeked.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan for a frame gives up on a corrupt or crafted region.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const end = this.reader.getLength();
            const streamInfo = this.streamInfo;
            if (end === null || !streamInfo || !Number.isFinite(seconds) || seconds < 0 || streamInfo.sampleRate <= 0)
                return null;
            const targetSample = Math.floor(seconds * streamInfo.sampleRate);
            let estimate;
            if (this.totalSamples > 0) {
                const fraction = LuminaAudioPlayer.clamp(targetSample / this.totalSamples, 0, 1);
                estimate = this.dataStart + Math.floor(fraction * (end - this.dataStart));
            } else 
                estimate = this.dataStart + Math.floor((seconds / (this.durationSeconds || 1)) * (end - this.dataStart));
            const start = Math.max(this.dataStart, Math.min(end - 1, estimate));
            const frame = await this.findNextFrameAsync(start, end);
            if (frame < 0)
                return null;
            const header = await this.parseHeaderAtAsync(frame);
            this.offset = frame;
            this.reachedEnd = false;
            // The coded number is the frame number on a fixed blocking stream and the sample number on a variable one, so
            // the sample the frame starts at is exact in both cases once the block size of the frame is known.
            if (header) {
                const sample = header.blockingStrategy === 1 ? header.codedNumber : header.codedNumber * header.blockSize;
                // The exact time of the sample the frame starts at, kept unrounded so the timeline it seeds does not drift.
                this.timelineMicros = Math.max(0, (sample / streamInfo.sampleRate) * 1000000);
            } else 
                this.timelineMicros = Math.max(0, seconds) * 1000000;
            return frame;
        }

        //+=======================================================================================+
        //|                                   Frame measurement                                   |
        //+=======================================================================================+

        /**
         * Walks a frame from its header to its footer and returns the offset just past it. The walk reads the subframes
         * of every channel and the coded residuals they hold, then steps over the zero padding to the byte boundary and
         * the 16 bit frame CRC.
         * @param {number} frameStart - The absolute offset of the frame's first byte.
         * @param {Object} header - The parsed header of the frame.
         * @returns {Promise<number>} The absolute offset just past the frame.
         * @throws {RangeError} When the frame is malformed, truncated, or longer than its budget.
         */
        async measureFrameEndAsync(frameStart, header) {
            const base = frameStart + header.headerBytes;
            const end = this.reader.getLength();
            const budget = this.maxFrameSize > 0
                ? this.maxFrameSize + FRAME_HEADER_WINDOW_BYTES
                : (end !== null ? Math.max(1, end - base) : DEFAULT_MAX_FRAME_BYTES);
            const reader = new FlacBitstreamReader(this.reader, { startOffset: base, maxBitOffset: budget * 8 });
            await this.walkSubframesAsync(reader, header);
            const misaligned = reader.getBitOffset() % 8;
            if (misaligned !== 0)
                await reader.skipBitsAsync(8 - misaligned);
            await reader.readBitsAsync(16);
            return reader.getBytePosition();
        }

        /**
         * Walks the subframes of a frame, one per channel, using the bit width each channel's subframe carries under the channel assignment of the frame.
         * @param {LuminaAudioPlayer.FlacBitstreamReader} reader - The frame bitstream reader.
         * @param {Object} header - The parsed header of the frame.
         * @returns {Promise<void>} A promise resolving when every subframe was walked.
         */
        async walkSubframesAsync(reader, header) {
            const assignment = header.channelAssignment;
            const subframes = assignment <= 7 ? assignment + 1 : 2;
            for (let index = 0; index < subframes; index++) {
                let bitsPerSample = header.bitsPerSample;
                // The side channel of a decorrelated stereo pair carries one bit more than the stream's bit depth.
                if ((assignment === 8 && index === 1) || (assignment === 9 && index === 0) || (assignment === 10 && index === 1))
                    bitsPerSample += 1;
                await this.walkSubframeAsync(reader, header.blockSize, bitsPerSample);
            }
        }

        /**
         * Walks one subframe to its end.
         * @param {LuminaAudioPlayer.FlacBitstreamReader} reader - The frame bitstream reader.
         * @param {number} blockSize - The number of samples of the frame.
         * @param {number} sampleBits - The bit width of the subframe, before its wasted bits.
         * @returns {Promise<void>} A promise resolving when the subframe was walked.
         * @throws {RangeError} When the subframe type is reserved or its parameters are invalid.
         */
        async walkSubframeAsync(reader, blockSize, sampleBits) {
            const padding = await reader.readBitsAsync(1);
            if (padding !== 0)
                throw new RangeError('The FLAC subframe does not begin with a zero padding bit.');
            const type = await reader.readBitsAsync(6);
            const hasWastedBits = await reader.readBitsAsync(1);
            let wastedBits = 0;
            if (hasWastedBits !== 0)
                wastedBits = (await reader.readUnaryAsync()) + 1;
            const bitsPerSample = sampleBits - wastedBits;
            if (bitsPerSample <= 0)
                throw new RangeError('The FLAC subframe has no bits per sample left.');
            if (type === 0) {
                // Constant: one sample value.
                await reader.readBitsAsync(bitsPerSample);
                return;
            }
            if (type === 1) {
                // Verbatim: every sample, unencoded.
                await reader.skipBitsAsync(blockSize * bitsPerSample);
                return;
            }
            if (type >= 8 && type <= 12) {
                // Fixed predictor: the order is the low three bits, and it is followed by a coded residual.
                const order = type - 8;
                await reader.skipBitsAsync(order * bitsPerSample);
                await this.walkResidualAsync(reader, blockSize, order);
                return;
            }
            if (type >= 32) {
                // LPC: the order is the low five bits plus one, with a precision, a shift and the coefficients before the residual.
                const order = (type & 0x1F) + 1;
                await reader.skipBitsAsync(order * bitsPerSample);
                const precision = (await reader.readBitsAsync(4)) + 1;
                if (precision === 16)
                    throw new RangeError('The FLAC LPC subframe names an invalid coefficient precision.');
                await reader.readBitsAsync(5);
                await reader.skipBitsAsync(order * precision);
                await this.walkResidualAsync(reader, blockSize, order);
                return;
            }
            throw new RangeError('The FLAC subframe names a reserved type.');
        }

        /**
         * Walks the coded residual of a fixed or LPC subframe, partition by partition. Each partition is walked by the
         * synchronous fast path of the reader, so the per value awaits that a naive walk would pay are replaced by one
         * reload per window; only a value longer than a whole window falls back to the awaited readers.
         * @param {LuminaAudioPlayer.FlacBitstreamReader} reader - The frame bitstream reader.
         * @param {number} blockSize - The number of samples of the frame.
         * @param {number} predictorOrder - The order of the predictor that produced the residual.
         * @returns {Promise<void>} A promise resolving when the residual was walked.
         * @throws {RangeError} When the residual coding method or its partition sizes are invalid.
         */
        async walkResidualAsync(reader, blockSize, predictorOrder) {
            const method = await reader.readBitsAsync(2);
            if (method > 1)
                throw new RangeError('The FLAC residual names a reserved coding method.');
            const parameterBits = method === 0 ? 4 : 5;
            const escape = method === 0 ? 15 : 31;
            const partitionOrder = await reader.readBitsAsync(4);
            const partitions = 1 << partitionOrder;
            if (blockSize % partitions !== 0)
                throw new RangeError('The FLAC residual partitions do not divide the block evenly.');
            const partitionSamples = blockSize / partitions;
            for (let partition = 0; partition < partitions; partition++) {
                // Walk each partition in turn; the first one also holds the warm-up samples of the predictor, which are
                // not coded as residual values and are therefore skipped from its count.
                const count = partition === 0 ? partitionSamples - predictorOrder : partitionSamples;
                if (count < 0)
                    throw new RangeError('The FLAC residual partition is shorter than the predictor order.');
                let parameter = reader.tryReadBits(parameterBits);
                if (parameter === null) {
                    await reader.refillAsync(false);
                    parameter = reader.tryReadBits(parameterBits);
                    if (parameter === null)
                        parameter = await reader.readBitsAsync(parameterBits);
                }
                if (parameter === escape) {
                    let rawBits = reader.tryReadBits(5);
                    if (rawBits === null) {
                        await reader.refillAsync(false);
                        rawBits = reader.tryReadBits(5);
                        if (rawBits === null)
                            rawBits = await reader.readBitsAsync(5);
                    }
                    await reader.skipBitsAsync(count * rawBits);
                    continue;
                }
                let remaining = count;
                while (remaining > 0) {
                    if (!reader.isResident())
                        await reader.refillAsync(false);
                    let completed = reader.walkRiceSamples(parameter, remaining);
                    if (completed === 0) {
                        // The value was left untouched by a window boundary, so it is retried against a fresh window.
                        await reader.refillAsync(true);
                        completed = reader.walkRiceSamples(parameter, remaining);
                    }
                    if (completed === 0) {
                        // A single value longer than a whole window, which only a pathological stream produces: it is walked with the awaited readers, which span windows.
                        await reader.readUnaryAsync();
                        if (parameter > 0)
                            await reader.readBitsAsync(parameter);
                        completed = 1;
                    }
                    remaining -= completed;
                }
            }
        }

        /**
         * Verifies the 16 bit frame CRC against the bytes that were walked.
         * @param {Uint8Array} frameBytes - The bytes of the whole frame, ending with the CRC.
         * @returns {void}
         * @throws {LuminaAudioPlayer.PlayerError} When the checksum does not match.
         */
        verifyFrameCrc16(frameBytes) {
            if (frameBytes.length < 3)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The FLAC frame is too short to carry a checksum.', { reason: 'frame-crc-mismatch' });
            const length = frameBytes.length;
            const expected = LuminaAudioPlayer.demuxReadUintBE(frameBytes, length - 2, 2);
            const computed = computeCrc16(frameBytes.subarray(0, length - 2));
            if (expected !== computed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The FLAC frame checksum does not match.', { reason: 'frame-crc-mismatch' });
        }

        //+=======================================================================================+
        //|                                   Internal helpers                                    |
        //+=======================================================================================+

        /**
         * Reads the STREAMINFO block and walks the remaining metadata blocks to find where the audio frames begin.
         * @returns {Promise<Object|null>} The stream information, or null when STREAMINFO is missing.
         */
        async readStreamInfoAsync() {
            const blockHeader = await this.reader.readAsync(4, 4);
            if (blockHeader.length < 4)
                return null;
            // The first byte of a metadata block header is the last-block flag in its top bit and the seven bit block type
            // below it; the next three bytes are the big endian length of the block body.
            const isLast = (blockHeader[0] & 0x80) !== 0;
            const blockType = blockHeader[0] & 0x7F;
            const length = LuminaAudioPlayer.demuxReadUintBE(blockHeader, 1, 3);
            if (blockType !== 0 || length !== STREAMINFO_BODY_BYTES)
                return null;
            const body = await this.reader.readAsync(8, STREAMINFO_BODY_BYTES);
            if (body.length < STREAMINFO_BODY_BYTES)
                return null;
            const reader = new LuminaAudioPlayer.DemuxerBitReader(body);
            const minBlockSize = reader.readBits(16);
            const maxBlockSize = reader.readBits(16);
            reader.readBits(24);
            const maxFrameSize = reader.readBits(24);
            const sampleRate = reader.readBits(20);
            const channels = reader.readBits(3) + 1;
            const bitsPerSample = reader.readBits(5) + 1;
            const totalSamples = reader.readBits(36);
            if (sampleRate <= 0 || channels <= 0 || minBlockSize < 16 || maxBlockSize < minBlockSize)
                return null;
            const description = LuminaAudioPlayer.concatDemuxerBytes([
                await this.reader.copyAsync(0, 4),
                blockHeader.slice(),
                body.slice()
            ]);
            const dataStart = await this.walkMetadataBlocksAsync(isLast, 4 + 4 + length);
            return {
                sampleRate,
                channels,
                bitsPerSample,
                totalSamples: totalSamples > 0 ? totalSamples : 0,
                maxFrameSize,
                description,
                dataStart
            };
        }

        /**
         * Walks the metadata blocks that follow STREAMINFO, to find where the audio frames begin. Like the equivalent walk
         * of the WAV demuxer, the number of blocks it will consider is capped, so a stream of zero length blocks that never
         * sets the last block flag cannot make the walk do one read per four bytes of the file.
         * @param {boolean} isStreamInfoLast - True when STREAMINFO is the last metadata block.
         * @param {number} offset - The offset just past the STREAMINFO block.
         * @returns {Promise<number>} The offset of the first audio frame.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream has more metadata blocks than the player accepts.
         */
        async walkMetadataBlocksAsync(isStreamInfoLast, offset) {
            let cursor = offset;
            let isLast = isStreamInfoLast;
            const end = this.reader.getLength();
            let blocks = 0;
            while (!isLast && blocks < MAX_METADATA_BLOCKS) {
                if (end !== null && cursor >= end)
                    return cursor;
                const header = await this.reader.readAsync(cursor, 4);
                if (header.length < 4)
                    return cursor;
                // The same metadata block header as STREAMINFO: the last-block flag is the top bit of the first byte, and the next three bytes are the big endian body length.
                isLast = (header[0] & 0x80) !== 0;
                const length = LuminaAudioPlayer.demuxReadUintBE(header, 1, 3);
                cursor += 4 + length;
                blocks++;
            }
            if (!isLast)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The FLAC stream has more metadata blocks than the player accepts.', { reason: 'metadata-too-many' });
            return cursor;
        }

        /**
         * Reads the header of a frame at an offset.
         * @param {number} offset - The offset of the frame.
         * @returns {Promise<Object|null>} The parsed header, or null when the bytes are not a valid frame header.
         */
        async parseHeaderAtAsync(offset) {
            if (!this.streamInfo)
                return null;
            const bytes = await this.reader.readAsync(offset, FRAME_HEADER_WINDOW_BYTES);
            if (bytes.length < 6)
                return null;
            return parseFrameHeader(bytes, 0, this.streamInfo);
        }

        /**
         * Scans for the next frame header consistent with the stream, starting at an offset. It is used from a distance,
         * by a seek or a resynchronization, before the frame is walked exactly.
         * @param {number} start - The offset to start the scan at.
         * @param {number|null} end - The exclusive end of the scan, or null to scan to the end of the stream.
         * @returns {Promise<number>} The offset of the next frame header, or -1 when there is none.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan gives up on a corrupt or crafted region.
         */
        async findNextFrameAsync(start, end) {
            return await LuminaAudioPlayer.scanForFrameAsync(this.reader, start, end, FLAC_SYNC_PREFIX, async (offset) => {
                const header = await this.parseHeaderAtAsync(offset);
                return header !== null;
            }, { prefixMask: FLAC_SYNC_MASK });
        }

        /**
         * Rescans for the next frame after the current offset deviated from a frame boundary, so that a corrupted or
         * partially downloaded region does not stop the stream.
         * @returns {Promise<number>} The offset of the next frame header, or -1 when there is none.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan gives up on a corrupt or crafted region.
         */
        async resyncAsync() {
            const end = this.reader.getLength();
            const next = await this.findNextFrameAsync(this.offset + 1, end);
            if (next >= 0)
                this.offset = next;
            return next;
        }
    }

    LuminaAudioPlayer.FlacDemuxer = FlacDemuxer;

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.FLAC,
        codec: LuminaAudioPlayer.DemuxerCodec.FLAC,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.FLAC,
        create: (options) => new FlacDemuxer(options)
    });
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
