#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingManifestEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestEndpointSummary : Summary<GetReadingManifestEndpoint, GetBookReadingManifestRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpointSummary"/> class.
    /// </summary>
    public GetReadingManifestEndpointSummary()
    {
        Summary = "Retrieves the reading manifest of a book.";
        Description = "Retrieves the reading manifest of a book, containing the metadata, the table of contents, the spine, and the resources needed to render the reader.";
        RequestParam(r => r.BookId, "The Id of the book whose reading manifest is retrieved. Required.");

        ExampleRequest = new GetBookReadingManifestRequest(
            BookId: Guid.NewGuid()
        );

        Response(200, "The reading manifest of the book is returned.");

        Response(200, "The book cannot be read because no book reader is available for its format, or its book reader is disabled for the library.", "application/json",
            example: new
            {
                success = false,
                errorCode = "ReaderDisabled"
            });
    }
}
