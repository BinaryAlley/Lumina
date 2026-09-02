# Lumina API

- [Lumina API](#lumina-api)
  - [Settings](#settings)
    - [Get User Settings](#get-user-settings)
      - [Get User Settings Request](#get-user-settings-request)
      - [Get User Settings Response](#get-user-settings-response)
    - [Update User Settings](#update-user-settings)
      - [Update User Settings Request](#update-user-settings-request)
      - [Update User Settings Response](#update-user-settings-response)

## Settings

### Get User Settings

#### Get User Settings Request

```js
GET api/v1/users/me/settings
```

Returns the settings of the currently authenticated user.

#### Get User Settings Response

```js
200 Ok
```

```json
{
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "isPaginationEnabled": true,
  "itemsPerPage": 50,
  "ignoreThePrefixForAlphaPicker": true,
  "isThemeCachingEnabled": true,
  "shouldAggregateMetadataWhenMissing": true,
  "shouldRenderPdfAsImages": false,
  "shouldPreserveBookStyles": true
}
```

### Update User Settings

#### Update User Settings Request

```js
PUT api/v1/users/me/settings
```

Updates the settings of the currently authenticated user.

```json
{
  "isPaginationEnabled": true,
  "itemsPerPage": 50,
  "ignoreThePrefixForAlphaPicker": true,
  "isThemeCachingEnabled": true,
  "shouldAggregateMetadataWhenMissing": true,
  "shouldRenderPdfAsImages": false,
  "shouldPreserveBookStyles": true
}
```

#### Update User Settings Response

```js
200 Ok
```
