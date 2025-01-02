# Lumina API

- [Lumina API](#lumina-api)
  - [Authorization](#authorization)
    - [Get Permissions](#get-permissions)
      - [Get Permissions Request](#get-permissions-request)
      - [Get Permissions Response](#get-permissions-response)
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
    - [Get Authorization](#get-authorization)
      - [Get Authorization Request](#get-authorization-request)
      - [Get Authorization Response](#get-authorization-response)
    - [Get User Permissions](#get-user-permissions)
      - [Get User Permissions Request](#get-user-permissions-request)
      - [Get User Permissions Response](#get-user-permissions-response)
    - [Get User Role](#get-user-role)
      - [Get User Role Request](#get-user-role-request)
      - [Get User Role Response](#get-user-role-response)
    - [Update User Role And Permissions](#update-user-role-and-permissions)
      - [Update User Role And Permissions Request](#update-user-role-and-permissions-request)
      - [Update User Role And Permissions Response](#update-user-role-and-permissions-response)

## Authorization

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
{
  "Role": {
    "Id": "61567253-c752-4680-989c-a40a73f3e4b9",
    "RoleName": "Editor"
  },
  "Permissions": [
    {
      "Id": "5264e3a8-bdec-4ec0-9423-839f3a9afe4b",
      "PermissionName": "CanDeleteUsers"
    },
    {
      "Id": "7830cd55-3585-4da1-bfad-994be8637da8",
      "PermissionName": "CanViewUsers"
    }
  ]
}
```

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
    "Id": "776e440d-39d2-4f31-8dcd-481de61e8792",
    "RoleName": "Admin"
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
[
  {
    "Id": "c64fc53f-ef32-4eba-9592-67b42ced63a3",
    "PermissionName": "CanViewUsers"
  },
  {
    "Id": "dbfb6847-7a05-49b5-b63b-b08b020389e8",
    "PermissionName": "CanDeleteUsers"
  },
  {
    "Id": "c3dcd487-01fb-4ad9-b14d-7767809bd07c",
    "PermissionName": "CanRegisterUsers"
  }
]
```

### Add Role

#### Add Role Request

```js
POST api/v1/auth/roles
```

#### Add Role Response

```js
200 Ok
```

```json
{
  "Role": {
    "Id": "e5b67972-6928-4be1-a757-e98879d4a12a",
    "RoleName": "Editor"
  },
  "Permissions": [
    {
      "Id": "7934ff13-1ccd-4c24-bab3-6423c905e249",
      "PermissionName": "CanDeleteUsers"
    },
    {
      "Id": "8f23e43a-bb4b-483a-99eb-7ab361df7482",
      "PermissionName": "CanViewUsers"
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
  "RoleId": "73d204e2-5922-441d-83d8-8d4d8fc29eaa",
  "RoleName": "Editor",
  "Permissions": [
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
  "Role": {
    "Id": "61567253-c752-4680-989c-a40a73f3e4b9",
    "RoleName": "Editor"
  },
  "Permissions": [
    {
      "Id": "5264e3a8-bdec-4ec0-9423-839f3a9afe4b",
      "PermissionName": "CanDeleteUsers"
    },
    {
      "Id": "7830cd55-3585-4da1-bfad-994be8637da8",
      "PermissionName": "CanViewUsers"
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

### Get Authorization

#### Get Authorization Request

```js
GET api/v1/auth/get-authorization
```

#### Get Authorization Response

```js
200 Ok
```

```json
{
  "UserId": "8e5e2be7-9f3b-4309-882b-8913dc6bda10",
  "Role": "Admin",
  "Permissions": [
    "CanRegisterUsers",
    "CanDeleteUsers"
  ]
}
```

### Get User Permissions

#### Get User Permissions Request

```js
GET api/v1/auth/users/{userId}/permissions
```

#### Get User Permissions Response

```js
200 Ok
```

```json
[
  {
    "Id": "52c58482-f792-4d8a-b97c-7944cf4500d5",
    "PermissionName": "CanViewUsers"
  },
  {
    "Id": "b3b7e562-2e42-455f-a9e7-fed38a3ff63e",
    "PermissionName": "CanDeleteUsers"
  },
  {
    "Id": "43bdf58b-1edf-4909-ac43-8be1aaf191f4",
    "PermissionName": "CanRegisterUsers"
  }
]
```

### Get User Role

#### Get User Role Request

```js
GET api/v1/auth/users/{userId}/role
```

#### Get User Role Response

```js
200 Ok
```

```json
{
  "Id": "54bfd571-9283-4949-bd47-9aa4416c5328",
  "RoleName": "Admin"
}
```

### Update User Role And Permissions

#### Update User Role And Permissions Request

```js
PUT api/v1/auth/users/{userId}/role-and-permissions
```

```json
{
  "RoleId": "92e996fd-645e-4e2f-96fe-30c27e4c6423",
  "Permissions": [
    "5957ea98-3deb-4ebe-a5b9-1aafa1480558",
    "5b9cedf5-9fc5-44d9-bc7e-1164409dc952"
  ]
}
```

#### Update User Role And Permissions Response

```js
200 Ok
```

```json
{
  "UserId": "910518c3-01a4-4622-8114-3a77eed0f392",
  "Role": "Editor",
  "Permissions": [
    "CanRegisterUsers",
    "CanDeleteUsers"
  ]
}
```
