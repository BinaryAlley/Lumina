# Lumina API

- [Lumina API](#lumina-api)
  - [File System Management](#file-system-management)
    - [Paths](#paths)
      - [Check Path Exists](#check-path-exists)
        - [Check Path Exists Request](#check-path-exists-request)
        - [Check Path Exists Response](#check-path-exists-response)
      - [Combine Path](#combine-path)
        - [Combine Path Request](#combine-path-request)
        - [Combine Path Response](#combine-path-response)
      - [Get Path Parent](#get-path-parent)
        - [Get Path Parent Request](#get-path-parent-request)
        - [Get Path Parent Response](#get-path-parent-response)
      - [Get Path Root](#get-path-root)
        - [Get Path Root Request](#get-path-root-request)
        - [Get Path Root Response](#get-path-root-response)
      - [Get Path Separator](#get-path-separator)
        - [Get Path Separator Request](#get-path-separator-request)
        - [Get Path Separator Response](#get-path-separator-response)
      - [Split Path](#split-path)
        - [Split Path Request](#split-path-request)
        - [Split Path Response](#split-path-response)
      - [Split Path](#split-path)
        - [Validate Path Request](#validate-path-request)
        - [Validate Path Response](#validate-path-response)

## File System Management

### Paths

#### Check Path Exists

##### Check Path Exists Request

```js
GET api/v1/path/check-path-exists?path=C%3A%5CUsers%5C&includeHiddenElements=true
```

##### Check Path Exists Response

```js
200 Ok
```

```json
{
  "Exists": true
}
```

#### Combine Path

##### Combine Path Request

```js
GET api/v1/path/combine?originalPath=C%3A%5C&newPath=Users
```

##### Combine Path Response

```js
200 Ok
```

```json
{
  "Path": "C:\\Users\\"
}
```

#### Get Path Parent

##### Get Path Parent Request

```js
GET api/v1/path/get-path-parent?path=C%3A%5CUsers%5C
```

##### Get Path Parent Response

```js
200 Ok
```

```json
[
  {
    "Path": "C:"
  }
]
```

#### Get Path Root

##### Get Path Root Request

```js
GET api/v1/path/get-path-root?path=C%3A%5CUsers%5C
```

##### Get Path Root Response

```js
200 Ok
```

```json
[
  {
    "Path": "C:\\"
  }
]
```

#### Get Path Separator

##### Get Path Separator Request

```js
GET api/v1/path/get-path-separator
```

##### Get Path Separator Response

```js
200 Ok
```

```json
[
  {
    "Separator": "\\"
  }
]
```

#### Split Path 

##### Split Path Request

```js
GET api/v1/path/split?path=C%3A%5CUsers%5C
```

##### Split Path Response

```js
200 Ok
```

```json
[
  {
    "Path": "C:\\"
  },
  {
    "Path": "Users"
  }
]
```

#### Validate Path 

##### Validate Path Request

```js
GET api/v1/path/validate?path=C%3A%5CUsers%5C
```

##### Validate Path Response

```js
200 Ok
```

```json
{
  "IsValid": true
}
```
