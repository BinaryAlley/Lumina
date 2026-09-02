#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingResourceEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointSummary : Summary<GetReadingResourceEndpoint, GetBookReadingResourceRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointSummary"/> class.
    /// </summary>
    public GetReadingResourceEndpointSummary()
    {
        Summary = "Retrieves a resource of a book, for reading.";
        Description = "Retrieves the binary content of a resource of a book, such as an image or a font referenced by a reading section.";
        RequestParam(r => r.BookId, "The Id of the book whose resource is retrieved. Required.");
        RequestParam(r => r.ResourceKey, "The opaque resource key of the resource. Required.");

        ExampleRequest = new GetBookReadingResourceRequest(
            BookId: Guid.NewGuid(),
            ResourceKey: "cover-image"
        );

        Response(200, "The resource of the book is returned.");
    }
}
