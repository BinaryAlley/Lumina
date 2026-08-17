#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="LibrariesIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrariesIndexViewEndpointSummary : Summary<LibrariesIndexViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesIndexViewEndpointSummary"/> class.
    /// </summary>
    public LibrariesIndexViewEndpointSummary()
    {
        Summary = "Renders the libraries management view.";
        Description = "Renders the view for managing the media libraries.";

        Response(200, "The view for managing the media libraries is rendered.");
    }
}
