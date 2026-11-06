#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeBookReaders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryTypeBookReadersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersEndpointSummary : Summary<GetLibraryTypeBookReadersEndpoint, GetLibraryTypeBookReadersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryTypeBookReadersEndpointSummary()
    {
        Summary = "Retrieves the book readers available for a media library type.";
        Description = "Retrieves the book readers that support the media library type identified by the route, with their supported file extensions, disabled and in alphabetical order, the way a new media library of that type starts.";
        RequestParam(r => r.LibraryType, "The media library type whose available book readers are retrieved. Required.");

        ExampleRequest = new GetLibraryTypeBookReadersRequest(
            LibraryType: "Book"
        );

        Response(200, "The book readers available for the media library type are returned.");
    }
}
