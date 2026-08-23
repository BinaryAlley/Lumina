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
    "items": [],
    "path": "/media/movies/The Matrix (1999)/",
    "name": "The Matrix",
    "dateCreated": "2025-01-01T13:50:38.3183787Z",
    "dateModified": "2025-01-01T13:40:38.3183791Z"
  },
  {
    "items": [],
    "path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/",
    "name": "The Lord of the Rings - The Fellowship of the Ring (2001)",
    "dateCreated": "2025-01-01T13:50:38.3184681Z",
    "dateModified": "2025-01-01T13:40:38.3184683Z"
  }
]
```
