# Lumina API

- [Lumina API](#lumina-api)
  - [Authorization](#authorization)
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

### Get Authorization

#### Get Authorization Request

```js
GET api/v1/auth/get-authorization?userId=8e5e2be7-9f3b-4309-882b-8913dc6bda10
```

#### Get Authorization Response

```js
200 Ok
```

```json
{
  "userId": "8e5e2be7-9f3b-4309-882b-8913dc6bda10",
  "role": "Admin",
  "permissions": [
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
    "id": "52c58482-f792-4d8a-b97c-7944cf4500d5",
    "permissionName": "CanViewUsers"
  },
  {
    "id": "b3b7e562-2e42-455f-a9e7-fed38a3ff63e",
    "permissionName": "CanDeleteUsers"
  },
  {
    "id": "43bdf58b-1edf-4909-ac43-8be1aaf191f4",
    "permissionName": "CanRegisterUsers"
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
  "id": "54bfd571-9283-4949-bd47-9aa4416c5328",
  "roleName": "Admin"
}
```


### Update User Role And Permissions

#### Update User Role And Permissions Request

```js
PUT api/v1/auth/users/{userId}/role-and-permissions
```

```json
{
  "roleId": "92e996fd-645e-4e2f-96fe-30c27e4c6423",
  "permissions": [
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
  "userId": "910518c3-01a4-4622-8114-3a77eed0f392",
  "role": "Editor",
  "permissions": [
    "CanRegisterUsers",
    "CanDeleteUsers"
  ]
}
```