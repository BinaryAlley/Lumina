/**
 * Common foundation of the audio player demuxers.
 *
 * WebCodecs cannot decode a file: an AudioDecoder consumes a configuration (a codec, a sample rate, a channel count and
 * the codec private data the container carries) and a stream of EncodedAudioChunk objects. Something must therefore parse
 * the bytes the download engine brings in, recognize the container, extract that configuration and split the audio into
 * packets, each with a presentation timestamp and a duration in microseconds. That is what a demuxer does, and this
 * module holds everything the per format demuxers (MP3, AAC/ADTS, FLAC, OGG and WAV) share, so that each one only
 * implements its own container grammar.
 *
 * What lives here
 * ---------------
 *   - The canonical names of the containers and codecs the bundled player supports, and the canonicalizers that map the
 *     free form descriptions the probe stores to those names. Both canonicalizers mirror the server on the same values
 *     (containers by trimming and upper casing, as PlaybackFormatService.CanonicalizeContainer does; codecs by the
 *     distinctive token of the description, as CanonicalizeCodec does), so the two sides agree on what a track holds and
 *     on which tracks are a direct play. Neither adds an alias the server does not, because a container the server's
 *     direct-play gate would reject must not resolve a demuxer here.
 *   - A byte source adapter that turns the range downloading TrackDownloader of the download engine into a flat,
 *     random access byte source that answers a read at an absolute offset and reports end of file instead of rejecting.
 *   - A windowing reader over that source, which keeps one read ahead window so that a parser that walks byte by byte
 *     does not issue one request per byte.
 *   - A bit reader for the big endian bitstream headers of MPEG, FLAC and OGG.
 *   - The packet factory, so every demuxer emits the same frozen packet shape the decode pipeline consumes.
 *   - The base class of a demuxer, which owns the reader, the recorded failure and the error callback, and the demuxer
 *     registry the format modules register themselves into.
 *
 * The demuxer contract
 * --------------------
 * Every demuxer (audio-player-demux-mp3.js and its siblings) exposes:
 *   - initializeAsync(): reads the header(s) of the stream and prepares getConfiguration(); resolves when the stream is
 *     ready to be read, and rejects with a typed error when the bytes are not a stream of this format.
 *   - getConfiguration(): the AudioDecoderConfig like value { codec, sampleRate, numberOfChannels, description? } after
 *     initializeAsync resolved, and null before that.
 *   - readPacketAsync(): the next encoded packet as { data, timestamp, duration, type }, where the timestamp and the
 *     duration are in microseconds, or null once the stream ended. It is incremental: it reads only the bytes of the
 *     packet it returns, never the whole file.
 *   - getDuration(): the duration of the whole stream in seconds, and null when no demuxer of the format can state one. A
 *     format whose duration only becomes exact once the whole stream is read (AAC, OGG) returns a progressive estimate in
 *     the meantime, so a caller must treat a non null value as an estimate that may still be refined, not as a final one.
 *   - seekToTimeAsync(seconds): repositions the read cursor to a packet boundary at or before the given time, so that the
 *     next readPacketAsync returns the packet that covers it. It returns the byte offset it repositioned to, or null when
 *     the format cannot be repositioned by time. How the first packet after a seek is trimmed to the exact target is the
 *     concern of the seek stage, not of the demuxer.
 *   - destroyAsync(): releases every resource, including the reader.
 *
 * A demuxer is a sub component, like the download engine and the decoder: it never moves the player state machine and
 * never emits on the event bus itself. A failure it cannot recover from is recorded on the instance for polling through
 * getError, is reported to the onError callback as a {@link LuminaAudioPlayer.PlayerError} with the DEMUX_FAILED code, and
 * makes the awaited calls (initializeAsync, readPacketAsync, seekToTimeAsync) reject with that same error. An engine must
 * forward onError to host.reportError, exactly as it forwards the download engine's and the decoder's, so the failure
 * reaches the player.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The size, in bytes, of the read ahead window the reader keeps. It is large enough that a parser walking a frame
     * header byte by byte issues one read per window rather than one per byte, and small enough to stay cheap to hold.
     * @readonly
     * @type {number}
     */
    const WINDOW_SIZE_BYTES = 65536;

    /**
     * The largest number of bytes a single read may ask for. It bounds the window a malformed stream can make the reader
     * allocate, so a corrupt length field can never turn a read into an unbounded allocation.
     * @readonly
     * @type {number}
     */
    const MAX_READ_BYTES = 16 * 1024 * 1024;

    /**
     * The size, in bytes, of a scan chunk. A scan for the next frame of a format reads at most this many bytes at a time.
     * @readonly
     * @type {number}
     */
    const SCAN_CHUNK_BYTES = 65536;

    /**
     * Enum-like object for the canonical names of the containers the bundled player demuxes.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.DemuxerContainer = Object.freeze({
        MP3: 'MP3',
        AAC: 'AAC',
        OGG: 'OGG',
        FLAC: 'FLAC',
        WAV: 'WAV'
    });

    /**
     * Enum-like object for the canonical names of the codecs the bundled player decodes.
     * @readonly
     * @enum {string}
     */
    LuminaAudioPlayer.DemuxerCodec = Object.freeze({
        MP3: 'MP3',
        AAC: 'AAC',
        OPUS: 'OPUS',
        VORBIS: 'VORBIS',
        FLAC: 'FLAC',
        PCM: 'PCM'
    });

    /**
     * Canonicalizes the container of an audio file by trimming and upper casing it, exactly as the server canonicalizes
     * the container of the same probe result. It deliberately adds no aliases: a container the server's direct-play gate
     * would not match against the advertised set must not resolve a demuxer here either, so the two sides agree on which
     * tracks are a direct play. Keeping the mapping identical to the server's is what makes a rename on either side a
     * matching change rather than a client that plays a track the server decided to remux.
     * @param {string|null|undefined} container - The container to canonicalize.
     * @returns {string} The canonical container, or an empty string when there is none.
     */
    function canonicalizeContainer(container) {
        if (typeof container !== 'string')
            return '';
        return container.trim().toUpperCase();
    }

    /**
     * Canonicalizes the codec of an audio file, mapping the free form descriptions the probe reports to the canonical codec
     * names the player advertises. It mirrors the mapping of the server, so that the demuxer the player selects for a track
     * is the one the server expected when it decided the delivery. The most specific tokens are matched first, and the MPEG
     * layer is matched by its layer number only, so 'MPEG-1 Layer 2' is not mistaken for an MP3.
     * @param {string|null|undefined} codec - The codec to canonicalize.
     * @returns {string} The canonical codec, or an empty string when there is none.
     */
    function canonicalizeCodec(codec) {
        if (typeof codec !== 'string')
            return '';
        const normalized = codec.trim().toUpperCase();
        if (normalized === '')
            return '';
        if (normalized.includes('VORBIS'))
            return 'VORBIS';
        if (normalized.includes('OPUS'))
            return 'OPUS';
        if (normalized.includes('FLAC'))
            return 'FLAC';
        if (normalized.includes('ALAC') || normalized.includes('APPLE LOSSLESS'))
            return 'ALAC';
        if (normalized.includes('AAC'))
            return 'AAC';
        if (normalized.includes('MP3') || normalized.includes('LAYER 3') || normalized.includes('LAYER III'))
            return 'MP3';
        if (normalized.includes('PCM') || normalized.includes('UNCOMPRESSED') || normalized.includes('LINEAR'))
            return 'PCM';
        if (normalized.includes('WINDOWS MEDIA') || normalized.includes('WMA'))
            return 'WMA';
        return normalized;
    }

    LuminaAudioPlayer.canonicalizeDemuxerContainer = canonicalizeContainer;
    LuminaAudioPlayer.canonicalizeDemuxerCodec = canonicalizeCodec;

    /**
     * Converts a count of samples to microseconds at a sample rate, the unit of every packet timestamp of the player.
     * @param {number} samples - The number of samples.
     * @param {number} sampleRate - The sample rate, in Hz.
     * @returns {number} The duration, in microseconds, rounded to the nearest microsecond.
     */
    function samplesToMicroseconds(samples, sampleRate) {
        if (!Number.isFinite(samples) || !Number.isFinite(sampleRate) || sampleRate <= 0 || samples <= 0)
            return 0;
        return Math.round((samples / sampleRate) * 1000000);
    }

    LuminaAudioPlayer.samplesToMicroseconds = samplesToMicroseconds;

    /**
     * Checks whether a run of bytes at an offset spells out an ASCII string. It is the check every magic number of a
     * container is recognized by.
     * @param {Uint8Array} bytes - The bytes to inspect.
     * @param {number} offset - The first byte of the run.
     * @param {string} text - The expected ASCII text.
     * @returns {boolean} True when the bytes spell out the text at the offset.
     */
    function bytesEqualAscii(bytes, offset, text) {
        if (!bytes || offset < 0 || offset + text.length > bytes.length)
            return false;
        for (let index = 0; index < text.length; index++)
            if (bytes[offset + index] !== text.charCodeAt(index))
                return false;
        return true;
    }

    /**
     * Reads an unsigned integer from a run of bytes in big endian order.
     * @param {Uint8Array} bytes - The bytes to inspect.
     * @param {number} offset - The first byte of the integer.
     * @param {number} length - The number of bytes of the integer, between 1 and 6.
     * @returns {number|null} The value, or null when the run is truncated.
     */
    function readUintBE(bytes, offset, length) {
        if (!bytes || offset < 0 || offset + length > bytes.length || length < 1 || length > 6)
            return null;
        let value = 0;
        for (let index = 0; index < length; index++)
            value = value * 256 + bytes[offset + index];
        return value;
    }

    /**
     * Reads an unsigned integer from a run of bytes in little endian order.
     * @param {Uint8Array} bytes - The bytes to inspect.
     * @param {number} offset - The first byte of the integer.
     * @param {number} length - The number of bytes of the integer, between 1 and 6.
     * @returns {number|null} The value, or null when the run is truncated.
     */
    function readUintLE(bytes, offset, length) {
        if (!bytes || offset < 0 || offset + length > bytes.length || length < 1 || length > 6)
            return null;
        let value = 0;
        let factor = 1;
        for (let index = 0; index < length; index++) {
            value += bytes[offset + index] * factor;
            factor *= 256;
        }
        return value;
    }

    /**
     * Reads a signed integer from a run of bytes in little endian order, two's complement.
     * @param {Uint8Array} bytes - The bytes to inspect.
     * @param {number} offset - The first byte of the integer.
     * @param {number} length - The number of bytes of the integer, between 1 and 4.
     * @returns {number|null} The value, or null when the run is truncated.
     */
    function readIntLE(bytes, offset, length) {
        const value = readUintLE(bytes, offset, length);
        if (value === null)
            return null;
        const limit = Math.pow(2, 8 * length);
        return value >= limit / 2 ? value - limit : value;
    }

    LuminaAudioPlayer.demuxReadUintBE = readUintBE;
    LuminaAudioPlayer.demuxReadUintLE = readUintLE;
    LuminaAudioPlayer.demuxReadIntLE = readIntLE;
    LuminaAudioPlayer.demuxBytesEqualAscii = bytesEqualAscii;

    /**
     * Reads a little endian signed 64 bit integer from a run of bytes, used by the granule position of an OGG page. The
     * value is built from two 32 bit halves so that it stays exact for every granule a real stream can carry.
     * @param {Uint8Array} bytes - The bytes to inspect.
     * @param {number} offset - The first byte of the integer.
     * @returns {number|null} The value, or null when the run is truncated.
     */
    function readInt64LE(bytes, offset) {
        const low = readUintLE(bytes, offset, 4);
        const high = readIntLE(bytes, offset + 4, 4);
        if (low === null || high === null)
            return null;
        // A granule position is a signed 64 bit value; a page that has not completed a packet carries -1, and the two
        // halves are combined without a bitwise shift, which would truncate to 32 bits. This is exact, because a granule
        // is a sample count that never approaches the 2^53 range a double can represent.
        return high * 4294967296 + low;
    }

    LuminaAudioPlayer.demuxReadInt64LE = readInt64LE;

    /**
     * Reads a big endian sync safe integer, the size encoding an ID3v2 tag uses, where the top bit of each of its four
     * bytes is zero. It is shared by the MP3 and AAC demuxers, which may both carry a leading ID3v2 tag.
     * @param {Uint8Array} bytes - The bytes to read from.
     * @param {number} offset - The offset of the first size byte.
     * @returns {number} The value.
     */
    function readSyncSafe(bytes, offset) {
        // Each byte contributes seven bits, so the multipliers are 128, 128 squared and 128 cubed, and the top bit of
        // every byte is masked off.
        return ((bytes[offset] & 0x7F) * 2097152)
            + ((bytes[offset + 1] & 0x7F) * 16384)
            + ((bytes[offset + 2] & 0x7F) * 128)
            + (bytes[offset + 3] & 0x7F);
    }

    LuminaAudioPlayer.demuxReadSyncSafe = readSyncSafe;

    /**
     * Builds the byte-at-a-time lookup table of a non-reflected CRC, so that a checksum over a run of bytes costs one
     * table lookup and one shift per byte instead of one iteration per bit. The checksum it produces is identical to the
     * bit-by-bit form, because it is the same polynomial, initial value and bit order, only evaluated a byte at a time.
     * @param {number} polynomial - The CRC polynomial, without its leading term (0x04C11DB7, 0x8005, 0x07, ...).
     * @param {number} width - The width of the checksum in bits: 8, 16 or 32.
     * @returns {Uint32Array} The 256 entry table, indexed by byte value.
     */
    function buildCrcTable(polynomial, width) {
        const table = new Uint32Array(256);
        const mask = width === 32 ? 0xFFFFFFFF : (1 << width) - 1;
        const topBit = 1 << (width - 1);
        for (let index = 0; index < 256; index++) {
            let crc = (index << (width - 8)) & mask;
            for (let bit = 0; bit < 8; bit++)
                crc = (crc & topBit) !== 0 ? ((crc << 1) ^ polynomial) & mask : (crc << 1) & mask;
            table[index] = crc >>> 0;
        }
        return table;
    }

    LuminaAudioPlayer.demuxBuildCrcTable = buildCrcTable;

    /**
     * A bit reader over a run of big endian bytes, for the bit packed headers of MPEG, FLAC and OGG.
     */
    class DemuxerBitReader {
        /**
         * Creates an instance of DemuxerBitReader.
         * @param {Uint8Array} bytes - The bytes to read from.
         */
        constructor(bytes) {
            this.bytes = bytes instanceof Uint8Array ? bytes : new Uint8Array(0);
            this.bitPosition = 0;
        }

        /**
         * Reads an unsigned integer of up to 53 bits. The accumulation is done in floating point, so the value is exact up
         * to 2^53, which covers every field a container header holds, including the 36 bit total sample count of FLAC.
         * @param {number} count - The number of bits to read, up to 53.
         * @returns {number} The value.
         * @throws {RangeError} When the bytes end before the requested bits are read.
         */
        readBits(count) {
            let value = 0;
            for (let index = 0; index < count; index++) {
                const byteIndex = this.bitPosition >> 3;
                if (byteIndex >= this.bytes.length)
                    throw new RangeError('The demuxer bit reader ran past the end of its bytes.');
                const shift = 7 - (this.bitPosition & 7);
                value = value * 2 + ((this.bytes[byteIndex] >> shift) & 1);
                this.bitPosition++;
            }
            return value;
        }

        /**
         * Gets the number of whole bytes consumed so far.
         * @returns {number} The byte position.
         */
        getBytePosition() {
            return this.bitPosition >> 3;
        }
    }

    LuminaAudioPlayer.DemuxerBitReader = DemuxerBitReader;

    /**
     * A random access byte source that answers a read at an absolute offset and reports end of file as a short read
     * instead of a failure. The demuxers read through this interface, so a test injects a plain in memory implementation
     * and never a downloader.
     */
    class DemuxerByteSource {
        /**
         * Creates an instance of DemuxerByteSource over a range downloading downloader.
         * @param {Object} downloader - The TrackDownloader of the download engine, with readBytesAsync, getTotalBytes, getRanges, isComplete and getError.
         */
        constructor(downloader) {
            this.downloader = downloader;
        }

        /**
         * Gets the total size of the stream, when the download already discovered it.
         * @returns {number|null} The length in bytes, or null when it is not known yet.
         */
        getLength() {
            const total = this.downloader && this.downloader.getTotalBytes ? this.downloader.getTotalBytes() : null;
            return Number.isInteger(total) && total >= 0 ? total : null;
        }

        /**
         * Reads a window of bytes at an absolute offset, waiting for it when the download has not fetched it yet. A window
         * that reaches past the end of the file is shortened, and a window entirely past it reads as empty, so a parser
         * sees end of file as a short read.
         * @param {number} offset - The first byte of the window.
         * @param {number} length - The number of bytes of the window.
         * @returns {Promise<Uint8Array>} The bytes, possibly fewer than requested at the end of the stream.
         */
        async readAsync(offset, length) {
            const start = Math.max(0, Math.floor(offset));
            let size = Math.max(0, Math.floor(length));
            if (size === 0)
                return new Uint8Array(0);
            const total = this.getLength();
            if (total !== null) {
                if (start >= total)
                    return new Uint8Array(0);
                size = Math.min(size, total - start);
            }
            try {
                const bytes = await this.downloader.readBytesAsync(start, size);
                return bytes && bytes.length > 0 ? bytes.subarray(0, size) : new Uint8Array(0);
            } catch (error) {
                // The read rejects when the window can no longer be filled. That is end of file when the window simply
                // reaches past the last downloaded byte and the download has drained: the bytes that are there are still
                // returned, and only the tail is shortened.
                if (!isWindowUnavailableError(error))
                    throw error;
                const coveredEnd = this.getCoveredEnd(start);
                if (coveredEnd > start) {
                    const available = Math.min(size, coveredEnd - start);
                    const bytes = await this.downloader.readBytesAsync(start, available);
                    return bytes && bytes.length > 0 ? bytes.subarray(0, available) : new Uint8Array(0);
                }
                return new Uint8Array(0);
            }
        }

        /**
         * Gets the end of the contiguous run of downloaded bytes that covers an offset.
         * @param {number} offset - The offset to look up.
         * @returns {number} The exclusive end of the covering run, or the offset itself when it is not covered.
         */
        getCoveredEnd(offset) {
            if (!this.downloader || typeof this.downloader.getRanges !== 'function')
                return offset;
            const ranges = this.downloader.getRanges();
            for (const range of ranges)
                if (offset >= range.start && offset < range.end)
                    return range.end;
            return offset;
        }
    }

    /**
     * Checks whether a value is the failure the downloader reports for a window it can never fill, which a byte source
     * reads as end of file.
     * @param {*} error - The value to check.
     * @returns {boolean} True when it is a window unavailable failure.
     */
    function isWindowUnavailableError(error) {
        return !!error
            && error.code === LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_FAILED
            && !!error.details
            && error.details.reason === LuminaAudioPlayer.PlayerErrorReason.WINDOW_UNAVAILABLE;
    }

    /**
     * Checks whether a value is the stop a download reports when it is intentionally aborted or destroyed. The download
     * module treats such a stop as a normal teardown, not as a failure of the track, so a demuxer must not record it.
     * @param {*} error - The value to check.
     * @returns {boolean} True when the value is an intentional download stop.
     */
    function isDownloadAbort(error) {
        if (!error)
            return false;
        if (error.code === LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED)
            return true;
        // A raw fetch abort is also an intentional stop, so a DOMException named AbortError is treated the same way.
        return LuminaAudioPlayer.isAbortError(error);
    }

    LuminaAudioPlayer.isDownloadAbort = isDownloadAbort;

    /**
     * Adapts a byte source for a demuxer: a caller passes the TrackDownloader of the download engine, an object that
     * already implements readAsync and getLength, or nothing.
     * @param {*} source - The source to adapt.
     * @returns {Object} An object with getLength and readAsync.
     */
    function toByteSource(source) {
        if (source && typeof source.readAsync === 'function' && typeof source.getLength === 'function')
            return source;
        if (source && typeof source.readBytesAsync === 'function')
            return new DemuxerByteSource(source);
        return {
            getLength: () => null,
            readAsync: async () => new Uint8Array(0)
        };
    }

    LuminaAudioPlayer.DemuxerByteSource = DemuxerByteSource;
    LuminaAudioPlayer.createDemuxerByteSource = toByteSource;

    /**
     * A windowing reader over a byte source. A parser walks the stream with absolute offsets, and the reader keeps the
     * last window it loaded, so that a sequence of nearby small reads is served from memory and issues one request per
     * window rather than one per read.
     */
    class DemuxerReader {
        /**
         * Creates an instance of DemuxerReader.
         * @param {Object} source - The byte source, per {@link LuminaAudioPlayer.createDemuxerByteSource}.
         * @param {Object} [options] - The reader options.
         * @param {number} [options.windowSize] - The number of bytes kept in the read ahead window. Defaults to 64 KiB.
         */
        constructor(source, options = {}) {
            this.source = toByteSource(source);
            this.windowSize = Number.isInteger(options.windowSize) && options.windowSize > 0 ? options.windowSize : WINDOW_SIZE_BYTES;
            this.windowStart = 0;
            /** @type {Uint8Array} */
            this.windowBytes = new Uint8Array(0);
            this.windowLength = 0;
            // Set by destroy; a destroyed reader is never nulled out from under a caller, it only stops returning bytes.
            this.destroyed = false;
        }

        /**
         * Releases the byte source of the reader, so that the downloader it holds is no longer referenced. The reader is
         * deliberately left in place as a dead reader rather than being cleared by its owner: an operation that was in
         * flight when the demuxer was destroyed keeps a reference to it and reads from it, so a freed object would turn
         * that continuation into a TypeError, which the operation would record as a failure of a track that was in fact
         * torn down. Every read after this returns an empty array and the length reads as unknown instead.
         * @returns {void}
         */
        destroy() {
            this.destroyed = true;
            this.source = null;
            this.windowStart = 0;
            this.windowBytes = new Uint8Array(0);
            this.windowLength = 0;
        }

        /**
         * Gets the total size of the stream, when it is known.
         * @returns {number|null} The length in bytes, or null.
         */
        getLength() {
            return this.source ? this.source.getLength() : null;
        }

        /**
         * Reads up to a number of bytes at an absolute offset. The returned view may be shorter than requested at the end
         * of the stream, and is empty past it. It is a view over the read ahead window, so a caller must not mutate it and
         * must not hold it across the end of the stream, though keeping it is safe while the reader is alive because a
         * reload replaces the window rather than reusing its buffer.
         * @param {number} offset - The first byte to read.
         * @param {number} length - The number of bytes to read.
         * @returns {Promise<Uint8Array>} The bytes, or an empty array when the offset is at or past the end of the stream.
         * @throws {RangeError} When the length is past the limit of a single read. It is refused rather than truncated,
         *     because a caller treats a short read as the end of the stream, so a truncated read would be misread as EOF.
         */
        async readAsync(offset, length) {
            // A destroyed reader reads as an empty stream, so an operation still in flight ends at an end of stream rather
            // than on a released reader.
            if (this.destroyed)
                return new Uint8Array(0);
            const start = Math.max(0, Math.floor(offset));
            const requested = Math.max(0, Math.floor(length));
            if (requested > MAX_READ_BYTES)
                throw new RangeError(`A single demuxer read cannot exceed ${MAX_READ_BYTES} bytes.`);
            const size = requested;
            if (size === 0)
                return new Uint8Array(0);
            if (start >= this.windowStart && start + size <= this.windowStart + this.windowLength)
                return this.windowBytes.subarray(start - this.windowStart, start - this.windowStart + size);
            const loadSize = Math.max(this.windowSize, size);
            const bytes = await this.source.readAsync(start, loadSize);
            this.windowStart = start;
            this.windowBytes = bytes;
            this.windowLength = bytes ? bytes.length : 0;
            if (this.windowLength === 0)
                return new Uint8Array(0);
            return this.windowLength >= size ? this.windowBytes.subarray(0, size) : this.windowBytes;
        }

        /**
         * Reads a window of bytes and copies it, so that the caller owns bytes that do not alias the read ahead window.
         * It is used for the codec private data a demuxer hands to the decoder.
         * @param {number} offset - The first byte to read.
         * @param {number} length - The number of bytes to read.
         * @returns {Promise<Uint8Array>} A copy of the bytes, possibly shorter than requested at the end of the stream.
         */
        async copyAsync(offset, length) {
            const bytes = await this.readAsync(offset, length);
            return bytes.length === 0 ? bytes : bytes.slice();
        }

        /**
         * Reads the last bytes of the stream, when its length is known. It is how a demuxer reads a trailer, such as the
         * last OGG page, without walking the whole stream.
         * @param {number} maxLength - The maximum number of trailing bytes to read.
         * @returns {Promise<{ bytes: Uint8Array, offset: number }|null>} The trailing bytes and their absolute offset, or null when the length is unknown or the stream is empty.
         */
        async readTailAsync(maxLength) {
            const total = this.getLength();
            if (total === null || total <= 0)
                return null;
            const size = Math.min(Math.max(1, Math.floor(maxLength)), total);
            const offset = total - size;
            const bytes = await this.copyAsync(offset, size);
            return bytes.length > 0 ? { bytes, offset } : null;
        }
    }

    LuminaAudioPlayer.DemuxerReader = DemuxerReader;

    /**
     * Coerces a value to a whole, non negative number of microseconds, so that a packet the decoder receives always
     * carries a finite timestamp and duration. A non finite value becomes zero rather than passing through, because this
     * is the last point before the decoder and a NaN or an Infinity there would corrupt the decode timeline silently.
     * @param {number} value - The value to coerce.
     * @returns {number} The whole, non negative microsecond value.
     */
    function toMicroseconds(value) {
        return Number.isFinite(value) ? Math.max(0, Math.round(value)) : 0;
    }

    /**
     * Builds the frozen packet shape the decode pipeline consumes.
     * @param {Uint8Array} data - The encoded bytes of the packet.
     * @param {number} timestamp - The presentation timestamp of the first sample, in microseconds.
     * @param {number} duration - The duration of the packet, in microseconds.
     * @returns {Object} The frozen packet.
     */
    function createPacket(data, timestamp, duration) {
        return Object.freeze({
            data,
            timestamp: toMicroseconds(timestamp),
            duration: toMicroseconds(duration),
            type: 'key'
        });
    }

    LuminaAudioPlayer.createDemuxerPacket = createPacket;

    /**
     * The number of candidate positions a single scan will validate before it gives up. A validator is an awaited read, so
     * a stream whose body is filled with byte patterns that look like a sync, but are not, would otherwise make one scan
     * do one awaited read per pattern. The cap is far above what any real stream needs, because a real frame or page is
     * found within one frame or page length of the start, so it only ever refuses a crafted or badly corrupt region.
     * @readonly
     * @type {number}
     */
    const SCAN_MAX_CANDIDATES = 65536;

    /**
     * Scans a stream for the next offset whose bytes start with a prefix and pass a validator. It reads forward in
     * chunks and only calls the validator on the offsets that match the prefix, so a byte that merely looks like a frame
     * sync costs one comparison while a real header is validated in full. The validator reads the extra bytes it needs
     * itself, so it is not limited by the size of a scan chunk.
     *
     * The prefix may carry a mask per byte, so a format whose sync is shorter than a whole byte can reject most false
     * candidates with a comparison rather than with the awaited validator; and the number of candidates a scan validates
     * is capped, so a crafted region cannot turn a scan into an unbounded number of awaited reads. A scan that reaches
     * that cap throws rather than returning the same -1 it uses for end of stream, so a refused region is reported as a
     * demux failure instead of ending playback silently.
     * @param {LuminaAudioPlayer.DemuxerReader} reader - The reader to scan through.
     * @param {number} start - The offset to start the scan at.
     * @param {number|null} end - The exclusive end of the scan, or null to scan to the end of the stream.
     * @param {Array<number>} prefix - The bytes a candidate must start with.
     * @param {Function} [validateAsync] - An async validator that receives the absolute offset of a candidate and returns whether it is a real frame header. When omitted, the prefix match alone accepts a candidate.
     * @param {Object} [options] - The scan options.
     * @param {Array<number>} [options.prefixMask] - A mask per prefix byte; a candidate matches when each byte matches under its mask. Defaults to an exact comparison of every byte.
     * @returns {Promise<number>} The offset of the first accepted candidate, or -1 when the scan reaches the end of the stream without one.
     * @throws {LuminaAudioPlayer.PlayerError} With {@link LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED} and <c>details.reason === 'scan-budget-exhausted'</c> when the scan evaluated its candidate budget without finding a frame, which is a corrupt or crafted region rather than an end of stream.
     */
    async function scanForFrameAsync(reader, start, end, prefix, validateAsync, options = {}) {
        const settings = options || {};
        const prefixMask = settings.prefixMask || null;
        let remaining = SCAN_MAX_CANDIDATES;
        const limit = end === null ? Number.POSITIVE_INFINITY : Math.floor(end);
        const overlap = Math.max(0, prefix.length - 1);
        let position = Math.max(0, Math.floor(start));
        // The outer loop walks the stream forward one chunk at a time: each iteration reads one chunk at `position`,
        // examines its candidates, and then advances `position` past it (less the overlap), until the scan limit or the
        // end of the stream is reached.
        while (position < limit) {
            const requested = Math.min(SCAN_CHUNK_BYTES, limit - position);
            const bytes = await reader.readAsync(position, requested);
            if (bytes.length === 0)
                return -1;
            // A candidate must fit entirely inside the chunk, so the last start offset is the one where a whole prefix still fits; the `<=` below includes it.
            const lastCandidate = bytes.length - prefix.length;
            // The middle loop tries every byte offset of the chunk as the start of a candidate frame.
            for (let index = 0; index <= lastCandidate; index++) {
                let matches = true;
                // The inner loop matches the candidate against the prefix, byte by byte and under each byte's mask,
                // stopping at the first mismatch. This is the cheap filter: only a candidate that matches the whole
                // prefix is worth the awaited validator, which reads further bytes.
                for (let prefixIndex = 0; prefixIndex < prefix.length; prefixIndex++) {
                    const mask = prefixMask ? prefixMask[prefixIndex] : 0xFF;
                    if ((bytes[index + prefixIndex] & mask) !== (prefix[prefixIndex] & mask)) {
                        matches = false;
                        break;
                    }
                }
                if (!matches)
                    continue;
                // The prefix matched, so this offset is a full candidate: it costs one unit of the scan budget, and the validator, when there is one, decides whether it is a real frame header.
                if (remaining <= 0)
                    throw new LuminaAudioPlayer.PlayerError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The scan evaluated its candidate budget without finding a frame.', { reason: 'scan-budget-exhausted' });
                remaining--;
                const offset = position + index;
                if (!validateAsync || await validateAsync(offset))
                    return offset;
            }
            // When the chunk came back short the stream ended, so there is nothing beyond it to scan.
            if (bytes.length < requested)
                return -1;
            // The next chunk overlaps this one by one byte less than the prefix, so a candidate that straddles the boundary is still validated instead of being skipped.
            position += Math.max(1, bytes.length - overlap);
        }
        return -1;
    }

    LuminaAudioPlayer.scanForFrameAsync = scanForFrameAsync;

    /**
     * Copies a run of bytes into a new array. It is used when a packet is assembled from more than one read, so that the
     * bytes the decoder receives do not alias a read ahead window.
     * @param {Array<Uint8Array>} parts - The runs to concatenate, in order.
     * @returns {Uint8Array} The concatenated bytes.
     */
    function concatBytes(parts) {
        let total = 0;
        for (const part of parts)
            total += part.byteLength;
        const result = new Uint8Array(total);
        let offset = 0;
        for (const part of parts) {
            result.set(part, offset);
            offset += part.byteLength;
        }
        return result;
    }

    LuminaAudioPlayer.concatDemuxerBytes = concatBytes;

    /**
     * The base class of a demuxer. It owns the byte reader, the recorded failure and the error callback, and it defines the interface every per format demuxer implements.
     */
    class Demuxer {
        /**
         * Creates an instance of Demuxer.
         * @param {Object} options - The demuxer options.
         * @param {*} options.source - The byte source, per {@link LuminaAudioPlayer.createDemuxerByteSource}.
         * @param {LuminaAudioPlayer.DemuxerReader} [options.reader] - An existing reader to use instead of building one from the source.
         * @param {Function} [options.onError] - Called with a {@link LuminaAudioPlayer.PlayerError} when the demuxer fails.
         * @param {number} [options.windowSize] - The size of the read ahead window of the reader.
         */
        constructor(options = {}) {
            const settings = options || {};
            this.reader = settings.reader instanceof DemuxerReader ? settings.reader : new DemuxerReader(settings.source, settings);
            this.onError = typeof settings.onError === 'function' ? settings.onError : null;
            /** @type {LuminaAudioPlayer.PlayerError|null} */
            this.error = null;
            this.destroyed = false;
            this.initialized = false;
        }

        /**
         * Checks whether the stream was initialized and can be read.
         * @returns {boolean} True when the stream is initialized.
         */
        isInitialized() {
            return this.initialized && !this.destroyed && this.error === null;
        }

        /**
         * Checks whether the demuxer can no longer read, because it was destroyed or failed.
         * @returns {boolean} True when the demuxer has stopped.
         */
        isStopped() {
            return this.destroyed || this.error !== null;
        }

        /**
         * Checks whether the demuxer was destroyed.
         * @returns {boolean} True when the demuxer was destroyed.
         */
        isDestroyed() {
            return this.destroyed;
        }

        /**
         * Gets the last failure the demuxer recorded, or null.
         * @returns {LuminaAudioPlayer.PlayerError|null} The recorded failure.
         */
        getError() {
            return this.error;
        }

        /**
         * Builds a typed error without recording it.
         * @param {string} code - One of the {@link LuminaAudioPlayer.PlayerErrorCode} values.
         * @param {string} message - The developer facing description of the failure.
         * @param {Object} [details] - Optional structured details.
         * @returns {LuminaAudioPlayer.PlayerError} The typed error.
         */
        createError(code, message, details = null) {
            return new LuminaAudioPlayer.PlayerError(code || LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, message, details || null);
        }

        /**
         * Records a fatal failure, stops the demuxer and notifies the error callback. The first failure wins, so a burst
         * of failures after the demuxer stopped never overwrites the cause or reports twice. A failure that another
         * component already typed with a player error keeps that component's code.
         * @param {*} error - The failure to record.
         * @param {string} [code] - The code to use when the failure is not already a player error.
         * @param {Object} [details] - Optional structured details.
         * @returns {LuminaAudioPlayer.PlayerError} The recorded typed error.
         */
        fail(error, code = LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, details = null) {
            if (this.error)
                return this.error;
            const typed = LuminaAudioPlayer.toOwnedPlayerError(error, code, details);
            this.error = typed;
            this.initialized = false;
            if (this.onError) {
                try {
                    this.onError(typed);
                } catch (callbackError) {
                    console.error('Audio player: the demuxer error callback threw.', callbackError);
                }
            }
            return typed;
        }

        /**
         * Skips an ID3v2 tag at an offset, when one is present. The MP3 and AAC demuxers both use it, because a stream of
         * either may carry a leading ID3v2 tag before its first frame.
         * @param {number} offset - The offset to look for the tag at.
         * @returns {Promise<number>} The offset just past the tag, or the given offset when there is none.
         */
        async skipId3v2Async(offset) {
            const bytes = await this.reader.readAsync(offset, 10);
            if (bytes.length < 10 || !LuminaAudioPlayer.demuxBytesEqualAscii(bytes, 0, 'ID3'))
                return offset;
            const flags = bytes[5];
            const size = LuminaAudioPlayer.demuxReadSyncSafe(bytes, 6);
            // A tag may carry a ten byte footer, which is not counted in the size.
            return offset + 10 + size + ((flags & 0x10) !== 0 ? 10 : 0);
        }

        /**
         * Records a fatal failure through {@link LuminaAudioPlayer.Demuxer#fail}, with two exceptions: an intentional
         * download stop, which is a normal teardown and not a failure of the track, is returned typed without being
         * recorded or reported; and a failure that another component already typed with a structured reason keeps its
         * own reason instead of the generic one of the caller.
         * @param {*} error - The failure to record.
         * @param {string} reason - The structured reason to attach when the failure carries none.
         * @returns {LuminaAudioPlayer.PlayerError} The typed error, recorded unless it is an intentional stop.
         */
        recordFailure(error, reason) {
            // A failure observed after the demuxer was destroyed belongs to the teardown of the operation, not to the track,
            // so it is returned typed as an abort and is neither recorded nor reported as a track failure.
            if (this.destroyed)
                return LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED);
            // An intentional stop is returned typed but not recorded, and it is wrapped with the abort code rather than
            // the demux code, so that a caller which classifies the failure again, as the capability module does, still
            // sees it as an abort and not as a track that has to be normalized.
            if (isDownloadAbort(error))
                return LuminaAudioPlayer.PlayerError.from(error, LuminaAudioPlayer.PlayerErrorCode.DOWNLOAD_ABORTED);
            if (error instanceof LuminaAudioPlayer.PlayerError && error.details && error.details.reason !== undefined)
                return this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED);
            return this.fail(error, LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, { reason });
        }

        /**
         * Reads the header(s) of the stream and prepares the configuration. It must be called before reading any packet.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not a stream of this format.
         */
        async initializeAsync() {
            throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'This demuxer does not implement initializeAsync.');
        }

        /**
         * Reads the next encoded packet of the stream.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'This demuxer does not implement readPacketAsync.');
        }

        /**
         * Gets the decoder configuration of the stream, available after initializeAsync resolved.
         * @returns {Object|null} The configuration, or null before initialization.
         */
        getConfiguration() {
            return null;
        }

        /**
         * Gets the duration of the whole stream, when the header states it.
         * @returns {number|null} The duration in seconds, or null when it is not known from the header.
         */
        getDuration() {
            return null;
        }

        /**
         * Repositions the read cursor to a packet boundary at or before a time.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the format cannot be repositioned by time.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan for a frame gives up on a corrupt or crafted region.
         */
        async seekToTimeAsync(seconds) {
            return null;
        }

        /**
         * Releases every resource of the demuxer. It is safe to call more than once. It releases the reader, and through
         * it the byte source that holds the download engine, without clearing the reader reference itself, because an
         * operation that is still in flight holds that reference and reads through it; the released reader reads as an
         * empty stream, so the operation ends at an end of stream instead of on a freed object. It also gives a subclass
         * the chance to release its own queued state through onDestroy.
         * @returns {Promise<void>} A promise resolving when the demuxer is released.
         */
        async destroyAsync() {
            this.destroyed = true;
            this.initialized = false;
            if (typeof this.onDestroy === 'function')
                this.onDestroy();
            if (this.reader && typeof this.reader.destroy === 'function')
                this.reader.destroy();
        }

        /**
         * Ensures the stream was initialized before it is read.
         * @returns {void}
         * @throws {LuminaAudioPlayer.PlayerError} When the demuxer was destroyed, failed, or was not initialized.
         */
        ensureInitialized() {
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            if (this.error)
                throw this.error;
            if (!this.initialized)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer is not initialized.');
        }
    }

    LuminaAudioPlayer.Demuxer = Demuxer;

    /**
     * A registry of demuxer descriptors, keyed by the canonical container and codec pair a demuxer handles.
     */
    class DemuxerRegistry {
        /**
         * Creates an instance of DemuxerRegistry.
         */
        constructor() {
            /** @type {Map<string, Object>} */
            this.descriptors = new Map();
        }

        /**
         * Builds the key a container and codec pair is registered and looked up under.
         * @param {string} container - The canonical container name.
         * @param {string} codec - The canonical codec name.
         * @returns {string} The registry key.
         */
        static buildKey(container, codec) {
            return `${container}/${codec}`;
        }

        /**
         * Registers a demuxer descriptor.
         * @param {Object} descriptor - The descriptor, with a container, a codec, a representative decoder probe and a create function that receives the demuxer options and returns a demuxer.
         * @returns {LuminaAudioPlayer.DemuxerRegistry} This registry, for chaining.
         * @throws {TypeError} When the descriptor is incomplete, including when it carries no representative probe. The probe is required so that the browser check of the demux capability module never has to guess from a name, which would silently report a supported codec as unsupported.
         */
        register(descriptor) {
            if (!descriptor || typeof descriptor.create !== 'function')
                throw new TypeError('A demuxer descriptor must carry a create function.');
            const container = canonicalizeContainer(descriptor.container);
            const codec = canonicalizeCodec(descriptor.codec);
            if (container === '' || codec === '')
                throw new TypeError('A demuxer descriptor must name a container and a codec.');
            const probe = descriptor.probe;
            if (!probe
                || typeof probe.codec !== 'string' || probe.codec === ''
                || !Number.isInteger(probe.sampleRate) || probe.sampleRate <= 0
                || !Number.isInteger(probe.numberOfChannels) || probe.numberOfChannels <= 0)
                throw new TypeError('A demuxer descriptor must carry a representative decoder probe with a codec string, a sample rate and a channel count.');
            this.descriptors.set(DemuxerRegistry.buildKey(container, codec), Object.freeze({
                container,
                codec,
                probe: Object.freeze({ codec: probe.codec, sampleRate: probe.sampleRate, numberOfChannels: probe.numberOfChannels }),
                create: descriptor.create
            }));
            return this;
        }

        /**
         * Finds the demuxer descriptor of a container and codec pair, matching canonicalized names.
         * @param {string} container - The container name, in any casing.
         * @param {string} codec - The codec name, in any casing or as a free form probe description.
         * @returns {Object|null} The descriptor, or null when no demuxer handles the pair.
         */
        find(container, codec) {
            const key = DemuxerRegistry.buildKey(canonicalizeContainer(container), canonicalizeCodec(codec));
            return this.descriptors.get(key) || null;
        }

        /**
         * Lists the registered descriptors, in registration order.
         * @returns {Array<Object>} The descriptors.
         */
        getDescriptors() {
            return [...this.descriptors.values()];
        }
    }

    LuminaAudioPlayer.DemuxerRegistry = DemuxerRegistry;

    /**
     * The process wide registry the per format modules register their demuxers into. It exists as soon as this module
     * loads, so a format module loaded after it can always register, whatever the load order among the formats.
     * @type {LuminaAudioPlayer.DemuxerRegistry}
     */
    LuminaAudioPlayer.demuxerRegistry = new DemuxerRegistry();

    /**
     * Registers a demuxer descriptor into the process wide registry.
     * @param {Object} descriptor - The descriptor, per {@link LuminaAudioPlayer.DemuxerRegistry#register}.
     * @returns {LuminaAudioPlayer.DemuxerRegistry} The process wide registry, for chaining.
     */
    LuminaAudioPlayer.registerDemuxer = function (descriptor) {
        return LuminaAudioPlayer.demuxerRegistry.register(descriptor);
    };
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
