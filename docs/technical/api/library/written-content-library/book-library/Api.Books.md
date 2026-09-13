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
    - [Update Book](#update-book)
      - [Update Book Request](#update-book-request)
      - [Update Book Response](#update-book-response)
    - [Update Book Cover](#update-book-cover)
      - [Update Book Cover Request](#update-book-cover-request)
      - [Update Book Cover Response](#update-book-cover-response)
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
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books
```

```json
{
  "path": "/media/libraries/books/the-fellowship-of-the-ring.pdf",
  "metadata": {
    "title": "The Fellowship of the Ring",
    "originalTitle": "The Fellowship of the Ring",
    "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
    "releaseInfo": {
      "originalReleaseDate": "1954-07-29",
      "originalReleaseYear": 1954,
      "reReleaseDate": "2001-09-06",
      "reReleaseYear": 2001,
      "releaseCountry": "GB",
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
      "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d",
      "role": "Author"
    },
    {
      "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e",
      "role": "Illustrator"
    }
  ],
  "ratings": [
    {
      "source": "GoogleBooks",
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
  "path": "/media/libraries/books/the-fellowship-of-the-ring.pdf",
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
      "releaseCountry": "GB",
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
  "contributors": [
    { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Author" },
    { "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e", "role": "Illustrator" }
  ],
  "ratings": [
    {
      "source": "GoogleBooks",
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
  ],
  "metadataStatus": "Pending",
  "lastMetadataUpdateUtc": null,
  "metadataProvider": null,
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null,
  "coverPath": null
}
```

### Get Book

#### Get Book Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}
```

#### Get Book Response

```js
200 Ok
```

Returns an empty response. This endpoint is not yet implemented.

### Get Books

#### Get Books Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books?currentPage=1&perPage=10&searchTerm=fellowship&sortBy=title&sortOrder=Ascending
```

| Query Parameter | Type | Description |
| --- | --- | --- |
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
      "path": "/media/libraries/books/the-fellowship-of-the-ring.pdf",
      "metadata": {
        "title": "The Fellowship of the Ring",
        "originalTitle": "The Fellowship of the Ring",
        "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings.",
        "releaseInfo": {
          "originalReleaseDate": "1954-07-29",
          "originalReleaseYear": 1954,
          "reReleaseDate": "2001-09-06",
          "reReleaseYear": 2001,
          "releaseCountry": "GB",
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
      "contributors": [
        { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Author" },
        { "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e", "role": "Illustrator" }
      ],
      "ratings": [
        {
          "source": "GoogleBooks",
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
      ],
      "metadataStatus": "Pending",
      "lastMetadataUpdateUtc": null,
      "metadataProvider": null,
      "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
      "updatedOnUtc": null,
      "coverPath": null
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
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/lite?currentPage=1&perPage=10&searchTerm=fellowship&filterAlphaKey=f&shouldIgnoreThePrefixForAlphaPicker=true&sortBy=title&sortOrder=Ascending
```

| Query Parameter | Type | Description |
| --- | --- | --- |
| `currentPage` | `int` | Optional. The page of results to retrieve. |
| `perPage` | `int` | Optional. The maximum number of books to retrieve per page. |
| `searchTerm` | `string` | Optional. The search term used to filter results. |
| `filterAlphaKey` | `string` | Optional. Filters results by the first character of their title. A single ASCII letter (case-insensitive), `#` for titles starting with a digit, or `*` for titles starting with any other character. |
| `shouldIgnoreThePrefixForAlphaPicker` | `bool` | Whether the leading "The " prefix of a title should be ignored when computing the alpha key. |
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

### Update Book

#### Update Book Request

```js
PUT api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}
```

```json
{
  "metadata": {
    "title": "The Fellowship of the Ring",
    "originalTitle": "The Fellowship of the Ring",
    "description": "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
    "releaseInfo": {
      "originalReleaseDate": "1954-07-29",
      "originalReleaseYear": 1954,
      "reReleaseDate": "2001-09-06",
      "reReleaseYear": 2001,
      "releaseCountry": "GB",
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
      "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d",
      "role": "Author"
    },
    {
      "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e",
      "role": "Illustrator"
    }
  ],
  "ratings": [
    {
      "source": "GoogleBooks",
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

#### Update Book Response

```js
200 Ok
```

```json
{
  "id": "32b336e8-dafc-4a08-9dec-9454e66dd55d",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/media/libraries/books/the-fellowship-of-the-ring.pdf",
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
      "releaseCountry": "GB",
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
  "contributors": [
    { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Author" },
    { "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e", "role": "Illustrator" }
  ],
  "ratings": [
    {
      "source": "GoogleBooks",
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
  ],
  "metadataStatus": "Enriched",
  "lastMetadataUpdateUtc": "2025-01-01T12:00:00.0000000Z",
  "metadataProvider": "GoogleBooks",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": "2025-02-01T12:00:00.0000000Z",
  "coverPath": "/media/books/books-3f2504e0-4f89-41d3-9a0c-0305e82c3301/The Lord of the Rings-2b0e5f5a-0b3f-4b7e-8f4a-8c9e3d2f5a6b/cover.jpg"
}
```

### Update Book Cover

Updates the cover image of the book identified by `bookId` with the image uploaded in the multipart form of the request. The book must belong to the media library identified by `libraryId`, and the uploaded file must be an image.

#### Update Book Cover Request

```js
PUT api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}/cover
Content-Type: multipart/form-data; boundary=LuminaBoundary

--LuminaBoundary
Content-Disposition: form-data; name="cover"; filename="cover.jpg"
Content-Type: image/jpeg

< ./cover.jpg
--LuminaBoundary--
```

#### Update Book Cover Response

```js
200 Ok
```

```json
{
  "coverPath": "/media/books/books-3f2504e0-4f89-41d3-9a0c-0305e82c3301/The Lord of the Rings-2b0e5f5a-0b3f-4b7e-8f4a-8c9e3d2f5a6b/cover.jpg"
}
```

### Get Reading Availability

#### Get Reading Availability Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}/reading/availability
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
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}/reading/manifest
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
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}/reading/sections/{locationRef}
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
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/books/{bookId}/reading/resources/{resourceKey}
```

Returns the binary resource identified by `resourceKey` of the book identified by `bookId` (for example an image, a font, or a stylesheet of the book, or a page image of a scanned PDF that is rendered on demand). The `resourceKey` of a resource is taken from the reading manifest of the book.

#### Get Reading Resource Response

```js
200 Ok
```

The response is the raw bytes of the resource, served with the `Content-Type` of the resource and with `X-Content-Type-Options: nosniff`. A resource whose declared media type could be rendered as an active document (for example an SVG) is served as an opaque binary download instead.
