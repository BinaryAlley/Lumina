/**
 * OGG demuxer of the audio player, for the Opus and Vorbis codecs.
 *
 * An OGG file is a sequence of pages, each one a header followed by a segment table and the page's data. The segmented
 * data is not framed by bytes but by lacing: a run of lacing values of 255 means the packet continues, and a value below
 * 255 ends a packet, so a packet may span several pages and a page may hold several packets. A page also carries a
 * granule position, which is the position, in the codec's own sample unit, of the last packet that completed on the page,
 * a serial number, so that several logical streams can be multiplexed into one physical stream, and a CRC-32 over the
 * whole page, which this demuxer verifies so that a corrupt page, or a byte pattern that merely looks like a page
 * capture, is never read as data.
 *
 * This module owns the page and packet assembly once, in the OggDemuxer base, and the two codec specific demuxers only
 * describe their identification headers, the codec private data WebCodecs needs, and how a packet's duration is measured.
 * The registry maps both the OGG/OPUS and the OGG/VORBIS pair to their demuxer.
 *
 * Telling the two apart
 * ---------------------
 * The first packet of the logical stream identifies the codec: it begins with 'OpusHead' for Opus and, for Vorbis, with
 * a packet type byte of 0x01 followed by 'vorbis'. Everything after the identification header is a codec header packet
 * (OpusTags for Opus, the comment and setup headers for Vorbis) that is not audio, and this demuxer consumes those during
 * initialization, so the first packet readPacketAsync returns is audio.
 *
 * Timestamps and the granule position
 * -----------------------------------
 * A granule is a count of samples for Vorbis and a count of 48 kHz samples, including the pre-skip the encoder inserted,
 * for Opus. The page granule gives the end position of its last completed packet exactly, so this demuxer anchors the
 * timeline to it at every page boundary and derives the packets that precede it, on the same page, by summing their own
 * durations from the packed packet header (the table of contents byte of an Opus packet, the block size bit of a Vorbis
 * packet). The pre-skip of an Opus stream is subtracted, because the WebCodecs decoder discards those priming samples
 * itself when the description carries the OpusHead, so the first decoded sample is reported at time zero.
 *
 * Seeking
 * -------
 * Page granule positions are non decreasing, so seeking is a bisection over the byte offsets of the stream: it reads the
 * page at the middle of the search interval and narrows the interval by its granule, which converges to the first page
 * whose granule is at or after the target in a logarithmic number of page reads, and never walks the stream. When that
 * page carries the continuation flag, its first packet began on the previous page, so the seek steps back one page, whose
 * exact end was verified, so that the packet is assembled from its true start. A seek never lands on a page of codec
 * headers either: the cursor is brought forward to the first page that holds audio, and a page that still carries a header
 * packet, which can happen when a setup header shares a page with audio, has that packet filtered out as well.
 *
 * The codec private data
 * ----------------------
 *   - Opus: the description is the OpusHead identification header, which also tells the decoder the pre-skip to discard.
 *   - Vorbis: the description is the Xiph extradata format the WebCodecs registration requires: a byte with the count of
 *     lacing values, the lacing values themselves, and then the identification, comment and setup header packets.
 */
