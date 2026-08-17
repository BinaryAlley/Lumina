#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.EditLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="EditLibraryViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditLibraryViewEndpointSummary : Summary<EditLibraryViewEndpoint, EditLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditLibraryViewEndpointSummary"/> class.
    /// </summary>
    public EditLibraryViewEndpointSummary()
    {
        Summary = "Renders the edit library view.";
        Description = "Renders the view for editing the media library identified by the request.";

        RequestParam(r => r.Id, "The unique identifier of the media library to edit.");

        Response(200, "The view for editing the media library is rendered.");
    }
}
