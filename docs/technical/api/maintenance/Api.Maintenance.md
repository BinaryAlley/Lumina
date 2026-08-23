# Lumina API

- [Lumina API](#lumina-api)
  - [Maintenance](#maintenance)
    - [Check Health](#check-health)
      - [Check Health Request](#check-health-request)
      - [Check Health Response](#check-health-response)
    - [Setup Application](#setup-application)
      - [Setup Application Request](#setup-application-request)
      - [Setup Application Response](#setup-application-response)
    - [Check Initialization](#check-initialization)
      - [Check Initialization Request](#check-initialization-request)
      - [Check Initialization Response](#check-initialization-response)

## Maintenance

### Check Health

#### Check Health Request

```js
GET api/v1/check-health
```

#### Check Health Response

```js
200 Ok
```

Returns an empty response. This endpoint is not yet implemented.

### Setup Application

#### Setup Application Request

```js
POST api/v1/initialization
```

```json
{
  "username": "admin",
  "password": "Abcd123$",
  "passwordConfirm": "Abcd123$",
  "use2fa": true
}
```

#### Setup Application Response

```js
201 Created
```

```json
{
  "id": "c8ec9858-ed98-4936-a893-cddfe40edf5c",
  "username": "admin",
  "totpSecret": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABCQAAAQkAQAAAACN7fKkAAAFW0lEQVR4nO3bQW4cOQwF0L6B73/L3MABjCp/ilR1BphkFAl..."
}
```

### Check Initialization

#### Check Initialization Request

```js
GET api/v1/initialization
```

#### Check Initialization Response

```js
200 Ok
```

```json
{
  "isInitialized": true
}
```
