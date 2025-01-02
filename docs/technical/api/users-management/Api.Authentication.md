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

## Authentication

### Register

#### Register Request

```js
POST api/v1/auth/register
```

```json
{
  "Username": "JohnDoe",
  "Password": "Abcd123$",
  "PasswordConfirm": "Abcd123$",
  "Use2fa": true
}
```

#### Register Response

```js
201 Created
```

```json
{
  "Id": "c8ec9858-ed98-4936-a893-cddfe40edf5c",
  "Username": "JohnDoe",
  "TotpSecret": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABCQAAAQkAQAAAACN7fKkAAAFW0lEQVR4nO3bQW4cOQwF0L6B73/L3MABjCp/ilR1BphkFAl..."
}
```

### Login

#### Login Request

```js
POST api/v1/auth/login
```

```json
{
  "Username": "JohnDoe",
  "Password": "Abcd123$",
  "TotpCode": "123456"
}
```

#### Login Response

```js
200 Ok
```

```json
{
  "Id": "e5ea6c64-992b-4173-9c1c-46d5786e4226",
  "Username": "JohnDoe",
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwNzE2Y2E1ZC1hZjhkLT...",
  "UsesTotp": true
}
```

### Recover Password

#### Recover Password Request

```js
POST api/v1/auth/recover-password
```

```json
{
  "Username": "JohnDoe",
  "TotpCode": "123456"
}
```

#### Recover Password Response

```js
200 Ok
```

```json
{
  "IsPasswordReset": true
}
```

### Change Password

#### Change Password Request

```js
POST api/v1/auth/change-password
```

```json
{
  "Username": "JohnDoe",
  "CurrentPassword": "Abcd123$",
  "NewPassword": "123$Abcd",
  "NewPasswordConfirm": "123$Abcd"
}
```

#### Change Password Response

```js
200 Ok
```

```json
{
  "IsPasswordChanged": true
}
```
