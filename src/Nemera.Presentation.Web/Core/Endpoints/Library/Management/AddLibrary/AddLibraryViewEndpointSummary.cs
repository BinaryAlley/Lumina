#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddLibraryViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryViewEndpointSummary : Summary<AddLibraryViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryViewEndpointSummary"/> class.
    /// </summary>
    public AddLibraryViewEndpointSummary()
    {
        Summary = "Renders the add library view.";
        Description = "Renders the view for adding a media library.";

        Response(200, "The view for adding a media library is rendered.");
    }
}
