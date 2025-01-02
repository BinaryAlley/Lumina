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

##### Get Directories Response

```js
200 Ok
```

```json
[
  {
    "Path": "C:\\",
    "Name": "C:\\",
    "ItemType": "Root",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  },
  {
    "Path": "D:\\",
    "Name": "D:\\",
    "ItemType": "Root",
    "IsExpanded": false,
    "ChildrenLoaded": false,
    "Children": []
  }
]
```
