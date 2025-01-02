# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [Directories](#directories)
      - [Get Directories](#get-directories)
        - [Get Directories Request](#get-directories-request)
        - [Get Directories Response](#get-directories-response)
      - [Get Directory Tree](#get-directory-tree)
        - [Get Directory Tree Request](#get-directory-tree-request)
        - [Get Directory Tree Response](#get-directory-tree-response)
      - [Get Tree Directories](#get-tree-directories)
        - [Get Tree Directories Request](#get-tree-directories-request)
        - [Get Tree Directories Response](#get-tree-directories-response)

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

#### Get Directory Tree

##### Get Directory Tree Request

```js
GET api/v1/directories/get-directory-tree?path=C%3A%5CUsers%5C&includeFiles=true&includeHiddenElements=true
```

##### Get Directory Tree Response

```js
200 Ok
```

```json
[
  {
    "Path": "/",
    "Name": "The Lord of the Rings - The Fellowship of the Ring (2001)",
    "ItemType": "Root",
    "IsExpanded": true,
    "ChildrenLoaded": true,
    "Children": [
      {
        "Path": "/media/",
        "Name": "media",
        "ItemType": "Directory",
        "IsExpanded": true,
        "ChildrenLoaded": true,
        "Children": [
          {
            "Path": "/media/movies/",
            "Name": "movies",
            "ItemType": "Directory",
            "IsExpanded": true,
            "ChildrenLoaded": true,
            "Children": [
              {
                "Path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/",
                "Name": "The Lord of the Rings - The Fellowship of the Ring (2001)",
                "ItemType": "Directory",
                "IsExpanded": true,
                "ChildrenLoaded": true,
                "Children": [
                  {
                    "Path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/Extras/",
                    "Name": "Extras",
                    "ItemType": "Directory",
                    "IsExpanded": false,
                    "ChildrenLoaded": false,
                    "Children": []
                  },
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
                  }
                ]
              }
            ]
          }
        ]
      }
    ]
  }
]
```

#### Get Tree Directories

##### Get Tree Directories Request

```js
GET api/v1/directories/get-tree-directories?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Get Tree Directories Response

```js
200 Ok
```

```json
[
  {
    "Path": "/media/movies/The Matrix (1999)/",
    "Name": "The Matrix (1999)",
    "ItemType": "Directory",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  },
  {
    "Path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/",
    "Name": "The Lord of the Rings - The Fellowship of the Ring (2001)",
    "ItemType": "Directory",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  }
]
```
