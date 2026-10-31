# Architecture Decision Record (ADR): 0016 - Playback Delivery Decision and Player Capability Negotiation

## Status
**Accepted** (2026-10-14)

## Context

Lumina's music player is implemented from scratch, on WebCodecs for decoding and the Web Audio API for output, with no `HTMLMediaElement` and no Media Source Extensions. That player can only demux a curated set of containers and decode a curated set of codecs, so the server has to tell it, before it fetches any audio, whether a track can be played as stored or whether the server has to change the container or the codec first.

The server already has each track's stored probe result (the container and the codec, plus the sample rate, the channels, the bit depth, the duration and the ReplayGain values) and a stream endpoint with HTTP range support that serves the original file. What is missing is the decision itself and a way for the player to state what it can play.

## Decision

Introduce a playback format service (`IPlaybackFormatService`, `PlaybackFormatService`) that, given the container and the codec of a track as read from its stored probe result and the capabilities a player advertises, decides one of three delivery modes: direct play, remux, or transcode. It stays in the Application layer (interface) and the Infrastructure layer (implementation), as a service: the byte and codec pipeline is not part of the playback domain aggregate.

### Delivery modes

| Mode | Condition | Delivery format | Delivery shape |
|---|---|---|---|
| Direct play | The player can demux the stored container and decode the stored codec | The stored container and codec, unchanged | Byte range seekable |
| Remux | The player can decode the stored codec but has no demuxer for the stored container | The stored codec, repackaged into a server container the player can demux | Time addressed segments |
| Transcode | The player cannot decode the stored codec | The most preferred codec the server can produce that the player advertises | Time addressed segments |

- The stored codec is the free form description reported by the probe (for example `MPEG-1 Layer 3`), so it is canonicalized to the codec name players advertise (for example `MP3`) before it is compared. The MPEG layer is matched by its layer number only (`LAYER 3`/`LAYER III`), so `MPEG-1 Layer 1` and `MPEG-1 Layer 2` are not mistaken for `MP3`, and a description that names no recognizable codec is left as it is, which keeps it from matching any advertised codec and forces a transcode. Containers are canonicalized by casing.
- The container a normalized delivery is produced in is always chosen from the containers the server can produce for the codec, never from the container the player names: the player only selects among the server's known containers, so a container supplied by a client never reaches the transcoder or a cache name. The containers the server produces each codec in are, in order of preference: `PCM` in `WAV`, `MP3` in `MP3`, `FLAC` in `FLAC`, `Opus` and `Vorbis` in `OGG`, `AAC` in `MP4` then `AAC`.
- The server can produce, in order of preference, `AAC`, `OPUS`, `MP3`, `VORBIS`, `FLAC` and `PCM` (the efficient lossy codecs before the lossless ones, and uncompressed last), so a transcode re-encodes to the first of these the player advertises in one of its containers. When the player advertises no producible codec, the decision fails with `NoSupportedPlaybackFormat`, which is a forbidden outcome (HTTP 403) rather than a malformed request, and no delivery is attempted.

### Capability input convention

The capabilities a player can demux and decode directly are carried on the playback manifest request as a query parameter named `capabilities`, whose value is a comma separated list of `container/codec` pairs:

```
GET /api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest?capabilities=mp3/mp3,flac/flac,ogg/opus,ogg/vorbis,wav/pcm,mp4/aac
```

- The container and the codec of a pair are matched case insensitively, and surrounding whitespace is ignored.
- A pair must name both a container and a codec, with neither empty (for example `flac`, `a/b/c` and `a//b` are ignored).
- At most 32 pairs are read from one request, so the parsing work stays bounded regardless of the input length.
- When the parameter is absent, empty, or lists no valid pair, the server applies the default capabilities of the bundled player, so a request that advertises nothing still gets a playable decision:
  `mp3/mp3`, `flac/flac`, `ogg/opus`, `ogg/vorbis`, `wav/pcm`, `aac/aac`. The default set is the bundled player's demuxer set (MP3, AAC/ADTS, FLAC, OGG and WAV), so it deliberately contains no `mp4/aac` pair: a track stored in MP4 is remuxed to raw AAC for a request that advertises nothing, rather than being marked direct play for a container the bundled player cannot demux.
- The convention is a query parameter rather than a request header, so that it is part of the request's own contract, is trivially testable, and is forwarded by the web app's manifest proxy without depending on header forwarding.

### Delivery shape

The `TimeAddressedSegments` delivery shape is a sequence of approximately fixed duration segments addressed and seeked by time, so that finding a time never requires mapping a byte range to it. The nominal segment duration is 6 seconds, and it is carried in the delivery decision so that the producer of the segments and the player agree on it. A segment is cut at the first packet at or after its nominal boundary, so its real start is not always an exact multiple of the nominal duration; the producer records the real start in its index, and the segment route reports it in a response header. Direct play is `ByteRangeSeekable`, because the original file is served by the existing range-capable stream endpoint.

