#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetBookReaders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBookReadersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadersEndpointSummary : Summary<GetBookReadersEndpoint, GetBookReadersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookReadersEndpointSummary"/> class.
    /// </summary>
    public GetBookReadersEndpointSummary()
    {
        Summary = "Retrieves the book readers of a media library.";
        Description = "Retrieves the book readers of the media library identified by the request, with their supported file extensions and enabled state.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose book readers are retrieved. Required.");

        ExampleRequest = new GetBookReadersRequest(
            LibraryId: Guid.NewGuid()
        );

        Response(200, "The book readers of the media library are returned.");
    }
}
