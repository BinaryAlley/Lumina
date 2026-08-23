# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [Drives](#drives)
      - [Get Drives](#get-drives)
        - [Get Drives Request](#get-drives-request)
        - [Get Drives Response](#get-drives-response)

## File System Management

### Drives

#### Get Drives

##### Get Drives Request

```js
GET api/v1/drives/get-drives
```

##### Get Drives Response

```js
200 Ok
```

```json
[
  {
    "path": "C:\\",
    "name": "C:\\",
    "itemType": "Root",
    "isExpanded": false,
    "childrenLoaded": false,
    "children": []
  },
  {
    "path": "D:\\",
    "name": "D:\\",
    "itemType": "Root",
    "isExpanded": false,
    "childrenLoaded": false,
    "children": []
  }
]
```
