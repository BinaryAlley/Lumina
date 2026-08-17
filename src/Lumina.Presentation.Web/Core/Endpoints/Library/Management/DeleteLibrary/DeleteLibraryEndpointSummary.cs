#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.DeleteLibrary;

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
        Description = "Deletes the media library identified by the request.";

        RequestParam(r => r.Id, "The unique identifier of the media library to delete.");

        Response(200, "The media library is deleted.", example: new SuccessResponse(true));
    }
}
