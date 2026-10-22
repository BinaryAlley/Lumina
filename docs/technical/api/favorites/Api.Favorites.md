# Lumina API

- [Lumina API](#lumina-api)
  - [Favorites](#favorites)
    - [Add Favorite](#add-favorite)
      - [Add Favorite Request](#add-favorite-request)
      - [Add Favorite Response](#add-favorite-response)
    - [Remove Favorite](#remove-favorite)
      - [Remove Favorite Request](#remove-favorite-request)
      - [Remove Favorite Response](#remove-favorite-response)
    - [Get Favorites](#get-favorites)
      - [Get Favorites Request](#get-favorites-request)
      - [Get Favorites Response](#get-favorites-response)
    - [Get Favorite Status](#get-favorite-status)
      - [Get Favorite Status Request](#get-favorite-status-request)
      - [Get Favorite Status Response](#get-favorite-status-response)
    - [Media Kinds](#media-kinds)
    - [Scoping and Library Access](#scoping-and-library-access)
    - [Errors](#errors)
    - [Pagination Limits](#pagination-limits)

## Favorites

A favorite is the marker the current user places on a media item of any media kind. It references the item only by its kind and its Id, so the same four operations serve music artists, albums and tracks, books, and the kinds that are planned. A favorite belongs to the user that created it, and only that user can list it, check it or remove it. Every route is under `users/me`, because it always acts on the currently authenticated user, and every operation requires authentication.

Favorites are independent of the catalogue they point to. A favorite stores the kind and the Id of the item without a foreign key to it, so deleting the item from its library does not delete the favorite. A favorite is deleted together with the user that owns it. A favorite whose item was deleted, or whose library the user can no longer access, is still listed, as an unavailable item with empty display fields, so that its owner can see it and remove it.

All the responses of the four operations are private to the authenticated user: they carry `Cache-Control: private, no-store` and `Vary: Authorization`.

### Add Favorite

#### Add Favorite Request

```js
POST api/v1/users/me/favorites
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "mediaKind": "MusicTrack",
  "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
}
```

| Field | Type | Required | Meaning |
| --- | --- | --- | --- |
| `mediaKind` | string | yes | The kind of the media item: `MusicArtist`, `MusicAlbum`, `MusicTrack` or `Book`. `Video` and `Photo` are planned kinds and are rejected, see [Media Kinds](#media-kinds). |
| `mediaItemId` | Guid | yes | The Id of the media item. It cannot be the empty Guid. |

Marks the media item as a favorite of the current user. The request is validated first, and then the media item is looked up in the catalogue of its kind. The item must exist, and the library that contains it must be accessible to the current user, see [Scoping and Library Access](#scoping-and-library-access). The user is always the authenticated user, so no user Id is accepted in the body.

#### Add Favorite Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "mediaKind": "MusicTrack",
  "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "favoritedOnUtc": "2026-10-21T12:00:00.0000000Z"
}
```

| Field | Meaning |
| --- | --- |
| `id` | The Id of the favorite. |
| `mediaKind` | The kind of the favorited media item. |
| `mediaItemId` | The Id of the favorited media item. |
| `favoritedOnUtc` | The date and time when the item was favorited. |

The response is `200 Ok` both when the favorite was created and when it already existed, so a client does not need to tell the two apart. The favorite is read back from storage after it is saved, so `favoritedOnUtc` is the stored value. A favorite that was created raises a `FavoriteAdded` domain event; an existing favorite raises none.

### Remove Favorite

#### Remove Favorite Request

```js
DELETE api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `mediaKind` | The name of the media kind, matched case insensitively. Only the member names are accepted: a number that would parse as a kind is rejected. |
| `mediaItemId` | The Id of the media item. It must be a Guid and cannot be the empty Guid. |

Removes the favorite of the current user on the media item identified by the route. Removal is scoped to the authenticated user and never touches another user's favorite. It does not look the media item up and does not evaluate the library access rule, so a user can always remove their own favorite even when the item was deleted or its library is no longer accessible. Another user's favorite is never visible to them, so it cannot be removed through this route. Removing a media kind that is planned but not supported is not an error by itself: it finds no favorite and the request succeeds without changing anything.

#### Remove Favorite Response

```js
200 Ok
```

The response body is an empty JSON object.

Removing an item that the current user did not favorite succeeds without changing anything, and the same is true for a favorite of another user, which is never visible to the current user. Removing is therefore idempotent: the second removal of the same favorite also returns `200 Ok` and raises no event. A favorite that was actually removed raises a `FavoriteRemoved` domain event.

### Get Favorites

#### Get Favorites Request

```js
GET api/v1/users/me/favorites?mediaKind=MusicTrack&currentPage=1&perPage=200
Authorization: Bearer <token>
```

| Query parameter | Type | Required | Meaning |
| --- | --- | --- | --- |
| `mediaKind` | string | no | Restricts the list to one kind: `MusicArtist`, `MusicAlbum`, `MusicTrack` or `Book`. Every kind is returned when it is omitted. |
| `currentPage` | integer | no | The one based page of favorites to return. Defaults to `1` when `perPage` is provided. Values below `1` are raised to `1`. |
| `perPage` | integer | no | The number of favorites per page. Defaults to `200` when `currentPage` is provided. Values below `1` are raised to `1`, and there is no maximum. |

Returns the favorites of the current user, from the most recently favorited. The page bounds one response, not the number of favorites a user can have: a client reads the next page until `currentPage` reaches `numberOfPages`, and `count` reports the total. When neither `currentPage` nor `perPage` is provided, every matching favorite is returned in a single page, the same way the other paginated collections of the API behave when pagination is omitted.

#### Get Favorites Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "mediaKind": "MusicTrack",
      "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
      "isAvailable": true,
      "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
      "title": "Bohemian Rhapsody",
      "collectionId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
      "collectionTitle": "A Night at the Opera",
      "creatorId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
      "creatorName": "Queen",
      "coverPath": "/media/music/Queen-3b3a19f3/A Night at the Opera-e8d52f7b/cover.jpg"
    },
    {
      "mediaKind": "Book",
      "mediaItemId": "4a5b6c7d-8e9f-4a0b-9c1d-2e3f4a5b6c7d",
      "isAvailable": true,
      "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
      "title": "The Hobbit",
      "collectionId": "00000000-0000-0000-0000-000000000000",
      "collectionTitle": "",
      "creatorId": "00000000-0000-0000-0000-000000000000",
      "creatorName": "J. R. R. Tolkien",
      "coverPath": null
    },
    {
      "mediaKind": "MusicAlbum",
      "mediaItemId": "0f1e2d3c-4b5a-4968-8776-5a4b3c2d1e0f",
      "isAvailable": false,
      "libraryId": "00000000-0000-0000-0000-000000000000",
      "title": "",
      "collectionId": "00000000-0000-0000-0000-000000000000",
      "collectionTitle": "",
      "creatorId": "00000000-0000-0000-0000-000000000000",
      "creatorName": "",
      "coverPath": null
    }
  ],
  "currentPage": 1,
  "perPage": 200,
  "count": 3,
  "numberOfPages": 1
}
```

| Field | Meaning |
| --- | --- |
| `data` | The favorites of the page, from the most recently favorited. |
| `currentPage` | The one based page number. |
| `perPage` | The number of favorites per page. |
| `count` | The number of favorites that match the `mediaKind` filter, even when only a page of them is returned. |
| `numberOfPages` | The total number of pages. |

Each item carries the kind and Id of the favorited media item together with its display details. The display fields are generic across media kinds, and "no value" is the empty Guid or the empty string:

| Field | Meaning |
| --- | --- |
| `mediaKind` | The kind of the favorited media item. |
| `mediaItemId` | The Id of the favorited media item. |
| `isAvailable` | Whether the item still exists and its library is accessible to the user. When it is `false`, every display field below is empty. |
| `libraryId` | The Id of the library that contains the item. |
| `title` | The title of the item. |
| `collectionId`, `collectionTitle` | The Id and the title of the collection the item belongs to. |
| `creatorId`, `creatorName` | The Id and the name of the creator of the item. |
| `coverPath` | The stored path of the cover art of the item, the same kind of value the artist, album and book resources expose as `coverPath`, or `null` when it has none. The cover of a track is the cover of its album. |

How the generic fields map to each kind:

| Kind | `title` | `collection` | `creator` |
| --- | --- | --- | --- |
| `MusicArtist` | The artist name. | None. | None. |
| `MusicAlbum` | The album title. | None. | The artist. |
| `MusicTrack` | The track title. | The album. | The artist. |
| `Book` | The book title. | None today: the series of a book is not catalogued yet, so the collection is always empty. | The first author, by display name. |

An item whose display details no longer exist, because the item was deleted, or whose library the current user cannot access, is returned with `isAvailable` set to `false` and every display field empty. It stays in `data` and in `count`, so its owner can see it and remove it with [Remove Favorite](#remove-favorite), and the response never exposes display data from a library the user cannot reach. The Id of the media item is the one the user favorited, and carries no information beyond that.

### Get Favorite Status

#### Get Favorite Status Request

```js
GET api/v1/users/me/favorites/status?mediaKind=MusicTrack&mediaItemIds=9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f&mediaItemIds=8e9f0a1b-2c3d-4e5f-9a6b-7c8d9e0f1a2b
Authorization: Bearer <token>
```

| Query parameter | Type | Required | Meaning |
| --- | --- | --- | --- |
| `mediaKind` | string | yes | The kind of every media item checked: `MusicArtist`, `MusicAlbum`, `MusicTrack` or `Book`. |
| `mediaItemIds` | Guid, repeated | yes | The Ids of the media items to check, one `mediaItemIds` parameter per Id, from 1 to 100. None can be the empty Guid. Repeated Ids are checked once. |

Tells which of the given media items are favorites of the current user. It reads only the favorites of the current user and evaluates no library access, so the answer reveals nothing about any other user, and an Id that is not a favorite, does not exist or belongs to an inaccessible library is simply absent from the answer. A page that renders many favorite toggles uses it to hydrate all of them with one request per kind.

#### Get Favorite Status Response

```js
200 Ok
```

```json
{
  "favoritedMediaItemIds": [
    "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
  ]
}
```

| Field | Meaning |
| --- | --- |
| `favoritedMediaItemIds` | The Ids, among the checked ones, that the current user favorited. Empty when none is. |

### Media Kinds

The media kind is stored by its name, together with the Id of the item.

| Kind | Status | Item |
| --- | --- | --- |
| `MusicArtist` | Supported | An artist of a music library. |
| `MusicAlbum` | Supported | An album of a music library. |
| `MusicTrack` | Supported | A track of a music library. |
| `Book` | Supported | A book of a book library. |
| `Video` | Planned | A video file. Video libraries are not catalogued yet. |
| `Photo` | Planned | A photo. Photo libraries are not catalogued yet. |

### Scoping and Library Access

Every operation is scoped to the current user by the server: the user is read from the authenticated principal, never from the route, the query or the body, so a user cannot read or change the favorites of another user.

Adding a favorite also applies the library access rule. The server resolves the library that contains the item, through the catalogue of its kind, and then evaluates the library ownership policy of the current user for that library, exactly as the library routes do. An item that does not exist, and an item whose library is not accessible to the current user, are both reported with the same kind specific not found error, so the response cannot be used to find out whether an item exists in a library of another user. The not found error depends on the kind:

| Kind | Not found error |
| --- | --- |
| `MusicArtist` | `ArtistNotFound` |
| `MusicAlbum` | `AlbumNotFound` |
| `MusicTrack` | `TrackNotFound` |
| `Book` | `BookNotFound` |

Removing, and checking the status of, favorites do not evaluate the rule, because they only touch the favorites of the current user. Listing evaluates the rule once for each distinct library among the items of the page, and an item of an inaccessible library is returned as unavailable, as described in [Get Favorites Response](#get-favorites-response).

### Errors

Errors are returned as problem details. A request without a valid token is rejected with `401 Unauthorized` before it reaches the operation.

| Status | Error | When |
| --- | --- | --- |
| `401` | | The request carries no token, or an invalid or expired one. The body is a problem whose `title` is `Unauthorized` and whose `detail` says why, for example `The token has expired`. |
| `403` | `NotAuthorized` | The authenticated principal carries no user identity. |
| `404` | `ArtistNotFound` | [Add Favorite](#add-favorite) of a `MusicArtist` that does not exist or whose library the user cannot access. |
| `404` | `AlbumNotFound` | [Add Favorite](#add-favorite) of a `MusicAlbum` that does not exist or whose library the user cannot access. |
| `404` | `TrackNotFound` | [Add Favorite](#add-favorite) of a `MusicTrack` that does not exist or whose library the user cannot access. |
| `404` | `BookNotFound` | [Add Favorite](#add-favorite) of a `Book` that does not exist or whose library the user cannot access. |
| `409` | `ConcurrencyConflict` | The storage reported a busy or locked database while the change was saved. Repeating the request is safe. |
| `422` | see below | The request failed validation. |

The `404` and `409` problems carry the generic error code in `title` and the specific name in `detail`:

```json
{
  "title": "General.NotFound",
  "status": 404,
  "detail": "TrackNotFound",
  "instance": "/api/v1/users/me/favorites",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

A request that fails validation is rejected with `422 Unprocessable Entity` and a validation problem whose `errors` object lists the names of the validation errors under the generic `General.Validation` code:

```json
{
  "title": "General.Validation",
  "status": 422,
  "detail": "OneOrMoreValidationErrorsOccurred",
  "instance": "/api/v1/users/me/favorites",
  "errors": {
    "General.Validation": [
      "FavoriteMediaKindIsNotSupported"
    ]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

The validation errors of each operation are:

| Error | Operation | When |
| --- | --- | --- |
| `FavoriteMediaKindIsInvalid` | Add, Remove, Get Favorites, Get Favorite Status | The kind is missing where it is required, or it is a number that is not a member of the media kind enumeration. In the Remove route, the segment is not a member name of the enumeration, and numbers are not accepted. |
| `FavoriteMediaKindIsNotSupported` | Add | The kind is `Video` or `Photo`, which are planned and cannot be favorited yet. |
| `FavoriteMediaItemIdCannotBeEmpty` | Add, Remove, Get Favorite Status | The item Id is missing or is the empty Guid. In the Remove route it is also reported when the segment is not a Guid. For the status request, any of the Ids is the empty Guid. |
| `FavoriteMediaItemIdsCannotBeEmpty` | Get Favorite Status | No `mediaItemIds` were given. |
| `FavoriteMediaItemIdsExceedMaximumCount` | Get Favorite Status | More than 100 Ids were given. |

A value that cannot be bound is not reported by the validators. This covers a `mediaKind` that is not a member of `FavoriteMediaKind` (for example `"Song"`, or a number outside the enumeration), an item Id that is not a Guid in the Add body or the status query, and a `currentPage` or `perPage` that is not an integer. The endpoints do not catch binding failures, so the request is answered by the exception handling middleware as a generic `500 Internal Server Error` problem, exactly like every other endpoint of the API that binds an enumeration or a number, and not with the `422` problem above. The web client never sends such values, because its proxy endpoints rebuild the query from typed values.

`FavoriteAlreadyExists` is defined among the favorites errors, but adding is idempotent, so no operation returns it. `FavoriteNotFound` remains defined for the storage read that follows an add, but removing is idempotent, so a remove request no longer returns it.

### Pagination Limits

| Limit | Value |
| --- | --- |
| Default per page of Get Favorites | 200 favorites, when a page is requested. |
| Minimum per page of Get Favorites | 1 favorite. |
| Maximum per page of Get Favorites | None. The server has no separate maximum page size, matching the other paginated collections of the API. |
| Ids of one Get Favorite Status request | From 1 to 100, one kind per request. The Ids travel in the query string, so the limit keeps the request line under the 8 KB default of Kestrel. |

The limits bound a response or a request, not the number of favorites a user can have. A client that needs more than 100 Ids checked splits them into several requests, as the bundled web client does.
