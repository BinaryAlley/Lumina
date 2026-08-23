# Lumina API

- [Lumina API](#lumina-api)
  - [Themes](#themes)
    - [Get Themes](#get-themes)
      - [Get Themes Request](#get-themes-request)
      - [Get Themes Response](#get-themes-response)
    - [Install Theme](#install-theme)
      - [Install Theme Request](#install-theme-request)
      - [Install Theme Response](#install-theme-response)
    - [Set Current Theme](#set-current-theme)
      - [Set Current Theme Request](#set-current-theme-request)
      - [Set Current Theme Response](#set-current-theme-response)
    - [Get Current Theme](#get-current-theme)
      - [Get Current Theme Request](#get-current-theme-request)
      - [Get Current Theme Response](#get-current-theme-response)
    - [Get Theme Settings](#get-theme-settings)
      - [Get Theme Settings Request](#get-theme-settings-request)
      - [Get Theme Settings Response](#get-theme-settings-response)
    - [Get Theme Template](#get-theme-template)
      - [Get Theme Template Request](#get-theme-template-request)
      - [Get Theme Template Response](#get-theme-template-response)
    - [Get Theme Asset](#get-theme-asset)
      - [Get Theme Asset Request](#get-theme-asset-request)
      - [Get Theme Asset Response](#get-theme-asset-response)
    - [Get Theme Archive](#get-theme-archive)
      - [Get Theme Archive Request](#get-theme-archive-request)
      - [Get Theme Archive Response](#get-theme-archive-response)
    - [Delete Theme](#delete-theme)
      - [Delete Theme Request](#delete-theme-request)
      - [Delete Theme Response](#delete-theme-response)
    - [Restore Theme](#restore-theme)
      - [Restore Theme Request](#restore-theme-request)
      - [Restore Theme Response](#restore-theme-response)

## Themes

### Get Themes

#### Get Themes Request

```js
GET api/v1/themes
```

#### Get Themes Response

```js
200 Ok
```

```json
[
  {
    "id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
    "themeId": "default-dark",
    "name": "Default Dark",
    "description": "A dark theme bundled with the application.",
    "author": "Lumina",
    "version": "1.0.0",
    "previewPath": "preview.png",
    "installSource": "Bundled",
    "isCurrent": true,
    "installedAtUtc": "2025-01-01T12:00:00.0000000Z",
    "isDeleted": false
  }
]
```

### Install Theme

#### Install Theme Request

```js
POST api/v1/themes
```

Uploads the theme pack as a ZIP file in a `multipart/form-data` request. The first file part of the form is used as the theme archive.

#### Install Theme Response

```js
200 Ok
```

```json
{
  "id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "themeId": "my-theme",
  "name": "My Theme",
  "description": "A theme uploaded by a user.",
  "author": "JohnDoe",
  "version": "1.0.0",
  "previewPath": null,
  "installSource": "Uploaded",
  "isCurrent": null,
  "installedAtUtc": "2025-01-01T12:00:00.0000000Z",
  "isDeleted": false
}
```

### Set Current Theme

#### Set Current Theme Request

```js
PUT api/v1/themes/current
```

```json
{
  "themeId": "my-theme"
}
```

#### Set Current Theme Response

```js
200 Ok
```

```json
{
  "id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "themeId": "my-theme",
  "name": "My Theme",
  "description": "A theme uploaded by a user.",
  "author": "JohnDoe",
  "version": "1.0.0",
  "previewPath": null,
  "installSource": "Uploaded",
  "isCurrent": true,
  "installedAtUtc": "2025-01-01T12:00:00.0000000Z",
  "isDeleted": false
}
```

### Get Current Theme

#### Get Current Theme Request

```js
GET api/v1/themes/current
```

#### Get Current Theme Response

```js
200 Ok
```

```json
{
  "id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "themeId": "my-theme",
  "name": "My Theme",
  "description": "A theme uploaded by a user.",
  "author": "JohnDoe",
  "version": "1.0.0",
  "previewPath": null,
  "installSource": "Uploaded",
  "isCurrent": true,
  "installedAtUtc": "2025-01-01T12:00:00.0000000Z",
  "isDeleted": false
}
```

### Get Theme Settings

#### Get Theme Settings Request

```js
GET api/v1/themes/settings
```

#### Get Theme Settings Response

```js
200 Ok
```

```json
{
  "maxArchiveBytes": 52428800,
  "defaultThemeId": "default-dark"
}
```

### Get Theme Template

#### Get Theme Template Request

```js
GET api/v1/themes/{themeId}/templates/{pageKey}
```

#### Get Theme Template Response

```js
200 Ok
```

```json
{
  "theme": {
    "id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
    "themeId": "my-theme",
    "name": "My Theme",
    "description": "A theme uploaded by a user.",
    "author": "JohnDoe",
    "version": "1.0.0",
    "previewPath": null,
    "installSource": "Uploaded",
    "isCurrent": true,
    "installedAtUtc": "2025-01-01T12:00:00.0000000Z",
    "isDeleted": false
  },
  "template": "<!DOCTYPE html>\n<html>\n...\n</html>"
}
```

### Get Theme Asset

#### Get Theme Asset Request

```js
GET api/v1/themes/{themeId}/assets/{assetPath}
```

#### Get Theme Asset Response

```js
200 Ok
```

Returns the asset file (e.g. a stylesheet, script or image) stored in the theme pack.

### Get Theme Archive

#### Get Theme Archive Request

```js
GET api/v1/themes/{themeId}/archive
```

#### Get Theme Archive Response

```js
200 Ok
```

Returns the theme pack as a downloadable ZIP archive.

### Delete Theme

#### Delete Theme Request

```js
DELETE api/v1/themes/{themeId}
```

#### Delete Theme Response

```js
204 No Content
```

### Restore Theme

#### Restore Theme Request

```js
POST api/v1/themes/{themeId}/restore
```

#### Restore Theme Response

```js
204 No Content
```
