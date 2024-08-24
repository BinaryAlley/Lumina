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
POST /books
```

```json
{
    "name": "Yummy Menu",
    "description": "A menu with yummy food",
    "sections": [
        {
            "name": "Appetizers",
            "description": "Starters",
            "items": [
                {
                    "name": "Fried Pickles",
                    "description": "Deep fried pickles"
                }
            ]
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
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "Yummy Menu",
    "description": "A menu with yummy food",
    "averageRating": null,
    "sections": [
        {
            "id": "00000000-0000-0000-0000-000000000000",
            "name": "Appetizers",
            "description": "Starters",
            "items": [
                {
                    "id": "00000000-0000-0000-0000-000000000000",
                    "name": "Fried Pickles",
                    "description": "Deep fried pickles"
                }
            ]
        }
    ],
    "hostId": "00000000-0000-0000-0000-000000000000",
    "dinnerIds": [
        "00000000-0000-0000-0000-000000000000",
    ],
    "menuReviewIds": [
        "00000000-0000-0000-0000-000000000000",
    ],
    "createdDateTime": "2020-01-01T00:00:00.0000000Z",
    "updatedDateTime": "2020-01-01T00:00:00.0000000Z"
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
        "id": "00000000-0000-0000-0000-000000000000",
        "name": "Yummy Menu",
        "description": "A menu with yummy food",
        "averageRating": null,
        "sections": [
            {
                "id": "00000000-0000-0000-0000-000000000000",
                "name": "Appetizers",
                "description": "Starters",
                "items": [
                    {
                        "id": "00000000-0000-0000-0000-000000000000",
                        "name": "Fried Pickles",
                        "description": "Deep fried pickles"
                    }
                ]
            }
        ],
        "hostId": "00000000-0000-0000-0000-000000000000",
        "dinnerIds": [
            "00000000-0000-0000-0000-000000000000",
        ],
        "menuReviewIds": [
            "00000000-0000-0000-0000-000000000000",
        ],
        "createdDateTime": "2020-01-01T00:00:00.0000000Z",
        "updatedDateTime": "2020-01-01T00:00:00.0000000Z"
    }
]
```