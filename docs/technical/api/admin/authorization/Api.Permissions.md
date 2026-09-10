# Lumina API

- [Lumina API](#lumina-api)
  - [Permissions](#permissions)
    - [Get Permissions](#get-permissions)
      - [Get Permissions Request](#get-permissions-request)
      - [Get Permissions Response](#get-permissions-response)

## Permissions

### Get Permissions

#### Get Permissions Request

```js
GET api/v1/auth/permissions
```

#### Get Permissions Response

```js
200 Ok
```

```json
[
  {
    "id": "5264e3a8-bdec-4ec0-9423-839f3a9afe4b",
    "permissionName": "CanDeleteUsers"
  },
  {
    "id": "7830cd55-3585-4da1-bfad-994be8637da8",
    "permissionName": "CanViewUsers"
  }
]
```