(function (LuminaAudioPlayer) {
    'use strict';

    /**
     * The four byte capture pattern every OGG page starts with.
     * @readonly
     * @type {Array<number>}
     */
    const OGG_CAPTURE_PATTERN = Object.freeze([0x4F, 0x67, 0x67, 0x53]);

    /**
     * The number of bytes of an OGG page header, before the segment table.
     * @readonly
     * @type {number}
     */
    const OGG_PAGE_HEADER_BYTES = 27;

    /**
     * The offset of the CRC field of an OGG page header, which is zeroed while the checksum is computed.
     * @readonly
     * @type {number}
     */
    const OGG_CRC_OFFSET = 22;

    /**
     * The largest size a single OGG page can have: the fixed header, the largest segment table, and the largest payload
     * the lacing values can describe. A candidate that claims more is not a page, and the bound is what keeps a crafted
     * length field from making a scan read and checksum an arbitrarily large span.
     * @readonly
     * @type {number}
     */
    const OGG_MAX_PAGE_BYTES = OGG_PAGE_HEADER_BYTES + 255 + 255 * 255;

    /**
     * The number of candidate pages a single scan may read in full and checksum before it gives up. A stream whose body is
     * filled with the capture pattern, with header fields crafted to pass the cheap structural checks, can surround a real
     * page with thousands of false candidates; the cheap checks reject most of them, and this budget bounds the rest, so a
     * scan can never be turned into unbounded checksum and allocation work.
     * @readonly
     * @type {number}
     */
    const OGG_MAX_SCAN_VALIDATIONS = 1024;

    /**
     * The number of pages a single seek may step back over when the page it landed on is a continuation of a packet that
     * began earlier. A packet that spans this many pages is far past anything a real stream produces, so the bound only
     * stops a pathological one; the shared validation budget of the seek bounds the work underneath it.
     * @readonly
     * @type {number}
     */
    const OGG_MAX_CONTINUED_STEPS = 255;

    /**
     * The number of bytes a single page lookup scans before it gives up. A valid stream has no gaps between pages, so the
     * page at or after any offset starts within one page length of it; the bound keeps a stream whose middle is filled
     * with the capture pattern from making one lookup walk an arbitrarily long region.
     * @readonly
     * @type {number}
     */
    const OGG_SCAN_SPAN_BYTES = OGG_MAX_PAGE_BYTES * 2;

    /**
     * The largest packet this demuxer will assemble. A packet may span any number of pages, and the parts of one are held
     * until it is complete, so without a cap a crafted stream could make a single packet hold the whole stream in memory.
     * The cap is far past any real audio packet of the supported codecs, so it only ever refuses a crafted one.
     * @readonly
     * @type {number}
     */
    const OGG_MAX_PACKET_BYTES = 16 * 1024 * 1024;

    /**
     * Draws one unit from a shared scan budget, reporting whether a unit was available. Every unit of work a seek does
     * that is proportional to the bytes or the page patterns of the stream draws from the same budget: the window read
     * that precedes a scan, each cheap header probe, and each full page validation. That is what keeps a crafted stream
     * from multiplying the work of a seek by the number of its steps.
     * @param {{ remaining: number }} budget - The shared budget.
     * @returns {boolean} True when a unit was available and was consumed.
     */
    function consumeBudget(budget) {
        if (budget.remaining <= 0)
            return false;
        budget.remaining--;
        return true;
    }

    /**
     * The number of bytes of the tail read to find the last page of a stream.
     * @readonly
     * @type {number}
     */
    const OGG_TAIL_BYTES = 65536;

    /**
     * The number of bytes read before a page to find the page that precedes it when a seek lands on a continued page.
     * @readonly
     * @type {number}
     */
    const OGG_PREVIOUS_WINDOW_BYTES = 131072;

    /**
     * The number of samples per second a granule of an Opus stream counts at, fixed by the Opus format.
     * @readonly
     * @type {number}
     */
    const OPUS_GRANULE_RATE = 48000;

    /**
     * The duration, in milliseconds, of one Opus frame per configuration number, indexed by the five bit configuration
     * of the packet's table of contents byte (RFC 6716, table 2).
     * @readonly
     * @type {Array<number>}
     */
    const OPUS_FRAME_DURATION_MS = Object.freeze([
        10, 20, 40, 60, 10, 20, 40, 60, 10, 20, 40, 60,
        10, 20, 10, 20,
        2.5, 5, 10, 20, 2.5, 5, 10, 20, 2.5, 5, 10, 20, 2.5, 5, 10, 20
    ]);

    /**
     * The header type flag of an OGG page that marks a continued packet.
     * @readonly
     * @type {number}
     */
    const OGG_CONTINUED_FLAG = 0x01;

    /**
     * The lookup table of the OGG page checksum, built once for the life of the page. It turns the checksum from eight
     * bit iterations to one byte lookup per byte of a page.
     * @readonly
     * @type {Uint32Array}
     */
    const OGG_CRC_TABLE = LuminaAudioPlayer.demuxBuildCrcTable(0x04C11DB7, 32);

    /**
     * Computes the CRC-32 of an OGG page, with the polynomial 0x04C11DB7 and an initial value of zero, over the page
     * with its CRC field read as zero, as the framing specification defines it. The zeroed field is passed as a range
     * rather than applied to a copy, so a page is never duplicated just to compute its checksum.
     * @param {Uint8Array} bytes - The page bytes.
     * @param {number} [zeroStart] - The first byte of the field to read as zero, or -1 for none.
     * @param {number} [zeroLength] - The number of bytes of the field to read as zero.
     * @returns {number} The checksum.
     */
    function computeOggCrc(bytes, zeroStart = -1, zeroLength = 0) {
        const zeroEnd = zeroStart + zeroLength;
        let crc = 0;
        for (let index = 0; index < bytes.length; index++) {
            const byte = index >= zeroStart && index < zeroEnd ? 0 : bytes[index];
            crc = ((crc << 8) ^ OGG_CRC_TABLE[((crc >>> 24) ^ byte) & 0xFF]) >>> 0;
        }
        return crc >>> 0;
    }

    /**
     * Demuxes an OGG logical stream, owning the page and packet assembly and leaving the codec specifics to a subclass.
     */
    class OggDemuxer extends LuminaAudioPlayer.Demuxer {
        /**
         * Creates an instance of OggDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            this.pageOffset = 0;
            this.currentSerial = null;
            // The offset of the last page read from the logical stream, and the offset of the first page that holds audio.
            this.lastPageOffset = null;
            this.audioStartOffset = null;
            /** @type {Array<Uint8Array>} */
            this.pendingParts = [];
            // The number of bytes the parts of the packet being assembled hold, so the assembled packet size is bounded without having to concatenate them first.
            this.pendingLength = 0;
            /** @type {Array<{ data: Uint8Array, granule: number|null }>} */
            this.pendingPackets = [];
            this.lastGranule = null;
            this.durationSeconds = null;
            // True once the duration was taken from the last page, so that the granule of an intermediate page, which is
            // the position reached and not the length of the stream, never overwrites it.
            this.durationIsFinal = false;
            this.reachedEnd = false;
            this.seekResync = false;
            this.timelineMicros = 0;
        }

        //+=======================================================================================+
        //|                                       Reading                                         |
        //+=======================================================================================+

        /**
         * Reads the next packet of the logical stream, assembling it across pages when it spans them.
         * @returns {Promise<{ data: Uint8Array, granule: number|null }|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the page structure is corrupt.
         */
        async readRawPacketAsync() {
            while (this.pendingPackets.length === 0) {
                if (this.reachedEnd)
                    return null;
                await this.readNextPageAsync();
            }
            return this.pendingPackets.shift();
        }

        /**
         * Reads the next audio packet of the logical stream as a demuxer packet, skipping any codec header packet it
         * meets. Initialization consumes the header packets of a stream once, but a seek may reposition the read cursor
         * onto a page that also carries them, so they are filtered here as well rather than only during initialization.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the page structure is corrupt.
         */
        async readNextAudioPacketAsync() {
            let packet = await this.readRawPacketAsync();
            while (packet && this.isHeaderPacket(packet.data))
                packet = await this.readRawPacketAsync();
            if (!packet)
                return null;
            const duration = this.computePacketDurationMicros(packet.data);
            const timestamp = packet.granule !== null
                ? Math.max(0, this.granuleToMicros(packet.granule) - duration)
                : this.timelineMicros;
            this.timelineMicros = timestamp + duration;
            return LuminaAudioPlayer.createDemuxerPacket(packet.data, timestamp, duration);
        }

        /**
         * Reads the next page and turns its lacing values into queued packets, carrying a packet that spans pages over to the next one.
         * @returns {Promise<void>} A promise resolving when the page was consumed.
         * @throws {LuminaAudioPlayer.PlayerError} When the page structure is corrupt.
         */
        async readNextPageAsync() {
            const page = await this.readPageAsync(this.pageOffset);
            if (!page) {
                this.reachedEnd = true;
                return;
            }
            this.pageOffset = page.nextOffset;
            if (this.currentSerial === null)
                this.currentSerial = page.serial;
            // A physical stream can multiplex several logical streams; only the one this demuxer locked onto is read.
            if (page.serial !== this.currentSerial)
                return;
            this.lastPageOffset = page.offset;
            if (page.granule !== null) {
                this.lastGranule = page.granule;
                // Only an unknown length is tracked from page to page; when the last page was already probed, the granule
                // of an intermediate page would replace the total with the position reached so far.
                if (!this.durationIsFinal)
                    this.refreshDuration();
            }
            const isContinued = (page.headerType & OGG_CONTINUED_FLAG) !== 0;
            if (isContinued !== (this.pendingParts.length > 0))
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG page continuation flags do not line up.', { reason: 'invalid-continuation' });
            const completions = [];
            let dataOffset = 0;
            // The segment table is a list of lacing values. A value of 255 means the packet continues into the next
            // segment, and possibly the next page, while any value below 255 ends the packet it belongs to, so a packet
            // completes exactly when such a value is reached.
            for (const lace of page.segments) {
                const slice = page.data.subarray(dataOffset, dataOffset + lace);
                dataOffset += lace;
                this.pendingParts.push(slice);
                this.pendingLength += lace;
                if (lace < 255) {
                    completions.push(LuminaAudioPlayer.concatDemuxerBytes(this.pendingParts));
                    this.pendingParts = [];
                    this.pendingLength = 0;
                }
            }
            // A packet can span any number of pages, and its parts are held until it completes, so its size is bounded here
            // rather than left to whatever the stream claims.
            if (this.pendingLength > OGG_MAX_PACKET_BYTES)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG packet being assembled exceeds the size the player accepts.', { reason: 'packet-too-large' });
            const lastCompletion = completions.length - 1;
            for (let index = 0; index < completions.length; index++)
                this.pendingPackets.push({ data: completions[index], granule: index === lastCompletion ? page.granule : null });
        }

        /**
         * Reads and validates the fixed part of a page header, without reading the segment table or the payload. It is the
         * cheap structural gate a scan runs on every capture pattern: the version and the reserved header type bits reject
         * a false capture before any page sized buffer is allocated or any checksum computed. The page size bound depends
         * on the lacing values, so it can only be enforced once the segment table is read, in {@link LuminaAudioPlayer.OggDemuxer#finishPageAsync}.
         * @param {number} offset - The offset of the candidate capture pattern.
         * @returns {Promise<Object>} An object with a <c>kind</c> of 'page', 'not-a-page' or 'unsupported-version', and, for a page, the parsed header fields.
         */
        async readPageHeaderAsync(offset) {
            if (offset < 0)
                return { kind: 'not-a-page' };
            const header = await this.reader.readAsync(offset, OGG_PAGE_HEADER_BYTES);
            if (header.length < OGG_PAGE_HEADER_BYTES || !LuminaAudioPlayer.demuxBytesEqualAscii(header, 0, 'OggS'))
                return { kind: 'not-a-page' };
            if (header[4] !== 0)
                return { kind: 'unsupported-version' };
            // Only the three defined header type flags may be set; any other bit is reserved, which marks a false capture.
            if ((header[5] & 0xF8) !== 0)
                return { kind: 'not-a-page' };
            // The fixed part of a page header is the four byte capture pattern, one byte version, one byte header type
            // flags, the eight byte granule position, the four byte serial, the four byte sequence number, the four byte
            // checksum and one byte segment count; the segment table and the payload follow it.
            return {
                kind: 'page',
                headerType: header[5],
                granule: LuminaAudioPlayer.demuxReadInt64LE(header, 6),
                serial: LuminaAudioPlayer.demuxReadUintLE(header, 14, 4),
                storedCrc: LuminaAudioPlayer.demuxReadUintLE(header, OGG_CRC_OFFSET, 4),
                pageSegments: header[26]
            };
        }

        /**
         * Reads and validates one OGG page, including its CRC-32.
         * @param {number} offset - The offset of the page's capture pattern.
         * @param {Object} [options] - The options.
         * @param {boolean} [options.strict] - True to treat a checksum mismatch or an unsupported version as a failure; false to report it as 'not a page'. Defaults to true, and a probe or a scan uses false.
         * @param {number|null} [options.serial] - The serial a page must carry to be accepted, or null to accept any. A scan passes the serial of its logical stream.
         * @returns {Promise<Object|null>} The page, or null at the end of the stream, at a truncated page, or at a page a non strict read refused.
         * @throws {LuminaAudioPlayer.PlayerError} When a strict read finds an unsupported version or a checksum mismatch.
         */
        async readPageAsync(offset, options = {}) {
            const strict = options.strict !== false;
            const probe = await this.readPageHeaderAsync(offset);
            if (probe.kind === 'not-a-page')
                return null;
            if (probe.kind === 'unsupported-version') {
                if (!strict)
                    return null;
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG page uses an unsupported version.', { reason: 'unsupported-ogg-version' });
            }
            if (options.serial !== undefined && options.serial !== null && probe.serial !== options.serial)
                return null;
            return await this.finishPageAsync(offset, probe, strict);
        }

        /**
         * Reads the segment table and the payload of a page whose header a caller already validated, computes the
         * checksum over the page with its own CRC field read as zero, and returns the page. The data a page carries is
         * exposed as a view of the one buffer that was read, so validating a page allocates a single page sized buffer.
         * @param {number} offset - The offset of the page's capture pattern.
         * @param {Object} probe - The parsed header, per {@link LuminaAudioPlayer.OggDemuxer#readPageHeaderAsync}.
         * @param {boolean} strict - True to treat a checksum mismatch as a failure, false to report it as 'not a page'.
         * @returns {Promise<Object|null>} The page, or null when it is truncated or refused.
         * @throws {LuminaAudioPlayer.PlayerError} When a strict read finds a checksum mismatch.
         */
        async finishPageAsync(offset, probe, strict) {
            const segmentTable = await this.reader.readAsync(offset + OGG_PAGE_HEADER_BYTES, probe.pageSegments);
            if (segmentTable.length < probe.pageSegments)
                return null;
            let dataLength = 0;
            for (let index = 0; index < probe.pageSegments; index++)
                dataLength += segmentTable[index];
            // The length is bounded by the page structure itself: at most 255 lacing values of at most 255 bytes each, so it can never exceed OGG_MAX_PAGE_BYTES and needs no separate check.
            const totalLength = OGG_PAGE_HEADER_BYTES + probe.pageSegments + dataLength;
            const pageBytes = await this.reader.copyAsync(offset, totalLength);
            if (pageBytes.length < totalLength)
                return null;
            if (computeOggCrc(pageBytes, OGG_CRC_OFFSET, 4) !== (probe.storedCrc >>> 0)) {
                if (!strict)
                    return null;
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG page checksum does not match.', { reason: 'page-crc-mismatch' });
            }
            return Object.freeze({
                offset,
                headerType: probe.headerType,
                granule: probe.granule !== null && probe.granule >= 0 ? probe.granule : null,
                serial: probe.serial,
                segments: segmentTable.slice(),
                data: pageBytes.subarray(OGG_PAGE_HEADER_BYTES + probe.pageSegments),
                nextOffset: offset + totalLength
            });
        }

        /**
         * Finds the first page whose capture pattern is at or after an offset and before a limit, skipping a pattern that
         * is not a valid page. A candidate is filtered by the cheap structural gate first, and only a candidate that
         * passes it is read in full and checksummed, drawing from the caller's budget, so that a whole seek shares one
         * bound instead of giving each of its steps a fresh one.
         * @param {number} offset - The offset to start the search at.
         * @param {number|null} [limit] - The exclusive end of the search, or null to search to the end of the stream.
         * @param {{ remaining: number }} [budget] - The shared validation budget, or null for a fresh one.
         * @returns {Promise<Object|null>} The page, or null when there is none before the limit.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan gives up on a corrupt or crafted region.
         */
        async findPageAtOrAfterAsync(offset, limit, budget) {
            const end = this.reader.getLength();
            let scanLimit;
            if (limit === null || limit === undefined)
                scanLimit = end;
            else
                scanLimit = end === null ? limit : Math.min(limit, end);
            const shared = budget || { remaining: OGG_MAX_SCAN_VALIDATIONS };
            // The scan itself reads a window, so it draws a unit before it does anything.
            if (!consumeBudget(shared))
                return null;
            let cursor = Math.max(0, Math.floor(offset));
            // A capture pattern that turns out not to be a valid page is skipped and the search resumes just past it, so
            // the loop repeats until a page is accepted or the cursor reaches the end of the search limit.
            while (scanLimit === null || cursor < scanLimit) {
                const found = await LuminaAudioPlayer.scanForFrameAsync(this.reader, cursor, scanLimit, OGG_CAPTURE_PATTERN, async (candidate) => {
                    // A cheap header probe is a unit of work too, so a stream whose body is tiled with capture patterns
                    // that fail the gate cannot make a seek do an unbounded number of probes.
                    if (!consumeBudget(shared))
                        return false;
                    const probe = await this.readPageHeaderAsync(candidate);
                    return probe.kind === 'page' && (this.currentSerial === null || probe.serial === this.currentSerial);
                });
                if (found < 0)
                    return null;
                if (!consumeBudget(shared))
                    return null;
                const page = await this.readPageAsync(found, { strict: false, serial: this.currentSerial });
                if (page)
                    return page;
                cursor = found + 1;
            }
            return null;
        }

        /**
         * Finds the page that ends exactly at an offset, used to step back over a continued packet. Each candidate is
         * filtered by the cheap structural gate, and the full reads draw from the caller's shared budget.
         * @param {number} offset - The offset a page ends at.
         * @param {{ remaining: number }} [budget] - The shared validation budget, or null for a fresh one.
         * @returns {Promise<Object|null>} The preceding page, or null when it cannot be found.
         */
        async findPreviousPageAsync(offset, budget) {
            const start = Math.max(0, offset - OGG_PREVIOUS_WINDOW_BYTES);
            if (offset <= start)
                return null;
            const shared = budget || { remaining: OGG_MAX_SCAN_VALIDATIONS };
            // The window read is a unit of work, so it draws one before it is done.
            if (!consumeBudget(shared))
                return null;
            const bytes = await this.reader.copyAsync(start, offset - start);
            for (let index = bytes.length - OGG_CAPTURE_PATTERN.length; index >= 0; index--) {
                if (!LuminaAudioPlayer.demuxBytesEqualAscii(bytes, index, 'OggS'))
                    continue;
                const candidateOffset = start + index;
                if (!consumeBudget(shared))
                    return null;
                const probe = await this.readPageHeaderAsync(candidateOffset);
                if (probe.kind !== 'page' || (this.currentSerial !== null && probe.serial !== this.currentSerial))
                    continue;
                if (!consumeBudget(shared))
                    return null;
                const candidate = await this.finishPageAsync(candidateOffset, probe, false);
                if (candidate && candidate.nextOffset === offset)
                    return candidate;
            }
            return null;
        }

        //+=======================================================================================+
        //|                                      Duration                                         |
        //+=======================================================================================+

        /**
         * Gets the duration of the logical stream. When the total length of the stream is known, it is the exact duration,
         * taken from the granule of the last page. When the length is unknown, it is a progressive estimate from the granule
         * of the last page read so far, so it grows as more of the stream is read and is only final once the last page has
         * been seen; durationIsFinal reports which of the two it is.
         * @returns {number|null} The duration in seconds, or null when no page with a granule has been read yet.
         */
        getDuration() {
            return this.durationSeconds;
        }

        /**
         * Refreshes the known duration from the last granule read.
         */
        refreshDuration() {
            if (this.lastGranule === null)
                return;
            const micros = this.granuleToMicros(this.lastGranule);
            if (Number.isFinite(micros) && micros >= 0)
                this.durationSeconds = micros / 1000000;
        }

        /**
         * Reads the granule position of the last page of the stream, when the length of the stream is known. It is what
         * gives the duration before the end of the stream is reached. A candidate is filtered by the cheap structural gate
         * first, and only a candidate that passes it is read in full and checksummed; the tail is bounded, and the cheap
         * probes and the full validations both draw from one budget, so a tail filled with the capture pattern cannot turn
         * this into unbounded work.
         * @returns {Promise<number|null>} The last granule, or null when it cannot be read.
         */
        async probeLastGranuleAsync() {
            const tail = await this.reader.readTailAsync(OGG_TAIL_BYTES);
            if (!tail)
                return null;
            const bytes = tail.bytes;
            const budget = { remaining: OGG_MAX_SCAN_VALIDATIONS };
            for (let index = bytes.length - OGG_CAPTURE_PATTERN.length; index >= 0; index--) {
                if (!LuminaAudioPlayer.demuxBytesEqualAscii(bytes, index, 'OggS'))
                    continue;
                const offset = tail.offset + index;
                if (!consumeBudget(budget))
                    return null;
                const probe = await this.readPageHeaderAsync(offset);
                if (probe.kind !== 'page' || (this.currentSerial !== null && probe.serial !== this.currentSerial))
                    continue;
                if (!consumeBudget(budget))
                    return null;
                const candidate = await this.finishPageAsync(offset, probe, false);
                if (candidate && candidate.granule !== null) {
                    this.currentSerial = candidate.serial;
                    return candidate.granule;
                }
            }
            return null;
        }

        //+=======================================================================================+
        //|                                        Seeking                                        |
        //+=======================================================================================+

        /**
         * Repositions the read cursor to the page that holds a time. Page granule positions are non decreasing, so the
         * offset of the first page that ends at or after the target is found by a lower bound binary search over byte
         * offsets: each step reads the page at or after the middle of the search interval and narrows it by whether that
         * page already reaches the target. Every step shares one validation budget, and each page lookup only scans a
         * bounded span, so the whole seek is bounded even against a stream whose middle is filled with the capture pattern.
         * @param {number} seconds - The target time, in seconds.
         * @returns {Promise<number|null>} The byte offset it repositioned to, or null when the stream cannot be seeked.
         * @throws {LuminaAudioPlayer.PlayerError} When the scan for a page gives up on a corrupt or crafted region.
         */
        async seekToTimeAsync(seconds) {
            this.ensureInitialized();
            const end = this.reader.getLength();
            if (end === null || !Number.isFinite(seconds) || seconds < 0)
                return null;
            // The target is clamped to the duration only when that duration is final. While the total length of the stream
            // is unknown, the duration is a progressive estimate from the pages read so far, and clamping to it would land
            // the seek short of the requested time; without a clamp the binary search positions on the granules themselves.
            const targetMicros = this.durationIsFinal && this.durationSeconds > 0
                ? Math.min(seconds, this.durationSeconds) * 1000000
                : seconds * 1000000;
            // One budget for the whole seek, shared by the binary search and the step back over a continued packet.
            const budget = { remaining: OGG_MAX_SCAN_VALIDATIONS };
            let low = 0;
            let high = end - 1;
            let iterations = 0;
            while (low < high && iterations < 64 && budget.remaining > 0) {
                iterations++;
                const middle = Math.floor((low + high) / 2);
                const page = await this.findPageAtOrAfterAsync(middle, Math.min(end, middle + OGG_SCAN_SPAN_BYTES), budget);
                // A page that is missing past the last page start means the interval is past the end, which counts as
                // reaching the target, so the lower bound narrows onto the last page rather than running past it.
                const reachesTarget = page === null || (page.granule !== null && this.granuleToMicros(page.granule) >= targetMicros);
                if (reachesTarget)
                    high = middle;
                else
                    low = middle + 1;
            }
            let result = await this.findPageAtOrAfterAsync(low, Math.min(end, low + OGG_SCAN_SPAN_BYTES), budget);
            if (!result)
                result = await this.findPageAtOrAfterAsync(0, Math.min(end, OGG_SCAN_SPAN_BYTES), budget);
            if (!result)
                return null;
            // A seek must never land on a page of codec headers: the identification, comment and setup packets are not
            // audio, so the cursor is brought forward to the first page that holds audio.
            if (this.audioStartOffset !== null && result.offset < this.audioStartOffset) {
                const firstAudioPage = await this.findPageAtOrAfterAsync(this.audioStartOffset, Math.min(end, this.audioStartOffset + OGG_SCAN_SPAN_BYTES), budget);
                if (firstAudioPage)
                    result = firstAudioPage;
            }
            // A continued page starts in the middle of a packet, so the walk steps back over every continued page until it
            // reaches the page that actually starts the packet, so a packet that spans more than two pages is assembled
            // from its true start. The walk is allowed to cross the first page that holds audio, because a large setup
            // header can continue into that page, and the codec header packets it would then read there are filtered out
            // on the read path by isHeaderPacket, so crossing can never re-emit a header as audio.
            let steps = 0;
            while ((result.headerType & OGG_CONTINUED_FLAG) !== 0 && steps < OGG_MAX_CONTINUED_STEPS) {
                const previous = await this.findPreviousPageAsync(result.offset, budget);
                if (!previous)
                    break;
                result = previous;
                steps++;
            }
            this.pageOffset = result.offset;
            this.pendingPackets = [];
            this.pendingParts = [];
            this.pendingLength = 0;
            this.reachedEnd = false;
            // The page granule gives the end of its last completed packet; the packets before it on the page are placed by
            // the seek resync, once the page was read, so they are all emitted with an absolute, correct timeline.
            this.seekResync = true;
            return result.offset;
        }

        /**
         * Prepares the first page read after a seek, so that every packet of that page is emitted with an absolute
         * timestamp derived from the page granule rather than from the requested time. It reads the page when the queue is
         * empty and sets the timeline so that the page's last audio packet ends at its granule. The page duration is
         * measured without touching any per packet state, so the packets are then emitted with the very same durations
         * they were measured with.
         * @returns {Promise<void>} A promise resolving when the timeline is set.
         */
        async prepareAfterSeekAsync() {
            this.seekResync = false;
            if (this.pendingPackets.length === 0)
                await this.readNextPageAsync();
            const packets = this.pendingPackets;
            if (packets.length === 0)
                return;
            const total = this.measurePacketsDurationMicros(packets);
            // The granule of the last audio packet of the page is the page's granule; a header packet that shares the page does not carry it.
            let granule = null;
            for (let index = packets.length - 1; index >= 0; index--) {
                if (!this.isHeaderPacket(packets[index].data)) {
                    granule = packets[index].granule;
                    break;
                }
            }
            this.timelineMicros = granule !== null
                ? Math.max(0, this.granuleToMicros(granule) - total)
                : Math.max(0, this.timelineMicros);
        }

        //+=======================================================================================+
        //|                                    Abstract surface                                   |
        //+=======================================================================================+

        /**
         * Gets the duration, in microseconds, of one packet of this codec. A codec must implement it: the base throws,
         * because a default of zero would freeze the timeline of the codec that forgot to override it, and the failure
         * would be silent.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {number} The duration in microseconds.
         * @throws {LuminaAudioPlayer.PlayerError} When the codec does not implement it.
         */
        computePacketDurationMicros(data) {
            throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'This OGG demuxer does not implement computePacketDurationMicros.');
        }

        /**
         * Converts a granule position to a media time, in microseconds. A codec must implement it: the base throws,
         * because the raw granule is in the codec's own sample unit and is not a media time, so a default that returned it
         * unchanged would report a wrong timeline for the codec that forgot to override it.
         * @param {number} granule - The granule position.
         * @returns {number} The media time in microseconds.
         * @throws {LuminaAudioPlayer.PlayerError} When the codec does not implement it.
         */
        granuleToMicros(granule) {
            throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'This OGG demuxer does not implement granuleToMicros.');
        }

        /**
         * Records the offset of the first page that holds audio, once every codec header packet was consumed. It is what
         * keeps a seek from landing on a page of identification or comment headers and emitting them as audio.
         * @returns {void}
         */
        beginAudio() {
            this.audioStartOffset = this.pendingPackets.length > 0 ? this.lastPageOffset : this.pageOffset;
        }

        /**
         * Releases the queued packet state of the demuxer when it is destroyed, so a torn down stream does not keep the pages it had already read alive.
         * @returns {void}
         */
        onDestroy() {
            this.pendingPackets = [];
            this.pendingParts = [];
            this.pendingLength = 0;
        }

        /**
         * Checks whether a packet is a codec header packet rather than audio, so that a page that carries headers, which
         * a seek can land on, does not emit them. The default is that every packet is audio.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {boolean} True when the packet is a codec header.
         */
        isHeaderPacket(data) {
            return false;
        }

        /**
         * Measures the total duration, in microseconds, of a list of packets, without advancing any per packet state, so
         * that measuring a page never disturbs the state the packets are then emitted with. A codec must implement it
         * together with computePacketDurationMicros, because the base throws rather than defaulting to a loop that calls
         * computePacketDurationMicros directly: for a codec whose packet duration depends on the packet before it, that
         * loop mutates the state, so the first page after a seek would then be emitted with the durations the measurement
         * left behind rather than the ones it measured.
         * @param {Array<{ data: Uint8Array, granule: number|null }>} packets - The packets to measure.
         * @returns {number} The total duration in microseconds.
         * @throws {LuminaAudioPlayer.PlayerError} When the codec does not implement it.
         */
        measurePacketsDurationMicros(packets) {
            throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'This OGG demuxer does not implement measurePacketsDurationMicros.');
        }
    }

    /**
     * Computes the number of samples one Opus packet decodes to, from its table of contents byte.
     * @param {Uint8Array} data - The Opus packet.
     * @returns {number} The number of samples at 48 kHz.
     */
    function getOpusPacketSamples(data) {
        if (!data || data.length < 1)
            return 0;
        const toc = data[0];
        // The table of contents byte is a five bit configuration, which selects the frame duration, and a two bit code, which states how many frames the packet holds.
        const config = toc >> 3;
        const code = toc & 0x03;
        let frameCount;
        if (code === 0)
            frameCount = 1;
        else if (code === 1 || code === 2)
            frameCount = 2;
        else {
            if (data.length < 2)
                return 0;
            frameCount = data[1] & 0x3F;
            if (frameCount <= 0)
                return 0;
        }
        const frameMilliseconds = OPUS_FRAME_DURATION_MS[config];
        if (!Number.isFinite(frameMilliseconds))
            return 0;
        return Math.round(frameMilliseconds * OPUS_GRANULE_RATE / 1000) * frameCount;
    }

    /**
     * Builds the Xiph extradata of a Vorbis stream: the count of lacing values, the lacing values, and the three header packets, in order.
     * @param {Array<Uint8Array>} packets - The identification, comment and setup header packets.
     * @returns {Uint8Array|null} The extradata, or null when the headers are too large to be laced into a single byte count.
     */
    function buildXiphExtradata(packets) {
        const lacing = [];
        for (const packet of packets) {
            let length = packet.length;
            while (length >= 255) {
                lacing.push(255);
                length -= 255;
            }
            lacing.push(length);
        }
        if (lacing.length > 255)
            return null;
        const total = 1 + lacing.length;
        let payload = 0;
        for (const packet of packets)
            payload += packet.length;
        const result = new Uint8Array(total + payload);
        result[0] = lacing.length;
        for (let index = 0; index < lacing.length; index++)
            result[1 + index] = lacing[index];
        let offset = total;
        for (const packet of packets) {
            result.set(packet, offset);
            offset += packet.length;
        }
        return result;
    }

    /**
     * Demuxes an OGG Opus stream.
     */
    class OggOpusDemuxer extends OggDemuxer {
        /**
         * Creates an instance of OggOpusDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            /** @type {Uint8Array|null} */
            this.description = null;
            this.preSkip = 0;
            this.channels = 0;
        }

        /**
         * Reads the OpusHead and the OpusTags packets and positions the read cursor on the first audio packet.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not an OGG Opus stream.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                this.pageOffset = 0;
                this.pendingParts = [];
                this.pendingLength = 0;
                this.pendingPackets = [];
                this.currentSerial = null;
                const head = await this.readRawPacketAsync();
                if (!head || !LuminaAudioPlayer.demuxBytesEqualAscii(head.data, 0, 'OpusHead'))
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG stream is not an Opus stream.', { reason: 'no-opus-head' });
                if (head.data.length < 19)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OpusHead packet is too short.', { reason: 'invalid-opus-head' });
                this.channels = head.data[9];
                this.preSkip = LuminaAudioPlayer.demuxReadIntLE(head.data, 10, 2) || 0;
                if (this.channels <= 0)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OpusHead packet names no channels.', { reason: 'invalid-opus-head' });
                this.description = head.data.slice();
                // The mapping requires the OpusTags comment packet after the identification header; any further packet is audio.
                const tags = await this.readRawPacketAsync();
                if (tags && !LuminaAudioPlayer.demuxBytesEqualAscii(tags.data, 0, 'OpusTags'))
                    // A stream that skips its comment packet is malformed, but the packet is kept so it is not lost.
                    this.pendingPackets.unshift(tags);
                const lastGranule = await this.probeLastGranuleAsync();
                if (lastGranule !== null) {
                    this.lastGranule = lastGranule;
                    this.durationIsFinal = true;
                    this.refreshDuration();
                }
                this.beginAudio();
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
            if (!this.description)
                return null;
            return {
                codec: 'opus',
                sampleRate: OPUS_GRANULE_RATE,
                numberOfChannels: this.channels,
                description: this.description
            };
        }

        /**
         * Checks whether a packet is one of the Opus header packets, which are not audio.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {boolean} True when the packet is the identification or the comment header.
         */
        isHeaderPacket(data) {
            return LuminaAudioPlayer.demuxBytesEqualAscii(data, 0, 'OpusHead')
                || LuminaAudioPlayer.demuxBytesEqualAscii(data, 0, 'OpusTags');
        }

        /**
         * Reads the next Opus audio packet.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            try {
                if (this.seekResync)
                    await this.prepareAfterSeekAsync();
                return await this.readNextAudioPacketAsync();
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Gets the duration, in microseconds, of one Opus packet.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {number} The duration in microseconds.
         */
        computePacketDurationMicros(data) {
            return LuminaAudioPlayer.samplesToMicroseconds(getOpusPacketSamples(data), OPUS_GRANULE_RATE);
        }

        /**
         * Measures the total duration of a list of Opus packets. An Opus packet's duration is self contained, so summing
         * the per packet durations touches no state and is exact.
         * @param {Array<{ data: Uint8Array, granule: number|null }>} packets - The packets to measure.
         * @returns {number} The total duration in microseconds.
         */
        measurePacketsDurationMicros(packets) {
            let total = 0;
            for (const packet of packets) {
                if (this.isHeaderPacket(packet.data))
                    continue;
                total += this.computePacketDurationMicros(packet.data);
            }
            return total;
        }

        /**
         * Converts an Opus granule position to a media time, subtracting the pre-skip the decoder discards.
         * @param {number} granule - The granule position.
         * @returns {number} The media time in microseconds.
         */
        granuleToMicros(granule) {
            return LuminaAudioPlayer.samplesToMicroseconds(Math.max(0, granule - this.preSkip), OPUS_GRANULE_RATE);
        }
    }

    /**
     * Demuxes an OGG Vorbis stream.
     */
    class OggVorbisDemuxer extends OggDemuxer {
        /**
         * Creates an instance of OggVorbisDemuxer.
         * @param {Object} options - The demuxer options, per {@link LuminaAudioPlayer.Demuxer}.
         */
        constructor(options = {}) {
            super(options);
            /** @type {Uint8Array|null} */
            this.description = null;
            this.channels = 0;
            this.sampleRate = 0;
            this.blockSizes = [0, 0];
            this.lastBlockSize = null;
        }

        /**
         * Reads the identification, comment and setup header packets and positions the read cursor on the first audio packet.
         * @returns {Promise<void>} A promise resolving when the stream is ready to be read.
         * @throws {LuminaAudioPlayer.PlayerError} When the bytes are not an OGG Vorbis stream.
         */
        async initializeAsync() {
            if (this.isInitialized())
                return;
            if (this.destroyed)
                throw this.createError(LuminaAudioPlayer.PlayerErrorCode.INVALID_STATE, 'The demuxer was destroyed.');
            try {
                this.pageOffset = 0;
                this.pendingParts = [];
                this.pendingLength = 0;
                this.pendingPackets = [];
                this.currentSerial = null;
                const identification = await this.readRawPacketAsync();
                if (!identification || identification.data.length < 30
                    || identification.data[0] !== 0x01
                    || !LuminaAudioPlayer.demuxBytesEqualAscii(identification.data, 1, 'vorbis'))
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The OGG stream is not a Vorbis stream.', { reason: 'no-vorbis-identification' });
                this.channels = identification.data[11];
                this.sampleRate = LuminaAudioPlayer.demuxReadUintLE(identification.data, 12, 4) || 0;
                // The block size byte of the identification header holds the two base-two exponents of the short and the long block size, one per nibble, so the sizes themselves are powers of two.
                const blockSizeByte = identification.data[28];
                this.blockSizes = [1 << (blockSizeByte & 0x0F), 1 << ((blockSizeByte >> 4) & 0x0F)];
                if (this.channels <= 0 || this.sampleRate <= 0 || this.blockSizes[0] < 16 || this.blockSizes[1] < this.blockSizes[0])
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The Vorbis identification header is invalid.', { reason: 'invalid-vorbis-identification' });
                const comment = await this.readRawPacketAsync();
                const setup = await this.readRawPacketAsync();
                if (!comment || !setup)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The Vorbis stream is missing its header packets.', { reason: 'missing-vorbis-headers' });
                const description = buildXiphExtradata([identification.data, comment.data, setup.data]);
                if (!description)
                    throw this.createError(LuminaAudioPlayer.PlayerErrorCode.DEMUX_FAILED, 'The Vorbis header packets are too large for the codec private data format.', { reason: 'vorbis-headers-too-large' });
                this.description = description;
                const lastGranule = await this.probeLastGranuleAsync();
                if (lastGranule !== null) {
                    this.lastGranule = lastGranule;
                    this.durationIsFinal = true;
                    this.refreshDuration();
                }
                this.lastBlockSize = null;
                this.beginAudio();
                this.initialized = true;
            } catch (error) {
                throw this.recordFailure(error, 'initialize-failed');
            }
        }

        /**
         * Gets the decoder configuration of the stream, including the required Vorbis description.
         * @returns {Object|null} The configuration, or null before initialization.
         */
        getConfiguration() {
            if (!this.description)
                return null;
            return {
                codec: 'vorbis',
                sampleRate: this.sampleRate,
                numberOfChannels: this.channels,
                description: this.description
            };
        }

        /**
         * Checks whether a packet is one of the three Vorbis header packets, which are not audio.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {boolean} True when the packet is the identification, comment or setup header.
         */
        isHeaderPacket(data) {
            if (!data || data.length < 7)
                return false;
            const type = data[0];
            return (type === 0x01 || type === 0x03 || type === 0x05) && LuminaAudioPlayer.demuxBytesEqualAscii(data, 1, 'vorbis');
        }

        /**
         * Measures the total duration of a list of Vorbis packets without touching the block size state, so that the
         * durations the packets are then emitted with are exactly the ones they were measured with.
         * @param {Array<{ data: Uint8Array, granule: number|null }>} packets - The packets to measure.
         * @returns {number} The total duration in microseconds.
         */
        measurePacketsDurationMicros(packets) {
            let total = 0;
            let previous = this.lastBlockSize;
            for (const packet of packets) {
                if (this.isHeaderPacket(packet.data) || !packet.data || packet.data.length < 1)
                    continue;
                const blockSize = this.blockSizes[(packet.data[0] >> 7) & 0x01];
                const prior = previous === null ? blockSize : previous;
                total += LuminaAudioPlayer.samplesToMicroseconds(Math.floor((prior + blockSize) / 4), this.sampleRate);
                previous = blockSize;
            }
            return total;
        }

        /**
         * Reads the next Vorbis audio packet.
         * @returns {Promise<Object|null>} The packet, or null once the stream ended.
         * @throws {LuminaAudioPlayer.PlayerError} When the stream can no longer be parsed.
         */
        async readPacketAsync() {
            this.ensureInitialized();
            try {
                if (this.seekResync)
                    await this.prepareAfterSeekAsync();
                return await this.readNextAudioPacketAsync();
            } catch (error) {
                throw this.recordFailure(error, 'read-failed');
            }
        }

        /**
         * Gets the duration, in microseconds, of one Vorbis packet. The first bit of an audio packet selects one of the
         * two block sizes of the stream, and a packet produces the average of its own block size and the previous one,
         * which is the amount of audio the overlap of two windows covers.
         * @param {Uint8Array} data - The packet bytes.
         * @returns {number} The duration in microseconds.
         */
        computePacketDurationMicros(data) {
            if (!data || data.length < 1)
                return 0;
            const blockFlag = (data[0] >> 7) & 0x01;
            const blockSize = this.blockSizes[blockFlag];
            const previous = this.lastBlockSize === null ? blockSize : this.lastBlockSize;
            this.lastBlockSize = blockSize;
            const samples = Math.floor((previous + blockSize) / 4);
            return LuminaAudioPlayer.samplesToMicroseconds(samples, this.sampleRate);
        }

        /**
         * Converts a Vorbis granule position to a media time. A Vorbis granule counts decoded samples.
         * @param {number} granule - The granule position.
         * @returns {number} The media time in microseconds.
         */
        granuleToMicros(granule) {
            return LuminaAudioPlayer.samplesToMicroseconds(Math.max(0, granule), this.sampleRate);
        }
    }

    LuminaAudioPlayer.OggDemuxer = OggDemuxer;
    LuminaAudioPlayer.OggOpusDemuxer = OggOpusDemuxer;
    LuminaAudioPlayer.OggVorbisDemuxer = OggVorbisDemuxer;
    LuminaAudioPlayer.computeOggCrc = computeOggCrc;

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.OGG,
        codec: LuminaAudioPlayer.DemuxerCodec.OPUS,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.OPUS,
        create: (options) => new OggOpusDemuxer(options)
    });

    LuminaAudioPlayer.registerDemuxer({
        container: LuminaAudioPlayer.DemuxerContainer.OGG,
        codec: LuminaAudioPlayer.DemuxerCodec.VORBIS,
        probe: LuminaAudioPlayer.AUDIO_CODEC_PROBES.VORBIS,
        create: (options) => new OggVorbisDemuxer(options)
    });
})(window.LuminaAudioPlayer = window.LuminaAudioPlayer || {});
