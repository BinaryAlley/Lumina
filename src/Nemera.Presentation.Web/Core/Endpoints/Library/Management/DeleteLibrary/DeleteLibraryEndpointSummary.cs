#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.DTO.Common;
using Nemera.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteLibraryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryEndpointSummary : Summary<DeleteLibraryEndpoint, DeleteLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpointSummary"/> class.
    /// </summary>
    public DeleteLibraryEndpointSummary()
    {
        Summary = "Deletes a media library.";
        Description = "Deletes the media library identified by the route.";

        RequestParam(r => r.Id, "The unique identifier of the media library to delete. Required.");

        ExampleRequest = new DeleteLibraryRequest(
            Id: Guid.NewGuid()
        );

        Response(200, "The media library is deleted.", example: new SuccessResponse(true));
    }
}
