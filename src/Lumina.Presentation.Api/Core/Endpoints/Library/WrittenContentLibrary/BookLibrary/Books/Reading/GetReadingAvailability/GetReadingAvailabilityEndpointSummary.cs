#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingAvailabilityEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointSummary : Summary<GetReadingAvailabilityEndpoint, GetReadingAvailabilityRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointSummary"/> class.
    /// </summary>
    public GetReadingAvailabilityEndpointSummary()
    {
        Summary = "Checks the reading availability of a book.";
        Description = "Checks whether the book can be opened for reading, resolving the book reader configured for its media library and verifying that the reader is enabled, without extracting the book.";

        ExampleRequest = new GetReadingAvailabilityRequest(
            BookId: Guid.NewGuid()
        );

        RequestParam(r => r.BookId, "The Id of the book whose reading availability is checked. Required.");

        ResponseParam<ReadingAvailabilityResponse>(r => r.BookId, "The Id of the book whose reading availability is reported.");
        ResponseParam<ReadingAvailabilityResponse>(r => r.LibraryId, "The Id of the media library the book belongs to.");
        ResponseParam<ReadingAvailabilityResponse>(r => r.IsAvailable, "Whether the book can be opened for reading.");
        ResponseParam<ReadingAvailabilityResponse>(r => r.ErrorCode, "The code of the error preventing the book from being read, when it cannot be read. Can be null when the book is available.");

        Response(200, "The reading availability of the book is returned.",
            example: new ReadingAvailabilityResponse(
                BookId: Guid.NewGuid(),
                LibraryId: Guid.NewGuid(),
                IsAvailable: false,
                ErrorCode: "ReaderDisabled"
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "You are not authorized",
                instance = "/api/v1/books/{bookId}/reading/availability"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books/{bookId}/reading/availability",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the book does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "BookNotFound",
                instance = "/api/v1/books/{bookId}/reading/availability",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/books/{bookId}/reading/availability",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "BookIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
