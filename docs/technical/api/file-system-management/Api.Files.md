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
    "Size": 754045401,
    "Path": "/media/movies/The Matrix (1999)/The Matrix.mkv",
    "Name": "The Matrix.mkv",
    "DateCreated": "2025-01-01T13:50:38.3143711Z",
    "DateModified": "2025-01-01T13:40:38.3143719Z"
  },
  {
    "Size": 6897,
    "Path": "/media/movies/The Matrix (1999)/The Matrix.en.srt",
    "Name": "The Matrix.en.srt",
    "DateCreated": "2025-01-01T13:50:38.3145606Z",
    "DateModified": "2025-01-01T13:40:38.3145609Z"
  }
]
```

#### Get Tree Files

##### Get Tree Files Request

```js
GET api/v1/get-tree-files?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Get Tree Files Response

```js
200 Ok
```

```json
[
  {
    "Path": "/media/movies/The Matrix (1999)/The Matrix.mkv",
    "Name": "The Matrix.mkv",
    "ItemType": "File",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  },
  {
    "Path": "/media/movies/The Matrix (1999)/The Matrix.en.srt",
    "Name": "The Matrix.en.srt",
    "ItemType": "File",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  },
  {
    "Path": "/media/movies/The Matrix (1999)/poster.jpg",
    "Name": "poster.jpg",
    "ItemType": "File",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  }
]
```
