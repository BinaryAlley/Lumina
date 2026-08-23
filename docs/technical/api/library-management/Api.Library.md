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
  "downloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true
}
```

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
  "downloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
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
    "downloadMetadataFromWeb": true,
    "shouldSaveMetadataInMediaDirectories": true,
    "shouldSkipUnchangedDirectoriesDuringScan": true,
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
    "downloadMetadataFromWeb": true,
    "shouldSaveMetadataInMediaDirectories": true,
    "shouldSkipUnchangedDirectoriesDuringScan": true,
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
  "downloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
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
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "title": "Books",
  "libraryType": "Book",
  "contentLocations": [
    "/media/books"
  ],
  "coverImage": null,
  "isEnabled": true,
  "isLocked": false,
  "downloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true
}
```

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
  "downloadMetadataFromWeb": true,
  "shouldSaveMetadataInMediaDirectories": true,
  "shouldSkipUnchangedDirectoriesDuringScan": true,
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null
}
```

### Delete Library

#### Delete Library Request

```js
DELETE api/v1/libraries/{id}
```

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
  }
]
```

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
