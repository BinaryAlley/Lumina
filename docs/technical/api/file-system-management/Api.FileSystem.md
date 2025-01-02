# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [File System](#file-system)
      - [Get File System Type](#get-file-system-type)
        - [Get Type Request](#get-type-request)
        - [Get Type Response](#get-type-response)

## File System Management

### File System

#### Get File System Type

##### Get Type Request

```js
GET api/v1/file-system/get-type
```

##### Get Type Response

```js
200 Ok
```

```json
{
  "PlatformType": "Unix"
}
```
