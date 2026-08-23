# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [Files](#files)
      - [Get Files](#get-files)
        - [Get Files Request](#get-files-request)
        - [Get Files Response](#get-files-response)
      - [Get Tree Files](#get-tree-files)
        - [Get Tree Files Request](#get-tree-files-request)
        - [Get Tree Files Response](#get-tree-files-response)

## File System Management

### Files

#### Get Files

##### Get Files Request

```js
GET api/v1/files/get-files?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Get Files Response

```js
200 Ok
```

```json
[
  {
    "size": 754045401,
    "path": "/media/movies/The Matrix (1999)/The Matrix.mkv",
    "name": "The Matrix.mkv",
    "dateCreated": "2025-01-01T13:50:38.3143711Z",
    "dateModified": "2025-01-01T13:40:38.3143719Z"
  },
  {
    "size": 6897,
    "path": "/media/movies/The Matrix (1999)/The Matrix.en.srt",
    "name": "The Matrix.en.srt",
    "dateCreated": "2025-01-01T13:50:38.3145606Z",
    "dateModified": "2025-01-01T13:40:38.3145609Z"
  }
]
```

#### Get Tree Files

##### Get Tree Files Request

```js
GET api/v1/files/get-tree-files?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Get Tree Files Response

```js
200 Ok
```

```json
[
  {
    "path": "/media/movies/The Matrix (1999)/The Matrix.mkv",
    "name": "The Matrix.mkv",
    "itemType": "File",
    "isExpanded": false,
    "childrenLoaded": false,
    "children": []
  },
  {
    "path": "/media/movies/The Matrix (1999)/The Matrix.en.srt",
    "name": "The Matrix.en.srt",
    "itemType": "File",
    "isExpanded": false,
    "childrenLoaded": false,
    "children": []
  },
  {
    "path": "/media/movies/The Matrix (1999)/poster.jpg",
    "name": "poster.jpg",
    "itemType": "File",
    "isExpanded": false,
    "childrenLoaded": false,
    "children": []
  }
]
```
