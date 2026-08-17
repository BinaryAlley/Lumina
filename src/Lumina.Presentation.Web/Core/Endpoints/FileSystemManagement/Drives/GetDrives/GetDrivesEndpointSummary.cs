#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDrivesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointSummary : Summary<GetDrivesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointSummary"/> class.
    /// </summary>
    public GetDrivesEndpointSummary()
    {
        Summary = "Retrieves the file system drives.";
        Description = "Retrieves the list of file system drives of the machine.";

        Response(200, "The list of file system drives is returned.", example: new { success = true, data = new { drives = new FileSystemTreeNodeDto[] { } } });
    }
}
