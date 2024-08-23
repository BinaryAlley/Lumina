- [Lumina Domain](#lumina-domain)
  - [Written Content Library Aggregate](#written-content-library-aggregate)
    - [Book](#book)

# [Lumina Domain](#lumina-domain)

## [Written Content Library Aggregate](#written-content-library-aggregate)

### [Book](#book)

```csharp
class Book
{
    ErrorOr<Book> Create(
        WrittenContentMetadata metadata,
        BookFormat format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<string> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> koboId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<Rating> ratings);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "metadata": {
    "title": "The Lord of the Rings: The Fellowship of the Ring",
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
      { "name": "epic fantasy" },
      { "name": "quest" },
      { "name": "middle-earth" }
    ],
    "genres": [
      { "name": "fantasy" },
      { "name": "adventure" },
      { "name": "classic" }
    ],
    "publisher": "Houghton Mifflin",
    "pageCount": 398
  },
  "format": "Paperback",
  "edition": "50th Anniversary Edition",
  "volumeNumber": 1,
  "series": "The Lord of the Rings",
  "asin": "B007978NPG",
  "goodreadsId": "3",
  "lccn": "2001025199",
  "oclcNumber": "48494995",
  "openLibraryId": "OL7603910M",
  "libraryThingId": "3203347",
  "googleBooksId": "aWZzLPhY4o0C",
  "barnesAndNobleId": "1100307790",
  "koboId": "the-fellowship-of-the-ring",
  "appleBooksId": "id395211",
  "isbns": [
    {
      {
        "value": "0395272238",
        "format": "Isbn10"
      },
      {
        "value": "9780395272237",
        "format": "Isbn13"
      }
    }
  ],
  "contributors": [
    {
      "value": "00000000-0000-0000-0000-000000000000",
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
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "metadata": {
    "title": "The Lord of the Rings - The Fellowship of the Ring",
    "originalTitle": "The Lord of the Rings - The Fellowship of the Ring",
    "description": "The greatest fantasy movie ever made",
    "releaseInfo": {
      "originalReleaseDate": "2001-12-19",
      "originalReleaseYear": 2001,
      "reReleaseDate": "2024-06-08",
      "reReleaseYear": 2024,
      "releaseCountry": "us",
      "releaseVersion": "Director's Cut"
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
      { "name": "epic quest" },
      { "name": "friendship" },
      { "name": "middle-earth" }
    ],
    "genres": [
      { "name": "fantasy" },
      { "name": "adventure" },
      { "name": "action" }
    ],
    "publisher": "Allen & Unwin",
    "pageCount": 432
  },
  "format": "Hardcover",
  "edition": "First Edition",
  "volumeNumber": 1,
  "series": "The Lord of the Rings",
  "isbn": "978-0261103573",
  "asin": "B00GO9BZLU",
  "goodreadsId": "3",
  "lccn": "2001271784",
  "oclcNumber": "46516480",
  "openLibraryId": "OL7603610M",
  "libraryThingId": "3203347",
  "googleBooksId": "aWZzLPhY4o0C",
  "barnesAndNobleId": "1100307790",
  "koboId": "lord-of-the-rings-fellowship-of-the-ring",
  "appleBooksId": "id395211",
  "contributors": [
    {
      "value": "00000000-0000-0000-0000-000000000000",
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