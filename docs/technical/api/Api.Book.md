# Lumina API

- [Lumina API](#lumina-api)
  - [Book](#book)
    - [Add Book](#add-book)
      - [Add Book Request](#add-book-request)
      - [Add Book Response](#add-book-response)
    - [Get Books](#get-books)
      - [Get Books Request](#get-books-request)
      - [Get Books Response](#get-books-response)

## Book

### Add Book

#### Add Book Request

```js
POST api/v1/books
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
  "isbNs": [
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
  "id": {
    "value": "32b336e8-dafc-4a08-9dec-9454e66dd55d"
  }
}
```

### Get Books

#### Get Books Request

```js
GET /books
```

#### Get Books Response

```js
200 Ok
```

```json
[
    {        
        "created": "2020-01-01T00:00:00.0000000Z",
        "updated": "2020-01-01T00:00:00.0000000Z"
    }
]
```
