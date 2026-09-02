#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// Class used for providing a textual description for the <see cref="ValidatePathEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointSummary : Summary<ValidatePathEndpoint, ValidatePathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointSummary"/> class.
    /// </summary>
    public ValidatePathEndpointSummary()
    {
        Summary = "Validates a file system path.";
        Description = "Validates the file system path identified by the request.";

        RequestParam(r => r.Path, "The file system path to validate. Required.");

        ExampleRequest = new ValidatePathRequest(
            Path: "/media/movies/"
        );

        Response(200, "Whether the file system path is valid is returned.", example: new { success = true, data = new { isValid = true } });
    }
}