### Segment addressing

The segments are exposed through a dedicated segment route, `.../stream/deliveries/{deliveryId}/segments/{segmentIndex}`, which extends the track stream endpoint's contract. The playback manifest carries its relative path in `segmentStreamPathTemplate`, with `{deliveryId}` and `{segmentIndex}` placeholders, and the web application rewrites that path to its own segment proxy so that the browser never reaches the API server. The manifest also reports the opaque `deliveryId` the placeholders are expanded with.

- An index addresses a whole segment, so no byte range is ever sent for a normalized delivery.
- The delivery handle the manifest issued is the only authority on the delivery of the segments. It is a random, opaque value bound to the user and the track, and it captures the delivery decision, the quality cap and the source file version the manifest declared. The segment route resolves it instead of recomputing the decision from a capability list, rereading the user's quality setting, or restating a file that may have changed. A changed file is rejected as a conflict rather than starting a second production whose segments would be mixed with the declared one.
- Library ownership is still evaluated on every segment request, so revoked access stops playback immediately. The track membership and the library content locations are the ones captured when the manifest was issued, so deleting a track row while its file remains, or removing a content location, can keep that media streamable until the handle expires. That is the deliberate trade-off for not reloading the track and library graph for every six second segment; the handle lifetime is bounded to 12 hours.
- The handles are process local, like the normalized audio cache they address. They expire after a bounded lifetime, are bounded globally and per user, and a repeated manifest for the same delivery inputs reuses the existing handle, so the registry cannot grow without bound. Expired handles are removed in issue order from a queue, so the sweep is amortized constant time rather than a scan of every handle.
- A production is atomic: the first segment request may wait for the whole normalized track to be produced and cached, and the following segment requests are served from that cache.
- The player keeps a bounded lookahead of one segment ahead of the segment the demuxer consumed, so the link is not idle between segments and the streamed indicator can run ahead. The lookahead stops while playback is paused unless the user enabled the keep-downloading-while-paused preference, and it never advances itself, so it cannot turn into a full download of the track.
- Gapless prefetch starts only near the end of the current track, not at load time. A normalized next track therefore transitions without a gap only when its production is already cached or finishes within that lead; otherwise it starts normally at the boundary. The lead is deliberately shorter than a long production, because holding a normalization slot for a track the user may never reach is worse than a small gap.
- The player keeps every segment it has fetched for the lifetime of the track, so seeking back into a fetched segment does not fetch it again. A seek reads the target segment from its first packet and drops the packets that end before the target, instead of seeking inside the segment: a container that numbers its frames across the whole track would otherwise add that number to the segment offset twice. A segment is short, so reading it from its start is exact for every delivered container.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Decoupling | The player states what it can play and never has to know the server's codecs; the server states the delivery and never has to know the player's implementation |
| Efficiency | A container a player cannot demux but a codec it can decode is repackaged instead of re-encoded, and only a genuinely unsupported codec is re-encoded |
| Determinism | The containers the server produces each codec in and the transcode preference order are fixed, so the same input always yields the same delivery |
| Seekability | Time addressed segments make a normalized delivery seekable without a byte to time index |
| Testability | The decision is a pure function of the stored container and codec and the advertised capabilities |

### Risks and Tradeoffs
| Risk | Mitigation Strategy |
|---|---|
| The probe reports a codec as a free form description that does not name the codec the player advertises | The stored description is canonicalized to the advertised codec names before comparison, and the MPEG layer is matched by its layer number, so a description that names no decodable codec is left unmatched and forces a transcode |
| A player advertises a container the server cannot produce | The delivered container is always chosen from the containers the server produces the codec in, never from the container the player names, so a client supplied container never reaches the transcoder or a cache name; a codec the player cannot be served in any server container is not used |
| The real segment start drifts from the nominal grid | The nominal duration is carried by the manifest, and the exact indexed start of each segment is reported in `X-Segment-Start-Seconds` and used by the player, so neither side assumes an exact grid |

## Alternatives Considered

### 1. Decide the delivery from the container only

**Rejected**: a container can carry several codecs (for example `OGG` carries both Vorbis and Opus), so the container alone cannot tell whether the player can decode the audio, and the difference between a remux and a transcode would be lost.

### 2. Let the player decide, by sending its full codec support to the server and reading the probe result itself

**Rejected**: it leaks the server's storage and probe details to the client and duplicates the decision on both sides. The server owns the probe result and the producible formats.

### 3. Carry the capabilities in a request header

**Rejected**: a header has to be explicitly forwarded by the web app's manifest proxy and is harder to exercise from a browser or a request file than a query parameter, with no benefit that a query parameter does not already provide.

### 4. Deliver a remuxed track as a single byte range seekable file instead of segments

**Rejected**: it makes remuxed and transcoded tracks two different delivery shapes for the player to consume, and it reintroduces the byte to time mapping the time addressed shape exists to avoid. Both normalized deliveries use the same shape.
