#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingAvailabilityEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointSummary : Summary<GetReadingAvailabilityEndpoint, GetBookReadingAvailabilityRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointSummary"/> class.
    /// </summary>
    public GetReadingAvailabilityEndpointSummary()
    {
        Summary = "Checks the reading availability of a book.";
        Description = "Checks whether the book can be opened for reading, resolving the book reader configured for its media library and verifying that the reader is enabled, without extracting the book.";
        RequestParam(r => r.BookId, "The Id of the book whose reading availability is checked. Required.");

        ExampleRequest = new GetBookReadingAvailabilityRequest(
            BookId: Guid.NewGuid()
        );

        Response(200, "The reading availability of the book is returned.",
            example: new
            {
                success = false,
                errorCode = "ReaderDisabled",
                libraryId = Guid.NewGuid()
            });
    }
}
