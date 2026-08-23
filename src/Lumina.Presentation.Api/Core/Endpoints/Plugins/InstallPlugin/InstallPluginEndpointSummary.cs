#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Class used for providing a textual description for the <see cref="InstallPluginEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointSummary : Summary<InstallPluginEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointSummary"/> class.
    /// </summary>
    public InstallPluginEndpointSummary()
    {
        Summary = "Installs a plugin.";
        Description = "Installs the plugin uploaded in the multipart form of the request, placing its assemblies into the plugin storage directory. The plugin is loaded by the host application at the next startup.";

        Response(200, "The plugin was successfully installed.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/plugins"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/plugins"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/plugins"
                }
            }
        );

        Response(403, "The request failed because the user is not an Admin, or the uploaded plugin archive is not readable.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/plugins",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "PluginArchiveNotReadable",
                    instance = "/api/v1/plugins",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "PluginArchiveContainsNoAssemblies",
                    instance = "/api/v1/plugins",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/plugins",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PluginArchiveCannotBeNull"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
