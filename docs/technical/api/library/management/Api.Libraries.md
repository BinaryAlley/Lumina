# Lumina API

- [Lumina API](#lumina-api)
  - [Library](#library)
    - [Add Library](#add-library)
      - [Add Library Request](#add-library-request)
      - [Add Library Response](#add-library-response)
    - [Get Libraries](#get-libraries)
      - [Get Libraries Request](#get-libraries-request)
      - [Get Libraries Response](#get-libraries-response)
    - [Get Enabled Libraries](#get-enabled-libraries)
      - [Get Enabled Libraries Request](#get-enabled-libraries-request)
      - [Get Enabled Libraries Response](#get-enabled-libraries-response)
    - [Get Library](#get-library)
      - [Get Library Request](#get-library-request)
      - [Get Library Response](#get-library-response)
    - [Update Library](#update-library)
      - [Update Library Request](#update-library-request)
      - [Update Library Response](#update-library-response)
    - [Delete Library](#delete-library)
      - [Delete Library Request](#delete-library-request)
      - [Delete Library Response](#delete-library-response)
    - [Scan Libraries](#scan-libraries)
      - [Scan Libraries Request](#scan-libraries-request)
      - [Scan Libraries Response](#scan-libraries-response)
    - [Scan Library](#scan-library)
      - [Scan Library Request](#scan-library-request)
      - [Scan Library Response](#scan-library-response)
    - [Get Running Library Scans](#get-running-library-scans)
      - [Get Running Library Scans Request](#get-running-library-scans-request)
      - [Get Running Library Scans Response](#get-running-library-scans-response)
    - [Get Library Scan Progress](#get-library-scan-progress)
      - [Get Library Scan Progress Request](#get-library-scan-progress-request)
      - [Get Library Scan Progress Response](#get-library-scan-progress-response)
    - [Cancel Libraries Scan](#cancel-libraries-scan)
      - [Cancel Libraries Scan Request](#cancel-libraries-scan-request)
      - [Cancel Libraries Scan Response](#cancel-libraries-scan-response)
    - [Cancel Library Scan](#cancel-library-scan)
      - [Cancel Library Scan Request](#cancel-library-scan-request)
      - [Cancel Library Scan Response](#cancel-library-scan-response)
    - [Get Library Path Template Parts](#get-library-path-template-parts)
      - [Get Library Path Template Parts Request](#get-library-path-template-parts-request)
      - [Get Library Path Template Parts Response](#get-library-path-template-parts-response)

## Library

### Add Library

#### Add Library Request

```js
POST api/v1/libraries
```

```json
{
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "canDownloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "isContentOnSsd": false,
  "shouldTrackConsumptionProgress": true,
  "completionThresholdPercentage": 100,
  "pathTemplateParts": [],
  "metadataProviders": [
    {
      "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
      "isEnabled": true
    }
  ],
  "artworkProviders": null,
  "bookReaders": [
    {
      "pluginId": "f0d1a2b3-4c5d-4e6f-8a7b-9c0d1e2f3a4b",
      "isEnabled": true
    }
  ]
}
```

The `metadataProviders`, `artworkProviders` and `bookReaders` lists configure the providers and readers of the library in the same request: each entry names a plugin returned by the matching `GET api/v1/libraries/{metadata-providers|artwork-providers|book-readers}/{libraryType}` request, and whether it is enabled, and the order of the provider entries is the order in which the providers are tried. A supported plugin left out of a list is added disabled, after the listed ones. The `isEnabled` of an entry can be omitted, in which case the stored state is kept and a provider without one starts disabled, so a list can reorder providers without changing their state. A list that is `null` or absent adds every supported plugin disabled, in alphabetical order. A plugin that does not support the library type fails the request with `PluginNotSupportedForLibraryType`.

#### Add Library Response

```js
201 Created
```

```json
{
  "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "canDownloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "isContentOnSsd": false,
  "shouldTrackConsumptionProgress": true,
  "completionThresholdPercentage": 100,
  "pathTemplateParts": [],
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null
}
```

### Get Libraries

#### Get Libraries Request

```js
GET api/v1/libraries
```

#### Get Libraries Response

```js
200 Ok
```

```json
[
  {
    "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
    "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
    "title": "Books",
    "libraryType": "Book",
    "contentLocations": [
      "/media/books"
    ],
    "coverImage": null,
    "isEnabled": true,
    "isLocked": false,
    "canDownloadMetadataFromWeb": true,
    "shouldSaveMetadataInMediaDirectories": true,
    "shouldSkipUnchangedDirectoriesDuringScan": true,
    "isContentOnSsd": false,
    "shouldTrackConsumptionProgress": true,
    "completionThresholdPercentage": 100,
    "pathTemplateParts": [],
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null
  }
]
```

### Get Enabled Libraries

#### Get Enabled Libraries Request

```js
GET api/v1/libraries/enabled
```

#### Get Enabled Libraries Response

```js
200 Ok
```

```json
[
  {
    "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
    "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
    "title": "Books",
    "libraryType": "Book",
    "contentLocations": [
      "/media/books"
    ],
    "coverImage": null,
    "isEnabled": true,
    "isLocked": false,
    "canDownloadMetadataFromWeb": true,
    "shouldSaveMetadataInMediaDirectories": true,
    "shouldSkipUnchangedDirectoriesDuringScan": true,
    "isContentOnSsd": false,
    "shouldTrackConsumptionProgress": true,
    "completionThresholdPercentage": 100,
    "pathTemplateParts": [],
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null
  }
]
```

### Get Library

#### Get Library Request

```js
GET api/v1/libraries/{id}
```

#### Get Library Response

```js
200 Ok
```

```json
{
  "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "canDownloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "isContentOnSsd": false,
  "shouldTrackConsumptionProgress": true,
  "completionThresholdPercentage": 100,
  "pathTemplateParts": [],
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null
}
```

### Update Library

#### Update Library Request

```js
PUT api/v1/libraries/{id}
```

```json
{
  "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "canDownloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "isContentOnSsd": false,
  "shouldTrackConsumptionProgress": true,
  "completionThresholdPercentage": 100,
  "pathTemplateParts": [],
  "metadataProviders": [
    {
      "pluginId": "f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3",
      "isEnabled": true
    }
  ],
  "artworkProviders": null,
  "bookReaders": null
}
```

Updating a library requires being its owner and holding the `CanCreateLibraries` permission, or being an admin, who can update any library; any other user is answered with `403 NotAuthorized` before the cover image is read. The owner of the library is the user who created it: an update never changes it, so the request carries no owner.

The `metadataProviders`, `artworkProviders` and `bookReaders` lists work as for [Add Library](#add-library). A list that is sent also removes the stored configurations of the plugins that do not support the library type, even when the list does not name them, and keeps the stored state of the supported plugins it leaves out, ordering them after the listed ones. A list that is `null` or absent keeps the stored configurations when the library type is unchanged, and when the type changes, removes the configurations of the plugins that do not support the new type and adds the newly supported plugins disabled.

#### Update Library Response

```js
200 Ok
```

```json
{
  "id": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "canDownloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "isContentOnSsd": false,
  "shouldTrackConsumptionProgress": true,
  "completionThresholdPercentage": 100,
  "pathTemplateParts": [],
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null
}
```

### Delete Library

#### Delete Library Request

```js
DELETE api/v1/libraries/{id}
```

Deleting a library requires being its owner and holding the `CanCreateLibraries` permission, or being an admin, who can delete any library; any other user is answered with `403 NotAuthorized`.

#### Delete Library Response

```js
200 Ok
```

### Scan Libraries

#### Scan Libraries Request

```js
POST api/v1/libraries/scans
```

#### Scan Libraries Response

```js
200 Ok
```

```json
[
  {
    "scanId": "e7a2c9f4-6b3d-4f80-9e3a-2c1b0d8a4f56",
    "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"
  }
]
```

### Scan Library

#### Scan Library Request

```js
POST api/v1/libraries/{id}/scans
```

#### Scan Library Response

```js
200 Ok
```

```json
{
  "scanId": "e7a2c9f4-6b3d-4f80-9e3a-2c1b0d8a4f56",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"
}
```

### Get Running Library Scans

#### Get Running Library Scans Request

```js
GET api/v1/libraries/scans/running
```

#### Get Running Library Scans Response

```js
200 Ok
```

```json
[
  {
    "scanId": "e7a2c9f4-6b3d-4f80-9e3a-2c1b0d8a4f56",
    "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
    "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
    "totalJobs": 5,
    "completedJobs": 2,
    "currentJobProgress": {
      "completedItems": 10,
      "totalItems": 50,
      "currentOperation": "Reading file system",
      "progressPercentage": 20.0
    },
    "status": "Running",
    "overallProgressPercentage": 40.0
  },
  {
    "scanId": "0b5f8d21-9c3e-4a7b-8f6d-1e2a3b4c5d6e",
    "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
    "libraryId": "5d4c3b2a-1f0e-4d9c-8b7a-6f5e4d3c2b1a",
    "totalJobs": 0,
    "completedJobs": 0,
    "currentJobProgress": {
      "completedItems": 0,
      "totalItems": 0,
      "currentOperation": "Queued",
      "progressPercentage": 0.0
    },
    "status": "Pending",
    "overallProgressPercentage": 0.0
  }
]
```

Queued scans are reported here too, with no jobs prepared yet, because media library scans are executed one at a time and a scan that was queued can wait for the scans ahead of it to finish before it starts.

### Get Library Scan Progress

#### Get Library Scan Progress Request

```js
GET api/v1/libraries/{libraryId}/scans/{scanId}/progress
```

#### Get Library Scan Progress Response

```js
200 Ok
```

```json
{
  "scanId": "e7a2c9f4-6b3d-4f80-9e3a-2c1b0d8a4f56",
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "totalJobs": 5,
  "completedJobs": 2,
  "currentJobProgress": {
    "completedItems": 10,
    "totalItems": 50,
    "currentOperation": "Reading file system",
    "progressPercentage": 20.0
  },
  "status": "Running",
  "overallProgressPercentage": 40.0
}
```

### Cancel Libraries Scan

#### Cancel Libraries Scan Request

```js
POST api/v1/libraries/scans/cancel
```

#### Cancel Libraries Scan Response

```js
204 No Content
```

### Cancel Library Scan

#### Cancel Library Scan Request

```js
POST api/v1/libraries/{libraryId}/scans/{scanId}/cancel
```

#### Cancel Library Scan Response

```js
204 No Content
```

### Get Library Path Template Parts

#### Get Library Path Template Parts Request

```js
GET api/v1/libraries/path-template-parts/{libraryType}
```

#### Get Library Path Template Parts Response

```js
200 Ok
```

```json
{
  "parts": [
    {
      "kind": "Artist",
      "valueType": "Text",
      "defaultRepresentation": "{0}",
      "isOptionalByDefault": false
    },
    {
      "kind": "DiscNumber",
      "valueType": "Integer",
      "defaultRepresentation": "{0:00}",
      "isOptionalByDefault": true
    }
  ],
  "defaultTemplateParts": [
    {
      "kind": "Artist",
      "representation": "{0}",
      "isOptional": false
    }
  ]
}
```
