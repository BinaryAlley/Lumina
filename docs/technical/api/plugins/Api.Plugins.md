# Lumina API

- [Lumina API](#lumina-api)
  - [Plugins](#plugins)
    - [Install Plugin](#install-plugin)
      - [Install Plugin Request](#install-plugin-request)
      - [Install Plugin Response](#install-plugin-response)
    - [Get Plugins](#get-plugins)
      - [Get Plugins Request](#get-plugins-request)
      - [Get Plugins Response](#get-plugins-response)
    - [Get Plugin Settings](#get-plugin-settings)
      - [Get Plugin Settings Request](#get-plugin-settings-request)
      - [Get Plugin Settings Response](#get-plugin-settings-response)
    - [Update Plugin Settings](#update-plugin-settings)
      - [Update Plugin Settings Request](#update-plugin-settings-request)
      - [Update Plugin Settings Response](#update-plugin-settings-response)
    - [Get Library Metadata Providers](#get-library-metadata-providers)
      - [Get Library Metadata Providers Request](#get-library-metadata-providers-request)
      - [Get Library Metadata Providers Response](#get-library-metadata-providers-response)
    - [Set Library Metadata Provider Enabled](#set-library-metadata-provider-enabled)
      - [Set Library Metadata Provider Enabled Request](#set-library-metadata-provider-enabled-request)
      - [Set Library Metadata Provider Enabled Response](#set-library-metadata-provider-enabled-response)
    - [Reorder Library Metadata Providers](#reorder-library-metadata-providers)
      - [Reorder Library Metadata Providers Request](#reorder-library-metadata-providers-request)
      - [Reorder Library Metadata Providers Response](#reorder-library-metadata-providers-response)
    - [Get Library Artwork Providers](#get-library-artwork-providers)
      - [Get Library Artwork Providers Request](#get-library-artwork-providers-request)
      - [Get Library Artwork Providers Response](#get-library-artwork-providers-response)
    - [Set Library Artwork Provider Enabled](#set-library-artwork-provider-enabled)
      - [Set Library Artwork Provider Enabled Request](#set-library-artwork-provider-enabled-request)
      - [Set Library Artwork Provider Enabled Response](#set-library-artwork-provider-enabled-response)
    - [Reorder Library Artwork Providers](#reorder-library-artwork-providers)
      - [Reorder Library Artwork Providers Request](#reorder-library-artwork-providers-request)
      - [Reorder Library Artwork Providers Response](#reorder-library-artwork-providers-response)
    - [Get Library Book Readers](#get-library-book-readers)
      - [Get Library Book Readers Request](#get-library-book-readers-request)
      - [Get Library Book Readers Response](#get-library-book-readers-response)
    - [Set Library Book Reader Enabled](#set-library-book-reader-enabled)
      - [Set Library Book Reader Enabled Request](#set-library-book-reader-enabled-request)
      - [Set Library Book Reader Enabled Response](#set-library-book-reader-enabled-response)
  - [Music Plugins](#music-plugins)

## Plugins

### Install Plugin

#### Install Plugin Request

```js
POST api/v1/plugins
```

Uploads the plugin as a `multipart/form-data` request. The first file part of the form is used as the plugin archive, which can be either a single `.dll` assembly or a `.zip` archive containing the plugin assembly and its dependencies. The assemblies are placed into the plugin storage directory of the API and are loaded at the next API startup.

#### Install Plugin Response

```js
200 Ok
```

### Get Plugins

#### Get Plugins Request

```js
GET api/v1/plugins
```

#### Get Plugins Response

```js
200 Ok
```

```json
[
  {
    "id": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
    "name": "MusicBrainz Metadata",
    "author": "Lumina",
    "version": "1.0.0",
    "description": "Retrieves artist, album and track metadata from MusicBrainz.",
    "loadStatus": "Loaded",
    "loadError": null,
    "settings": {
      "BaseUrl": "https://musicbrainz.org/ws/2/",
      "DoesAllowPrivateBaseUrl": "false",
      "ContactEmail": "",
      "SearchResultLimit": "10",
      "ReleaseLookupLimit": "25",
      "MinimumRequestIntervalSeconds": "1.0"
    }
  }
]
```

### Get Plugin Settings

#### Get Plugin Settings Request

```js
GET api/v1/plugins/{pluginId}/settings
```

#### Get Plugin Settings Response

```js
200 Ok
```

The `type` of a setting descriptor is one of `Text`, `Number`, `Boolean` or `Select`. A `Select` setting also carries the list of `allowedValues` it can take.

```json
{
  "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
  "schema": [
    {
      "key": "BaseUrl",
      "label": "Base URL",
      "type": "Text",
      "defaultValue": "https://musicbrainz.org/ws/2/",
      "allowedValues": null
    },
    {
      "key": "DoesAllowPrivateBaseUrl",
      "label": "Allow LAN/Private Base URL",
      "type": "Boolean",
      "defaultValue": "false",
      "allowedValues": null
    },
    {
      "key": "ContactEmail",
      "label": "Contact Email",
      "type": "Text",
      "defaultValue": null,
      "allowedValues": null
    },
    {
      "key": "SearchResultLimit",
      "label": "Search Result Limit",
      "type": "Number",
      "defaultValue": "10",
      "allowedValues": null
    },
    {
      "key": "ReleaseLookupLimit",
      "label": "Release Lookup Limit",
      "type": "Number",
      "defaultValue": "25",
      "allowedValues": null
    },
    {
      "key": "MinimumRequestIntervalSeconds",
      "label": "Minimum Request Interval (seconds)",
      "type": "Number",
      "defaultValue": "1.0",
      "allowedValues": null
    }
  ],
  "settings": {
    "BaseUrl": "https://musicbrainz.org/ws/2/",
    "DoesAllowPrivateBaseUrl": "false",
    "ContactEmail": "",
    "SearchResultLimit": "10",
    "ReleaseLookupLimit": "25",
    "MinimumRequestIntervalSeconds": "1.0"
  }
}
```

### Update Plugin Settings

#### Update Plugin Settings Request

```js
PUT api/v1/plugins/{pluginId}/settings
```

```json
{
  "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
  "settings": {
    "BaseUrl": "https://musicbrainz.org/ws/2/",
    "SearchResultLimit": "25"
  }
}
```

#### Update Plugin Settings Response

```js
200 Ok
```

### Get Library Metadata Providers

#### Get Library Metadata Providers Request

```js
GET api/v1/libraries/{libraryId}/metadata-providers
```

Returns the metadata providers available for the media library, one entry per plugin that provides metadata, along with whether the provider is enabled for the media library and its rank, which determines the order in which the providers are tried.

#### Get Library Metadata Providers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
    "name": "MusicBrainz Metadata",
    "isEnabled": true,
    "rank": 1
  }
]
```

### Set Library Metadata Provider Enabled

#### Set Library Metadata Provider Enabled Request

```js
PUT api/v1/libraries/{libraryId}/metadata-providers/{pluginId}/enabled
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
  "isEnabled": true
}
```

#### Set Library Metadata Provider Enabled Response

```js
200 Ok
```

### Reorder Library Metadata Providers

#### Reorder Library Metadata Providers Request

```js
PUT api/v1/libraries/{libraryId}/metadata-providers/reorder
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "pluginIds": [
    "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
    "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"
  ]
}
```

#### Reorder Library Metadata Providers Response

```js
200 Ok
```

### Get Library Artwork Providers

#### Get Library Artwork Providers Request

```js
GET api/v1/libraries/{libraryId}/artwork-providers
```

Returns the artwork providers available for the media library, one entry per plugin that provides artwork, along with whether the provider is enabled for the media library and its rank, which determines the order in which the providers are tried when resolving the artwork.

#### Get Library Artwork Providers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "c9f1a7d3-2b64-4e18-8f05-7a3d9c1b6e42",
    "name": "Local Music Artwork",
    "isEnabled": true,
    "rank": 1
  },
  {
    "pluginId": "b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14",
    "name": "Cover Art Archive",
    "isEnabled": true,
    "rank": 2
  }
]
```

### Set Library Artwork Provider Enabled

#### Set Library Artwork Provider Enabled Request

```js
PUT api/v1/libraries/{libraryId}/artwork-providers/{pluginId}/enabled
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "pluginId": "b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14",
  "isEnabled": true
}
```

#### Set Library Artwork Provider Enabled Response

```js
200 Ok
```

### Reorder Library Artwork Providers

#### Reorder Library Artwork Providers Request

```js
PUT api/v1/libraries/{libraryId}/artwork-providers/reorder
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "pluginIds": [
    "c9f1a7d3-2b64-4e18-8f05-7a3d9c1b6e42",
    "b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14"
  ]
}
```

#### Reorder Library Artwork Providers Response

```js
200 Ok
```

### Get Library Book Readers

#### Get Library Book Readers Request

```js
GET api/v1/libraries/{libraryId}/book-readers
```

Returns the book readers available for the media library, one entry per plugin that provides a book reader, along with whether the reader is enabled for the media library. A book can be opened for reading only through a book reader that is enabled for its media library.

#### Get Library Book Readers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
    "name": "EPUB Reader",
    "supportedExtensions": [
      ".epub"
    ],
    "isEnabled": true
  },
  {
    "pluginId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
    "name": "PDF Reader",
    "supportedExtensions": [
      ".pdf"
    ],
    "isEnabled": false
  }
]
```

### Set Library Book Reader Enabled

#### Set Library Book Reader Enabled Request

```js
PUT api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
  "isEnabled": true
}
```

#### Set Library Book Reader Enabled Response

```js
200 Ok
```

## Music Plugins

The music media library type is served by four plugins, which register their capabilities through `IPluginServiceRegistrator`. A single plugin can expose more than one capability, and the capabilities are enabled and ranked per media library through the endpoints above.

| Plugin | Capability | Web access | Description |
| --- | --- | --- | --- |
| `MusicBrainz Metadata` | Metadata provider (artist, album, track) | Yes | Retrieves artist, album and track metadata from MusicBrainz. |
| `ID3 Metadata` | Metadata provider (artist, album, track) | No | Populates artist, album and track metadata from the embedded tags of the audio files. |
| `Local Music Artwork` | Artwork provider (artist, album) | No | Reads the artwork of the artists and the albums from the images stored in the folders of a local music library, and can extract the embedded album cover from the audio files. |
| `Cover Art Archive` | Artwork provider (album) | Yes | Retrieves the cover, back, booklet, medium and other artwork of music albums from the Cover Art Archive. |

The metadata providers are resolved during the metadata enrichment phase of a scan, and the artwork providers during the artwork enrichment phase, in the order given by their rank.
