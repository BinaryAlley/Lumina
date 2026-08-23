# Lumina API

- [Lumina API](#lumina-api)
  - [Plugins](#plugins)
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

## Plugins

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
    "id": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
    "name": "OpenLibrary",
    "author": "Binary Alley",
    "version": "1.0.0",
    "description": "Downloads book metadata from Open Library.",
    "loadStatus": "Loaded",
    "loadError": null,
    "settings": {
      "language": "en"
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

```json
{
  "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
  "schema": [
    {
      "key": "language",
      "label": "Language",
      "type": "Text",
      "defaultValue": "en",
      "allowedValues": null
    },
    {
      "key": "maxResults",
      "label": "Maximum results",
      "type": "Number",
      "defaultValue": "10",
      "allowedValues": null
    }
  ],
  "settings": {
    "language": "en",
    "maxResults": "10"
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
  "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
  "settings": {
    "language": "en",
    "maxResults": "25"
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

#### Get Library Metadata Providers Response

```js
200 Ok
```

```json
[
  {
    "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
    "name": "OpenLibrary",
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
  "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
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
    "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
    "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"
  ]
}
```

#### Reorder Library Metadata Providers Response

```js
200 Ok
```
