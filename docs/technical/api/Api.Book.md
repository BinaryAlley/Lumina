# Lumina API

- [Lumina API](#lumina-api)
  - [Book](#book)
    - [Add Book](#add-book)
      - [Add Book Request](#add-book-request)
      - [Add Book Response](#add-book-response)
    - [Get Book](#get-book)
      - [Get Book Request](#get-book-request)
      - [Get Book Response](#get-book-response)
    - [Get Books](#get-books)
      - [Get Books Request](#get-books-request)
      - [Get Books Response](#get-books-response)
    - [Get Books Lite](#get-books-lite)
      - [Get Books Lite Request](#get-books-lite-request)
      - [Get Books Lite Response](#get-books-lite-response)
    - [Get Reading Availability](#get-reading-availability)
      - [Get Reading Availability Request](#get-reading-availability-request)
      - [Get Reading Availability Response](#get-reading-availability-response)
    - [Get Reading Manifest](#get-reading-manifest)
      - [Get Reading Manifest Request](#get-reading-manifest-request)
      - [Get Reading Manifest Response](#get-reading-manifest-response)
    - [Get Reading Resource](#get-reading-resource)
      - [Get Reading Resource Request](#get-reading-resource-request)
      - [Get Reading Resource Response](#get-reading-resource-response)
    - [Get Reading Section](#get-reading-section)
      - [Get Reading Section Request](#get-reading-section-request)
      - [Get Reading Section Response](#get-reading-section-response)

## Book

### Add Book

#### Add Book Request

```js
POST api/v1/books
```

```json
{
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/books/the-fellowship-of-the-ring.pdf",
  "metadata": {
    "title": "The Fellowship of the Ring",
    "originalTitle": "The Fellowship of the Ring",
    "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
    "releaseInfo": {
      "originalReleaseDate": "1954-07-29",
      "originalReleaseYear": 1954,
      "reReleaseDate": "2001-09-06",
      "reReleaseYear": 2001,
      "releaseCountry": "uk",
      "releaseVersion": "50th Anniversary Edition"
    },
    "genres": [
      { "name": "fantasy" },
      { "name": "adventure" },
      { "name": "classic" }
    ],
    "tags": [
      { "name": "epic fantasy" },
      { "name": "quest" },
      { "name": "middle-earth" }
    ],
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "publisher": "Houghton Mifflin",
    "pageCount": 398
  },
  "format": "Paperback",
  "edition": "50th Anniversary Edition",
  "volumeNumber": 1,
  "series": {
    "title": "The Lord of the Rings"
  },
  "asin": "B007978NPG",
  "goodreadsId": "3",
  "lccn": "54009621",
  "oclcNumber": "ocm00012345",
  "openLibraryId": "OL7603910M",
  "libraryThingId": "3203347",
  "googleBooksId": "aWZzLPhY4o0C",
  "barnesAndNobleId": "1100307790",
  "appleBooksId": "id395211",
  "isbns": [
    {
    "value": "0395272238",
    "format": "Isbn10"
    },
    {
    "value": "9780395272237",
    "format": "Isbn13"
    }
  ],
  "contributors": [
    {
      "name": "J.R.R. Tolkien",
      "role": {
          "name": "author",
          "category": "book"
      }
    },
    {
      "name": "Alan Lee",
      "role": {
          "name": "illustrator",
          "category": "book"
      }
    }
  ],
  "ratings": [
    {
      "source": "Goodreads",
      "value": 4.36,
      "maxValue": 5,
      "voteCount": 2345678
    },
    {
      "source": "Amazon",
      "value": 4.7,
      "maxValue": 5,
      "voteCount": 87654
    }
  ]
}
```

#### Add Book Response

```js
201 Created
```

```json
{
  "id": "32b336e8-dafc-4a08-9dec-9454e66dd55d",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/books/the-fellowship-of-the-ring.pdf",
  "metadata": {
    "publisher": "Houghton Mifflin",
    "pageCount": 398,
    "title": "The Fellowship of the Ring",
    "originalTitle": "The Fellowship of the Ring",
    "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
    "releaseInfo": {
      "originalReleaseDate": "1954-07-29",
      "originalReleaseYear": 1954,
      "reReleaseDate": "2001-09-06",
      "reReleaseYear": 2001,
      "releaseCountry": "uk",
      "releaseVersion": "50th Anniversary Edition"
    },
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "tags": [
      {
        "name": "epic fantasy"
      },
      {
        "name": "quest"
      },
      {
        "name": "middle-earth"
      }
    ],
    "genres": [
      {
        "name": "fantasy"
      },
      {
        "name": "adventure"
      },
      {
        "name": "classic"
      }
    ]
  },
  "format": "Paperback",
  "edition": "50th Anniversary Edition",
  "volumeNumber": 1,
  "series": null,
  "asin": "B007978NPG",
  "goodreadsId": "3",
  "lccn": "54009621",
  "oclcNumber": "ocm00012345",
  "openLibraryId": "OL7603910M",
  "libraryThingId": "3203347",
  "googleBooksId": "aWZzLPhY4o0C",
  "barnesAndNobleId": "1100307790",
  "appleBooksId": "id395211",
  "isbns": [
    {
      "value": "0395272238",
      "format": "Isbn10"
    },
    {
      "value": "9780395272237",
      "format": "Isbn13"
    }
  ],
  "contributors": [],
  "ratings": [
    {
      "value": 4.36,
      "maxValue": 5,
      "voteCount": 2345678
    },
    {
      "value": 4.7,
      "maxValue": 5,
      "voteCount": 87654
    }
  ],
  "metadataStatus": "Pending",
  "lastMetadataUpdateUtc": null,
  "metadataProvider": null,
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null
}
```

### Get Book

#### Get Book Request

```js
GET api/v1/books/{id}
```

#### Get Book Response

```js
200 Ok
```

Returns an empty response. This endpoint is not yet implemented.

### Get Books

#### Get Books Request

```js
GET api/v1/books?libraryId=3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a&currentPage=1&perPage=10&searchTerm=fellowship&sortBy=title&sortOrder=Ascending
```

| Query Parameter | Type | Description |
| --- | --- | --- |
| `libraryId` | `string` (GUID) | Required. The Id of the media library whose books are retrieved. |
| `currentPage` | `int` | Optional. The page of results to retrieve. |
| `perPage` | `int` | Optional. The maximum number of books to retrieve per page. |
| `searchTerm` | `string` | Optional. The search term used to filter results. |
| `sortBy` | `string` | Optional. The name of the field by which to sort the results. |
| `sortOrder` | `string` | Optional. The direction in which to sort the results (`Ascending` or `Descending`). |

#### Get Books Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "id": "32b336e8-dafc-4a08-9dec-9454e66dd55d",
      "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
      "path": "/books/the-fellowship-of-the-ring.pdf",
      "metadata": {
        "title": "The Fellowship of the Ring",
        "originalTitle": "The Fellowship of the Ring",
        "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings.",
        "releaseInfo": {
          "originalReleaseDate": "1954-07-29",
          "originalReleaseYear": 1954,
          "reReleaseDate": "2001-09-06",
          "reReleaseYear": 2001,
          "releaseCountry": "uk",
          "releaseVersion": "50th Anniversary Edition"
        },
        "genres": [
          { "name": "fantasy" },
          { "name": "adventure" },
          { "name": "classic" }
        ],
        "tags": [
          { "name": "epic fantasy" },
          { "name": "quest" },
          { "name": "middle-earth" }
        ],
        "language": {
          "languageCode": "en",
          "languageName": "English",
          "nativeName": "English"
        },
        "originalLanguage": {
          "languageCode": "en",
          "languageName": "English",
          "nativeName": "English"
        },
        "publisher": "Houghton Mifflin",
        "pageCount": 398
      },
      "format": "Paperback",
      "edition": "50th Anniversary Edition",
      "volumeNumber": 1,
      "series": {
        "title": "The Lord of the Rings"
      },
      "asin": "B007978NPG",
      "goodreadsId": "3",
      "lccn": "54009621",
      "oclcNumber": "ocm00012345",
      "openLibraryId": "OL7603910M",
      "libraryThingId": "3203347",
      "googleBooksId": "aWZzLPhY4o0C",
      "barnesAndNobleId": "1100307790",
      "appleBooksId": "id395211",
      "isbns": [
        {
          "value": "0395272238",
          "format": "Isbn10"
        },
        {
          "value": "9780395272237",
          "format": "Isbn13"
        }
      ],
      "contributors": [],
      "ratings": [
        {
          "value": 4.36,
          "maxValue": 5,
          "voteCount": 2345678
        }
      ],
      "metadataStatus": "Pending",
      "lastMetadataUpdateUtc": null,
      "metadataProvider": null,
      "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
      "updatedOnUtc": null
    }
  ],
  "currentPage": 1,
  "perPage": 10,
  "count": 1,
  "numberOfPages": 1
}
```

### Get Books Lite

#### Get Books Lite Request

```js
GET api/v1/books/lite?libraryId=3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a&currentPage=1&perPage=10&searchTerm=fellowship&filterAlphaKey=f&ignoreThePrefixForAlphaPicker=true&sortBy=title&sortOrder=Ascending
```

| Query Parameter | Type | Description |
| --- | --- | --- |
| `libraryId` | `string` (GUID) | Required. The Id of the media library whose books are retrieved. |
| `currentPage` | `int` | Optional. The page of results to retrieve. |
| `perPage` | `int` | Optional. The maximum number of books to retrieve per page. |
| `searchTerm` | `string` | Optional. The search term used to filter results. |
| `filterAlphaKey` | `string` | Optional. Filters results by the first character of their title. A single ASCII letter (case-insensitive), `#` for titles starting with a digit, or `*` for titles starting with any other character. |
| `ignoreThePrefixForAlphaPicker` | `bool` | Whether the leading "The " prefix of a title should be ignored when computing the alpha key. |
| `sortBy` | `string` | Optional. The name of the field by which to sort the results. |
| `sortOrder` | `string` | Optional. The direction in which to sort the results (`Ascending` or `Descending`). |

#### Get Books Lite Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "id": "32b336e8-dafc-4a08-9dec-9454e66dd55d",
      "title": "The Fellowship of the Ring",
      "releaseYear": 1954,
      "coverPath": "/media/covers/the-fellowship-of-the-ring.jpg"
    }
  ],
  "currentPage": 1,
  "perPage": 10,
  "count": 1,
  "numberOfPages": 1
}
```

### Get Reading Availability

#### Get Reading Availability Request

```js
GET api/v1/books/{bookId}/reading/availability
```

Reports whether the book identified by `bookId` can be opened for reading: the book reader that supports the format of the book is resolved and its enablement for the media library of the book is checked, without extracting the book. When the book cannot be read, the response carries the code of the error preventing it, so the client can tell a missing book reader apart from a disabled one.

#### Get Reading Availability Response

```js
200 Ok
```

```json
{
  "bookId": "32b336e8-dafc-4a08-9dec-9454e66dd55d",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "isAvailable": true,
  "errorCode": null
}
```

### Get Reading Manifest

#### Get Reading Manifest Request

```js
GET api/v1/books/{bookId}/reading/manifest
```

Opens the book identified by `bookId` for reading by extracting its contents through the book reader that is enabled for its media library, and returns the reading manifest: the title and author of the book, its hierarchical table of contents, its ordered spine of reading sections, the keys of its resources, and whether it has extractable text content. A scanned book, whose pages are only images, has no text content and is displayed as page images.

#### Get Reading Manifest Response

```js
200 Ok
```

```json
{
  "title": "The Fellowship of the Ring",
  "author": "J.R.R. Tolkien",
  "coverResourceKey": "0f4d6e2b1c9a4f3e8d7c6b5a4f3e2d1c",
  "tableOfContents": [
    {
      "label": "Chapter One",
      "locationRef": "chapter1",
      "children": [
        {
          "label": "Part One",
          "locationRef": "chapter1",
          "children": []
        }
      ]
    }
  ],
  "spine": [
    {
      "locationRef": "chapter1",
      "title": "Chapter One"
    },
    {
      "locationRef": "chapter2",
      "title": "Chapter Two"
    }
  ],
  "resourceKeys": [
    "0f4d6e2b1c9a4f3e8d7c6b5a4f3e2d1c"
  ],
  "hasTextContent": true
}
```

### Get Reading Section

#### Get Reading Section Request

```js
GET api/v1/books/{bookId}/reading/sections/{locationRef}
```

Returns the reading section identified by `locationRef` of the book identified by `bookId`. The section content is sanitized by the host before it is served, and its references to the resources of the book are resolved by the client through the resource endpoint. The `locationRef` of a section is taken from the spine of the reading manifest of the book.

#### Get Reading Section Response

```js
200 Ok
```

```json
{
  "locationRef": "chapter1",
  "title": "Chapter One",
  "contentHtml": "<section><h1>Chapter One</h1><p>First paragraph.</p></section>"
}
```

### Get Reading Resource

#### Get Reading Resource Request

```js
GET api/v1/books/{bookId}/reading/resources/{resourceKey}
```

Returns the binary resource identified by `resourceKey` of the book identified by `bookId` (for example an image, a font, or a stylesheet of the book, or a page image of a scanned PDF that is rendered on demand). The `resourceKey` of a resource is taken from the reading manifest of the book.

#### Get Reading Resource Response

```js
200 Ok
```

The response is the raw bytes of the resource, served with the `Content-Type` of the resource and with `X-Content-Type-Options: nosniff`. A resource whose declared media type could be rendered as an active document (for example an SVG) is served as an opaque binary download instead.
