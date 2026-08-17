#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetEnabledLibrariesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointSummary : Summary<GetEnabledLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointSummary"/> class.
    /// </summary>
    public GetEnabledLibrariesEndpointSummary()
    {
        Summary = "Retrieves the enabled media libraries.";
        Description = "Retrieves the collection of enabled media libraries.";

        Response(200, "The collection of enabled media libraries is returned.", example: new SuccessResponse<LibraryDto[]>(true, default));
    }
}
