#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibrariesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesEndpointSummary : Summary<GetLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpointSummary"/> class.
    /// </summary>
    public GetLibrariesEndpointSummary()
    {
        Summary = "Retrieves the media libraries.";
        Description = "Retrieves the collection of media libraries.";

        Response(200, "The collection of media libraries is returned.", example: new SuccessResponse<LibraryDto[]>(true, default));
    }
}
