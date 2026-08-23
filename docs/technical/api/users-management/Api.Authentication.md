# Lumina API

- [Lumina API](#lumina-api)
  - [Authentication](#authentication)
    - [Register](#register)
      - [Register Request](#register-request)
      - [Register Response](#register-response)
    - [Login](#login)
      - [Login Request](#login-request)
      - [Login Response](#login-response)
    - [Recover Password](#recover-password)
      - [Recover Password Request](#recover-password-request)
      - [Recover Password Response](#recover-password-response)
    - [Change Password](#change-password)
      - [Change Password Request](#change-password-request)
      - [Change Password Response](#change-password-response)
    - [Get Users](#get-users)
      - [Get Users Request](#get-users-request)
      - [Get Users Response](#get-users-response)

## Authentication

### Register

#### Register Request

```js
POST api/v1/auth/register
```

```json
{
  "username": "JohnDoe",
  "password": "Abcd123$",
  "passwordConfirm": "Abcd123$",
  "use2fa": true
}
```

#### Register Response

```js
201 Created
```

```json
{
  "id": "c8ec9858-ed98-4936-a893-cddfe40edf5c",
  "username": "JohnDoe",
  "totpSecret": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABCQAAAQkAQAAAACN7fKkAAAFW0lEQVR4nO3bQW4cOQwF0L6B73/L3MABjCp/ilR1BphkFAl..."
}
```

### Login

#### Login Request

```js
POST api/v1/auth/login
```

```json
{
  "username": "JohnDoe",
  "password": "Abcd123$",
  "totpCode": "123456"
}
```

#### Login Response

```js
200 Ok
```

```json
{
  "id": "e5ea6c64-992b-4173-9c1c-46d5786e4226",
  "username": "JohnDoe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwNzE2Y2E1ZC1hZjhkLT...",
  "usesTotp": true
}
```

### Recover Password

#### Recover Password Request

```js
POST api/v1/auth/recover-password
```

```json
{
  "username": "JohnDoe",
  "totpCode": "123456"
}
```

#### Recover Password Response

```js
200 Ok
```

```json
{
  "isPasswordReset": true
}
```

### Change Password

#### Change Password Request

```js
POST api/v1/auth/change-password
```

```json
{
  "username": "JohnDoe",
  "currentPassword": "Abcd123$",
  "newPassword": "123$Abcd",
  "newPasswordConfirm": "123$Abcd"
}
```

#### Change Password Response

```js
200 Ok
```

```json
{
  "isPasswordChanged": true
}
```

### Get Users

#### Get Users Request

```js
GET api/v1/auth/users
```

#### Get Users Response

```js
200 Ok
```

```json
[
  {
    "id": "c8ec9858-ed98-4936-a893-cddfe40edf5c",
    "username": "JohnDoe",
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null
  },
  {
    "id": "e5ea6c64-992b-4173-9c1c-46d5786e4226",
    "username": "JaneDoe",
    "createdOnUtc": "2025-01-02T12:00:00.0000000Z",
    "updatedOnUtc": "2025-01-03T12:00:00.0000000Z"
  }
]
```
