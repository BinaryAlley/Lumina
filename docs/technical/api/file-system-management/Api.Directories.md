# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [Directories](#directories)
      - [Get Directories](#get-directories)
        - [Get Directories Request](#get-directories-request)
        - [Get Directories Response](#get-directories-response)

## File System Management

### Directories

#### Get Directories

##### Get Directories Request

```js
GET api/v1/directories/get-directories?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Get Directories Response

```js
200 Ok
```

```json
[
  {
    "Items": [],
    "Path": "/media/movies/The Matrix (1999)/",
    "Name": "The Matrix",
    "DateCreated": "2025-01-01T13:50:38.3183787Z",
    "DateModified": "2025-01-01T13:40:38.3183791Z"
  },
  {
    "Items": [],
    "Path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/",
    "Name": "The Lord of the Rings - The Fellowship of the Ring (2001)",
    "DateCreated": "2025-01-01T13:50:38.3184681Z",
    "DateModified": "2025-01-01T13:40:38.3184683Z"
  }
]
```
