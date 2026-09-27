# Lumina API

- [Lumina API](#lumina-api)
  - [Roles](#roles)
    - [Get Roles](#get-roles)
      - [Get Roles Request](#get-roles-request)
      - [Get Roles Response](#get-roles-response)
    - [Get Role Permissions](#get-role-permissions)
      - [Get Role Permissions Request](#get-role-permissions-request)
      - [Get Role Permissions Response](#get-role-permissions-response)
    - [Add Role](#add-role)
      - [Add Role Request](#add-role-request)
      - [Add Role Response](#add-role-response)
    - [Update Role](#update-role)
      - [Update Role Request](#update-role-request)
      - [Update Role Response](#update-role-response)
    - [Delete Role](#delete-role)
      - [Delete Role Request](#delete-role-request)
      - [Delete Role Response](#delete-role-response)

## Roles

### Get Roles

#### Get Roles Request

```js
GET api/v1/auth/roles
```

#### Get Roles Response

```js
200 Ok
```

```json
[
  {
    "id": "776e440d-39d2-4f31-8dcd-481de61e8792",
    "roleName": "Admin"
  }
]
```


### Get Role Permissions

#### Get Role Permissions Request

```js
GET api/v1/auth/roles/{roleId}/permissions
```

#### Get Role Permissions Response

```js
200 Ok
```

```json
{
  "role": {
    "id": "61567253-c752-4680-989c-a40a73f3e4b9",
    "roleName": "Editor"
  },
  "permissions": [
    {
      "id": "5264e3a8-bdec-4ec0-9423-839f3a9afe4b",
      "permissionName": "CanDeleteUsers"
    },
    {
      "id": "7830cd55-3585-4da1-bfad-994be8637da8",
      "permissionName": "CanViewUsers"
    }
  ]
}
```


### Add Role

#### Add Role Request

```js
POST api/v1/auth/roles
```

```json
{
  "roleName": "Editor",
  "permissions": [
    "7934ff13-1ccd-4c24-bab3-6423c905e249",
    "8f23e43a-bb4b-483a-99eb-7ab361df7482"
  ]
}
```

#### Add Role Response

```js
200 Ok
```

```json
{
  "role": {
    "id": "e5b67972-6928-4be1-a757-e98879d4a12a",
    "roleName": "Editor"
  },
  "permissions": [
    {
      "id": "7934ff13-1ccd-4c24-bab3-6423c905e249",
      "permissionName": "CanDeleteUsers"
    },
    {
      "id": "8f23e43a-bb4b-483a-99eb-7ab361df7482",
      "permissionName": "CanViewUsers"
    }
  ]
}
```


### Update Role

#### Update Role Request

```js
PUT api/v1/auth/roles
```

```json
{
  "roleId": "73d204e2-5922-441d-83d8-8d4d8fc29eaa",
  "roleName": "Editor",
  "permissions": [
    "ac113d18-69bb-4162-8c05-ff7a5559059a",
    "3c2fd936-8d4a-4963-b237-9f8d3aeeab26"
  ]
}
```

#### Update Role Response

```js
200 Ok
```

```json
{
  "role": {
    "id": "61567253-c752-4680-989c-a40a73f3e4b9",
    "roleName": "Editor"
  },
  "permissions": [
    {
      "id": "5264e3a8-bdec-4ec0-9423-839f3a9afe4b",
      "permissionName": "CanDeleteUsers"
    },
    {
      "id": "7830cd55-3585-4da1-bfad-994be8637da8",
      "permissionName": "CanViewUsers"
    }
  ]
}
```


### Delete Role

#### Delete Role Request

```js
DELETE api/v1/auth/roles/{roleId}
```

#### Delete Role Response

```js
204 No Content
```
