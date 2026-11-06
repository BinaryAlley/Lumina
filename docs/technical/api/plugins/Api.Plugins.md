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
    - [Get Library Artwork Providers](#get-library-artwork-providers)
      - [Get Library Artwork Providers Request](#get-library-artwork-providers-request)
      - [Get Library Artwork Providers Response](#get-library-artwork-providers-response)
    - [Get Library Book Readers](#get-library-book-readers)
      - [Get Library Book Readers Request](#get-library-book-readers-request)
      - [Get Library Book Readers Response](#get-library-book-readers-response)
    - [Get Library Type Metadata Providers](#get-library-type-metadata-providers)
      - [Get Library Type Metadata Providers Request](#get-library-type-metadata-providers-request)
      - [Get Library Type Metadata Providers Response](#get-library-type-metadata-providers-response)
    - [Get Library Type Artwork Providers](#get-library-type-artwork-providers)
      - [Get Library Type Artwork Providers Request](#get-library-type-artwork-providers-request)
      - [Get Library Type Artwork Providers Response](#get-library-type-artwork-providers-response)
    - [Get Library Type Book Readers](#get-library-type-book-readers)
      - [Get Library Type Book Readers Request](#get-library-type-book-readers-request)
      - [Get Library Type Book Readers Response](#get-library-type-book-readers-response)
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

Returns the metadata providers available for the media library, one entry per plugin that provides metadata, along with whether the provider is enabled for the media library and its rank, which determines the order in which the providers are tried. The enabled state and the rank are changed by sending the `metadataProviders` of the [Add Library](../library/management/Api.Libraries.md#add-library) or [Update Library](../library/management/Api.Libraries.md#update-library) request.

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

### Get Library Artwork Providers

#### Get Library Artwork Providers Request

```js
GET api/v1/libraries/{libraryId}/artwork-providers
```

Returns the artwork providers available for the media library, one entry per plugin that provides artwork, along with whether the provider is enabled for the media library and its rank, which determines the order in which the providers are tried when resolving the artwork. The enabled state and the rank are changed by sending the `artworkProviders` of the [Add Library](../library/management/Api.Libraries.md#add-library) or [Update Library](../library/management/Api.Libraries.md#update-library) request.

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

### Get Library Book Readers

#### Get Library Book Readers Request

```js
GET api/v1/libraries/{libraryId}/book-readers
```

Returns the book readers available for the media library, one entry per plugin that provides a book reader, along with whether the reader is enabled for the media library. A book can be opened for reading only through a book reader that is enabled for its media library. The enabled state is changed by sending the `bookReaders` of the [Add Library](../library/management/Api.Libraries.md#add-library) or [Update Library](../library/management/Api.Libraries.md#update-library) request.

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

### Get Library Type Metadata Providers

#### Get Library Type Metadata Providers Request

```js
GET api/v1/libraries/metadata-providers/{libraryType}
```

Returns the metadata providers available for a media library type, without a media library, so that the providers of a library can be chosen before it is created or before a change of its type is saved. The providers are returned the way a new media library of the type starts with them: disabled, and ranked in alphabetical order.

#### Get Library Type Metadata Providers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
    "name": "MusicBrainz Metadata",
    "isEnabled": false,
    "rank": 1
  }
]
```

### Get Library Type Artwork Providers

#### Get Library Type Artwork Providers Request

```js
GET api/v1/libraries/artwork-providers/{libraryType}
```

Returns the artwork providers available for a media library type, without a media library, disabled and ranked in alphabetical order.

#### Get Library Type Artwork Providers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14",
    "name": "Cover Art Archive",
    "isEnabled": false,
    "rank": 1
  },
  {
    "pluginId": "c9f1a7d3-2b64-4e18-8f05-7a3d9c1b6e42",
    "name": "Local Music Artwork",
    "isEnabled": false,
    "rank": 2
  }
]
```

### Get Library Type Book Readers

#### Get Library Type Book Readers Request

```js
GET api/v1/libraries/book-readers/{libraryType}
```

Returns the book readers available for a media library type, without a media library, with their supported file extensions, disabled and in alphabetical order.

#### Get Library Type Book Readers Response

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
    "isEnabled": false
  }
]
```

## Music Plugins

The music media library type is served by four plugins, which register their capabilities through `IPluginServiceRegistrator`. A single plugin can expose more than one capability, and the capabilities are enabled and ranked per media library by sending the provider lists of the [Add Library](../library/management/Api.Libraries.md#add-library) or [Update Library](../library/management/Api.Libraries.md#update-library) request.

| Plugin | Capability | Web access | Description |
| --- | --- | --- | --- |
| `MusicBrainz Metadata` | Metadata provider (artist, album, track) | Yes | Retrieves artist, album and track metadata from MusicBrainz. |
| `ID3 Metadata` | Metadata provider (artist, album, track) | No | Populates artist, album and track metadata from the embedded tags of the audio files. |
| `Local Music Artwork` | Artwork provider (artist, album) | No | Reads the artwork of the artists and the albums from the images stored in the folders of a local music library, and can extract the embedded album cover from the audio files. |
| `Cover Art Archive` | Artwork provider (album) | Yes | Retrieves the cover, back, booklet, medium and other artwork of music albums from the Cover Art Archive. |

The metadata providers are resolved during the metadata enrichment phase of a scan, and the artwork providers during the artwork enrichment phase, in the order given by their rank.
