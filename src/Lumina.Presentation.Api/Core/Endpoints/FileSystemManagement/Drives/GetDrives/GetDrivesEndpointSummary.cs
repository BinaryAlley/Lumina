#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDrivesEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetDrivesEndpointSummary : Summary<GetDrivesEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointSummary"/> class.
    /// </summary>
    public GetDrivesEndpointSummary()
    {
        Summary = "Retrieves the list of file system drives.";
        Description = "Returns all available drives in the file system, on Windows, or the root path, on UNIX.";

        Response<IEnumerable<FileSystemTreeNodeResponse>>(200, "The list of file system drives is returned.");
    }
}
