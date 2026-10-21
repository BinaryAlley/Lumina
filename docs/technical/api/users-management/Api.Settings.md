# Lumina API

- [Lumina API](#lumina-api)
  - [Settings](#settings)
    - [Get User Settings](#get-user-settings)
      - [Get User Settings Request](#get-user-settings-request)
      - [Get User Settings Response](#get-user-settings-response)
    - [Update User Settings](#update-user-settings)
      - [Update User Settings Request](#update-user-settings-request)
      - [Update User Settings Response](#update-user-settings-response)
    - [Get Playback State](#get-playback-state)
      - [Get Playback State Request](#get-playback-state-request)
      - [Get Playback State Response](#get-playback-state-response)
    - [Update Playback State](#update-playback-state)
      - [Update Playback State Request](#update-playback-state-request)
      - [Update Playback State Response](#update-playback-state-response)

## Settings

### Get User Settings

#### Get User Settings Request

```js
GET api/v1/users/me/settings
```

Returns the settings of the currently authenticated user.

#### Get User Settings Response

```js
200 Ok
```

```json
{
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "isPaginationEnabled": true,
  "itemsPerPage": 50,
  "shouldIgnoreThePrefixForAlphaPicker": true,
  "isThemeCachingEnabled": true,
  "shouldAggregateMetadataWhenMissing": true,
  "shouldRenderPdfAsImages": false,
  "shouldPreserveBookStyles": true,
  "shouldKeepDownloadingWhilePaused": true,
  "shouldAutoplay": true,
  "isGaplessPlaybackEnabled": true,
  "replayGainMode": "Off",
  "streamingQuality": "Auto",
  "vuMeterOrientation": "Vertical",
  "vuMeterSegmentCount": 12,
  "vuMeterSegmentGap": 1,
  "vuMeterMediumThreshold": 0.6,
  "vuMeterHighThreshold": 0.8,
  "vuMeterShowPeak": true,
  "vuMeterPeakHoldDurationInMilliseconds": 650,
  "vuMeterReleasePerSecond": 3,
  "vuMeterPeakReleasePerSecond": 0.65
}
```

### Update User Settings

#### Update User Settings Request

```js
PUT api/v1/users/me/settings
```

Updates the settings of the currently authenticated user.

The VU meter settings configure the segmented meter of the player bar: `vuMeterOrientation` is `Vertical` or `Horizontal`, `vuMeterSegmentCount` is between 1 and 128, `vuMeterSegmentGap` is between 0 and 32 pixels, `vuMeterMediumThreshold` and `vuMeterHighThreshold` are between 0 and 1 and the high threshold must not be lower than the medium one, `vuMeterPeakHoldDurationInMilliseconds` is between 0 and 10000, and `vuMeterReleasePerSecond` and `vuMeterPeakReleasePerSecond` are between 0 and 100 level units per second. All of them are required.

```json
{
  "isPaginationEnabled": true,
  "itemsPerPage": 50,
  "shouldIgnoreThePrefixForAlphaPicker": true,
  "isThemeCachingEnabled": true,
  "shouldAggregateMetadataWhenMissing": true,
  "shouldRenderPdfAsImages": false,
  "shouldPreserveBookStyles": true,
  "shouldKeepDownloadingWhilePaused": true,
  "shouldAutoplay": true,
  "isGaplessPlaybackEnabled": true,
  "replayGainMode": "Off",
  "streamingQuality": "Auto",
  "vuMeterOrientation": "Vertical",
  "vuMeterSegmentCount": 12,
  "vuMeterSegmentGap": 1,
  "vuMeterMediumThreshold": 0.6,
  "vuMeterHighThreshold": 0.8,
  "vuMeterShowPeak": true,
  "vuMeterPeakHoldDurationInMilliseconds": 650,
  "vuMeterReleasePerSecond": 3,
  "vuMeterPeakReleasePerSecond": 0.65
}
```

#### Update User Settings Response

```js
200 Ok
```

### Get Playback State

#### Get Playback State Request

```js
GET api/v1/users/me/settings/playback-state
```

Returns the playback state (the last volume and mute state) the player reported for the currently authenticated user. The values are written automatically by the player and are never surfaced as settings fields.

#### Get Playback State Response

```js
200 Ok
```

```json
{
  "volume": 0.75,
  "isMuted": false
}
```

### Update Playback State

#### Update Playback State Request

```js
PUT api/v1/users/me/settings/playback-state
```

Updates the playback state the player reported for the currently authenticated user. Both fields are required: the volume is a value between 0 and 1, and the mute state is a boolean. A request that omits either field, or sends a volume outside that range, is rejected with a `422 Unprocessable Entity`.

```json
{
  "volume": 0.75,
  "isMuted": false
}
```

#### Update Playback State Response

```js
200 Ok
```
